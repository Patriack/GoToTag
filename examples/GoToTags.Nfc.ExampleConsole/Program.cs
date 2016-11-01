using GoToTags.Common;
using GoToTags.Common.Json;
using GoToTags.Common.Licensing;
using GoToTags.Nfc;
using GoToTags.Nfc.Devices;
using GoToTags.Nfc.Ndef;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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
                LicenseManager.Instance.Unlock("XXXXXXXXXXXXXXXXXXXXXXXXXXXX");

                // what version are we using?                
                Console.WriteLine(AssemblyHelper.GetFullName(typeof(NfcHelper).Assembly));
                Console.WriteLine();

                // get infomation about the license
                License license = LicenseManager.Instance.License;
                Console.WriteLine("LICENSE");
                Console.WriteLine(license.ToJson(true) + Environment.NewLine);

                // get the current devices
                IEnumerable<Device> devices = DeviceManager.Instance.GetDevices();

                // show the devices as json
                Console.WriteLine("DEVICES");
                Console.WriteLine(JsonHelper.ToJson(devices, true) + Environment.NewLine);

                // get first device
                    
                // can use LINQ to get device(s) based on device properties and type; ex: devices.Where(d => d.DeviceType == DeviceType.Acr122).FirstOrDefault as ACR122U();
                Device device = devices.FirstOrDefault();

                if (device != null)
                {
                    Console.WriteLine($"DEVICE: {device.Name}");
                    Console.WriteLine();

                    // set device specific properties like the tag buzzer
                    if (device.HasTagBuzzer)
                        device.EnableTagBuzzer(true);

                    // is the device currenlty connected to any tags?
                    TagInformation[] tagInfos = device.GetTags();

                    // multiple NFC tags can be in the RF field of the device at any one time
                    // however PCSC based devices typically force there to onle be one
                    foreach (TagInformation tagInfo in tagInfos)
                    {
                        HandleNfcTagFound(device, tagInfo);
                    }

                    // start listeneing for tag that show up in the device's RF field
                    device.TagFound += Device_TagFound;
                    device.StartListenTags();

                    // listen for a minute
                    Thread.Sleep(TimeSpan.FromMinutes(1));

                    // stop listening
                    device.StopListenTags();
                    device.TagFound -= Device_TagFound;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(Environment.NewLine + ex.ToString() + Environment.NewLine);
            }

            Console.WriteLine("DONE: Press 'Enter' to quit." + Environment.NewLine);
            Console.ReadLine();
        }

        private static void Device_TagFound(Device device, TagInformation tagInfo)
        {
            HandleNfcTagFound(device, tagInfo);
        }

        private static void HandleNfcTagFound(Device device, TagInformation tagInfo)
        {
            Console.WriteLine($"TAG FOUND; DEVICE: {device.Name}");
            Console.WriteLine(JsonHelper.ToJson(tagInfo, true) + Environment.NewLine);

            // read the nfc tag
            ReadNdef(device, tagInfo);

            // write the nfc tag
            WriteNdef(device, tagInfo);

            // read the nfc tag again
            ReadNdef(device, tagInfo);
        }

        private static void ReadNdef(Device device, TagInformation tagInfo)
        {
            Console.WriteLine("READING NDEF" + Environment.NewLine);

            // connect via NDEF
            using (var ndef = NdefTechnology.Connect(device, tagInfo))
            {
                Console.WriteLine("NDEF TECHNOLOGY");
                Console.WriteLine(ndef.ToJson(true) + Environment.NewLine);

                // get properties from NDEF
                bool canFormat = ndef.CanFormat;

                // now we can get the full nfc tag
                var nfcTag = ndef.Tag;

                Console.WriteLine("TAG");
                Console.WriteLine(ndef.Tag.ToJson(true) + Environment.NewLine);        

                // get properties from nfc tag

                // uid
                byte[] uid = nfcTag.Uid;
                var uidString = uid.ToHexString(false);

                // nfc chip type
                ChipType chipType = nfcTag.ChipType;

                // read the ndef message
                NdefMessage ndefMessage = ndef.GetNdefMessage();

                Console.WriteLine("NDEF MESSAGE");
                Console.WriteLine(ndefMessage.ToJson(true) + Environment.NewLine);
            }
        }

        private static void WriteNdef(Device device, TagInformation tagInfo)
        {
            Console.WriteLine("WRITING NDEF" + Environment.NewLine);

            // connect via ndef
            using (var ndef = NdefTechnology.Connect(device, tagInfo))
            {
                var nfcTag = ndef.Tag;

                // see if the nfc tag is not locked so we can write to it
                if (!nfcTag.Locked)
                {
                    // format the nfc tag as ndef
                    // note that writing an NdefMessage will do this for you
                    if (ndef.CanFormat && !nfcTag.Formatted)
                    {
                        ndef.Format();
                    }

                    // create some ndef records and a Ndef message
                    NdefUriRecord ndefUriRecord = new NdefUriRecord(new Uri("https://gototags.com"));
                    NdefTextRecord ndefTextRecord = new NdefTextRecord($"Now: {DateTime.Now.ToString()}", "en-us", NdefTextRecord.TextEncoding.UTF8);

                    // create a ndef message
                    NdefMessage ndefMessage = new NdefMessage(new NdefRecord[] { ndefUriRecord });

                    // encode the ndef message to the nfc tag
                    ndef.WriteNdefMessage(ndefMessage);
                }
            }
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs args)
        {
            Console.WriteLine("CANCELLING" + Environment.NewLine);

            Environment.Exit(-1);
        }
    }
}
