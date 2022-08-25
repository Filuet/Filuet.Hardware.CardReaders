using System.Runtime.InteropServices;

namespace Filuet.Hardware.CardReaders.ICT3K5
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Command
    {
        public byte bCommandCode;
        public byte bParameterCode;
        public int dwSize;
        public byte[] lpbBody;
    }
}