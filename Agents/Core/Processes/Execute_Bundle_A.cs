using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net;

namespace Technologai.Agents.Processes
{
    class Execute_Bundle_A
    {
        /// <summary>
        /// List files from a local directory.
        /// </summary>
        internal class ListFiles : IExecute
        {
            public string Description { get; } = "List file from the local directory.";
            public string SampleJsonIn { get; } = "{\"directory\":\"string\",\"includeSubDirectories\":\"bool\",\"fileExtension\":\"string\"}"; // optional: includeSubDirectories, fileExtension
            public string SampleJsonOut { get; } = "{\"files\":\"string[]\"}";

            public async Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
            {
                string directory = ((string)data["directory"]).Trim();
                try
                {
                    if (!Directory.Exists(directory))
                    {
                        return new Dictionary<string, object> { { "error", $"Directory doesn't exists'" } };
                    }

                    var files = await Task.Run(() =>
                    {
                        SearchOption searchOption = SearchOption.TopDirectoryOnly;
                        data.TryGetValue("includeSubDirectories", out object includeSubDirectories);
                        if (includeSubDirectories != null && (bool)includeSubDirectories == true)
                        {
                            searchOption = SearchOption.AllDirectories;
                        }

                        data.TryGetValue("fileExtension", out object fileExtension);
                        var fileExt = "*";
                        if (fileExtension != null && !string.IsNullOrEmpty((string)fileExtension))
                        {
                            fileExt += ((string)fileExtension).Trim();
                        }
                        var files = Directory.GetFiles(directory, fileExt, searchOption);

                        return files;
                    });
                    return new Dictionary<string, object>() { { "files", files } };
                }
                catch (Exception ex)
                {
                    return new Dictionary<string, object> { { "error", $"Error reading directory '{directory}': {ex.Message}" } };
                }
            }
        }

        /// <summary>
        /// Read a text file on the local filesystem.
        /// </summary>
        internal class ReadFile : IExecute
        {
            public string Description { get; } = "Read a text file on the local filesystem.";
            public string SampleJsonIn { get; } = "{\"fileName\":\"string\"}";
            public string SampleJsonOut { get; } = "{\"contents\":\"string\"}";

            public async Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
            {
                string fileName = ((string)data["fileName"]).Trim();
                try
                {
                    using StreamReader reader = new(fileName);
                    var contents = await reader.ReadToEndAsync();
                    return new Dictionary<string, object> { { "contents", contents } };
                }
                catch (Exception ex)
                {
                    return new Dictionary<string, object> { { "error", $"Error reading file '{fileName}': {ex.Message}" } };
                }
            }
        }

        /// <summary>
        /// Write a text file on the local filesystem.
        /// </summary>
        internal class WriteFile : IExecute
        {
            public string Description { get; } = "Write a text file on the local filesystem.";
            public string SampleJsonIn { get; } = "{\"fileName\":\"string\", \"content\":\"string\", \"overrideIfExists\":\"bool\"}";// optional: overrideIfExists
            public string SampleJsonOut { get; } = string.Empty;
            public async Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
            {
                string fileName = ((string)data["fileName"]).Trim();
                try
                {
                    data.TryGetValue("overrideIfExists", out object overrideIfExists);
                    if (overrideIfExists == null || (overrideIfExists != null && (bool)overrideIfExists == false))
                    {
                        if (File.Exists(fileName))
                        {
                            return new Dictionary<string, object> { { "error", $"File already exists." } };
                        }
                    }

                    using StreamWriter writer = new(fileName);
                    await writer.WriteAsync((string)data["content"]);
                    return new Dictionary<string, object>();
                }
                catch (Exception ex)
                {
                    return new Dictionary<string, object> { { "error", $"Error writting content to file '{fileName}': {ex.Message}" } };
                }
            }
        }

        /// <summary>
        /// Append to a text file on the local filesystem.
        /// </summary>
        internal class AppendToFile : IExecute
        {
            public string Description { get; } = "Append text to file on the local filesystem.";
            public string SampleJsonIn { get; } = "{\"fileName\":\"string\", \"content\":\"string\", \"createIfNotExists\":\"bool\"}";// optional: createIfNotExists
            public string SampleJsonOut { get; } = string.Empty;

