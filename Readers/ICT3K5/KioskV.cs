using Filuet.Hardware.CardReaders.Abstractions.Events;
using IDTechSDK;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Filuet.Hardware.CardReaders
{
    public class KioskV
    {
        public event EventHandler<CardDataEventArgs> OnCardData;
        public event EventHandler<CardReadFailedEventArgs> OnReadFailed;

        private static Regex regex = new Regex("^[0-9]+$", RegexOptions.Compiled);

        public KioskV(int? timeoutSec = null) {
            if (timeoutSec.HasValue) {
                if (timeoutSec.Value < 1 || timeoutSec > 255)
                    throw new ArgumentException("Timeout must be between 1 and 255 seconds");

                _timeoutSec = timeoutSec.Value;
            }

            IDT_Device.setCallbackIP(MessageCallBack);
            IDT_Device.startUSBMonitoring();
        }

        private void MessageCallBack(IDT_DEVICE_Types type, DeviceState state, byte[] data,
            IDTTransactionData cardData, EMV_Callback emvCallback, RETURN_CODE transactionResultCode, string ident) {

            if (state == DeviceState.Connected) {
                _deviceIdentifier = ident;
            }
            else if (state == DeviceState.TransactionData) {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                string decrypted = Encoding.GetEncoding("big5").GetString(FromHex(Common.getHexStringFromBytes(cardData.emv_unencryptedTags)));
                string cardNumber = ExtractCardNumber(decrypted);
                string expiration = string.Empty;
                if (!string.IsNullOrWhiteSpace(cardNumber)) {
                    expiration = GetExpirationDate(decrypted, cardNumber);

                    OnCardData(this, new CardDataEventArgs {
                        CardNumber = cardNumber,
                        ExpiryMonth = expiration.Length == 4 ? uint.Parse(expiration.Substring(2, 2)) : (uint)DateTime.Now.Month,
                        ExpiryYear = expiration.Length == 4 ? uint.Parse(expiration.Substring(0, 2)) : (uint)DateTime.Now.Year - 2000,
                        System = ExtractSystem(decrypted)
                    });
                }
            }
        }

        /// <summary>
        /// Activate transaction
        /// </summary>
        public void Activate() {
            IDT_Device.startUSBMonitoring();
            RETURN_CODE rt = IDT_Device.SharedController.ctls_activateTransaction(_timeoutSec, new byte[0], false, false, _deviceIdentifier);
            if (rt != RETURN_CODE.RETURN_CODE_DO_SUCCESS) {
                string error = $"Activate Transaction failed Error Code: 0x{string.Format("{0:X}", (ushort)rt)}: {IDTechSDK.errorCode.getErrorString(rt)}";
                OnReadFailed?.Invoke(this, new CardReadFailedEventArgs { Error = error });
            }
        }

        public void Reset() {
            if (string.IsNullOrWhiteSpace(_deviceIdentifier))
                IDT_Device.SharedController.device_rebootDevice(_deviceIdentifier);
        }

        public void Stop() {
            IDT_Device.stopUSBMonitoring();
            IDT_Device.closeAllCommConnections();
        }

        public async Task<bool> Ping() {
            IDT_Device.startUSBMonitoring();

            int index = 0;
            while (string.IsNullOrWhiteSpace(_deviceIdentifier) && index < 30) {
                await Task.Delay(100);
                index++;
            }

            if (string.IsNullOrWhiteSpace(_deviceIdentifier))
                return false;

            RETURN_CODE rt = IDT_Device.SharedController.device_pingDevice(_deviceIdentifier);
            return rt == RETURN_CODE.RETURN_CODE_DO_SUCCESS;
        }

        private byte[] FromHex(string hex) {
            byte[] raw = new byte[hex.Length / 2];
            for (int i = 0; i < raw.Length; i++) {
                raw[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return raw;
        }

        private string ExtractCardNumber(string data) {
            int index = 0;
            while (true) {
                int a = data.IndexOf(";", index);
                if (a == -1)
                    break;

                int b = data.Substring(a + 1).IndexOf("=", index) + a;
                if (b == -1)
                    break;

                string card = data.Substring(a + 1, b - a);

                if (regex.IsMatch(card))
                    return card;

                index = b + 1;
            }

            return string.Empty;
        }

        private string GetExpirationDate(string data, string cardNumber) {
            int index = data.IndexOf(cardNumber) + 1 + cardNumber.Length;
            return data.Substring(index, 4);
        }

        private string ExtractSystem(string data) {
            data = data.ToLower();

            if (data.Contains("mastercard"))
                return "MC";
            else if (data.Contains("visa"))
                return "VI";
            else if (data.Contains("americanexpress") || data.Contains("amex"))
                return "AX";
            else if (data.Contains("jcb"))
                return "JC";
            else if (data.Contains("kookmin") || data.Contains("국민"))
                return "KB"; // Kookmin bank
            else if (data.Contains("samsung") || data.Contains("삼성"))
                return "SS"; // Samsung
            else if (data.Contains("shinhan") || data.Contains("신한"))
                return "SH"; // Shinhan bank
            else if (data.Contains("bc") || data.Contains("비씨"))
                return "BC"; // BC card
            else if (data.Contains("hd") || data.Contains("hyundai") || data.Contains("현대"))
                return "HD"; // Hyundai
            else if (data.Contains("hana") || data.Contains("하나"))
                return "KEB"; // KEB Hana Card
            else if (data.Contains("lotte") || data.Contains("롯데"))
                return "LT"; // Lottecard https://www.lottecard.co.kr/
            else if (data.Contains("nonghyup") || data.Contains("농협"))
                return "NH"; // NongHyup Bank

            return string.Empty;
        }

        private readonly int _timeoutSec = 15;
        private string _deviceIdentifier;
    }
}
