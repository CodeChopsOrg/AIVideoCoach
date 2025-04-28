using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Oqtane.Modules;
using Oqtane.Services;
using Oqtane.Models;
using Oqtane.Shared;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Azure;
using System.IO;
using System.IO.Enumeration;
using CodeChops.Module.AIVideoCoach.Services;
using CodeChops.Module.AIVideoCoach.Models;
using CodeChops.Module.AIVideoCoach;

namespace CodeChops.Module.AIVideoCoach
{
    public partial class Settings : ModuleBase
    {
        [Inject] public ISettingService SettingService { get; set; }
        [Inject] public IStringLocalizer<Settings> Localizer { get; set; }
        [Inject] NavigationManager NavigationManager { get; set; }
        [Inject] AIVideoCoachService ChatBotService { get; set; }
        [Inject] public IFileService FileService { get; set; }

        private string resourceType = "Trifoia.Module.ChatBot.Settings, Trifoia.Module.Pride.Client.Oqtane"; // for localization

        public override string Title => "ChatBot Settings";

        private SettingsViewModel _settingsVM;
        bool loading;
        public string returnURL;
        public string videoUrl;
        public string title;
        public List<string> embeddingTitles = new List<string>();


        protected override async Task OnInitializedAsync()
        {
            loading = true;

            try
            {
                var settings = await SettingService.GetSiteSettingsAsync(1);
                _settingsVM = new SettingsViewModel(SettingService, settings);
            }
            catch (Exception ex)
            {
                AddModuleMessage(ex.Message, MessageType.Error);
            }

            loading = false;
        }

        protected override async Task OnParametersSetAsync()
        {
            if (!ShouldRender()) return;

            var uri = new Uri(NavigationManager.Uri);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            returnURL = query.Get("returnUrl") ?? "/";
            PageState.ReturnUrl = returnURL;

            var embeddings = await ChatBotService.GetEmbeddedChunksAsync();

            if (embeddings.Embeddings is not null)
            {
                foreach (var embedding in embeddings.Embeddings)
                {
                    if (!embeddingTitles.Contains(embedding.Title))
                    {
                        embeddingTitles.Add(embedding.Title);
                    }
                }
            }
        }

        public async Task UpdateSettings()
        {

            Dictionary<string, string> settings = await SettingService.GetSiteSettingsAsync(1);
            _settingsVM.SetSettings(SettingService, settings);
            await SettingService.UpdateSiteSettingsAsync(settings, 1);
        }

        private async Task HandleUpload(int fileId)
        {
            try
            {
                var file = await FileService.GetFileAsync(fileId);
                // Fetch the uploaded file details using the File ID
                // Check for files in root/Rag Docs and embed their content
                string ragDocsDirectory = Path.Combine(
                    Directory.GetCurrentDirectory(),
                         $"wwwroot/Content/Tenants/{PageState.Alias.TenantId}/Sites/{PageState.Alias.SiteId}/Rag Docs"); // Tenant and Site folders

                var txtFile = Directory.GetFiles(ragDocsDirectory, file.Name);

                if (txtFile != null)
                {
                    if (file.Extension == "txt")
                    {

                        var txtEmbedding = new TextEmbedding()
                        {
                            Title = title,
                            Text = await System.IO.File.ReadAllTextAsync(txtFile[0]),
                            Embedding = null
                        };
                        if (!string.IsNullOrWhiteSpace(txtEmbedding.Text))
                        {
                            // Embed the content of the file
                            await ChatBotService.ChunkText(txtEmbedding);
                            embeddingTitles.Add(txtEmbedding.Title);
                        }
                    }

                    if (file.Extension == "vtt")
                    {

                        var vttEmbedding = new TextEmbedding()
                        {
                            Title = title,
                            VideoUrl = videoUrl,
                            Text = await System.IO.File.ReadAllTextAsync(txtFile[0]),
                            Embedding = null
                        };

                        if (!string.IsNullOrWhiteSpace(vttEmbedding.Text))
                        {
                            // Embed the content of the file
                            await ChatBotService.ChunkTextFromVttContentAsync(vttEmbedding);
                            embeddingTitles.Add(vttEmbedding.Title);

                        }
                    }
                    else
                    {
                        await logger.LogError("Error: Uploaded file not found.");
                    }
                }
                StateHasChanged();

            }
            catch (Exception ex)
            {
                await logger.LogError($"Error retrieving uploaded file details: {ex.Message}");
            }
        }



        public async Task OnDeleteEmbedding(string title)
        {
            try
            {
                await ChatBotService.DeleteEmbeddingsAsync(title);
                embeddingTitles.Remove(title);
            }
            catch (Exception ex)
            {
                AddModuleMessage(ex.Message, MessageType.Error);
            }

            StateHasChanged();
        }
    }
}
