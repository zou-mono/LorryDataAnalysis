using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using log4net;
using NETGeographicLib;

namespace LorryDataAnalysis
{
    public class DataConvert
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static string _CurrentDirectory = Environment.CurrentDirectory; //程序当前运行的路径
        private static string _dataDirectory = null; //输入数据目录
        private static string _lpFile = null; //车牌文件名称
        private static string _outDirectory = null; //输出数据目录
        private static bool _bFilter = false; //是否提取stay points
        private static int _maxThread = 0;
        private HashSet<string> _lpDics = null;
        private static readonly int disError = 5000;
        private static readonly int speedError = 120; //前后两点限速120公里/小时
        private static readonly int disThreh = 200;
        private static readonly int timeThreh = 600;

        public DataConvert()
        {

        }

        public bool init(ConvertSubOptions opts)
        {
            try
            {
                _dataDirectory = Path.IsPathRooted(opts.inDirectoryName) ? opts.inDirectoryName : _CurrentDirectory + @"\" + opts.inDirectoryName;

                if (Directory.Exists(_dataDirectory) == false) throw new Exception("Error: input directory is not exist!");

                if (!String.IsNullOrEmpty(opts.lpFileName))
                {
                    _lpFile = Path.IsPathRooted(opts.lpFileName) ? opts.lpFileName : _CurrentDirectory + @"\" + opts.lpFileName;
                }

                if (!String.IsNullOrEmpty(opts.outDirectoryName))
                {
                    _outDirectory = Path.IsPathRooted(opts.outDirectoryName) ? opts.outDirectoryName : _CurrentDirectory + @"\"+ opts.outDirectoryName;
                }
                else
                {
                    _outDirectory = _CurrentDirectory + @"\gps_result\";
                }

                _bFilter = opts.filter;
                _maxThread = opts.maxThread;

                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return false;
            }
        }

        private bool LoadlpFile()
        {
            //得到车牌信息
            StreamReader reader = new StreamReader(_lpFile, Encoding.GetEncoding("GB2312"));
            _lpDics = new HashSet<string>();
            //ArrayList temp = new ArrayList();
            string line;

            logger.Info("Loading license plate file...");
            try
            {
                while ((line = reader.ReadLine()) != null)
                {
                    string[] a = line.Split(new char[] { ',' });

                    _lpDics.Add(a[0]);
                }

                logger.Info("Load license plate file success！");
                return true;
            }
            catch
            {
                logger.Error("Load license plate file failed！");
                return false;
            }
        }

