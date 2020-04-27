using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using BingoCards.Models;
using BingoCards.Services;
using Microsoft.AppCenter.Crashes;
using MvvmHelpers;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace BingoCards.ViewModels
{
    public class BingoPageViewModel : BaseViewModel
    {
        enum BingoColumns
        {
            B,I,N,G,O
        }

        readonly string startListening = "Start Listening";
        readonly string stopListening = "Stop Listening";
        bool isListening = false;

        MicrophoneService micService;

        public BingoPageViewModel()
        {
            BingoHeader = new ObservableRangeCollection<string>(
                new string[] { "B","I","N","G","O"}
            );

            micService = new MicrophoneService();

            ResetCardsCommand = new Command(() => ExecuteResetCardsCommand());
            NumberTappedCommand = new Command<BingoNumber>(async (number) => await ExecuteNumberTappedCommand(number));
            ListenCommand = new Command(async () => await ExecuteListenCommand());

            // initialze the card
            BNumbers = new ObservableRangeCollection<BingoNumber>(InitializeColumn(BingoColumns.B));
            INumbers = new ObservableRangeCollection<BingoNumber>(InitializeColumn(BingoColumns.I));
            NNumbers = new ObservableRangeCollection<BingoNumber>(InitializeColumn(BingoColumns.N));
            GNumbers = new ObservableRangeCollection<BingoNumber>(InitializeColumn(BingoColumns.G));
            ONumbers = new ObservableRangeCollection<BingoNumber>(InitializeColumn(BingoColumns.O));

            ListenStatus = startListening;
        }

        ObservableRangeCollection<string> bingoHeader;
        public ObservableRangeCollection<string> BingoHeader { get => bingoHeader; set => SetProperty(ref bingoHeader, value); }

        ObservableRangeCollection<BingoNumber> bNumbers;
        public ObservableRangeCollection<BingoNumber> BNumbers { get => bNumbers; set => SetProperty(ref bNumbers, value); }

        ObservableRangeCollection<BingoNumber> iNumbers;
        public ObservableRangeCollection<BingoNumber> INumbers { get => iNumbers; set => SetProperty(ref iNumbers, value); }

        ObservableRangeCollection<BingoNumber> nNumbers;
        public ObservableRangeCollection<BingoNumber> NNumbers { get => nNumbers; set => SetProperty(ref nNumbers, value); }

        ObservableRangeCollection<BingoNumber> gNumbers;
        public ObservableRangeCollection<BingoNumber> GNumbers { get => gNumbers; set => SetProperty(ref gNumbers, value); }

        ObservableRangeCollection<BingoNumber> oNumbers;
        public ObservableRangeCollection<BingoNumber> ONumbers { get => oNumbers; set => SetProperty(ref oNumbers, value); }

        string listenStatus;
        public string ListenStatus { get => listenStatus; set => SetProperty(ref listenStatus, value); }

        public ICommand ResetCardsCommand { get; }

        public ICommand NumberTappedCommand { get; }

        public ICommand ListenCommand { get; }

        void ExecuteResetCardsCommand()
        {
            BNumbers = new ObservableRangeCollection<BingoNumber>(InitializeColumn(BingoColumns.B));
            INumbers = new ObservableRangeCollection<BingoNumber>(InitializeColumn(BingoColumns.I));
            NNumbers = new ObservableRangeCollection<BingoNumber>(InitializeColumn(BingoColumns.N));
            GNumbers = new ObservableRangeCollection<BingoNumber>(InitializeColumn(BingoColumns.G));
            ONumbers = new ObservableRangeCollection<BingoNumber>(InitializeColumn(BingoColumns.O));
        }

        async Task ExecuteNumberTappedCommand(BingoNumber number)
        {
            // Check the free space
            if (number.Number == 0)
                return;

            if (number.Selected)
                number.Selected = false;
            else
                number.Selected = true;

            if (CheckWinners())
            {
                var displayTask = Shell.Current.DisplayAlert("WINNER!", "WINNER WINNER CHICKEN DINNER!", "I WON!");

                var speechTask = TextToSpeech.SpeakAsync("Winner winner chicken dinner! Matt is the best because wait until you get a load of this zinger!");

                await Task.WhenAll(displayTask, speechTask);
            }
        }

        async Task ExecuteListenCommand()
        {
            if (isListening)
            {
                await StopListening();
            }
            else
            {
                await StartListening();
            }            
        }

        async Task StartListening()
        {            
            var permissionStatus = await micService.CheckAndRequestMicrophonePermission();

            if (permissionStatus != PermissionStatus.Granted)
            {
                await Shell.Current.DisplayAlert("No Mic!", "Can't get access to the mic!", "OK");
                return;
            }


            try
            {
                await micService.StartTranscription();

                micService.BingoNumberCalled += MicService_BingoNumberCalled;

                // Display on the button you can stop listening
                ListenStatus = stopListening;

                isListening = true;
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);

                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");

                isListening = false;
            }
        }       

        async Task StopListening()
        {
            await micService.StopTranscription();

            micService.BingoNumberCalled -= MicService_BingoNumberCalled;

            // Display on the buttn you can start listening
            ListenStatus = startListening;

            isListening = false;
        }

        private void MicService_BingoNumberCalled(object sender, int calledNumber)
        {
            // go looking for our number
            BNumbers.FirstOrDefault(b => b.Number == calledNumber)?.SetSelected();
            INumbers.FirstOrDefault(i => i.Number == calledNumber)?.SetSelected();
            NNumbers.FirstOrDefault(n => n.Number == calledNumber)?.SetSelected();
            GNumbers.FirstOrDefault(g => g.Number == calledNumber)?.SetSelected();
            ONumbers.FirstOrDefault(o => o.Number == calledNumber)?.SetSelected();

            if (CheckWinners())
            {
                MainThread.BeginInvokeOnMainThread(async () => {
                    var displayTask = Shell.Current.DisplayAlert("WINNER!", "WINNER WINNER CHICKEN DINNER!", "I WON!");

                    var speechTask = TextToSpeech.SpeakAsync("Winner winner chicken dinner!");

                    await Task.WhenAll(displayTask, speechTask);
                });
            }
        }

        bool CheckWinners()
        {           
            // Run through B
            if (BNumbers[0].Selected && BNumbers[1].Selected && BNumbers[2].Selected && BNumbers[3].Selected && BNumbers[4].Selected)
                return true;

            // Run through I
            if (INumbers[0].Selected && INumbers[1].Selected && INumbers[2].Selected && INumbers[3].Selected && INumbers[4].Selected)
                return true;

            // Run through N
            if (NNumbers[0].Selected && NNumbers[1].Selected && NNumbers[2].Selected && NNumbers[3].Selected && NNumbers[4].Selected)
                return true;

            // Run through G
            if (GNumbers[0].Selected && GNumbers[1].Selected && GNumbers[2].Selected && GNumbers[3].Selected && GNumbers[4].Selected)
                return true;

            // Run through O
            if (ONumbers[0].Selected && ONumbers[1].Selected && ONumbers[2].Selected && ONumbers[3].Selected && ONumbers[4].Selected)
                return true;

            // Check the diagonols
            if (BNumbers[0].Selected && INumbers[1].Selected && NNumbers[2].Selected && GNumbers[3].Selected && ONumbers[4].Selected)
                return true;

            if (BNumbers[4].Selected && INumbers[3].Selected && NNumbers[2].Selected && GNumbers[1].Selected && ONumbers[0].Selected)
                return true;

            // Check the horizontals
            if (BNumbers[0].Selected && INumbers[0].Selected && NNumbers[0].Selected && GNumbers[0].Selected && ONumbers[0].Selected)
                return true;

            if (BNumbers[1].Selected && INumbers[1].Selected && NNumbers[1].Selected && GNumbers[1].Selected && ONumbers[1].Selected)
                return true;

            if (BNumbers[2].Selected && INumbers[2].Selected && NNumbers[2].Selected && GNumbers[2].Selected && ONumbers[2].Selected)
                return true;

            if (BNumbers[3].Selected && INumbers[3].Selected && NNumbers[3].Selected && GNumbers[3].Selected && ONumbers[3].Selected)
                return true;

            if (BNumbers[4].Selected && INumbers[4].Selected && NNumbers[4].Selected && GNumbers[4].Selected && ONumbers[4].Selected)
                return true;

            return false;
        }

        List<BingoNumber> InitializeColumn(BingoColumns column)
        {
            int min = 0;
            int max = 0;

            // Figure out what the max & min are for the column
            switch (column)
            {
                case BingoColumns.B:
                    min = 1;
                    max = 15;
                    break;
                case BingoColumns.I:
                    min = 16;
                    max = 30;
                    break;
                case BingoColumns.N:
                    min = 31;
                    max = 45;
                    break;
                case BingoColumns.G:
                    min = 46;
                    max = 60;
                    break;
                case BingoColumns.O:
                    min = 61;
                    max = 75;
                    break;
                default:
                    throw new ArgumentException();                    
            }

            List<BingoNumber> numbers = new List<BingoNumber>();
     
            for (var rowPosition = 0; rowPosition < 5; rowPosition++)
            {
                var generator = new Random();
                    
                var random = generator.Next(min, max);

                while (numbers.Any(x => x.Number == random))
                {
                    random = generator.Next(min, max);
                }

                if (column == BingoColumns.N && rowPosition == 2)
                {
                    numbers.Add(new BingoNumber { Column = "N", Number = 0, RowPosition = rowPosition, Selected = true });
                    continue;
                }

                numbers.Add(new BingoNumber { Column = column.ToString(), Number = random, RowPosition = rowPosition });
            }
                    

            return numbers;
        }
    }    
}
