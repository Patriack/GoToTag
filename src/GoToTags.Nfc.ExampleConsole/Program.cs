using GoToTags.Common;
using GoToTags.Common.Console;
using GoToTags.Common.Licensing;
using GoToTags.Common.Serialization;
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
                LicenseManager.Instance.Unlock("XXXXXXXXXX");

                // what version are we using?                
                ConsoleHelper.WriteLine(AssemblyHelper.GetFullName(typeof(NfcHelper).Assembly), false, ConsoleColor.Green);
                ConsoleHelper.WriteLine($"RUN AT {DateTime.Now.ToString()}", true);

                // get infomation about the license
                License license = LicenseManager.Instance.License;
                ConsoleHelper.WriteLine("LICENSE");
                ConsoleHelper.WriteLine(license.ToJson(true), true);
                
                // stop the Windows Certificate Propagation Service
                // https://gototags.com/nfc/windows-certificate-propagation-service/
                NfcHelper.StopCertPropSvc();

                // get the current devices
                IEnumerable<Device> devices = DeviceManager.Instance.GetDevices();

                // show the devices as json
                ConsoleHelper.WriteLine("DEVICES");
                ConsoleHelper.WriteLine(JsonHelper.ToJson(devices, true), true);

                // get first device

                // can use LINQ to get device(s) based on device properties and type; ex: devices.Where(d => d.Manufacturer == Manufacturer.Acs).FirstOrDefault()
                Device device = devices.FirstOrDefault();

                if (device != null)
                {
                    // can also cast a Device to a subclass to get access to device specific functions and properties
                    if (device.DeviceType == DeviceType.Acr122)
                    {
                        Console.WriteLine("DEVICE IS ACR122" + Environment.NewLine);

                        ACR122 acr122 = device as ACR122;
                        ACR122.PiccOperatingParameters piccOperatingParameter = acr122.PiccOperatingParameter;
                    }
                    else if (device.DeviceType == DeviceType.Acr1252)
                    {
                        Console.WriteLine("DEVICE IS ACR1252" + Environment.NewLine);

                        ACR1252 acr1252 = device as ACR1252;
                        ACR1252.LedBuzzerBehaviors ledBuzzerBehavior = acr1252.LedBuzzerBehavior;
                    }

                    ConsoleHelper.WriteLine($"USING DEVICE: {device.Name}", true);

                    // set device specific properties like the tag buzzer
                    if (device.HasTagBuzzer)
                        device.EnableTagBuzzer(true);

                    // is the device currenlty connected to any tags?
                    TagInformation[] tagInfos = device.GetTags();

                    // multiple NFC tags can be in the RF field of the device at any one time
                    // however PCSC based devices typically force there to only be one
                    if (tagInfos.Length > 0)
                    {
                        ConsoleHelper.WriteLine("TAGS IN RF FIELD", true);

                        foreach (TagInformation tagInfo in tagInfos)
                        {
                            NfcTagFound(device, tagInfo);
                        }
                    }
                    else
                    {
                        ConsoleHelper.WriteLine("NO TAGS IN RF FIELD", true);
                    }

                    // start listeneing for tag that show up in the device's RF field
                    ConsoleHelper.WriteLine("LISTENING FOR TAGS", true);

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
                ConsoleHelper.WriteException(ex);
            }

            ConsoleHelper.WriteLine("DONE: Press 'Enter' to quit.", true);
            Console.ReadLine();
        }

        private static void Device_TagFound(Device device, TagInformation tagInfo)
        {
            // note that this method is not called on the main thread, but from a background thread

            try
            {
                NfcTagFound(device, tagInfo);
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteException(ex);
            }
        }

        private static void NfcTagFound(Device device, TagInformation tagInfo)
        {
            ConsoleHelper.WriteLine($"TAG FOUND; DEVICE: {device.Name}");
            ConsoleHelper.WriteLine(JsonHelper.ToJson(tagInfo, true), true);

            // read the nfc tag
            ReadNdef(device, tagInfo);

            // write the nfc tag
            WriteNdef(device, tagInfo);

            // read the nfc tag again
            ReadNdef(device, tagInfo);
        }

        private static void ReadNdef(Device device, TagInformation tagInfo)
        {
            ConsoleHelper.WriteLine("READING NDEF");

            // connect via NDEF
            using (var ndef = NdefTechnology.Connect(device, tagInfo))
            {
                ConsoleHelper.WriteLine("NDEF TECHNOLOGY");
                ConsoleHelper.WriteLine(ndef.ToJson(true), true);

                // get properties from NDEF
                bool canFormat = ndef.CanFormat;

                // now we can get the full nfc tag with all of its properties
                var nfcTag = ndef.Tag;

                ConsoleHelper.WriteLine("TAG");
                ConsoleHelper.WriteLine(ndef.Tag.ToJson(true), true);        

                // get properties from nfc tag

                // uid
                byte[] uid = nfcTag.Uid; // uid as byte[]
                string uidString = uid.ToHexString(false); // uid as string

                // nfc chip type
                ChipType chipType = nfcTag.ChipType;

                // read the ndef message
                NdefMessage ndefMessage = ndef.GetNdefMessage();

                ConsoleHelper.WriteLine("NDEF MESSAGE");
                ConsoleHelper.WriteLine(ndefMessage.ToJson(true), true);
            }
        }

        private static void WriteNdef(Device device, TagInformation tagInfo)
        {
            ConsoleHelper.WriteLine("WRITING NDEF", true);

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

                    // lock the tag
                    if (nfcTag.CanLock && !nfcTag.Locked)
                    {
                        // locking is a pemament operation!
                        // ndef.Lock();
                    }
                }
            }
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs args)
        {
            ConsoleHelper.WriteLine("CANCELLING", true);

            Environment.Exit(-1);
        }
    }
}
