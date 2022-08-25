using System.Runtime.InteropServices;

namespace Filuet.Hardware.CardReaders.ICT3K5
{
    [StructLayout(LayoutKind.Sequential)]
    public struct StatusCode
    {
        public byte bSt0;
        public byte bSt1;
    };
}