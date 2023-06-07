using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAI.GPT3.ObjectModels.RequestModels;
using OpenAI.GPT3.ObjectModels;
using OpenAI.GPT3.Extensions;
using OpenAI.GPT3.Interfaces;
using LaserCatEyes.HttpClientListener;
using OpenAI.GPT3.ObjectModels.ResponseModels;

namespace Technologai.External.TestApp
{
    internal class OpenAI
    {
        private readonly IOpenAIService _sdk;
        private readonly IConfiguration _config;

        internal OpenAI()
        {
            _config = new ConfigurationBuilder()
            .AddUserSecrets<OpenAI>()
            .Build();

            var serviceCollection = new ServiceCollection()
            .AddScoped(_ => _config)
            //.AddLaserCatEyesHttpClientListener()
            .AddOpenAIService();

            _sdk = serviceCollection.BuildServiceProvider().GetRequiredService<IOpenAIService>();
        }

        internal async Task<string?> GetPromptCompletion(string prompt)
        {
            var completionResult = await _sdk.Completions.CreateCompletion(new CompletionCreateRequest()
            {
                Prompt = prompt,
                MaxTokens = 4000
            },
                Models.TextDavinciV3
            );

            if (completionResult.Successful)
            {
                return completionResult.Choices.FirstOrDefault()?.Text;
            }
            else
            {
                if (completionResult.Error == null)
                {
                    return "Unknown Error";
                }
                return $"{completionResult.Error.Code}: {completionResult.Error.Message}";
            }
        }

        internal async Task<string> GetSolution()
        {
            string filePath = "<filepath>";

            string longText = File.ReadAllText(filePath);

            string result = string.Empty;

            foreach (string prompt in TextUtility.SplitText(longText))
            {
                var completionResult = await _sdk.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
                {
                    Messages = new List<ChatMessage>
                    {
                        ChatMessage.FromSystem("You are a helpful assistant proficient with Salesforce."),                        
                        ChatMessage.FromUser("Please design and format a salesforce solution based on the following summary. Elaborate as much as possible, using up to 4000 characters in the response:" + prompt),
                    },
                    Model = Models.ChatGpt3_5Turbo0301
                });

                if (completionResult.Successful)
                {
                    result += completionResult.Choices.FirstOrDefault()?.Message.Content;
                }
                else
                {
                    if (completionResult.Error == null)
                    {
                        result += "Unknown Error";
                    }

                    result += $"{completionResult.Error.Code}: {completionResult.Error.Message}";
                }

            }
            return result;
        }

        internal async Task Summarize()
        {            
            string filePath = "<filepath>";

            string longText = File.ReadAllText(filePath);

            foreach(string prompt in TextUtility.SplitText(longText))
            {                
                var completionResult = await _sdk.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
                {
                    Messages = new List<ChatMessage>
                    {
                        ChatMessage.FromSystem("You are a helpful assistant proficient at generating software requirements."),
                        //ChatMessage.FromUser("Clean-up, deduplicate, and consolidate the requirements for a Salesforce implementation from the following text:\n" + prompt),
                        ChatMessage.FromUser("Clean-up, deduplicate, and consolidate the requirements for a Salesforce implementation from the following text - focusing and bucketing into these categories: \"Lead and Opportunity Management\", \"Property Program\", and \"Insurance Program\".\n\n\n" + prompt),
                    },
                    Model = Models.ChatGpt3_5Turbo0301
                });

                if (completionResult.Successful)
                {
                    string result = completionResult.Choices.FirstOrDefault()?.Message.Content;
                    File.AppendAllText("<filepath>", result + "\n");
                }
                else
                {
                    if (completionResult.Error == null)
                    {
                        Console.WriteLine("Unknown Error");
                    }
                    else
                    {
                        Console.WriteLine($"{completionResult.Error.Code}: {completionResult.Error.Message}");
                    }

                    File.AppendAllText("<filepath>", prompt + "\n");
                }
            }
        }

        internal async Task DoTelusCodeAnalysis()
        {
            string path = "<filepath>";

            foreach (string filepath in Directory.GetFiles(path))
            {
                var filename = Path.GetFileName(filepath);

                if (!filename.EndsWith(".cls") || filename.EndsWith("Test.cls")) { continue; }

                List<string> headers = new List<string> { "Method_Name", "Result" };

                CSV result = new CSV(headers.ToArray());

                string code = CodeUtility.RemoveCommentsFromApexCode(File.ReadAllText($"{path}\\{filename}"));

                string[] codeComponents = CodeUtility.ExtractMethodsAndClasses(code);

                string promptPrefix = "Does the following Salesforce Apex class or method contain any specific errors that would cause performance issues or violate best practices? Provide a succinct answer without a description. Only provide details if an error is found.";

                foreach (string codeComponent in codeComponents)
                {
                    string methodOrClassName = CodeUtility.ExtractMethodOrClassName(codeComponent);

                    string prompt = $"{promptPrefix}\n{codeComponent}";

                    prompt = prompt.Length > 4000 ? prompt.Substring(0, 4000) : prompt.Substring(0, prompt.Length);

                    var completionResult = await _sdk.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
                    {
                        Messages = new List<ChatMessage>
                    {
                        ChatMessage.FromSystem("You are a helpful assistant proficient in Salesforce Apex code analysis."),
                        ChatMessage.FromUser(prompt),
                    },
                        Model = Models.ChatGpt3_5Turbo0301
                    });

                    if (completionResult.Successful)
                    {
                        result.AddRow(new List<string> { methodOrClassName, completionResult.Choices.FirstOrDefault()?.Message.Content });
                    }
                    else
                    {
                        if (completionResult.Error == null)
                        {
                            result.AddRow(new List<string> { methodOrClassName, "Unknown Error" });                            
                        }
                        result.AddRow(new List<string> { methodOrClassName, $"{completionResult.Error.Code}: {completionResult.Error.Message}" });                        
                    }
                }
                result.Write($"<filepath>{filename}.csv");
            }
        }
    }
}
