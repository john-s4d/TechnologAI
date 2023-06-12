using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Technologai.External.TestApp
{
    internal class CodeUtility
    {
        public static string RemoveCommentsFromApexCode(string fileContents)
        {
            // Remove single-line comments (excluding those inside quotes)
            fileContents = Regex.Replace(fileContents, "(\"(?:\\\\[^\"]|[^\"\\\\])*\"|'(?:\\\\[^\']|[^\'\\\\])*')|//.*", "$1", RegexOptions.Multiline);

            // Remove multi-line comments (excluding those inside quotes)
            fileContents = Regex.Replace(fileContents, "(\"(?:\\\\[^\"]|[^\"\\\\])*\"|'(?:\\\\[^\']|[^\'\\\\])*')|/\\*.*?\\*/", "$1", RegexOptions.Singleline);

            return fileContents;
        }

        public static string[] ExtractMethodsAndClasses(string apexCode)
        {
            List<string> methodsAndClasses = new List<string>();

            // Split the Apex code into lines
            string[] lines = apexCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            // Keep track of the current method or class
            StringBuilder currentMethodOrClass = new StringBuilder();
            int bracketCount = 0;

            // Iterate over the lines of the Apex code
            foreach (string line in lines)
            {
                // Check if the line contains the start of a new method or class
                if (Regex.IsMatch(line, @"^\s*(public|private|protected)?\s*(virtual\s+)?(class|interface)\s+\w+.*\{"))
                {
                    currentMethodOrClass = new StringBuilder(line.TrimEnd());
                    bracketCount = 1;
                }
                else if (Regex.IsMatch(line, @"^\s*(public|private|protected)?\s*(virtual\s+)?(static\s+)?(void|[\w<>]+)\s+\w+\s*\(.*\)\s*(throws\s+\w+\s*(,\s*\w+)*)?\s*\{"))
                {
                    currentMethodOrClass = new StringBuilder(line.TrimEnd());
                    bracketCount = 1;
                }
                else if (bracketCount > 0)
                {
                    currentMethodOrClass.AppendLine(line.TrimEnd());
                    bracketCount += line.Count(c => c == '{') - line.Count(c => c == '}');

                    // Check if the current method or class has ended
                    if (bracketCount == 0)
                    {
                        methodsAndClasses.Add(currentMethodOrClass.ToString());
                        currentMethodOrClass.Clear();
                    }
                }
            }

            return methodsAndClasses.ToArray();
        }

        public static string ExtractMethodOrClassName(string methodOrClass)
        {
            string pattern = @"(public|private|protected)?\s*(virtual\s+)?(class|interface|enum|annotation)\s+(\w+)";
            Match match = Regex.Match(methodOrClass, pattern);

            if (match.Success)
            {
                // Return the name of the class, interface, enum, or annotation
                return match.Groups[4].Value;
            }
            else
            {
                pattern = @"(public|private|protected)?\s*(virtual\s+)?(static\s+)?(void|[\w<>]+)\s+(\w+)\s*\(";
                match = Regex.Match(methodOrClass, pattern);

                if (match.Success)
                {
                    // Return the name of the method
                    return match.Groups[5].Value;
                }
                else
                {
                    // No match found
                    return string.Empty;
                }
            }
        }
    }
}
