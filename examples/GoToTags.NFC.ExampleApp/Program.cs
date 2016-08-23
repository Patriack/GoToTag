using GoToTags.Nfc;
using GoToTags.Nfc.Devices;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GoToTags.NFC.ExampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // listen for user trying to cancel app
                Console.CancelKeyPress += Console_CancelKeyPress;

                // init the NfcManager; must call once at start of app
                NfcManager.Instance.Init();

                // refresh the devices
                DeviceManager.Instance.RefreshDevices();

                // get the current devices
                var devices = DeviceManager.Instance.Devices;

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
            finally
            {
                Dispose();
            }

            Console.WriteLine();
            Console.WriteLine("DONE: Press 'Enter' to quit.");
            Console.ReadLine();
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs args)
        {
            Console.WriteLine("CANCELLING");
            Console.WriteLine();

            Dispose();

            Environment.Exit(-1);
        }

        private static void Dispose()
        {
            // cleanly shutdown the NfcManager
            NfcManager.Instance.Dispose();
        }
    }
}