        private List<GPS> ConvertBinary(string file)
        {
            List<GPS> GpsArray = new List<GPS>();
            try
            {
                System.IO.FileStream FS = new System.IO.FileStream(file, FileMode.Open, FileAccess.Read);
                BinaryReader BR = new BinaryReader(FS);
                Byte[] r = BR.ReadBytes(Convert.ToInt32(FS.Length));

                FileInfo fi = new FileInfo(file);
                string id = fi.Name.Substring(0, fi.Name.Length - 4);

                for (int i = 0; i < FS.Length / 39; i++)
                {
                    Byte[] longitude = new Byte[4];
                    Byte[] latitude = new Byte[4];
                    Byte[] GPStime = new Byte[8];
                    Byte[] state = new Byte[1];
                    Byte[] speed = new Byte[2];
                    Byte[] direction = new Byte[2];
                    Byte[] km = new Byte[4];
                    Byte[] temp = new Byte[2];
                    Byte[] oil = new Byte[2];

                    for (int j = 0; j < 4; j++)
                    {
                        longitude[j] = r[39 * i + 3 - j];
                    }

                    for (int j = 0; j < 4; j++)
                    {
                        latitude[j] = r[39 * i + 7 - j];
                    }

                    for (int j = 0; j < 8; j++)
                    {
                        GPStime[j] = r[39 * i + 15 - j];
                        //GPStime[j] = r[39 * i +  j];
                    }

                    state[0] = r[39 * i + 16];

                    //char[] ch = Encoding.UTF8.GetChars(state);

                    for (int j = 0; j < 2; j++)
                    {
                        speed[j] = r[39 * i + 18 - j];
                    }

                    for (int j = 0; j < 2; j++)
                    {
                        direction[j] = r[39 * i + 20 - j];
                    }

                    for (int j = 0; j < 4; j++)
                    {
                        km[j] = r[39 * i + 24 - j];
                    }

                    for (int j = 0; j < 2; j++)
                    {
                        temp[j] = r[39 * i + 26 - j];
                    }

                    for (int j = 0; j < 2; j++)
                    {
                        oil[j] = r[39 * i + 38 - j];
                    }

                    float a = BitConverter.ToSingle(longitude, 0);
                    float b = BitConverter.ToSingle(latitude, 0);
                    long c = BitConverter.ToInt64(GPStime, 0);
                    DateTime dt = GPS.ConvertIntDatetime(c);

                    string dts = dt.ToString("yyyy-MM-dd HH:mm:ss");
                    //int hour = System.Int32.Parse(dt.ToString("HH"));
                    //int min = System.Int32.Parse(dt.ToString("mm"));
                    //int sec = System.Int32.Parse(dt.ToString("ss"));

                    //long second = hour * 3600 + min * 60 + sec;

                    Int16 f = BitConverter.ToInt16(speed, 0);
                    Int16 h = (Int16)(BitConverter.ToInt16(direction, 0) * 45);

                    float k = BitConverter.ToSingle(km, 0);
                    Int16 t = BitConverter.ToInt16(temp, 0);
                    Int16 o = BitConverter.ToInt16(oil, 0);

                    //float k = BitConverter.ToSingle(km, 0);
                    GPS gpsData = new GPS()
                    {
                        ID = id,
                        Longitude = a,
                        Latitude = b,
                        Time = dt,
                        Speed = f,
                        State = state[0],
                        Direction = h,
                        KM = k,
                        TEMP = t,
                        Oil = o
                    };

                    if (dt.Year == 2016 && gpsData.Longitude <= 180 && gpsData.Longitude >= -180 && gpsData.Latitude <= 90 && gpsData.Latitude >= -90)
                    {
                        GpsArray.Add(gpsData);
                    }
                }

                GpsArray = GpsArray.OrderBy(a => a.Time).ToList();   //按照GPS时间排序

                FS.Close();
                BR.Close();
                return GpsArray;
            }
            catch (Exception ex)
            {
                logger.Error("Error: File format incorrect! ");
                return null;
            }
        }

        private void ExportToFile(string file, bool bFilter)
        {
            List<GPS> GPSArray = ConvertBinary(file);
            StreamWriter sw;

            if (GPSArray == null) return;
            if (GPSArray.Count == 0) return;

            FileInfo fi = new FileInfo(file);

            if (!Directory.Exists(_outDirectory))
            {
                Directory.CreateDirectory(_outDirectory);
            }

            string out_file = Path.GetFileNameWithoutExtension(_outDirectory + "\\" + fi.Name) + ".csv";
           
            if (bFilter == true) GPSArray = CalculateStayPoint(GPSArray);
            
            if (GPSArray.Count > 0)
            {
                sw = new StreamWriter(_outDirectory + "\\" + out_file, false, Encoding.GetEncoding("UTF-8"));
                WriteToFile(GPSArray, fi, sw);
                sw.Close();
            }
        }

