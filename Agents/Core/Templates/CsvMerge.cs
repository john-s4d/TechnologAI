using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Technologai.Agents.Core
{
    public class CsvMerge
    {
        public static void DoMerge(string directoryName)
        {
            string[] headers = { "FileName", "MethodName", "Result" };
            CSV output = new CSV(headers);

            foreach (string filePath in Directory.GetFiles(directoryName))
            {
                if(!filePath.EndsWith(".csv")) { continue; }

                string fileName = new FileInfo(filePath).Name;

                CSV input = CSV.FromFile(filePath);
             
                foreach(List<string> rowData in input.Data)
                {
                    output.AddRow(new List<string> { Path.GetFileNameWithoutExtension(fileName), rowData[0], rowData[1] });
                }
            }
            output.Write($"{directoryName}\\MergedData.csv");
        }

    }
}
