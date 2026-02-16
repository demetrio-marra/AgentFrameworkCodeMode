namespace AgentFrameworkCodeMode.Skills
{
    internal class SkillProvider : ISkillProvider
    {
        private const string SkillsDirectory = "Skills";
        private const string SkillFilePrefix = "Skill.";
        private const string SkillFileExtension = ".txt";

        public async Task<IEnumerable<string>> GetAvailableSkillsAsync(CancellationToken cancellationToken = default)
        {
            var skillsPath = Path.Combine(Directory.GetCurrentDirectory(), SkillsDirectory);

            if (!Directory.Exists(skillsPath))
            {
                return Enumerable.Empty<string>();
            }

            var skillFiles = Directory.GetFiles(skillsPath, $"{SkillFilePrefix}*{SkillFileExtension}");
            var skillNames = new HashSet<string>();

            foreach (var filePath in skillFiles)
            {
                var fileName = Path.GetFileName(filePath);
                var skillName = ExtractSkillName(fileName);
                
                if (!string.IsNullOrWhiteSpace(skillName))
                {
                    skillNames.Add(skillName);
                }
            }

            return await Task.FromResult(skillNames.OrderBy(s => s));
        }

        public async Task<string> GetSkillAsync(string skillName, string agentName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(skillName))
                throw new ArgumentException("Skill name cannot be null or empty.", nameof(skillName));

            if (string.IsNullOrWhiteSpace(agentName))
                throw new ArgumentException("Agent name cannot be null or empty.", nameof(agentName));

            var skillFileName = $"{SkillFilePrefix}{skillName}.{agentName}{SkillFileExtension}";
            var skillFilePath = Path.Combine(Directory.GetCurrentDirectory(), SkillsDirectory, skillFileName);

            if (!File.Exists(skillFilePath))
            {
                throw new FileNotFoundException($"Skill file not found: {skillFileName}");
            }

            return await File.ReadAllTextAsync(skillFilePath, cancellationToken);
        }

        private string ExtractSkillName(string fileName)
        {
            // Expected format: Skill.<SkillName>.<AgentName>.txt
            if (!fileName.StartsWith(SkillFilePrefix) || !fileName.EndsWith(SkillFileExtension))
            {
                return string.Empty;
            }

            var withoutPrefix = fileName.Substring(SkillFilePrefix.Length);
            var withoutExtension = withoutPrefix.Substring(0, withoutPrefix.Length - SkillFileExtension.Length);
            
            var parts = withoutExtension.Split('.');
            
            if (parts.Length < 2)
            {
                return string.Empty;
            }

            // Return the skill name (first part before agent name)
            return parts[0];
        }
    }
}
