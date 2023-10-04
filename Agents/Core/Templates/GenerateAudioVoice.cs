using System.Speech.Synthesis;

namespace Technologai.Templates
{
    /// <summary>
    /// Generate Audio Voice
    /// </summary>
    public class GenerateAudioVoice : Template
    {
        
        public GenerateAudioVoice()
        {
            Id = "generate_audio_voice";
            Description = "Generate Audio Voice";
            InputKeys = new[] { "voiceGender", "voiceAge", "pathToSaveAudioFile", "textToCreateAudio" };
            OutputKeys = new[] { " filename"};
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
                    return filename;
                });

                if (response != null)
                   return Data.Create(filename);

                return Data.Create("Error", $"unable to generateAudioVoice");
            }
            catch (Exception e)
            {
                return Data.Create(e);
            }
        }
    }
}