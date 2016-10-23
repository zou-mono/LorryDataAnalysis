using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace LorryDataAnalysis
{
    public class Options
    {
        [Option('i', DefaultValue = "inputDirectory", Required = true,
        HelpText = "Input directory contains files are about to be converted. ")]
        public string inDirectoryName { get; set; }

        [Option('n', "MaxThread", DefaultValue = 8,
        HelpText = "Set the maximum count of running thread.")]
        public int maxThread { get; set; }
    }

    public class VerbCommand
    {
        public VerbCommand()
          {
            // Since we create this instance the parser will not overwrite it
              ConvertVerb = new ConvertSubOptions();
              MarkpointVerb = new mpSubOptions();
          }

        [VerbOption("convert", HelpText = "Convert original GPS binary files to csv files.")]
        public ConvertSubOptions ConvertVerb { get; set; }

        [VerbOption("mp", HelpText = "Generate mark points by grid index.")]
        public mpSubOptions MarkpointVerb { get; set; }

        [VerbOption("extract", HelpText = "Extract feature points from the converted data.")]
        public ExtractSubOptions ExtractVerb { get; set; }

        [HelpVerbOption]
        public string GetUsage(string verb)
        {
            return HelpText.AutoBuild(this, verb);
        }
    }

    public class ConvertSubOptions : Options
    {
        [Option('l', "lpFile",
        HelpText = "License plate file to be extracted input files. The default is empty, means not to extract.")]
        public string lpFileName { get; set; }

        [Option('f', "filter", DefaultValue = false,
        HelpText = "To filter stay points or not.")]
        public bool filter { get; set; }

        [Option('o', DefaultValue = "gps_result",
        HelpText = "Output directory to be stored result files. The default is in current directory and the name is gps_result")]
        public string outDirectoryName { get; set; }
    } 

    public class mpSubOptions : Options 
    {
        [Option('l', "lpFile",
        HelpText = "License plate file to be extracted input files. The default is empty, means not to extract.")]
        public string lpFileName { get; set; }
        //public int maxThread { get; set; }
    }

    public class ExtractSubOptions : Options
    {
        [Option('o', DefaultValue = "extracted_result",
        HelpText = "Output file to be stored result files. The default is in current directory and the name is extracted_result")]
        public string outFileName { get; set; }

        //[Option('i', DefaultValue = "inputDirectory", Required = true,
        //HelpText = "Input directory contains files are about to be converted. ")]
        //public string inDirectoryName { get; set; }

        [Option('p', DefaultValue = false,
        HelpText = "Only export position(x and y) for charting. The default is false.")]
        public bool onlyExportPos { get; set; }

        [Option('t', Required = true,
        HelpText = "Extraction types. 0 - running points; 1 - arrive stay points; 2 - leave stay points; 3 - arrive and leave stay points. The default is 0.")]
        public int pointType { get; set; }

    }
}
