using System.Runtime.InteropServices;

namespace Filuet.Hardware.CardReaders.ICT3K5
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct InitCommand
    {
        public byte bCommandCode;
        public byte bParameterCode;
        public int dwSize;
        public void* ptr;
    }
}
