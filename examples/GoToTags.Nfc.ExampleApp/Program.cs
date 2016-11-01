using GoToTags.Common;
using GoToTags.Common.Json;
using GoToTags.Common.Licensing;
using GoToTags.Nfc;
using GoToTags.Nfc.Devices;
using System;
using System.Collections.Generic;
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
                // keep your license code safe!
                LicenseManager.Instance.Unlock("");

                // what version are we using?                
                Console.WriteLine(AssemblyHelper.GetFullName(typeof(NfcHelper).Assembly));
                Console.WriteLine();

                // get infomation about the license
                License license = LicenseManager.Instance.License;
                Console.WriteLine("LICENSE");
                Console.WriteLine(license.ToJson(true));
                Console.WriteLine();

                // get the current devices
                IEnumerable<Device> devices = DeviceManager.Instance.GetDevices();

                // show the devices as json
                Console.WriteLine("DEVICES");
                Console.WriteLine(JsonHelper.ToJson(devices, true));
                Console.WriteLine();

                // get first device
                // can use LINQ to get device(s) based on device properties and type; ex: devices.Where(d => d.DeviceType == DeviceType.Acr122).All();
                Device device = devices.FirstOrDefault();

                if (device != null)
                {
                    Console.WriteLine($"USING DEVICE: {device.Name}");
                    Console.WriteLine();

                    // is the device currenlty connected to any tags?
                    TagInformation[] tagInformations = device.GetTags();
                    Console.WriteLine(JsonHelper.ToJson(tagInformations, true));
                    Console.WriteLine();

                }
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
