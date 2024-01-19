using Filuet.Hardware.CardReaders.Readers.ICT3K5;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Filuet.Hardware.CardReaders.Tests
{
    public class KioskVTest
    {
        [Fact]
        public async Task Test_Contactless_read()
        {
            
            KioskV device = new KioskV();

            device.OnCardData += (sender, e) => {
                Assert.NotNull(e);
            };

            device.OnReadFailed += (sender, e) => {
                Assert.NotNull(e);
            };

            while (true) {
                // Prepare
                

                // Perform
                device.Activate();

                //await Task.Delay(10000);

                //device.Stop();
            }
        }

        [Fact]
        public async Task Test_Contactless_reset() {
            // Prepare
            KioskV device = new KioskV();

            // Perform
            device.Reset();
        }

        [Fact]
        public async Task Test_Contactless_ping() {
            // Prepare
            KioskV device = new KioskV();

            // Perform
            bool status = await device.Ping();
        }
    }
}
