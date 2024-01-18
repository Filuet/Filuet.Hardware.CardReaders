using Filuet.Hardware.CardReaders.Abstractions.Events;
using Filuet.Hardware.CardReaders.Readers.ICT3K5.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Filuet.Hardware.CardReaders.Tests")]
[assembly: InternalsVisibleTo("Filuet.Hardware.CardReaders.PoC")]
namespace Filuet.Hardware.CardReaders.Readers.ICT3K5
{
    public class ICT3K5Device
    {
        public event EventHandler<CardDataEventArgs> OnCardData;
        public event EventHandler<CardReadFailedEventArgs> OnReadFailed;

        public ICT3K5Device() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="holdCardTimeout">How long hold back the card before ejecting</param>
        /// <param name="beforeNextTryTimeout">How long wait before next try to read the card</param>
        /// <param name="logger"></param>
        public ICT3K5Device(TimeSpan holdCardTimeout, TimeSpan beforeNextTryTimeout, ILogger<ICT3K5Device> logger = null)
        {
            Initialize(holdCardTimeout, beforeNextTryTimeout, logger);
        }

        public void Initialize(TimeSpan holdCardTimeout, TimeSpan beforeNextTryTimeout, ILogger<ICT3K5Device> logger = null)
        {
            _holdCardTimeout = holdCardTimeout.TotalMilliseconds == 0 ? TimeSpan.FromMilliseconds(500) : holdCardTimeout;
            _beforeNextTryTimeout = beforeNextTryTimeout.TotalMilliseconds == 0 ? TimeSpan.FromMilliseconds(500) : beforeNextTryTimeout;
            _logger = logger;
        }

        public bool IsAvailable
        {
            get
            {
                try
                {
                    if (OpenCOM() && Enabler())
                        return true;
                }
                catch { }
                finally {
                    Disable();
                    CloseCOM();
                }

                return false;
            }
        }

        public async Task Read()
            => await Task.Factory.StartNew(() => ReadCard());

        /// <summary>
        /// Start reading the card
        /// </summary>
        private void ReadCard()
        {
            _stopped = false;

            if (!OpenCOM())
                return;

            if (!Enabler() && Initialize() && !Enabler())
                return;

            int rereadAttempts = 0;
            bool on = false;

            while (!_stopped)
            {
                if (CheckStatus() != ICT3K5CardReaderStatus.CardPresent)
                {
                    on = !on;
                    SetLed(false, on, false, false);
                }
                else // card is being detected
                {
                    on = true;

                    string[] tracks = ReadTracks();

                    if (tracks != null && tracks.Length > 0)
                    {
                        var splitted = tracks[0].Split('^', StringSplitOptions.RemoveEmptyEntries);
                        if (splitted.Length >= 3)
                            OnCardData?.Invoke(this, new CardDataEventArgs { CardNumber = splitted[0].Replace("B", string.Empty).Trim(),
                                CardHolder = splitted[1].Trim(),
                                ExpiryYear = Convert.ToUInt32(splitted[2].Split(' ')[0].Substring(0, 2).Trim()),
                                ExpiryMonth = Convert.ToUInt32(splitted[2].Split(' ')[0].Substring(2, 2).Trim()) });

                        SetLed(false, false, true, false);
                        Thread.Sleep(_holdCardTimeout);
                        Eject();
                        rereadAttempts = 0;
                    }
                    else
                    {
                        if (rereadAttempts > 0)
                        {
                            Eject();
                            rereadAttempts = 0;
                            _logger?.LogError("Failed while reading the card");
                        }
                        else
                        {
                            SetLed(true, false, false, false);
                            ReReadCard();
                            rereadAttempts++;
                        }
                    }
                }

                Thread.Sleep(_beforeNextTryTimeout);
            }

            Eject();

            CloseCOM();

            _logger?.LogInformation("The device has been stopped");
        }

        /// <summary>
        /// Eject card
        /// </summary>
        /// <returns></returns>
        public bool Eject()
        {
            ICT3K5Command cmd = new ICT3K5Command();
            cmd.bCommandCode = 0x33;
            cmd.bParameterCode = 0x30;

            cmd.dwSize = 0x00;
            ICT3K5Response reply = new ICT3K5Response();
            ICT3K5ErrorCode err = ICT3K5UnsafeNativeMethods.ExecuteCommand(_serialNumber, cmd, 20000, ref reply);
            if (err == ICT3K5ErrorCode.NO_ERROR)
                if (reply.replyType == ICT3K5ResponseType.PositiveReply)
                    return true;

            return false;
        }

