using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Technologai.External.TestApp
{
    internal class TextUtility
    {
        public static string[] SplitText(string text, int maxLength = 3900)
        {
            List<string> result = new List<string>();
            int start = 0;            
            while (start < text.Length)
            {
                int length = Math.Min(maxLength, text.Length - start);
                string substr = text.Substring(start, length);

                // if the substring ends in the middle of a sentence, adjust the length accordingly
                if (substr.LastIndexOfAny(new char[] { '.', '!', '?' }) != substr.Length - 1)
                {
                    int lastPeriod = substr.LastIndexOf('.');
                    int lastExclamation = substr.LastIndexOf('!');
                    int lastQuestion = substr.LastIndexOf('?');
                    int lastEnd = Math.Max(lastPeriod, Math.Max(lastExclamation, lastQuestion));
                    if (lastEnd != -1)
                    {
                        length = lastEnd + 1;
                        substr = text.Substring(start, length);
                    }
                }

                result.Add(substr);
                start += length;
            }
            return result.ToArray();
        }

    }
}
