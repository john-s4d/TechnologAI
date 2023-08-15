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

        public override Task<bool> Assess(InformationAdapter information)
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

        public override Task<Data?> Process(InformationAdapter information)
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

            var result = TextUtility.SplitText(text ?? string.Empty, size);

            return Task.FromResult((Data?)new Data(new Dictionary<string, Data> { { "chunks", JsonSerializer.Serialize(result) } }));
        }
    }
}
