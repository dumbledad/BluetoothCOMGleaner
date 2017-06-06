using System;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;

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
            if (serialDevices.ContainsKey(deviceInfo.Id))
            {
                return serialDevices[deviceInfo.Id].PortName;
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
