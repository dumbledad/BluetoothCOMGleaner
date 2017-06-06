using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;

namespace TorinoBluetooth
{
    public delegate void DeviceConnectionEventHandler(UwpRfcommDevice device);

    public class UwpRfcommConnector
    {
        public string[] DeviceNamePrefixes = new string[] { "Torino ", "WT32" };
        public event DeviceConnectionEventHandler DeviceConnected;
        public event DeviceConnectionEventHandler DeviceDisconnected;
        private Dictionary<string, UwpRfcommDevice> connectedDevices = new Dictionary<string, UwpRfcommDevice>();

        public UwpRfcommConnector(string[] replacementDeviceNamePrefixes = null)
        {
            if (replacementDeviceNamePrefixes != null) DeviceNamePrefixes = replacementDeviceNamePrefixes;
        }

        public void Initialize()
        {
            // Watch for nearby/paired devices
            var requestedProperties = new string[] { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected" };
            var deviceWatcher = DeviceInformation.CreateWatcher("(System.Devices.Aep.ProtocolId:=\"{e0cbf06c-cd8b-4647-bb8a-263b43f0f974}\")", requestedProperties, DeviceInformationKind.AssociationEndpoint); // ClassGuid = {e0cbf06c-cd8b-4647-bb8a-263b43f0f974} includes all Bluetooth devices
            deviceWatcher.Added += DeviceWatcher_Added;
            deviceWatcher.Updated += DeviceWatcher_Updated;
            deviceWatcher.Start();
        }

        private void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation deviceInformation)
        {
            var match = DeviceNamePrefixes.FirstOrDefault(prefix => deviceInformation.Name.StartsWith(prefix));
#pragma warning disable CS4014  // Fire and forget, it has an infinite loop
            if (match != null) Connect(deviceInformation);
#pragma warning restore CS4014
        }

        private void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate deviceInformationUpdate)
        {
            // Do not delete this empty event handler, its mere presence speeds things up!
        }

        private async Task Connect(DeviceInformation deviceInformation)
        {
            // Perform device access checks before trying to get the device.
            // First, we check if consent has been explicitly denied by the user.
            var accessStatus = DeviceAccessInformation.CreateFromId(deviceInformation.Id).CurrentStatus;
            if (accessStatus == DeviceAccessStatus.DeniedByUser)
            {
                throw new UnauthorizedAccessException("This app does not have access to connect to the remote device (please grant access in Settings > Privacy > Other Devices");
            }
            var bluetoothDevice = await BluetoothDevice.FromIdAsync(deviceInformation.Id);
            // This should return a list of uncached Bluetooth services (so if the server was not active when paired, it will still be detected by this call)
            var rfcommServices = await bluetoothDevice.GetRfcommServicesAsync();
            RfcommDeviceService bluetoothService = null;
            foreach (var service in rfcommServices.Services)
            {
                System.Diagnostics.Debug.WriteLine("Service {0}: {1}", service.ConnectionHostName, service.ConnectionServiceName);
                if (service.ServiceId.Uuid == RfcommServiceId.SerialPort.Uuid)
                {
                    bluetoothService = service;
                    break;
                }
            }
            if (bluetoothService != null)
            {
                bluetoothDevice.ConnectionStatusChanged += BluetoothDevice_ConnectionStatusChanged;
                var device = new UwpRfcommDevice(deviceInformation, bluetoothService);
                connectedDevices.Add(device.deviceInfo.Id, device);
                DeviceConnected?.Invoke(device);
            }
        }

        private void BluetoothDevice_ConnectionStatusChanged(BluetoothDevice sender, object args)
        {
            if (sender.ConnectionStatus == BluetoothConnectionStatus.Disconnected)
            {
                if (connectedDevices.ContainsKey(sender.DeviceId))
                {
                    var device = connectedDevices[sender.DeviceId];
                    connectedDevices.Remove(device.deviceInfo.Id);
                    DeviceDisconnected?.Invoke(device);
                }
            }
        }
    }
}
