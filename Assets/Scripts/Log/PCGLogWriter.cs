using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Log
{
    public static class PCGLogWriter
    {
        private const string FolderPath = @"PCGLogs";
        private static readonly Encoding encoding = Encoding.GetEncoding("Shift_JIS");

        //private static string fileName;

        //public static void SetFileName()
        //{
        //    int fileCount = Directory.EnumerateFiles(FolderPath, "*.txt", SearchOption.TopDirectoryOnly).Count();
        //    fileName = $"{FolderPath}/LogFile_{fileCount.ToString("000000")}.txt";
        //}

        public static void WriteLog(string log)
        {
            int fileCount = Directory.EnumerateFiles(FolderPath, "*.txt", SearchOption.TopDirectoryOnly).Count();
            var fileName = $"{FolderPath}/LogFile_{fileCount.ToString("000000")}.txt";

            StreamWriter writer = new StreamWriter(fileName, true, encoding);
            writer.WriteLine($"{log}");
            writer.Close();
        }

    }
}

