using System;

namespace Filuet.Hardware.CardReaders.ICT3K5.Events
{
    public class OnDataEventArgs : EventArgs
    {
        public string CardNumber { get; set; }
        public string CardHolder { get; set; }
        public uint ExpiryMonth { get; set; }
        public uint ExpiryYear { get; set; }
    }
}
