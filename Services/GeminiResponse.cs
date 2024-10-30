using Google.Cloud.AIPlatform.V1;

namespace blazor_server_wind.Services
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<List<Root>>(myJsonResponse);
    public class Candidate
    {
        public Content content { get; set; }
        public List<SafetyRating> safetyRatings { get; set; }
        public string finishReason { get; set; }
    }

    public class Content
    {
        public string role { get; set; }
        public List<Part> parts { get; set; }
    }

    public class Part
    {
        public string text { get; set; }
    }

    public class Root
    {
        public List<Candidate> candidates { get; set; }
        public UsageMetadata usageMetadata { get; set; }
    }

    public class SafetyRating
    {
        public string category { get; set; }
        public string probability { get; set; }
        public double probabilityScore { get; set; }
        public string severity { get; set; }
        public double severityScore { get; set; }
    }

    public class UsageMetadata
    {
        public int promptTokenCount { get; set; }
        public int candidatesTokenCount { get; set; }
        public int totalTokenCount { get; set; }
    }

}