            public async Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
            {
                string fileName = ((string)data["fileName"]).Trim();
                try
                {
                    data.TryGetValue("createIfNotExists", out object createIfNotExists);
                    if (createIfNotExists == null || (createIfNotExists != null && (bool)createIfNotExists == false))
                    {
                        if (!File.Exists(fileName))
                        {
                            return new Dictionary<string, object> { { "error", $"File doesn't exists." } };
                        }
                    }

                    using StreamWriter writer = new(fileName, true);
                    await writer.WriteAsync((string)data["content"]);
                    return new Dictionary<string, object>();
                }
                catch (Exception ex)
                {
                    return new Dictionary<string, object> { { "error", $"Error writting content to file '{fileName}': {ex.Message}" } };
                }
            }
        }

        /// <summary>
        /// Delete a file from the local filesystem.
        /// </summary>
        public class DeleteFile : IExecute
        {
            public string Description { get; } = "Delete a text file on the local filesystem";
            public string SampleJsonIn { get; } = "{\"filename\":\"string\"}";
            public string SampleJsonOut { get; } = "{\"contents\":\"string\"}";

            public async Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
            {
                string fileName = ((string)data["fileName"]).Trim();
                try
                {
                    if (!File.Exists(fileName))
                    {
                        return new Dictionary<string, object> { { "error", $"Error file name is not found'" } };
                    }
                    await Task.Run(() =>
                    {
                        File.Delete(fileName);
                    });

                    return new Dictionary<string, object>();
                }
                catch (Exception ex)
                {
                    return new Dictionary<string, object> { { "error", $"Error writting content to file '{fileName}': {ex.Message}" } };
                }

            }
        }

        /// <summary>
        /// Get text length.
        /// </summary>
        internal class GetTextLength : IExecute
        {
            public string Description { get; } = "Get text length.";
            public string SampleJsonIn { get; } = "{\"text\":\"string\"}";
            public string SampleJsonOut { get; } = "{\"length\":\"string\"}";

            public async Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
            {
                var length = await Task.Run(() =>
                {
                    return ((string)data["text"]).Length;
                });

                return new Dictionary<string, object>() { { "length", length } };
            }
        }

        /// <summary>
        /// Get chunk text.
        /// </summary>
        internal class ChunkText : IExecute
        {
            public string Description { get; } = "Get chunk text.";
            public string SampleJsonIn { get; } = "{\"text\":\"string\", \"chunkSize\":\"string\"}";
            public string SampleJsonOut { get; } = "{\"content\":\"string[]\"}";

            public async Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
            {
                var response = new Dictionary<string, object>();
                // text from the user input
                string text = ((string)data["text"]);

                // chunk size in integer
                int chunkSize = int.Parse(((string)data["chunkSize"]));
                var listOfChunk = new List<object>();
                if (string.IsNullOrEmpty(text))
                    return new Dictionary<string, object> { { "error", $"Entered text in null or empty : '{text}' " } };
                await Task.Run(() =>
                {
                    try
                    {
                        for (int i = 0; i < text.Length; i += chunkSize)
                        {
                            int length = Math.Min(chunkSize, text.Length - i);
                            string chunk = text.Substring(i, length);
                            listOfChunk.Add(chunk);
                        }
                        response.Add("content", listOfChunk);
                    }
                    catch (Exception ex)
                    {
                        response.Add("error", $"error chunking the text '{text}': '{ex.Message}'");
                    }
                });
                return response;
            }
        }

        /// <summary>
        /// Get current datetime.
        /// </summary>
        internal class GetCurrentDateTime : IExecute
        {
            public string Description { get; } = "Get current datetime.";
            public string SampleJsonIn { get; } = "{\"returnDateTimeFormat\":\"string\", \"getUtcTime\":\"bool\"}";
            public string SampleJsonOut { get; } = "{\"currentDateTime\":\"string\"}";

