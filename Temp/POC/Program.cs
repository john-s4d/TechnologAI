
using System.ComponentModel.DataAnnotations;
using TestApp;

namespace Technologai.External.TestApp
{
    internal class Program
    {

        private static OpenAI _openAI = new OpenAI();
        private static SymblAI _symblAI = new SymblAI();


        internal static void Main(string[] args)
        {
            Run().Wait();            
        }


        private async static Task Run()
        {   
            do
            {
                Console.WriteLine("input:>");
                
                string value = await Task.Run(() =>
                {
                    return Console.ReadLine() ?? "";
                });

                if (value.Equals("quit", StringComparison.OrdinalIgnoreCase)) { break; }

                if (value.Equals("start", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine(await _symblAI.SubmitVideoAsync());
                    //await _openAI.DoTelusCodeAnalysis();
                }

                if (value.StartsWith("status ", StringComparison.OrdinalIgnoreCase) && value.Length > 7)
                {
                    Console.WriteLine(await _symblAI.GetJobStatus(value.Split(" ")[1]));
                }

                if (value.Equals("transcript", StringComparison.OrdinalIgnoreCase))
                {
                    _symblAI.SaveTranscript();
                }

                if (value.Equals("summarize", StringComparison.OrdinalIgnoreCase))
                {
                   await _openAI.Summarize();
                }

                if (value.Equals("solution", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine(await _openAI.GetSolution());
                }

                if (value.Equals("merge", StringComparison.OrdinalIgnoreCase))
                {
                    CsvMerge.DoMerge("<filepath>");                    
                }


                //Console.WriteLine(await _openAI.GetPromptCompletion(value));

            }
            while (true);

        }

    }
}