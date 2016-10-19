using System;
using System.Collections.Generic;
using System.Text;
//using ESRI.ArcGIS.esriSystem;
using System.Diagnostics;
using System.Linq;
using CommandLine;
using log4net;
using log4net.Config;

namespace LorryDataAnalysis
{
    class Program
    {
        //private static LicenseInitializer m_AOLicenseInitializer = new LorryDataAnalysis.LicenseInitializer();
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        //[STAThread()]
        static void Main(string[] args)
        {
            //args = ("convert -i TRK20160307 -l lpFile.csv -f -t 6").Split();
            //args = ("convert -i test -f -t 1 -o test_sgps").Split();
            //args = ("convert -i TRK20160307 -l lpFile.csv -t 6 -f -o stay_gps").Split();
            args = ("extract -i stay_gps -n 6 -t 3").Split();
            //args = ("help").Split();

            if (args.Length > 0) { if (args[0].ToLowerInvariant() == "help") args = args.Skip(1).ToArray(); }

            Stopwatch watch = new Stopwatch();
            
            Action<bool> printIfNotEmpty = flag =>
            {
                if (flag == false)
                {
                    return;
                }
                watch.Stop();
                logger.Info("All done! Total cost " + watch.Elapsed.TotalSeconds.ToString() + "s");
            };

            watch.Start();

            Func<ConvertSubOptions, bool> convertor = opts => { return ConvertFiles(opts); };
            Func<ExtractSubOptions, bool> extractor = opts => { return ExtractFiles(opts); };

            string invokedVerb = null;
            object invokedVerbInstance = null;

            var options = new VerbCommand();
            bool exitCode = CommandLine.Parser.Default.ParseArguments(args, options,
              (verb, subOptions) =>
              {
                  // if parsing succeeds the verb name and correct instance
                  // will be passed to onVerbCommand delegate (string,object)
                  invokedVerb = verb;
                  invokedVerbInstance = subOptions; 
              });

            if (exitCode)
            {
                switch (invokedVerb)
                {
                    case "convert":
                        ConvertSubOptions convertSubOptions = (ConvertSubOptions)invokedVerbInstance;
                        convertor(convertSubOptions);
                        break;
                    case "extract":
                        ExtractSubOptions extractSubOptions = (ExtractSubOptions)invokedVerbInstance;
                        extractor(extractSubOptions);
                        break;
                }
            }

            printIfNotEmpty(exitCode);

            //Console.ReadKey(true);
        }

        private static bool ConvertFiles(ConvertSubOptions opts)
        {
            logger.Info("Starting...");
            DataConvert dc = new DataConvert();
            bool bRes = dc.init(opts);
            return bRes? dc.Step(): false;
        }
        private static bool ExtractFiles(ExtractSubOptions opts)
        {
            logger.Info("Starting...");
            ExtractData ed = new ExtractData();
            bool bRes = ed.init(opts);
            return bRes ? ed.Step() : false;
        }

        private static Tuple<bool> MakeError()
        {
            return Tuple.Create(false);
        }
    }
}
