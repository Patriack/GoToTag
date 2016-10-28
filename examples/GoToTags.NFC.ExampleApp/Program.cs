using GoToTags.Common.Licensing;
using GoToTags.Nfc;
using GoToTags.Nfc.Devices;
using System;
using System.Linq;

namespace GoToTags.Nfc.ExampleConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // listen for user trying to cancel app
                Console.CancelKeyPress += Console_CancelKeyPress;

                // set your license code here
                LicenseManager.Instance.Unlock("");

                // get the current devices
                var devices = DeviceManager.Instance.GetDevices();

                // show the devices as json
                Console.WriteLine("DEVICES");
                devices.ToList().ForEach(device => Console.WriteLine(device.ToJson(true)));
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine();
            }

            Console.WriteLine();
            Console.WriteLine("DONE: Press 'Enter' to quit.");
            Console.ReadLine();
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs args)
        {
            Console.WriteLine("CANCELLING");
            Console.WriteLine();

            Environment.Exit(-1);
        }
    }
}