            public async Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
            {
                var dateTime = await Task.Run(() =>
                {
                    DateTime currentDateTime;
                    data.TryGetValue("getUtcTime", out object getUtcTime);
                    if (getUtcTime == null || (getUtcTime != null && (bool)getUtcTime == false))
                    {
                        currentDateTime = DateTime.Now;
                    }
                    else
                    {
                        currentDateTime = DateTime.UtcNow;
                    }

                    string dateTimeFormat;
                    data.TryGetValue("returnDateTimeFormat", out object returnDateTimeFormat);
                    if (returnDateTimeFormat == null || (returnDateTimeFormat != null && string.IsNullOrEmpty((string)returnDateTimeFormat)))
                    {
                        return currentDateTime.ToString();
                    }
                    else
                    {
                        dateTimeFormat = returnDateTimeFormat != null ? (string)returnDateTimeFormat : string.Empty;
                        return currentDateTime.ToString(dateTimeFormat);
                    }
                });

                return new Dictionary<string, object>() { { "currentDateTime", dateTime } };
            }
        }

        /// <summary>
        /// Take a response from the user.
        /// </summary>
        public class GetUserInput : IExecute
        {
            public string Description { get; set; } = "Receive a response from the user.";
            public string SampleJsonIn { get; set; } = string.Empty;
            public string SampleJsonOut { get; set; } = "{\"output\":\"string\"}";

            public async Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
            {
                var value = await Task.Run(() =>
                {
                    return Console.ReadLine() ?? string.Empty;
                });

                return new Dictionary<string, object> { { "output", value } };
            }
        }

        /// <summary>
        /// Git clone from the method
        /// </summary>
        public class GitClone : IExecute
        {
            public string Description { get; } = "Takes clone of a repository.";
            public string SampleJsonIn { get; } = "{\"repoLink\":\"string\", \"directory\":\"string\"}";
            public string SampleJsonOut { get; } = string.Empty;

            public async Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
            {
                var reopoLink = ((string)data["repoLink"]).Trim();
                try
                {
                    await Task.Run(() =>
                    {
                        var process = new System.Diagnostics.Process
                        {
                            StartInfo = new ProcessStartInfo()
                            {
                                FileName = "git",
                                Arguments = $"clone {((string)data["repoLink"]).Trim()}",
                                WorkingDirectory = ((string)data["directory"]).Trim(),
                            }
                        };
                        process.Start();
                    });
                    return new Dictionary<string, object>();
                }
                catch (Exception ex)
                {
                    return new Dictionary<string, object> { { "error", $"Error cloning the given ropo '{reopoLink}': {ex.Message}" } };
                }

            }
        }

        /// <summary>
        /// Download file from web url
        /// </summary>
        public class DownloadFile : IExecute
        {
            public string Description { get; } = "Downloaded a file from web url in the local filesystem.";
            public string SampleJsonIn { get; } = "{\"weburl\":\"string\",\"filepath\":\"string\" }";
            public string SampleJsonOut { get; } = string.Empty;

            public async Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
            {
                var url = new Uri($"{((string)data["weburl"]).Trim()}");
                string filePath = ((string)data["filepath"]).Trim();
                try
                {
                    await Task.Run(() =>
                    {
                        using (var client = new WebClient())
                        {
                            client.DownloadFile(url, filePath);
                        };
                    });

                    return new Dictionary<string, object>();
                }
                catch (WebException ex)
                {
                    return new Dictionary<string, object> { { "error", $"Error downloading file '{url}': {ex.Message}" } };
                }
            }
        }

        /// <summary>
        /// Execute python
        /// </summary>
        public class ExecutePython : IExecute
        {
            public string Description { get; } = "Execute python in the local system.";
            public string SampleJsonIn { get; } = "{\"cmd\":\"string\",\"args\":\"string\" }";
            public string SampleJsonOut { get; set; } = "{\"output\":\"string\"}";

            public async Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
            {
                try
                {
                    var output = await Task.Run(() =>
                    {
                        ProcessStartInfo start = new()
                        {
                            FileName = data["cmd"].ToString(),
                            Arguments = data["args"].ToString(),
                            UseShellExecute = false,
                            RedirectStandardOutput = true
                        };
                        using System.Diagnostics.Process process = System.Diagnostics.Process.Start(start);
                        using StreamReader reader = process.StandardOutput;
                        string output = reader.ReadToEnd();

                        return output;
                    });

                    if (output != string.Empty)
                    {
                        return new Dictionary<string, object> { { "output", output } };
                    }
                    return new Dictionary<string, object>();
                }
                catch (Exception ex)
                {
                    return new Dictionary<string, object> { { "error", $"Error occured while executing python cmd : {ex.Message}" } };
                }
            }
        }

