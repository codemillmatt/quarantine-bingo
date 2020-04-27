using System;
using System.Collections.Generic;
using System.Text;
using MvvmHelpers;

namespace BingoCards.Models
{
    public class BingoNumber : ObservableObject
    {
        public string Column { get; set; }
        public int Number { get; set;  }

        bool selected;
        public bool Selected { get => selected; set =>  SetProperty(ref selected, value); }

        public int RowPosition { get; set; }

        public void SetSelected()
        {
            Selected = true;
        }
    }
}
