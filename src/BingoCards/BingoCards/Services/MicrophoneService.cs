using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Xamarin.Essentials;

using Microsoft.AppCenter.Crashes;

namespace BingoCards.Services
{
    public class MicrophoneService
    {
        public event EventHandler<int> BingoNumberCalled;

        SpeechRecognizer recognizer;
        bool isRecognizing;

        public async Task<PermissionStatus> CheckAndRequestMicrophonePermission()
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.Microphone>();

                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.Microphone>();
                }

                return status;
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);

                return PermissionStatus.Denied;
            }
        }

        public async Task StartTranscription()
        {
            if (recognizer == null)
            {
# error enter your cog services api key
                var apiKey = "<<ENTER YOUR COG SVCS KEY HERE>>";
                var config = SpeechConfig.FromSubscription(apiKey, "westus");

                recognizer = new SpeechRecognizer(config);

                recognizer.Recognized += Recognizer_Recognized;
            }

            if (isRecognizing)
                return;

            await recognizer.StartContinuousRecognitionAsync();


            isRecognizing = true;
        }

        public async Task StopTranscription()
        {
            if (!isRecognizing)
                return;

            await recognizer.StopContinuousRecognitionAsync();

            recognizer.Recognized -= Recognizer_Recognized;

            recognizer = null;

            isRecognizing = false;
        }

        private void Recognizer_Recognized(object sender, SpeechRecognitionEventArgs e)
        {
            // parse for a number
            int bingoNumber = 0;

            System.Diagnostics.Debug.WriteLine(e.Result.Text);

            StringBuilder bingoBuilder = new StringBuilder();

            int individualNumber = 0;

            // loop through each character returned
            foreach (var item in e.Result.Text)
            {
                if (int.TryParse(item.ToString(), out individualNumber))
                {
                    bingoBuilder.Append(individualNumber);
                }

                if (bingoBuilder.Length == 3)
                    break;
            }

            if (bingoBuilder.Length == 3)
            {
                // get rid of the first number
                bingoBuilder.Remove(0, 1);
            }


            if (int.TryParse(bingoBuilder.ToString(), out bingoNumber))
            {
                if (bingoNumber > 0 && bingoNumber < 76)
                {
                    // Raise the event!
                    BingoNumberCalled?.Invoke(this, bingoNumber);
                }
            }

        }
    }
}