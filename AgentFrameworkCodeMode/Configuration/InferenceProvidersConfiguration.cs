using System.Collections.Generic;

namespace AgentFrameworkCodeMode.Configuration
{
    public class InferenceProviderConfiguration
    {
        public string Endpoint { get; set; } = string.Empty;
        public string? ApiKey { get; set; }
    }
}
