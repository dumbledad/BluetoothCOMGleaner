using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Networking.Connectivity;

namespace TorinoBluetooth
{
    public class UwpRfcommDevice
    {
        public DeviceInformation deviceInfo { get; internal set; }
        public RfcommDeviceService bluetoothService { get; internal set; }

        public UwpRfcommDevice(DeviceInformation deviceInformation, RfcommDeviceService bluetoothService)
        {
            this.deviceInfo = deviceInformation;
            this.bluetoothService = bluetoothService;
        }

        public string ComPort(UwpHidConnector hidConnector)
        {
            var serialDevices = hidConnector.SerialDevices;
            // Bluetooth DeviceInfo.Id: "Bluetooth#Bluetooth9c:b6:d0:d6:d7:56-00:07:80:cb:56:6d"
            // And from the Control Panel device properties:
            //     Association Endpoint Address: "00:07:80:cb:56:6d"
            //     Bluetooth Device Address: "000780CB566D"
            var lengthOfTrailingAssociationEndpointAddresss = (2 * 6) + 5;
            var bluetoothDeviceAddress = deviceInfo.Id.Substring(deviceInfo.Id.Length - lengthOfTrailingAssociationEndpointAddresss, lengthOfTrailingAssociationEndpointAddresss).Replace(":", "").ToUpper();
            var matchingKey = serialDevices.Keys.FirstOrDefault(id => id.Contains(bluetoothDeviceAddress));
            if (matchingKey != null)
            {
                return serialDevices[matchingKey].PortName;
            }
            return "";
        }

        public async Task<string> ComPort(DeviceInformation deviceInfo)
        {
            var serialDevices = new Dictionary<string, SerialDevice>();
            var serialSelector = SerialDevice.GetDeviceSelector();
            var serialDeviceInformations = (await DeviceInformation.FindAllAsync(serialSelector)).ToList();
            var hostNames = NetworkInformation.GetHostNames().Select(hostName => hostName.DisplayName.ToUpper()).ToList(); // So we can ignore inbuilt ports
            foreach (var serialDeviceInformation in serialDeviceInformations)
            {
                if (hostNames.FirstOrDefault(hostName => hostName.StartsWith(serialDeviceInformation.Name.ToUpper())) == null)
                {
                    try
                    {
                        var serialDevice = await SerialDevice.FromIdAsync(serialDeviceInformation.Id);
                        if (serialDevice != null)
                        {
                            serialDevices.Add(deviceInfo.Id, serialDevice);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                    }
                }
            }
            // Example Bluetooth DeviceInfo.Id: "Bluetooth#Bluetooth9c:b6:d0:d6:d7:56-00:07:80:cb:56:6d"
            // from device with Association Endpoint Address: "00:07:80:cb:56:6d"
            var lengthOfTrailingAssociationEndpointAddresss = (2 * 6) + 5;
            var bluetoothDeviceAddress = deviceInfo.Id.Substring(deviceInfo.Id.Length - lengthOfTrailingAssociationEndpointAddresss, lengthOfTrailingAssociationEndpointAddresss).Replace(":", "").ToUpper();
            var matchingKey = serialDevices.Keys.FirstOrDefault(id => id.Contains(bluetoothDeviceAddress));
            if (matchingKey != null)
            {
                return serialDevices[matchingKey].PortName;
            }
            return "";
        }

        public async Task<string> ComPort()
        {
            try
            {
                var serialDevice = await SerialDevice.FromIdAsync(deviceInfo.Id);
                if (serialDevice != null)
                {
                    return serialDevice.PortName;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
            return "";
        }
    }
}
