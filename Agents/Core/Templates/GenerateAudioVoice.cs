using System.Speech.Synthesis;

namespace Technologai.Templates
{
    /// <summary>
    /// Generate Audio Voice
    /// </summary>
    public class GenerateAudioVoice : Template
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
        
        public GenerateAudioVoice()
        {
            Id = "generate_audio_voice";
            Description = "Generate Audio Voice";
            InputKeys = new[] { "voiceGender", "voiceAge", "pathToSaveAudioFile", "textToCreateAudio" };
            OutputKeys = new[] { "contents" };
        }

        public override Task<bool> Assess(Information information) => Task.FromResult(true);

        public async override Task<Data?> Process(Information information)
        {
            var voiceGender = Convert.ToInt32(information.Input.Structured?["voiceGender"]);
            if (voiceGender != 1 && voiceGender != 2)
                voiceGender = 1;
            var voiceAge = Convert.ToInt32(information.Input.Structured?["voiceAge"]);
            if (voiceAge != 65 && voiceAge != 30 && voiceAge != 10 && voiceAge != 15)
                voiceAge = 30;
            var writeToFolder = (string)information.Input.Structured?["pathToSaveAudioFile"];
            string text = (string)information.Input.Structured?["textToCreateAudio"];
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
                    return Task.FromResult((Data?)new Data(new Dictionary<string, string> { { "output", $"Created file {text.Split(" ").First()} at location {filename}" } }));
                });
                if (response != null)
                    return await Task.FromResult((Data?)new Data(new Dictionary<string, string> { { "contents", $"{response}" } }));

                return await Task.FromResult((Data?)new Data(new Dictionary<string, string> { { "Error", $"unable to find the response result" } }));
            }
            catch (Exception e)
            {
                return await Task.FromResult((Data?)new Data(new Dictionary<string, string> { { "Error", $"unable to convert to audio file '{e.Message}'" } }));
            }
        }
    }
}