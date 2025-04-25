using Microsoft.AspNetCore.Components;
using Oqtane.Modules;
using Oqtane.Services;
using Oqtane.Themes.Controls;
using Oqtane.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trifoia.Module.AIVideoCoach
{
    public partial class Edit : ModuleBase
    {
        [Inject] public ISettingService SettingService { get; set; }

        public bool isLoaded = false;

        public override bool UseAdminContainer => false;

        public override string Actions => "Add,Edit";

        public override string Title => "Manage ChatBot Credentials";

        SettingsViewModel _settingsVM;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var settings = await SettingService.GetSiteSettingsAsync(ModuleState.SiteId);
                _settingsVM = new SettingsViewModel(SettingService, settings);


                isLoaded = true;
            }
            catch (Exception ex)
            {
                await logger.LogError(ex, "Error getting settings", ex.Message);
                AddModuleMessage("Failed to fetch settings", MessageType.Error);
            }
        }

        private async Task SaveSettings()
        {
            try
            {
                // Save the updated settings
                var settings = await SettingService.GetSiteSettingsAsync(ModuleState.SiteId);

                _settingsVM.SetSettings(SettingService, settings);

                await SettingService.UpdateSiteSettingsAsync(settings, ModuleState.SiteId);

                await logger.LogInformation("AI Video Coach credentials saved successfully.");

                AddModuleMessage("AI Video Coach credentials successfully saved.", MessageType.Success);
            }
            catch (Exception ex)
            {
                await logger.LogError(ex, "Error saving AI Video Coach credentials: {Error}", ex.Message);
                AddModuleMessage("Failed to save AI Video Coach credentials.", MessageType.Error);
            }
        }
    }




}
