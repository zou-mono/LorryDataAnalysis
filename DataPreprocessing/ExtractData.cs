using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using log4net;
using System.IO;

namespace LorryDataAnalysis
{
    public class ExtractData
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static string _CurrentDirectory = Environment.CurrentDirectory; //程序当前运行的路径
        private static string _dataDirectory = null; //输入数据目录
        private static string _outTempDirectory = null; //输出数据临时目录
        private static string _outFile = null; //输出文件
        private static int _pointType = -1;
        private static int _maxThread = 0;
        private StreamWriter _sw=null;
private static object locker = new Object();
        public bool init(ExtractSubOptions opts)
        {
            try
            {
                _dataDirectory = Path.IsPathRooted(opts.inDirectoryName) ? opts.inDirectoryName : _CurrentDirectory + @"\" + opts.inDirectoryName;

                if (Directory.Exists(_dataDirectory) == false) throw new Exception("Error: input directory is not exist!");

                if (!String.IsNullOrEmpty(opts.outFileName))
                {
                    _outFile = Path.IsPathRooted(opts.outFileName) ? opts.outFileName + ".csv" : _CurrentDirectory + @"\" + opts.outFileName + ".csv";
                }
                else
                {
                    _outFile = _CurrentDirectory + @"\extracted_result.csv";
                }

                _outTempDirectory = _CurrentDirectory + @"\temp";
                _pointType = opts.pointType;
                _maxThread = opts.maxThread;

                _sw = new StreamWriter(_outFile, false, Encoding.GetEncoding("UTF-8"));

                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return false;
            }
        }

        private void ExportToFile(string file, int TypeStay)
        {
            //List<GPS> GPSArray = ToList(file);
            //List<GPS> GpsArray = new List<GPS>();

            FileInfo fi = new FileInfo(file);

            if (!Directory.Exists(_outTempDirectory))
            {
                Directory.CreateDirectory(_outTempDirectory);
            }

            try
            {
                //string out_file = Path.GetFileNameWithoutExtension(_outTempDirectory + "\\" + fi.Name) + ".csv";
                            
                string id = fi.Name.Substring(0, fi.Name.Length - 4);

                System.IO.StreamReader reader = new System.IO.StreamReader(file, Encoding.GetEncoding("UTF-8"));
                string line = "";
                lock (locker)
                {
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] a = line.Split(new char[] { ',' });
                        string out_line = id + "," + line;

                        if (TypeStay == 3)
                        {
                            if (a[5] == "1" || a[5] == "2")
                            {

                                _sw.WriteLine(out_line);
                                _sw.Flush();
                            }
                        }
                        else
                        {
                            if (a[5] == TypeStay.ToString())
                            {
                                _sw.WriteLine(out_line);
                                _sw.Flush();
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                logger.Error(ex.Message);
            }
        }

        public bool Step()
        {
            string[] files = new GetLogFiles().readlist(_dataDirectory);
            int FileCount = files.Length; int iCount = 0;
            List<Task> tsArr = new List<Task>();
            FileInfo fi = null;

            using (var progress = new ProgressBar())
            {
                try
                {
                    logger.Info("Extraction Processing...");

                    LimitedConcurrencyLevelTaskScheduler lcts = new LimitedConcurrencyLevelTaskScheduler(_maxThread);
                    TaskFactory factory = new TaskFactory(lcts);

                    foreach (string file in files)
                    {
                        fi = new FileInfo(file);

                        string filename = fi.Name.Substring(0, fi.Name.Length - 4);
                        if (filename.Length >= 7) { filename = filename.Substring(0, 7); }

                        //ExportToFile(file, _bFilter);
                        Task ts = factory.StartNew(() =>
                        {
                            try
                            {
                                ExportToFile(file, _pointType);
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
                    //else { Interlocked.Increment(ref iCount); progress.Report((double)iCount / FileCount); }

                    Task AllTasks = Task.WhenAll(tsArr.ToArray());
                    return AllTasks.ContinueWith<bool>((a) =>//当所有task完成后，执行这个回调
                    {
                        //Common.Execute("copy  " + _outTempDirectory + @"\*.csv " + _outFile, 0);
                        _sw.Close();
                        return true;
                    }).Result;
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                    return false;
                }
            }
        }
    }
}
