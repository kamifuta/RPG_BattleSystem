using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Log
{
    public static class PCGLog
    {
        private const string FolderPath = @"PCGLogs";
        private static readonly Encoding encoding = Encoding.GetEncoding("Shift_JIS");

        public static void WriteLog(string log)
        {
            int fileCount = Directory.EnumerateFiles(FolderPath, "*.txt", SearchOption.TopDirectoryOnly).Count();
            var fileName = $"{FolderPath}/LogFile_{fileCount.ToString("000000")}.txt";

            using (StreamWriter writer = new StreamWriter(fileName, true, encoding))
            {
                writer.WriteLine($"{log}");
                writer.Close();
            }
        }

        public static bool CheckExistJsonFile()
        {
            var fileName = $"{FolderPath}/StatusLog";

            return File.Exists(fileName);
        }

        public static void WriteJSONLog(string json)
        {
            var fileName = $"{FolderPath}/StatusLog";

            using (StreamWriter writer = new StreamWriter(fileName, true, encoding))
            {
                writer.WriteLine($"{json}");
                writer.Close();
            }
        }

        public static string ReadJSONLog()
        {
            var fileName = $"{FolderPath}/StatusLog";

            using (StreamReader reader = new StreamReader(fileName, encoding))
            {
                var result=reader.ReadToEnd();
                reader.Close();
                return result;
            }
        }

        public static void DeleteJSONLog()
        {
            var fileName = $"{FolderPath}/StatusLog";

            using (StreamWriter writer = new StreamWriter(fileName, false, encoding))
            {
                writer.Write("");
                writer.Close();
            }
        }
    }
}

