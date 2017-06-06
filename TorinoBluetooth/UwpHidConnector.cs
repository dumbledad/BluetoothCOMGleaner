using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Networking.Connectivity;

namespace TorinoBluetooth
{
    public class UwpHidConnector
    {
        public event DeviceConnectionEventHandler DeviceConnected;
        public event DeviceConnectionEventHandler DeviceDisconnected;

        private Dictionary<string, SerialDevice> _SerialDevices = new Dictionary<string, SerialDevice>();
        public ReadOnlyDictionary<string, SerialDevice> SerialDevices
        {
            get
            {
                return new ReadOnlyDictionary<string, SerialDevice>(_SerialDevices);
            }
        }

        public async Task Initialize()
        {
            var serialSelector = SerialDevice.GetDeviceSelector();
            var serialDevices = (await DeviceInformation.FindAllAsync(serialSelector)).ToList();
            var hostNames = NetworkInformation.GetHostNames().Select(hostName => hostName.DisplayName.ToUpper()).ToList(); // So we can ignore inbuilt ports
            foreach (var deviceInfo in serialDevices)
            {
                if (hostNames.FirstOrDefault(hostName => hostName.StartsWith(deviceInfo.Name.ToUpper())) == null)
                {
                    try
                    {
                        var serialDevice = await SerialDevice.FromIdAsync(deviceInfo.Id);
                        if (serialDevice != null)
                        {
                            _SerialDevices.Add(deviceInfo.Id, serialDevice);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                    }
                }
            }
        }
    }
}
