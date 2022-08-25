using Filuet.Hardware.CardReaders.ICT3K5;
using System;
using System.Threading;
using System.Threading.Tasks;


namespace PoC
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            ICT3K5Device device = new ICT3K5Device(TimeSpan.FromMilliseconds(500),
              TimeSpan.FromMilliseconds(500),
              null);

            device.OnCardData += (sender, e) =>
            {
                Console.WriteLine($"{e.CardNumber}/{e.CardHolder}/{e.ExpiryYear}/{e.ExpiryMonth}");
            };

            device.OnReadFailed += (sender, e) =>
            {
                Console.WriteLine("Card reading failed");
            };

            // Perform
            await device.Read();

            await device.Read();

            Console.ReadLine();
        }
    }
}
