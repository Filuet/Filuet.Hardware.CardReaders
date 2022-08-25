using Filuet.Hardware.CardReaders.Readers.ICT3K5.Enums;
using System.Runtime.InteropServices;

namespace Filuet.Hardware.CardReaders.Readers.ICT3K5
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ICT3K5Response
    {
        public ICT3K5ResponseType replyType;
        public byte bCommandCode;
        public byte bParameterCode;

        public ICT3K5StatusCode statusCode;

        public int dwSize;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
        public byte[] bBody;
    }
}