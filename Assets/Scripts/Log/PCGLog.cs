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
        private const string StatusLogFolderPath = @"PCGLogs/StatusLog";
        private const string AnalyzeFolderPath = @"PCGLogs/Analyze";
        private static readonly Encoding encoding = Encoding.GetEncoding("Shift_JIS");

        public static void WriteStatusLog(string log)
        {
            int fileCount = Directory.EnumerateFiles(StatusLogFolderPath, "*.txt", SearchOption.TopDirectoryOnly).Count();
            var fileName = $"{StatusLogFolderPath}/LogFile_{fileCount.ToString("000000")}.txt";

            using (StreamWriter writer = new StreamWriter(fileName, true, encoding))
            {
                writer.WriteLine($"{log}");
                writer.Close();
            }
        }

        public static int CountLogFile()
        {
            var count = Directory.GetFiles($"{StatusLogFolderPath}").Select(x => x.Contains("LogFile")).Count();
            return count;
        }

        public static void WriteStatusJSONLog(string json)
        {
            var fileName = $"{StatusLogFolderPath}/StatusLog.json";

            using (StreamWriter writer = new StreamWriter(fileName, true, encoding))
            {
                writer.WriteLine($"{json}");
                writer.Close();
            }
        }

        public static string ReadStatusJSONLog()
        {
            var fileName = $"{StatusLogFolderPath}/StatusLog.json";

            using (StreamReader reader = new StreamReader(fileName, encoding))
            {
                var result = reader.ReadToEnd();
                reader.Close();
                return result;
            }
        }

        public static void DeleteStatusJSONLog()
        {
            var fileName = $"{StatusLogFolderPath}/StatusLog.json";
            File.Delete(fileName);
        }

        public static bool CheckExistJsonFile()
        {
            var fileName = $"{StatusLogFolderPath}/StatusLog.json";

            return File.Exists(fileName);
        }

        public static void WriteEvaluationCSV(float value)
        {
            var fileName = $"{AnalyzeFolderPath}/Evaluation.csv";

            using (StreamWriter writer = new StreamWriter(fileName, true, encoding))
            {
                writer.WriteLine($"{value.ToString()},");
                writer.Close();
            }
        }

        public static void WriteAnalyzeCSV(float winningRate, float distanceAverage, float synergy)
        {
            var fileName = $"{AnalyzeFolderPath}/Analyze.csv";

            using (StreamWriter writer = new StreamWriter(fileName, true, encoding))
            {
                writer.WriteLine($"{winningRate.ToString()},{distanceAverage.ToString()},{synergy.ToString()}");
                writer.Close();
            }
        }

        public static void WriteActionCSV(string characterName, string ActionName)
        {
            var fileName = $"{AnalyzeFolderPath}/Action.csv";

            using (StreamWriter writer = new StreamWriter(fileName, true, encoding))
            {
                writer.WriteLine($"{characterName.ToString()},{ActionName.ToString()}");
                writer.Close();
            }
        }

        public static void WriteCosineSimilarity(int index1, int index2, float cosineSimilarity)
        {
            var fileName = $"{AnalyzeFolderPath}/CosineSimilarity.csv";

            using (StreamWriter writer = new StreamWriter(fileName, true, encoding))
            {
                writer.WriteLine($"{index1.ToString()}:{index2.ToString()},{cosineSimilarity}");
                writer.Close();
            }
        }
    }
}

