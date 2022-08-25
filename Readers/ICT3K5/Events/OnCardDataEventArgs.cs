using System;

namespace Filuet.Hardware.CardReaders.ICT3K5.Events
{
    public class OnCardDataEventArgs : EventArgs
    {
        public string Track1 { get; set; }
        public string Track2 { get; set; }
    }
}
