namespace AgentFrameworkCodeMode.Infrastructure.Skills
{
    public class QDrantConfiguration
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public bool Https { get; set; }
        public int VectorSize { get; set; }
    }
}
