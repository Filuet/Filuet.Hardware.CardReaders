using System;

namespace Filuet.Hardware.CardReaders.Abstractions.Events
{
    public class CardReadFailedEventArgs : EventArgs
    {
        public string Error { get; set; }
    }
}
