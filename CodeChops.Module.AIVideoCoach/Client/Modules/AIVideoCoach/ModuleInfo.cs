using Oqtane.Models;
using Oqtane.Modules;
using Oqtane.Shared;
using System.Collections.Generic;

namespace Trifoia.Module.AIVideoCoach
{
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "AIVideoCoach",
            Description = "Open source edition of the Trifoia AI Video Coach Oqtane Module",
            Version = "1.0.0",
            ServerManagerType = "Trifoia.Module.AIVideoCoach.Manager.AIVideoCoachManager, Trifoia.Module.AIVideoCoach.Server.Oqtane",
            ReleaseVersions = "1.0.0",
            Dependencies = "Trifoia.Module.AIVideoCoach.Shared.Oqtane,MudBlazor",
            PackageName = "Trifoia.AIVideoCoach",
            Resources = new List<Resource>()
            {
                new Resource { ResourceType = ResourceType.Stylesheet,  Url = "https://fonts.googleapis.com/css?family=Roboto:300,400,500,700&display=swap" },
                new Resource { ResourceType = ResourceType.Stylesheet,  Url = "_content/MudBlazor/MudBlazor.min.css" },
                new Resource { ResourceType = ResourceType.Stylesheet,  Url = "~/Module.css" },
                new Resource { ResourceType = ResourceType.Script,     Url = "_content/MudBlazor/MudBlazor.min.js", Location = ResourceLocation.Body, Level = ResourceLevel.Site },
                new Resource { ResourceType = ResourceType.Script, Url = "~/Module.js" },
            }
        };
    }
}
