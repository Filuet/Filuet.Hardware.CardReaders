namespace Filuet.Hardware.CardReaders.ICT3K5.Enums
{
    internal enum REPLY_TYPE : uint
    {
        PositiveReply = 0,
        NegativeReply = 1,
        ReplyReceivingFailure = 2,
        CommandCancellation = 3,
        ReplyTimeout = 4,
    }
}