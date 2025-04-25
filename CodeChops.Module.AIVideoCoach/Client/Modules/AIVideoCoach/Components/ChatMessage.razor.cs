using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trifoia.Module.AIVideoCoach.Models;

namespace Trifoia.Module.AIVideoCoach
{
    public partial class ChatMessage
    {
        // Parameter to accept the state of the message, required for the component to function
        [Parameter, EditorRequired] public Message State { get; set; } = default!;

        // Optional parameter to indicate if the message is streaming
        [Parameter] public bool Streaming { get; set; }

        // Determine the CSS class for the message based on the streaming state
        private string GetClass(bool isStreaming)
        {
            return $"p-3 visible rounded-1 {(isStreaming ? "streaming" : string.Empty)}";
        }
    }
}
