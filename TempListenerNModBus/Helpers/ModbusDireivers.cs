using NModbus;
using NModbus.Device;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TempListenerNModBus.Helpers
{
    public static class ModbusDireivers
    {
        public class TcpMaster
        {
            private IPEndPoint _IP { get; set; }
            private TcpClient _TCP { get; set; }
            private IModbusMaster _Master { get; set; }
            public string MasterName { get; set; } = "未命名";
            public TcpMaster(IPEndPoint TargetIP, ModbusFactory Factory)
            {
                this._IP = TargetIP;
                TryReLink(Factory);
            }

            public IModbusMaster GetModbusMaster() => this._Master;
            public bool IsConnected => this._TCP.Connected;
            public void TryReLink(ModbusFactory Factory)
            {
                this._TCP = new TcpClient(_IP.Address.ToString(), _IP.Port);
                var mt = Factory.CreateMaster(_TCP);
                this._Master = mt;
            }
        }
        public class MasteController
        {
            protected TcpMaster Master = null;
            public MasteController(TcpMaster Master)
            {
                this.Master = Master;
            }
        }
        public class TempController : MasteController
        {
            public TempController(TcpMaster Master) : base(Master)
            {
            }

            public double GetTemp()
            {
                var data = Master.GetModbusMaster().ReadHoldingRegisters(1, 0, 1)[0];
                return data * 1.0 / 10;
            }
            public void SetTemp(double Temp)
            {
                var t = (ushort)(Temp * 10);
                Master.GetModbusMaster().WriteSingleRegister(0, 0, t);
            }
            public void SetHumidity(double Temp)
            {
                var t = (ushort)(Temp * 10);
                Master.GetModbusMaster().WriteSingleRegister(0, 1, t);
            }
            public double GetHumidity()
            {
                return Master.GetModbusMaster().ReadHoldingRegisters(0, 1, 1)[0] * 1.0 / 10;
            }
            public void GetAllState(out double Temp, out double Humidity)
            {
                var data = Master.GetModbusMaster().ReadHoldingRegisters(1, 0, 2);
                Temp = data[0] * 1.0 / 10;
                Humidity = data[1] * 1.0 / 10;
            }
        }
        public class ControllerController : MasteController
        {
            public ControllerController(TcpMaster Master) : base(Master)
            {
            }

            public void SetHeatingState(bool State)
            {
                Master.GetModbusMaster().WriteSingleCoil(1, 0, State);
            }

            public bool GetHeatingState()
            {
                var b = Master.GetModbusMaster().ReadCoils(1, 0, 1)[0];
                return b;
            }


            public void SetHumidState(bool State)
            {
                Master.GetModbusMaster().WriteSingleCoil(1, 1, State);
            }

            public bool GetHumidState()
            {
                var b = Master.GetModbusMaster().ReadCoils(1, 1, 1)[0];
                return b;
            }

            public void SetLightState(bool State)
            {
                Master.GetModbusMaster().WriteSingleCoil(1, 2, State);
            }

            public bool GetLightState()
            {
                var b = Master.GetModbusMaster().ReadCoils(1, 2, 1)[0];
                return b;
            }

            public void GetAllState(out bool IsHeating, out bool IsHumiding, out bool IsLighting)
            {
                var b = Master.GetModbusMaster().ReadCoils(1, 0, 3);
                IsHeating = b[0];
                IsHumiding = b[1];
                IsLighting = b[2];
            }

        }

        public class SunTempController : MasteController
        {
            public SunTempController(TcpMaster Master) : base(Master)
            {
            }

            public ushort GetSunLightVal()
            {
                var d = Master.GetModbusMaster().ReadInputRegisters(1, 0, 1)[0];
                return d;
            }
        }

        private static Dictionary<string, MasteController> Masters = new();
        private static bool IsInited = false;
        static ModbusDireivers()
        {
        }
        async public static Task InitMaster()
        {
            Task.Run(() =>
            {
                ModbusFactory mf = new();
                Masters.Add("TempMaster", new TempController(new(IPEndPoint.Parse("127.0.0.1:502"), mf) { MasterName = "温度传感器" }));
                Masters.Add("ControllerMaster", new ControllerController(new(IPEndPoint.Parse("127.0.0.1:503"), mf) { MasterName = "控制设备" }));
                Masters.Add("SunTempMaster", new SunTempController(new(IPEndPoint.Parse("127.0.0.1:504"), mf) { MasterName = "光照传感器" }));
            }).Wait();

        }

        public static TempController TempMaster => (TempController)Masters["TempMaster"];
        public static ControllerController ControllerMaster => (ControllerController)Masters["ControllerMaster"];
        public static SunTempController SunTempMaster => (SunTempController)Masters["SunTempMaster"];


    }
}
