# GoToTags .NET SDK - Examples
Code Examples for the GoToTags .NET NFC, Barcode and I/O SDK

https://gototags.com/products/net-sdk/

## Example Code
Example projects with source code along with sample output using the SDK are within this GitHub repository.  

### License
A license is required to run the examples; the **code in this repositry will not run without a license**. 

To request a free trial license, submit a [request for trial license](https://gototags.com/products/net-sdk/).

## Enhancements & Bugs
Requested enhancements and potential bugs should be submited as [GitHub issues](https://github.com/GoToTags/GoToTags-NET/issues) in this repository.

## Technical Support
Technical support is provided to customers with an active license. Please [contact us](https://gototags.com/contact/) and provide your license id along with your technical questions.

## Currently Supported NFC Readers and NFC Chips

### Readers
- ACS ACR122
- ACS ACR1222L
- ACS ACR1251
- ACS ACR1252
- ACS ACR1255

### Chips
- MIFARE Ultralight
- MIFARE Ultralight C
- MIFARE Ultralight EV1
- NTAG 203
- NTAG 210
- NTAG 210 Micro
- NTAG 212
- NTAG 213
- NTAG 215
- NTAG 216
- Kovio 2K

## Example Snippets

Unlock the SDK with a provided license. GoToTags will provide you with the license.
```
LicenseManager.Instance.Unlock(<insert license as string>);
```

Finding readers:
```
void FindReaders()
{
    DeviceManager.Instance.DeviceAdded += DeviceAdded;
    DeviceManager.Instance.StartListenDevices();
}

void DeviceAdded(DeviceManager manager, Device device)
{
    ...
}
```

Finding tags:
```
void DeviceAdded(DeviceManager manager, Device device)
{
    device.TagFound += Device_TagFound;
    device.StartListenTags();
}

void TagFound(Device device, TagInformation tagInformation)
{
    ...
}
```

Reading NDEF messages from a tag:
```
void TagFound(Device device, TagInformation tagInformation)
{
   using (var ndef = NdefTechnology.Connect(device, tagInformation))
   {
       NdefMessage ndefMessage = ndef.GetNdefMessage();
   }
}
```

Writing an NDEF Text Record to a tag:
```
void TagFound(Device device, TagInformation tagInformation)
{
   using (var ndef = NdefTechnology.Connect(device, tagInformation))
   {
       NdefTextRecord ndefTextRecord = new NdefTextRecord("This is a text record", "en", NdefTextRecord.TextEncoding.UTF8);
       NdefMessage ndefMessage = new NdefMessage(ndefTextRecord);

       ndef.WriteNdefMessage(ndefMessage);
   }
}
```

Writing an NDEF URI record to a tag:
```
void TagFound(Device device, TagInformation tagInformation)
{
    using (var ndef = NdefTechnology.Connect(device, tagInformation))
    {
        NdefUriRecord ndefUriRecord = new NdefUriRecord(new Uri("http://www.gototags.com"));
        NdefMessage ndefMessage = new NdefMessage(ndefUriRecord);
        ndef.WriteNdefMessage(ndefMessage);
    }
}
```

Erasing, formatting, and locking a tag:
```
void TagFound(Device device, TagInformation tagInformation)
{
    using (var ndef = NdefTechnology.Connect(device, tagInformation))
    {
        ndef.Erase();
        ndef.Format();
        ndef.Lock();
    }
}
```
