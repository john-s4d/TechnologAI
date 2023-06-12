using System.Speech.Synthesis;

namespace Technologai
{
    /// <summary>
    /// Generate Audio Voice
    /// </summary>
    public class GenerateAudioVoice : Process
    {
        /*
        //Generate Audio Voice
        GenerateAudioVoice generateAudioVoice = new();
        var generateAudioVoiceDict = new Dictionary<string, object>()
             {
                 //"1: MALE, 2: FEMALE"
                 {"voiceGender" ,"1"},
                 //"65: Senior, 10: Child, 15: Teen, 30: Adult"
                 {"voiceAge", "10"},
                 //Enter text to convert to speech:
                 {"textToCreateAudio", "Hey i m dummy file to convert in audio" },
                 {"pathToSaveAudioFile",@"path of local directory" }
             };
        var responseFromOpenAI = generateAudioVoice.Execute(generateAudioVoiceDict).Result;
        */

        public string Description { get; } = "Generate Audio Voice";
        public string SampleJsonIn { get; set; } = "{\"voiceGender\":\"string\",\"voiceAge\":\"string\",\"pathToSaveAudioFile\":\"string\",\"textToCreateAudio\":\"string\"}";
        public string SampleJsonOut { get; set; } = "{\"contents\":\"object\"}";

        public async Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
        {
            var voiceGender = Convert.ToInt32(data["voiceGender"]);
            if (voiceGender != 1 && voiceGender != 2)
                voiceGender = 1;
            var voiceAge = Convert.ToInt32(data["voiceAge"]);
            if (voiceAge != 65 && voiceAge != 30 && voiceAge != 10 && voiceAge != 15)
                voiceAge = 30;
            var writeToFolder = (string)data["pathToSaveAudioFile"];
            string text = (string)data["textToCreateAudio"];
            var filename = writeToFolder + "\\" + text.Split(" ").First();
            try
            {
                // Get Audio from file
                var response = await Task.Run(() =>
                {
                    using SpeechSynthesizer synth = new();
                    // Saves Audio to file
                    synth.SetOutputToWaveFile($"{filename}.wav");

                    // Set the voice to use for synthesis
                    synth.SelectVoiceByHints((VoiceGender)voiceGender, (VoiceAge)voiceAge);
                    synth.Speak(text);
                    return $"Created file {text.Split(" ").First()} at location {filename}";
                });
                if (response != null)
                    return new Dictionary<string, object> { { "contents", $"{response}" } };

                return new Dictionary<string, object> { { "Error", $"unable to find the response result" } };
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "Error", $"unable to convert to audio file '{e.Message}'" } };
            }
        }
    }
}