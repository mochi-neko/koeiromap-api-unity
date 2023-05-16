#nullable enable
using System.IO;

namespace Mochineko.KoeiromapAPI
{
    public readonly struct SpeechSynthesisResult
    {
        public readonly Stream Audio;
        public readonly string[] Phonemes;

        public SpeechSynthesisResult(Stream audio, string[] phonemes)
        {
            Audio = audio;
            Phonemes = phonemes;
        }
    }
}