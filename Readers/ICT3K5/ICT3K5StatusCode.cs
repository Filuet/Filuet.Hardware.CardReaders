using System.Runtime.InteropServices;

namespace Filuet.Hardware.CardReaders.Readers.ICT3K5
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ICT3K5StatusCode
    {
        public byte bSt0;
        public byte bSt1;
    };
}