        private void WriteToFile(List<GPS> GPSArray, FileInfo fi, StreamWriter sw)
        {
            GPS pPrePt = null; bool bflag = false;
            foreach (GPS n in GPSArray)
            {
                bflag = false;
                if (pPrePt == null) bflag = true;
                else if (n.Time != pPrePt.Time) bflag = true;

                if (bflag)
                {
                    //string s = fi.Name.Substring(0, fi.Name.Length - 4) + "," + Convert.ToString(n.Longitude) + "," + Convert.ToString(n.Latitude) + "," +
                    //           Convert.ToString(n.Time) + "," + Convert.ToString(n.State) + "," + Convert.ToString(n.Speed) + "," + Convert.ToString(n.Direction) + "," +
                    //           Convert.ToString(n.KM) + "," + Convert.ToString(n.TEMP) + "," + Convert.ToString(n.Oil);
                    string s =Convert.ToString(n.Longitude) + "," + Convert.ToString(n.Latitude) + "," +
                                   Convert.ToString(n.Time) + "," + Convert.ToString(n.Speed) + "," + Convert.ToString(n.Direction) + "," + Convert.ToString(n.Stay);

                    sw.WriteLine(s);
                    sw.Flush();
                }
                pPrePt = n;
            }
        }

        public bool Step()
        {
            bool bFlag = true;

            string[] files = new GetLogFiles().readlist(_dataDirectory);
            int FileCount = files.Length; int iCount = 0;
            List<Task> tsArr = new List<Task>();
            FileInfo fi = null;

            using (var progress = new ProgressBar())
            {
                try
                {
                    if (_lpFile != null) LoadlpFile();

                    logger.Info("Conversion Processing...");

                    LimitedConcurrencyLevelTaskScheduler lcts = new LimitedConcurrencyLevelTaskScheduler(_maxThread);
                    TaskFactory factory = new TaskFactory(lcts);

                    foreach (string file in files)
                    {
                        fi = new FileInfo(file);
                        bFlag = true;

                        string filename = fi.Name.Substring(0, fi.Name.Length - 4);
                        if (filename.Length >= 7) { filename = filename.Substring(0, 7); }
                        if (_lpFile != null)
                        {
                            if (_lpDics.Contains(filename) == false) bFlag = false;
                        }

                        if (bFlag)
                        {
                            //ExportToFile(file, _bFilter);
                            Task ts = factory.StartNew(() =>
                            {
                                try
                                {
                                    ExportToFile(file, _bFilter);
                                    Interlocked.Increment(ref iCount);
                                    progress.Report((double)iCount / FileCount);
                                }
                                catch (Exception ex)
                                {
                                    logger.Error("Error: Failed in " + file + ". " + ex.Message);
                                }
                            });

                            tsArr.Add(ts);
                        }
                        else { Interlocked.Increment(ref iCount); progress.Report((double)iCount / FileCount); }
                    }

                    Task AllTasks = Task.WhenAll(tsArr.ToArray());
                    return AllTasks.ContinueWith<bool>((a) =>//当所有task完成后，执行这个回调
                    {
                        return true;
                    }).Result;
                    //return true;
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                    return false;
                }
            }
            //return false;
        }

