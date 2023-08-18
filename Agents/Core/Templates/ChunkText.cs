using Core.Templates;
using System.Text.Json;

namespace Technologai
{
    public class ChunkText : Template
    {
        private const int DEFAULT_SIZE = 4000;

        public ChunkText()
        {
            Id = "chunk_text";
            Description = "Split Text Into Chunks";
            InputKeys = new string[] { "text", "size" };
            OutputKeys = new string[] { "chunks" };
        }

        public override Task<bool> Assess(Information information)
        {
            switch (information.Input?.Format)
            {
                case DataFormat.RAW:
                    return Task.FromResult(information.Input?.Raw != null);
                case DataFormat.STRUCTURED:
                    return Task.FromResult(
                        (information.Input?.Structured?.ContainsKey("text") ?? false) &&
                        (information.Input?.Structured?.ContainsKey("size") ?? false)
                    );
            }

            return Task.FromResult(false);
        }

        public override Task<Data?> Process(Information information)
        {
            string? text = string.Empty;
            int size = DEFAULT_SIZE;

            switch (information.Input?.Format)
            {
                case DataFormat.RAW:
                    text = information.Input?.Raw ?? string.Empty;
                    break;
                case DataFormat.STRUCTURED:
                    text = information.Input.Structured?["text"] ?? string.Empty;
                    size = int.TryParse(information.Input.Structured?["size"] ?? string.Empty, out size) ? size : DEFAULT_SIZE;
                    break;
            }

            var result = SplitText(text ?? string.Empty, size);

            return Task.FromResult((Data?)new Data(new Dictionary<string, string> { { "chunks", JsonSerializer.Serialize(result) } }));
        }

        public static string[] SplitText(string text, int maxLength = DEFAULT_SIZE)
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
