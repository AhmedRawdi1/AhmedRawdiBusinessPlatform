using System.Collections.Generic;

namespace AhmedRawdiBusinessPlatform.Models
{
    public class NavigationMenuViewModel
    {
        public List<ModuleMenuItem> Modules { get; set; } = new();
    }

    public class ModuleMenuItem
    {
        public long ModuleID { get; set; }
        public string ModuleCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? EnglishName { get; set; }
        public string? ArabicName { get; set; }
        public string IconClass { get; set; } = "bi-folder2-open";
        public List<SubModuleMenuItem> SubModules { get; set; } = new();
    }

    public class SubModuleMenuItem
    {
        public long SubModuleID { get; set; }
        public string SubModuleCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? EnglishName { get; set; }
        public string? ArabicName { get; set; }
        public string IconClass { get; set; } = "bi-layers-half";
        public List<FormMenuItem> Forms { get; set; } = new();
    }

    public class FormMenuItem
    {
        public long FormID { get; set; }
        public string FormCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? EnglishName { get; set; }
        public string? ArabicName { get; set; }
        public string IconClass { get; set; } = "bi-file-earmark-text";
        public string Url { get; set; } = "#";
        public bool CanSave { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanDelete { get; set; }
        public bool CanSearch { get; set; }
        public bool CanPrint { get; set; }
    }
}
