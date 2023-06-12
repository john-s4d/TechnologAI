
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;

namespace Technologai.External.TestApp
{
    internal class CSV
    {
        public string[] Headers { get; private set; }
        public List<List<string>> Data { get { return _data; } }

        private List<List<string>> _data = new List<List<string>>();
        
        public CSV(string[] headers)
        {
            Headers = headers;
        }

        public void AddRow(List<string> rowData)
        {
            List<string> escaped = new List<string>();

            foreach(string column in rowData)
            {
                var value = column;                
                escaped.Add($"\"{value.Replace("\"", "\"\"")}\"");
            }

            Data.Add(escaped);
        }

        public void Write(string filePath)
        {
            using (StreamWriter sw = new StreamWriter(filePath))
            {   
                sw.WriteLine(string.Join(",", Headers));

                foreach (List<string> row in Data.ToArray())
                {
                    string line = string.Join(",", row);
                    sw.WriteLine(line);
                }
            }
        }

        public static CSV FromFile(string filePath)
        {
            CSV output;

            using (StreamReader reader = new StreamReader(filePath))
            {   
                string? line = reader.ReadLine();

                output = new CSV(ParseLine(line).ToArray());
                
                List<List<string>> rowData = new List<List<string>>();

                while ((line = reader.ReadLine()) != null)
                {
                    List<string> row = ParseLine(line);
                    output.AddRow(row);
                }
            }

            return output;

        }

        private static List<string> ParseLine(string line)
        {
            List<string> values = new List<string>();
            int startIndex = 0;
            int endIndex;

            while (startIndex < line.Length)
            {
                if (line[startIndex] == '\"')
                {
                    // Quoted value
                    endIndex = startIndex + 1;
                    while (endIndex < line.Length)
                    {
                        if (line[endIndex] == '\"')
                        {
                            // Found closing quote, add value to list and exit loop
                            values.Add(line.Substring(startIndex + 1, endIndex - startIndex - 1)
                                .Replace("\"\"", "\""));
                            startIndex = endIndex + 2; // Skip comma and space after closing quote
                            break;
                        }
                        endIndex++;
                    }
                }
                else
                {
                    // Non-quoted value
                    endIndex = line.IndexOf(',', startIndex);
                    if (endIndex == -1)
                    {
                        endIndex = line.Length;
                    }
                    values.Add(line.Substring(startIndex, endIndex - startIndex));
                    startIndex = endIndex + 1;
                }
            }

            return values;
        }
    }
}
