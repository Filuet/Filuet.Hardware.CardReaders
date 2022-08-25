using Filuet.Hardware.CardReaders.Readers.ICT3K5;
using System;
using Xunit;

namespace Filuet.Hardware.CardReaders.Tests
{
    public class ICT3K5Test
    {
        [Fact]
        public void Test_Manual_Read()
        {
            // Prepare
            ICT3K5Device device = new ICT3K5Device(TimeSpan.FromMilliseconds(5000),
                TimeSpan.FromMilliseconds(500),
                null);

            device.OnCardData += (sender, e) =>
            {
                Assert.NotNull(e);
            };

            // Perform
            device.Read();
        }
    }
}
