using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;

namespace LorryDataAnalysis
{
    public class GetLogFiles
    {
        public ArrayList alst;

        //public int FileCount;

        public GetLogFiles()
        {
        }

        public string[] readlist(string path)
        {
            alst = new System.Collections.ArrayList();//建立ArrayList对象
            GetDirs(path);//得到文件夹
            return (string[])alst.ToArray(typeof(string));//把ArrayList转化为string[]
        }

        public void GetFiles(string dir)
        {
            try
            {
                string[] files = Directory.GetFiles(dir);//得到文件
                foreach (string file in files)//循环文件
                {
                    //string exname = file.Substring(file.LastIndexOf(".") + 1);//得到后缀名
                    //// if (".txt|.aspx".IndexOf(file.Substring(file.LastIndexOf(".") + 1)) > -1)//查找.LOG .aspx结尾的文件
                    //if (".LOG".IndexOf(file.Substring(file.LastIndexOf(".") + 1)) > -1)
                    //{
                    FileInfo fi = new FileInfo(file);//建立FileInfo对象
                    alst.Add(fi.FullName);//把.LOG文件全名加人到FileInfo对象
                    //}
                }
            }
            catch
            {

            }
        }

        public void GetDirs(string d)//得到所有文件夹
        {
            GetFiles(d);//得到所有文件夹里面的文件
            try
            {
                string[] dirs = Directory.GetDirectories(d);
                foreach (string dir in dirs)
                {
                    GetDirs(dir);//递归
                }
            }
            catch
            {
            }
        }
    }
}
