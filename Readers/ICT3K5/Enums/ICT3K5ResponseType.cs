namespace Filuet.Hardware.CardReaders.Readers.ICT3K5.Enums
{
    internal enum ICT3K5ResponseType : uint
    {
        PositiveReply = 0,
        NegativeReply = 1,
        ReplyReceivingFailure = 2,
        CommandCancellation = 3,
        ReplyTimeout = 4,
    }
}