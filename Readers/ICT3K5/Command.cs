using System.Runtime.InteropServices;

namespace Filuet.Hardware.CardReaders.Readers.ICT3K5
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ICT3K5Command
    {
        public byte bCommandCode;
        public byte bParameterCode;
        public int dwSize;
        public byte[] lpbBody;
    }
}