using System.Text.Json.Serialization;

namespace AgentFrameworkCodeMode.Models.StructuredOutputs
{
    public class RouterAgentOutput
    {
        /// <summary>
        /// The advisor type that the RouterAgent should route the request to. This will help the RouterAgent determine which specialized agent (e.g., BusinessAdvisor, SalesAdvisor, MarketingAdvisor) is best suited to handle the user's request based on the content and context of the request.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RouterAgentRequestSubject RequestSubject { get; set; }

        /// <summary>
        /// Gets or sets the explanation or justification for a decision or action.
        /// </summary>
        public string Rationale { get; set; } = string.Empty;

        public enum RouterAgentRequestSubject
        {
            PersonalAssistant,
            Documentation,
            BusinessAnalyst
        }
    }
}
