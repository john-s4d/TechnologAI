namespace Technologai.Agents.Models
{
    public class ContentSafetyLabels
    {
        public string status { get; set; }
        public List<object> results { get; set; }
        public Summary? summary { get; set; }
    }

    public class IabCategoriesResult
    {
        public string status { get; set; }
        public List<object> results { get; set; }
        public Summary? summary { get; set; }
    }

    public class TranscriptModel
    {
        public string id { get; set; }
        public string language_model { get; set; }
        public string acoustic_model { get; set; }
        public string language_code { get; set; }
        public string status { get; set; }
        public string audio_url { get; set; }
        public string text { get; set; }
        public List<Word> words { get; set; }
        public object utterances { get; set; }
        public double? confidence { get; set; }
        public int? audio_duration { get; set; }
        public bool punctuate { get; set; }
        public bool format_text { get; set; }
        public object dual_channel { get; set; }
        public object webhook_url { get; set; }
        public object webhook_status_code { get; set; }
        public bool webhook_auth { get; set; }
        public object webhook_auth_header_name { get; set; }
        public bool speed_boost { get; set; }
        public object auto_highlights_result { get; set; }
        public bool auto_highlights { get; set; }
        public object audio_start_from { get; set; }
        public object audio_end_at { get; set; }
        public List<object> word_boost { get; set; }
        public object boost_param { get; set; }
        public bool filter_profanity { get; set; }
        public bool redact_pii { get; set; }
        public bool redact_pii_audio { get; set; }
        public object redact_pii_audio_quality { get; set; }
        public object redact_pii_policies { get; set; }
        public object redact_pii_sub { get; set; }
        public bool speaker_labels { get; set; }
        public bool content_safety { get; set; }
        public bool iab_categories { get; set; }
        public ContentSafetyLabels content_safety_labels { get; set; }
        public IabCategoriesResult iab_categories_result { get; set; }
        public bool language_detection { get; set; }
        public object custom_spelling { get; set; }
        public object throttled { get; set; }
        public bool auto_chapters { get; set; }
        public bool summarization { get; set; }
        public object summary_type { get; set; }
        public object summary_model { get; set; }
        public bool custom_topics { get; set; }
        public List<object> topics { get; set; }
        public bool disfluencies { get; set; }
        public bool sentiment_analysis { get; set; }
        public object chapters { get; set; }
        public object sentiment_analysis_results { get; set; }
        public bool entity_detection { get; set; }
        public object entities { get; set; }
        public object summary { get; set; }
        public object speakers_expected { get; set; }
    }

    public class Summary
    {
    }

    public class Word
    {
        public string? text { get; set; }
        public int? start { get; set; }
        public int? end { get; set; }
        public double? confidence { get; set; }
        public object speaker { get; set; }
    }
}