        //[STAThread()]
        private List<GPS> CalculateStayPoint(List<GPS> GPSDataCol)
        {
            int i = 0;
            int pointNum = GPSDataCol.Count;
            List<GPS> vSP = new List<GPS>();
            GPS pCurrentPt = null; GPS pNextPt = null;
            int ptrStart = 0; int ptrEnd = 0; double interval = 0.0; string stationName = "";

            if (pointNum < 2) return GPSDataCol;

            //先剔除位置错误的点
            for (int j = 0; j < GPSDataCol.Count; j++)
            {
                if(j < GPSDataCol.Count-1)
                {
                    pCurrentPt = GPSDataCol[j];
                    pNextPt = GPSDataCol[j + 1];

                    //if (pCurrentPt.Time.ToString() == "3/7/2016 12:03:14 AM")
                    //{
                    //    Console.WriteLine("test");
                    //}

                    double distance = 0.0;
                    using (Geodesic geod = new Geodesic())
                    {
                        geod.Inverse(pCurrentPt.Latitude, pCurrentPt.Longitude, pNextPt.Latitude, pNextPt.Longitude, out distance);
                    }
                    double deltaTime = (pNextPt.Time - pCurrentPt.Time).TotalHours;

                    //速度不可能达到的极限，说明存在错误点
                    if (distance/(1000*deltaTime) > speedError) 
                    {
                        GPSDataCol.RemoveAt(j + 1);
                        j = j - 1;
                    }
                }
            }

            pointNum = GPSDataCol.Count;
            if (pointNum < 2) return GPSDataCol;

            ptrStart = ptrEnd = 0;
            while (ptrEnd < pointNum)
            {
                if (ptrEnd == pointNum - 1)
                {
                    if (ptrStart != ptrEnd)
                    {
                        stationName = "";
                        Point Position = ComputeMeanCoord(GPSDataCol, ptrStart, ptrEnd, ref stationName);

                        pCurrentPt.Longitude = Position.x; pCurrentPt.Latitude = Position.y; pCurrentPt.Stay = 1;
                        vSP.Add(pCurrentPt);
                        pNextPt.Longitude = Position.x; pNextPt.Latitude = Position.y; pNextPt.Stay = 2;
                        vSP.Add(pNextPt);
                    }
                }

                if (ptrStart > pointNum - 1 || ptrEnd > pointNum - 1) return vSP;

                if (ptrStart == 0)
                {
                    pCurrentPt = GPSDataCol[0];
                    pNextPt = GPSDataCol[1];
                    ptrStart++;
                }
                else
                {
                    if (ptrStart == ptrEnd && ptrStart < pointNum - 1) ptrEnd = ptrStart + 1;

                    pCurrentPt = GPSDataCol[ptrStart];
                    pNextPt = GPSDataCol[ptrEnd];
                }

                double distance = 0.0;
                using (Geodesic geod = new Geodesic())
                {
                    geod.Inverse(pCurrentPt.Latitude, pCurrentPt.Longitude, pNextPt.Latitude, pNextPt.Longitude, out distance);
                }

                //if (double.IsNaN(distance)) distance = 0;
                if (distance >= disError)
                {
                    vSP.Add(pCurrentPt); ptrStart = ++ptrEnd;
                    continue;
                }

                if (distance < disThreh) { ptrEnd++; continue; }
                else
                {
                    interval = (pNextPt.Time - pCurrentPt.Time).TotalSeconds;
                    if (interval >= timeThreh)
                    {
                        stationName = "";
                        Point Position = ComputeMeanCoord(GPSDataCol, ptrStart, ptrEnd, ref stationName);

                        pCurrentPt.Longitude = Position.x; pCurrentPt.Latitude = Position.y; pCurrentPt.Stay = 1;
                        vSP.Add(pCurrentPt);
                        pNextPt.Longitude = Position.x; pNextPt.Latitude = Position.y; pNextPt.Stay = 2;
                        vSP.Add(pNextPt);

                        ptrStart = ++ptrEnd; continue;
                    }
                    else
                    {
                        if (ptrStart == 1)
                        {
                            pCurrentPt.Stay = 0;
                            vSP.Add(pCurrentPt);   
                        } 
                        pNextPt.Stay = 0;
                        vSP.Add(pNextPt);
                        ptrStart = ++ptrEnd; continue;
                    }
                }
            }
            return vSP;
        }

        private Point ComputeMeanCoord(List<GPS> GPSDataCol, int start, int end, ref string stationName)
        {
            double longitude = 0; double latitude = 0;
            GPS pGPS;

            for (int i = start; i <= end; i++)
            {
                pGPS = GPSDataCol[i];

                Point pPt = new Point();
                pPt.x = pGPS.Longitude;
                pPt.y = pGPS.Latitude;

                //foreach (Station pStation in m_StationCol)
                //{
                //    if (pPt.Within(pStation.Rect))
                //    {
                //        stationName = pStation.StationName;
                //        break;
                //    }
                //}

                longitude = longitude + pGPS.Longitude;
                latitude = latitude + pGPS.Latitude;
            }

            Point meanPosition = new Point();
            meanPosition.x = longitude * 1.0 / (end - start + 1);
            meanPosition.y = latitude * 1.0 / (end - start + 1);

            return meanPosition;
        }
    }
}
