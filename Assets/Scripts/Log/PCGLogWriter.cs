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

        public static void WriteEvaluationCSV(float value)
        {
            var fileName = $"{FolderPath}/Evaluation.csv";

            using (StreamWriter writer = new StreamWriter(fileName, true, encoding))
            {
                writer.WriteLine($"{value.ToString()},");
                writer.Close();
            }
        }

        public static void WriteAnalyzeCSV(float winningRate, float distanceAverage, float synergy)
        {
            var fileName = $"{FolderPath}/Analyze.csv";

            using (StreamWriter writer = new StreamWriter(fileName, true, encoding))
            {
                writer.WriteLine($"{winningRate.ToString()},{distanceAverage.ToString()},{synergy.ToString()}");
                writer.Close();
            }
        }

        public static void WriteActionCSV(string characterName, string ActionName)
        {
            var fileName = $"{FolderPath}/Action.csv";

            using (StreamWriter writer = new StreamWriter(fileName, true, encoding))
            {
                writer.WriteLine($"{characterName.ToString()},{ActionName.ToString()}");
                writer.Close();
            }
        }

        public static void WriteCosineSimilarity(int index1, int index2, float cosineSimilarity)
        {
            var fileName = $"{FolderPath}/Action.csv";

            using (StreamWriter writer = new StreamWriter(fileName, true, encoding))
            {
                writer.WriteLine($"{index1.ToString()}:{index2.ToString()} {cosineSimilarity}\n");
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
            File.Delete(fileName);
        }
    }
}

