using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentFrameworkCodeMode.Models.Inputs
{
    internal class ContextAnalyzerInput
    {
        public string RequestByUser { get; set; } = string.Empty;
        public IEnumerable<ChatMessage> History { get; set; } = Enumerable.Empty<ChatMessage>();

    }
}
