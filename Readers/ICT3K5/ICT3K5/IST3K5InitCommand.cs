using System.Runtime.InteropServices;

namespace Filuet.Hardware.CardReaders.Readers.ICT3K5
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct IST3K5InitCommand
    {
        public byte bCommandCode;
        public byte bParameterCode;
        public int dwSize;
        public void* ptr;
    }
}
