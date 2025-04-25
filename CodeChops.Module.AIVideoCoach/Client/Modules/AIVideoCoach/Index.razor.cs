using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using System;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Models;
using Oqtane.Modules;
using Oqtane.Shared;
using Oqtane.Services;
using M = Trifoia.Module.AIVideoCoach.Models;
using Trifoia.Module.AIVideoCoach.Services;
using Trifoia.Module.AIVideoCoach.Models;
using Microsoft.AspNetCore.Components.Web;
using System.Linq;
using Trifoia.Module.AIVideoCoach;
using Trifoia.Module.AIVideoCoach.Shared.Interfaces;
using Microsoft.Extensions.AI;

namespace Trifoia.Module.AIVideoCoach;

public partial class Index : ModuleBase
{
    [Inject] public NavigationManager NavigationManager { get; set; }
    [Inject] public IStringLocalizer<Index> Localizer { get; set; }
    [Inject] public ISettingService SettingService { get; set; }
    [Inject] public AIVideoCoachService ChatBotService { get; set; }
    // The API for communicating with the LLM.
    [Inject] internal AIVideoCoachService? ChatHandler { get; init; }
    [Inject] IChatClientFactory<IChatClient> ChatClientFactory { get; set; }

    //View
    public string moduleName;
    private string settingsUrl;
    private string returnUrl;
    private bool IsLoaded;
    private bool responding;
    private SettingsViewModel _settingsVM;
    private ElementReference form;
    private bool validated;

    // Chat history.
    readonly List<Message> messages = [];

    // What the user is typing.
    string? userMessageText;

    // Initial message for display purposes within the chat history.
    readonly Message initialMessage = new(true, string.Empty, "Hi, I'm a helpful assistant, how may I assist you?");

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var url = PageState.Uri.ToString();
            if (url.Contains("/admin/"))
            {
                var editUrl = EditUrl("Edit");
                NavigationManager.NavigateTo(editUrl);
            }
            else
            {
                IsLoaded = true;
            }
        }
        catch (Exception ex)
        {
            await logger.LogError(ex, "{Error}", ex.Message);
            AddModuleMessage(Localizer["Message.LoadError"], MessageType.Error);
        }

        // create the url for opening the module settings tab in edit mode.
        returnUrl = WebUtility.UrlEncode(PageState.Uri.AbsolutePath.ToString());
        settingsUrl = EditUrl("Settings", $"returnurl={returnUrl}&tab=ModuleSettings");
    }

    protected override async Task OnParametersSetAsync()
    {
        if (!ShouldRender()) return;
        await ChatBotService.InitializeAsync(ChatClientFactory);
        await ChatHandler.InitializeAsync(ChatClientFactory);
    }


    /// <summary>
    /// This method is used to send the user's message to the LLM. It is the main "communication loop."
    /// </summary>
    /// <returns>An asynchronous operation.</returns>
    async Task SendMessage()
    {
        if (ChatHandler is null) { return; }
        // used to prevent multiple requests from being sent at once
        responding = true;
        // clear the user's message to disable send button
        var query = userMessageText;
        userMessageText = null;

        // if they entered something
        if (!string.IsNullOrWhiteSpace(query))
        {
            //get the similarity score between the user's query and the closest context and the text chunk
            (var data, var response) = await ChatBotService.EmbedQueryAsync(query);

            // create a new message with the user's query
            var message = new Message(
                // not the assistant
                false,
                // instruct the model to use the provided context
                $"Using only this context:\n\n{data.Embedding.Text}\n\nanswer this question:\n\n{query} and do not mention the text or context in anyway. Do your best to be helpful.",
                // the user's query
                query
                );

            // add the message to the chat history
            messages.Add(message);

            // create a new request with the chat history
            ChatRequest request = new(messages);
            // show the generic message while waiting for the response
            var assistantText = "...";

            //Add the assistant message to the chat history
            Message assistantMessage = new(true, string.Empty, assistantText)
            {
                Streaming = true
            };
            messages.Add(assistantMessage);
            StateHasChanged();

            // if the similarity is too low, ask the user to rephrase the question and don't send the request
            if (data.Similarity < 0.4)
            {
                assistantText = GetRandomFailedResponse();
                assistantMessage.TextContent = assistantText;
                StateHasChanged();
            }
            // if the similarity is high enough, send the request
            else
            {

                IAsyncEnumerable<string> chunks = ChatHandler.Stream(request);

                // using streaming, the message can come in "chunks"so we'll build
                // the chunk and swap out the agent message until it's done
                await foreach (var chunk in chunks)
                {
                    if (assistantText == "...")
                    {
                        assistantText = string.Empty;
                    }

                    assistantText += chunk;
                    assistantMessage.TextContent = assistantText;
                    StateHasChanged();
                }
                if (!string.IsNullOrWhiteSpace(data.Embedding.VideoUrl))
                {
                    assistantMessage.TextContent += $"\n\n<a href=\"{data.Embedding.VideoUrl}?videoSeek={data.Embedding.StartTime}\" target=\"_blank\" style=\"color: blue; text-decoration: underline;\">For more info watch the {data.Embedding.Title} video at {data.Embedding.StartTime.ToString().Substring(0, 8)}.</a>";

                }

                assistantMessage.Streaming = false;
            }
            responding = false;
        }
    }

    public string GetRandomFailedResponse()
    {
        string[] cannedResponses =
        {
        "I don\u2019t know that. Can you try rephrasing the question?",
        "I\u2019m not sure I understand. Could you provide more details?",
        "That\u2019s a bit unclear to me. Can you elaborate?",
        "I wish I could help with that, but I might need more information.",
        "Hmm, I\u2019m not sure about that. Can you ask in another way?",
        "Sorry, I\u2019m unable to provide an answer to that right now.",
        "That\u2019s outside of my expertise. Can you ask something else?",
        "I couldn\u2019t figure that out. Perhaps try rephrasing or providing more context?",
        "I\u2019m having trouble understanding. Can you clarify?",
        "I wish I knew how to answer that. Let\u2019s try something else!"
    };

        Random random = new Random();
        int index = random.Next(cannedResponses.Length);
        return cannedResponses[index];
    }

    static bool IsSuccessStatusCode(HttpStatusCode statusCode)
    {
        return (int)statusCode >= 200 && (int)statusCode <= 299;
    }

    
}



