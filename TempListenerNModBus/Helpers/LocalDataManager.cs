using QBlockData;
using QBlockData.DataStructs;
using System;
using System.Collections.Generic;
using System.Text;

namespace TempListenerNModBus.Helpers
{

    public class LocalLogData
    {
        public double Temp { get; set; }
        public double Humid { get; set; }
        public double Sun { get; set; }

        public bool IsHeating { get; set; }
        public bool IsHumid { get; set; }
        public bool IsLighting { get; set; }
        public DateTime Date { get; set; }

    }
    public static class LocalDataManager
    {
        private static QBlockData.BaseTypeFileBlockDataShell LogData = new QBlockData.BaseTypeFileBlockDataShell(new XmlFileBlockData("./Datas/LocalLogData.qbm"));
        private const int MaxLogCount = 10000;
        public static void WriteLog(LocalLogData log)
        {
            string key = CreateKey(log);
            if (LogData.GetControllerTarget().HasKey(key))
            {
                LogData.Delete(key);
            }
            LogData.AddOrUpdateTlvDatas(key, new List<TLVData>()
            {
                new TLVData( TLVDataTags.Double,log.Temp),
                new TLVData( TLVDataTags.Double,log.Humid),
                new TLVData( TLVDataTags.Double,log.Sun),
                new TLVData( TLVDataTags.Boolean,log.IsHeating),
                new TLVData( TLVDataTags.Boolean,log.IsHumid),
                new TLVData( TLVDataTags.Boolean,log.IsLighting),
                new TLVData( TLVDataTags.DateTime,log.Date),
            });
            var ks = LogData.GetControllerTarget().GetKeys();
            if (ks.Count() > MaxLogCount)
            {
                LogData.Delete(ks.First());
            }
        }
        public static IEnumerable<LocalLogData> GetLogs(int? LastCount = null)
        {
            List<LocalLogData> datas = new();
            IEnumerable<string> ks = LogData.GetControllerTarget();
            if (LastCount != null) ks = ks.TakeLast(LastCount.Value);

            foreach (var i in ks)
            {
                //var data = LogData.QueryClass<LocalLogData>(i);
                //datas.Add(data);
                //LogData.GetControllerTarget().add
                var data = LogData.QueryTlvDatasQueue(i);
                datas.Add(new LocalLogData()
                {
                    Temp = data.Dequeue().ReadToDouble(),
                    Humid = data.Dequeue().ReadToDouble(),
                    Sun = data.Dequeue().ReadToDouble(),
                    IsHeating = data.Dequeue().ReadToBoolean(),
                    IsHumid = data.Dequeue().ReadToBoolean(),
                    IsLighting = data.Dequeue().ReadToBoolean(),
                    Date = data.Dequeue().ReadToDateTime(),
                });
            }
            return datas;
        }


        private static string CreateKey(LocalLogData data)
        {
            return data.Date.ToString();
            //return data.Date.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
