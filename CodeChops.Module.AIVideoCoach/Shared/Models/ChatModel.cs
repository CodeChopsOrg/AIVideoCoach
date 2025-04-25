using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trifoia.Module.AIVideoCoach.Models
{
    /// <summary>
    /// Chat with history.
    /// </summary>
    /// <param name="Messages">The history of <see cref="Message"/>instances.</param>
    public record ChatRequest(List<Message> Messages);

    /// <summary>
    /// Image request.
    /// </summary>
    /// <param name="Filename">Original filename.</param>
    /// <param name="ImageBytes">Image content.</param>
    /// <param name="MimeType">Image type, ex: image/jpeg.</param>
    public class Message
    {
        public Message(bool isAssistant, string? context, string textContent)
        {
            IsAssistant = isAssistant;
            Context = !string.IsNullOrEmpty(context) ? context : string.Empty;
            TextContent = textContent;
        }

        public bool IsAssistant { get; private set; }
        public string Context { get; set; }
        public string TextContent { get; set; }
        public bool Streaming { get; set; }
        public string Id { get; init; } = Guid.NewGuid().ToString();
    }
}
