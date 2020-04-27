using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace BingoCards.Converters
{
    public class SelectedColorConverter : IValueConverter
    {

        List<ColorCombo> ColorCombos = new List<ColorCombo>
        {
            new ColorCombo{ Start = Color.FromHex("#a8edea"), Finish = Color.FromHex("#fed6e3") },
            new ColorCombo{ Start = Color.FromHex("#2af598"), Finish = Color.FromHex("#009efd") },
            new ColorCombo{ Start = Color.FromHex("#00c6fb"), Finish = Color.FromHex("#005bea") },
            new ColorCombo{ Start = Color.FromHex("#f43b47"), Finish = Color.FromHex("#453a94") },
            new ColorCombo{ Start = Color.FromHex("#7028e4"), Finish = Color.FromHex("#e5b2ca") }
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var rando = new Random().Next(0, 4);

            if (!(bool)value)
                return Color.FromHex("#FFFFFF");

            if ((string)parameter == "start")
                return ColorCombos[rando].Start;
            else
                return ColorCombos[rando].Finish;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class ColorCombo
    {
        public Color Start { get; set; }
        public Color Finish { get; set; }
    }
}
