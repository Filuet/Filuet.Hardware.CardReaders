using Filuet.Hardware.CardReaders.ICT3K5.Enums;
using System.Runtime.InteropServices;

namespace Filuet.Hardware.CardReaders.ICT3K5
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct REPLY
    {
        public REPLY_TYPE replyType;
        public byte bCommandCode;
        public byte bParameterCode;

        public StatusCode statusCode;

        public int dwSize;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
        public byte[] bBody;
    }
}