        public void StopCardReader()
        {
            _stopped = true;
        }

        /// <summary>
        /// Open COM port to communicate
        /// </summary>
        /// <returns></returns>
        private unsafe bool OpenCOM()
        {
            try
            {
                if (!_deviceOpened)
                {
                    StringBuilder serial = new StringBuilder();
                    ICT3K5ErrorCode e = ICT3K5UnsafeNativeMethods.ConnectDevice(null, serial);
                    if (e == ICT3K5ErrorCode.CANNOT_CREATE_OBJECT_ERROR
                        || e == ICT3K5ErrorCode.CANNOT_OPEN_DRIVER_ERROR
                        || e == ICT3K5ErrorCode.FAILED_TO_BEGIN_THREAD_ERROR)
                    {
                        _logger?.LogError($"Unable to open port: {e}");
                        return false;
                    }
                    _serialNumber = serial.ToString();
                    _deviceOpened = true;
                    if (!Initialize())
                        _logger?.LogError("Failed to initialize");
                    else _logger?.LogInformation("COM port was opened successfully");
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, string.Empty);
                return false;
            }
        }

        private bool CloseCOM()
        {
            try
            {
                if (_deviceOpened)
                {
                    Disable();
                    ICT3K5UnsafeNativeMethods.DisconnectDevice(_serialNumber);
                    _deviceOpened = false;
                }
                _logger?.LogInformation("COM port was closed successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, string.Empty);
                return false;
            }
        }

        private unsafe bool Initialize()
        {
            for (int i = 0; i < 3; i++)
            {
                ICT3K5Command cmd = new ICT3K5Command();
                cmd.bCommandCode = 0x30;
                cmd.bParameterCode = 0x30;
                cmd.dwSize = 0x0a;
                cmd.lpbBody = new byte[10];
                cmd.lpbBody[0] = 0x33;
                cmd.lpbBody[1] = 0x32;
                cmd.lpbBody[2] = 0x34;
                cmd.lpbBody[3] = 0x31;
                cmd.lpbBody[4] = 0x30;
                cmd.lpbBody[5] = 0x30;
                cmd.lpbBody[6] = 0x31;
                cmd.lpbBody[7] = 0x30;
                cmd.lpbBody[8] = 0x30;
                cmd.lpbBody[9] = 0x30;

                IntPtr iPtr = Marshal.AllocHGlobal(10);
                Marshal.Copy(cmd.lpbBody, 0, iPtr, 10);

                IST3K5InitCommand iCmd = new IST3K5InitCommand();
                iCmd.bCommandCode = 0x30;
                iCmd.bParameterCode = 0x30;
                iCmd.dwSize = 0x0a;
                iCmd.ptr = iPtr.ToPointer();

                ICT3K5Response reply = new ICT3K5Response();
                ICT3K5ErrorCode err = ICT3K5UnsafeNativeMethods.ExecuteCommand(_serialNumber, iCmd, 20000, ref reply);
                Marshal.FreeHGlobal(iPtr);

                if (err != ICT3K5ErrorCode.NO_ERROR)
                {
                    _logger?.LogError($"Driver returned {err}");
                    return false;
                }

                if (reply.replyType == ICT3K5ResponseType.PositiveReply)
                {
                    _logger?.LogInformation($"Positive reply {reply.statusCode.bSt0:X2} {reply.statusCode.bSt1:X2}");
                    return true;
                }
            }
            return false;
        }

        private void ReReadCard()
        {
            ICT3K5Command cmd = new ICT3K5Command();
            cmd.bCommandCode = 0x33;
            cmd.bParameterCode = 0x30;

            cmd.dwSize = 0x00;
            ICT3K5Response reply = new ICT3K5Response();
            ICT3K5ErrorCode err = ICT3K5UnsafeNativeMethods.ExecuteCommand(_serialNumber, cmd, 20000, ref reply);

            cmd.bCommandCode = 0x34;
            reply = new ICT3K5Response();
            err = ICT3K5UnsafeNativeMethods.ExecuteCommand(_serialNumber, cmd, 20000, ref reply);
        }

        private bool Enabler()
        {
            ICT3K5Command cmd = new ICT3K5Command();
            cmd.bCommandCode = 0x3a;
            cmd.bParameterCode = 0x30;
            cmd.dwSize = 0;
            ICT3K5Response reply = new ICT3K5Response();
            ICT3K5ErrorCode err = ICT3K5UnsafeNativeMethods.ExecuteCommand(_serialNumber, cmd, 1000, ref reply);
            if (err != ICT3K5ErrorCode.NO_ERROR)
            {
                _logger?.LogError($"Driver returned {err}");
                return false;
            }

            if (reply.replyType == ICT3K5ResponseType.PositiveReply)
            {
                _logger?.LogInformation($"Positive response detected {reply.statusCode.bSt0:X2} {reply.statusCode.bSt1:X2}");
                return true;
            }
            else
            {
                _logger?.LogInformation($"Negative response detected {reply.statusCode.bSt0:X2} {reply.statusCode.bSt1:X2}");
                return false;
            }
        }

        private ICT3K5CardReaderStatus CheckStatus()
        {
            ICT3K5Command cmd = new ICT3K5Command();
            cmd.bCommandCode = 0x31;
            cmd.bParameterCode = 0x31;
            cmd.dwSize = 0x00;
            ICT3K5Response reply = new ICT3K5Response();
            ICT3K5ErrorCode err = ICT3K5UnsafeNativeMethods.ExecuteCommand(_serialNumber, cmd, 1000, ref reply);
            if (reply.replyType == ICT3K5ResponseType.PositiveReply && reply.statusCode.bSt0 == 0x30 && reply.statusCode.bSt1 == 0x32)
                return ICT3K5CardReaderStatus.CardPresent;
            else if (reply.replyType == ICT3K5ResponseType.NegativeReply)
            {
                OnReadFailed?.Invoke(this, new CardReadFailedEventArgs());
                _stopped = true;
            }

            return ICT3K5CardReaderStatus.NoCard;
        }

        private string[] ReadTracks()
        {
            string tr1 = "", tr2 = "";
            ICT3K5Command cmd = new ICT3K5Command();
            cmd.bCommandCode = 0x36;
            cmd.bParameterCode = 0x31;
            cmd.dwSize = 0;

            ICT3K5Response response = new ICT3K5Response();
            ICT3K5ErrorCode err = ICT3K5UnsafeNativeMethods.ExecuteCommand(_serialNumber, cmd, 1000, ref response);
            if (err != ICT3K5ErrorCode.NO_ERROR)
            {
                _logger?.LogError("Driver return an error " + err.ToString());
                return null;
            }
            tr1 = Encoding.ASCII.GetString(response.bBody, 0, response.dwSize);

            if (string.IsNullOrEmpty(tr1)) return null;
            cmd.bParameterCode = 0x32;

            ICT3K5Response response1 = new ICT3K5Response();
            err = ICT3K5UnsafeNativeMethods.ExecuteCommand(_serialNumber, cmd, 1000, ref response1);
            if (err != ICT3K5ErrorCode.NO_ERROR)
            {
                _logger?.LogError($"Driver returned {err}");
                return null;
            }
            tr2 = Encoding.ASCII.GetString(response1.bBody, 0, response1.dwSize);
            if (string.IsNullOrEmpty(tr2)) return null;
            return new string[] { tr1, tr2 };
        }

        private bool Disable()
        {
            SetLed(false, false, false, false);
            ICT3K5Command cmd = new ICT3K5Command();
            cmd.bCommandCode = 0x3a;
            cmd.bParameterCode = 0x31;
            cmd.dwSize = 0;
            ICT3K5Response reply = new ICT3K5Response();
            ICT3K5ErrorCode err = ICT3K5UnsafeNativeMethods.ExecuteCommand(_serialNumber, cmd, 1000, ref reply);
            if (err != ICT3K5ErrorCode.NO_ERROR)
            {
                _logger?.LogError(err.ToString());
                return false;
            }
            if (reply.replyType == ICT3K5ResponseType.PositiveReply)
                return true;
            return false;
        }

        private void SetLed(bool Red, bool Green, bool Orange, bool Flash)
        {
            ICT3K5Command cmd = new ICT3K5Command();
            cmd.bCommandCode = 0x35;
            if (Red)
                cmd.bParameterCode = 0x32;
            else if (Green)
                cmd.bParameterCode = 0x31;
            else if (Orange)
                cmd.bParameterCode = 0x33;
            else
                cmd.bParameterCode = 0x30;

            cmd.dwSize = 0;
            ICT3K5Response reply = new ICT3K5Response();
            ICT3K5UnsafeNativeMethods.ExecuteCommand(_serialNumber, cmd, 20000, ref reply);
        }

        private TimeSpan _holdCardTimeout;
        private TimeSpan _beforeNextTryTimeout;
        private ILogger<ICT3K5Device> _logger;
        private bool _deviceOpened = false;
        private string _serialNumber = string.Empty;
        private bool _stopped = true;
    }
}