        /// <summary>
        /// Search Google
        /// </summary>
        public class SearchGoogle : IExecute
        {
            public string Description { get; } = "Search Google using serach query";
            public string SampleJsonIn { get; set; } = "{\"apikey\":\"string\",\"searchEngineID\":\"string\",\"searchQuery\":\"string\"}";
            public string SampleJsonOut { get; set; } = "{\"output\":\"string\"}";

            private string ApiKey { get; set; }

            public SearchGoogle(string apiKey)
            {
                ApiKey = apiKey;
            }

            public async Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
            {
                var searchEngineId = ((string)data["searchEngineID"]).Trim(); ;

                // Define the search query
                var query = ((string)data["searchQuery"]).Trim();

                // Create a new instance of HttpClient to send HTTP requests
                var httpClient = new HttpClient();

                try
                {
                    // Send a GET request to the Google API to perform the search
                    var url = $"https://www.googleapis.com/customsearch/v1?key={ApiKey}&cx={searchEngineId}&q={query}";
                    var response = await httpClient.GetAsync(url);
                    if (response != null)
                    {
                        // Read the content of the response as a string
                        var jsonString = response.Content.ReadAsStringAsync().Result;

                        // Parse the JSON response using Newtonsoft.Json
                        var jsonObject = JObject.Parse(jsonString);

                        return new Dictionary<string, object>() { { "result", jsonObject } };
                    }
                    else
                    {
                        return new Dictionary<string, object>();
                    }
                }
                catch (Exception ex)
                {
                    return new Dictionary<string, object> { { "error", $"Error executing request on google: {ex.Message}" } };
                }
            }
        }

        /// <summary>
        /// Execute Shell
        /// </summary>
        public class ExecuteShell : IExecute
        {
            public string Description { get; } = "Execute Shell in the local system.";
            public string SampleJsonIn { get; set; } = "{\"fileName\":\"string\", \"arguments\":\"string\"}";
            public string SampleJsonOut { get; set; } = "{\"output\":\"string\"}";

            public async Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
            {
                try
                {
                    var output = await Task.Run(() =>
                    {
                        System.Diagnostics.Process process = new();

                        // Configure the process to run the command
                        process.StartInfo.FileName = ((string)data["fileName"]).Trim();
                        process.StartInfo.Arguments = ((string)data["arguments"]).Trim();
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.RedirectStandardOutput = true;

                        // Start the process and wait for it to complete
                        process.Start();
                        process.WaitForExit();

                        // Read the output from the command and print it to the console
                        string output = process.StandardOutput.ReadToEnd();

                        return output;
                    });

                    if (output != string.Empty)
                    {
                        return new Dictionary<string, object> { { "output", output } };
                    }
                    return new Dictionary<string, object>();
                }
                catch (Exception ex)
                {
                    return new Dictionary<string, object> { { "error", $"Error occured while processing request: {ex.Message}" } };
                }
            }
        }

        // <summary>
        /// Read Web Page
        /// </summary>
        public class ReadWebPage : IExecute
        {
            public string Description { get; } = "Read Web Page to scrape html.";
            public string SampleJsonIn { get; set; } = "{\"url\":\"string\", \"x-path\":\"string\"}";
            public string SampleJsonOut { get; set; } = "{\"result\":\"string\"}";

            public async Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
            {
                try
                {
                    var nodes = await Task.Run(() =>
                    {
                        // the URL of the target page
                        string url = ((string)data["url"]).Trim();
                        var web = new HtmlWeb();

                        // downloading to the target page
                        // and parsing its HTML content
                        var document = web.Load(url);

                        // selecting the HTML nodes of interest  
                        var nodes = document.DocumentNode.SelectNodes($"//*{((string)data["x-path"]).Trim()}");
                        var list = nodes.ToList();
                        return list;
                    });
                    if (nodes.Any())
                    {
                        return new Dictionary<string, object>() { { "result", nodes } };
                    }

                    return new Dictionary<string, object>();
                }
                catch (Exception ex)
                {
                    return new Dictionary<string, object> { { "error", $"Error occured while processing request: {ex.Message}" } };
                }
            }
        }

    }
}



