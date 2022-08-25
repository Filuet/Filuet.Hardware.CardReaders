using Filuet.Hardware.CardReaders.ICT3K5.Enums;
using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Filuet.Hardware.CardReaders.ICT3K5
{
    /// <summary>
    /// Besides ICT3K5_6290DLL.dll, PrtclUH.dll is needed as well!
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    internal class UnsafeNativeMethods
    {
        [DllImport("C:\\Filuet\\Filuet.Pos.Agent\\ICT3K5_6290DLL.dll", SetLastError = true, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern ICT3K5ErrorCode ConnectDevice(string inpStr, [MarshalAs(UnmanagedType.LPStr)] StringBuilder outString);

        [DllImport("C:\\Filuet\\Filuet.Pos.Agent\\ICT3K5_6290DLL.dll", SetLastError = true, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern ICT3K5ErrorCode DisconnectDevice(string inputSerial);

        [DllImport("C:\\Filuet\\Filuet.Pos.Agent\\ICT3K5_6290DLL.dll", CharSet = CharSet.None, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern ICT3K5ErrorCode ExecuteCommand(string serial, [MarshalAs(UnmanagedType.Struct)] Command command, int timeout, [MarshalAs(UnmanagedType.Struct)] ref ICT3K5Response reply);

        [DllImport("C:\\Filuet\\Filuet.Pos.Agent\\ICT3K5_6290DLL.dll", CharSet = CharSet.None, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern ICT3K5ErrorCode ExecuteCommand(string serial, [MarshalAs(UnmanagedType.Struct)] ICT3K5Response cmd, int timeout, [MarshalAs(UnmanagedType.Struct)] ref ICT3K5Response reply);

        [DllImport("C:\\Filuet\\Filuet.Pos.Agent\\ICT3K5_6290DLL.dll", CharSet = CharSet.None, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern ICT3K5ErrorCode ExecuteCommand(string serial, [MarshalAs(UnmanagedType.Struct)] InitCommand cmd, int timeout, [MarshalAs(UnmanagedType.Struct)] ref ICT3K5Response reply);

        [DllImport("C:\\Filuet\\Filuet.Pos.Agent\\ICT3K5_6290DLL.dll", CharSet = CharSet.None, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int CancelCommand(String comport);
    }
}
