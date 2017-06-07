using Microsoft.VisualStudio.TestTools.UnitTesting;
using TorinoBluetooth;
using System.Threading;
using System.Threading.Tasks;

namespace TorinoComGleanerUnitTestProject
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public async Task TestComPortReturn()
        {
            var resetEvent = new AutoResetEvent(false);
            var comPort = "";
            var hidConnector = new UwpHidConnector();
            await hidConnector.Initialize();
            var connector = new UwpRfcommConnector();
            //connector.DeviceConnected += async (UwpRfcommDevice device) =>
            connector.DeviceConnected += (UwpRfcommDevice device) =>
            {
                if (comPort == "")
                {
                    //comPort = await device.ComPort();
                    comPort = device.ComPort(hidConnector);
                    resetEvent.Set();
                }
            };
            connector.Initialize();
            var wasSignaled = resetEvent.WaitOne(); // timeout: TimeSpan.FromSeconds(1));

            Assert.IsTrue(comPort == "COM8");
        }
    }
}
