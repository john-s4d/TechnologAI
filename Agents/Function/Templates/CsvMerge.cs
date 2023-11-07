using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace Technologai.Agents.Core
{
    public class CsvMerge : Template
    {
        public CsvMerge()
        {
            Id = "csv_merge";
            Description = "Merging CSV file";
            InputKeys = new[] { "FileName", "MethodName" , "Result", "directoryName" };
            OutputKeys = new[] { "MergedData.csv" };
        }

        public override Task<bool> Assess(Information information) => Task.FromResult(true);
        public override async Task<Data?> Process(Information information)
        {
            var FileName = information.Input?.Structured?["FileName"] ?? string.Empty;
            var MethodName = information.Input?.Structured?["MethodName"] ?? string.Empty;
            var Result = information.Input?.Structured?["Result"] ?? string.Empty;
            var directoryName = information.Input?.Structured?["directoryName"] ?? string.Empty;
            string[] headers = { FileName, MethodName, Result };

            CSV output = new CSV(headers);

            foreach (string filePath in Directory.GetFiles(directoryName))
            {
                if (!filePath.EndsWith(".csv")) { continue; }

                string fileName = new FileInfo(filePath).Name;

                CSV input = CSV.FromFile(filePath);

                foreach (List<string> rowData in input.Data)
                {
                    output.AddRow(new List<string> { Path.GetFileNameWithoutExtension(fileName), rowData[0], rowData[1] });
                }
            }
            return Data.Create($"{directoryName}\\MergedData.csv");
        }
    }
}
