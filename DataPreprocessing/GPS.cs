using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LorryDataAnalysis
{
    public class GPS : ICloneable
    {
        public string ID { get; set; }
        public int index { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public DateTime Time { get; set; }
        public byte State { get; set; }
        public Int16 Speed { get; set; }
        public Int16 Direction { get; set; }
        public float KM { get; set; }
        public Int16 TEMP { get; set; }
        public Int16 Oil { get; set; }
        public int Stay { get; set; }
        
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        //时间转换
        public static double ConvertDateTimeInt(System.DateTime time)
        {
            double intResult = 0;
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));

            intResult = (time - startTime).TotalSeconds;
            return intResult;
        }
        public static DateTime ConvertIntDatetime(double utc)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));

            try
            {
                startTime = startTime.AddMilliseconds(utc);
                //string s = startTime.ToString("yyyy-MM-dd HH:mm");
                //startTime = startTime.AddHours(8);//转化为北京时间(北京时间=UTC时间+8小时 )
                return startTime;
            }
            catch
            {
                return startTime;
            }
        }
    }

    public class StayPoint : GPS
    {
        public long arvTime { get; set; }
        public long levTime { get; set; }
        public string stationName { get; set; }
    }

    //public class Station
    //{
    //    public string StationName { get; set; }
    //    public List<Vehicle> Vehicles { get; set; }
    //    public RectR Rect { get; set; }
    //}
}
