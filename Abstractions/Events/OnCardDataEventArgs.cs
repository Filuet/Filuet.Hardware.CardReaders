using System;
using System.Text.Json.Serialization;

namespace Filuet.Hardware.CardReaders.Abstractions.Events
{
    public class CardDataEventArgs : EventArgs
    {
        [JsonPropertyName("cardNumber")]
        public string CardNumber { get; set; }

        [JsonPropertyName("cardHolder")]
        public string CardHolder { get; set; }

        [JsonPropertyName("expiryMonth")]
        public uint ExpiryMonth { get; set; }

        [JsonPropertyName("expiryYear")]
        public uint ExpiryYear { get; set; }

        public override string ToString() => $"{CardHolder} {CardNumber} {ExpiryMonth}/{ExpiryYear}";
    }
}