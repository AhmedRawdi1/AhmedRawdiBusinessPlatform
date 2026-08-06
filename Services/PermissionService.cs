using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AhmedRawdiBusinessPlatform.Data;
using AhmedRawdiBusinessPlatform.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AhmedRawdiBusinessPlatform.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILanguageService _languageService;

        public PermissionService(ApplicationDbContext context, ILanguageService languageService)
        {
            _context = context;
            _languageService = languageService;
        }

        public async Task<List<UserPermissionDto>> GetUserPermissionsAsync(long? userId, long? groupId = null)
        {
            if (!userId.HasValue && !groupId.HasValue)
            {
                return new List<UserPermissionDto>();
            }

            var groupParam = new SqlParameter("@GroupID", SqlDbType.BigInt)
            {
                Value = (object?)groupId ?? DBNull.Value
            };

            var userParam = new SqlParameter("@UserID", SqlDbType.BigInt)
            {
                Value = (object?)userId ?? DBNull.Value
            };

            try
            {
                var result = await _context.Database
                    .SqlQueryRaw<UserPermissionDto>("EXEC dbo.usp_Get_UserPermissions @GroupID = @GroupID, @UserID = @UserID", groupParam, userParam)
                    .ToListAsync();

                return result ?? new List<UserPermissionDto>();
            }
            catch (Exception)
            {
                // In case of error (e.g. database connection issue), safely return empty list
                return new List<UserPermissionDto>();
            }
        }

        public async Task<NavigationMenuViewModel> GetNavigationMenuAsync(long? userId, long? groupId = null)
        {
            var permissions = await GetUserPermissionsAsync(userId, groupId);
            var isRtl = _languageService.IsRightToLeft;

            var viewModel = new NavigationMenuViewModel();

            if (permissions == null || !permissions.Any())
            {
                return viewModel;
            }

            // Group by Module -> SubModule -> Form
            var moduleGroups = permissions.GroupBy(p => p.ModuleID);

            foreach (var moduleGroup in moduleGroups)
            {
                var firstMod = moduleGroup.First();
                var moduleName = isRtl
                    ? (!string.IsNullOrWhiteSpace(firstMod.ModuleArabicName) ? firstMod.ModuleArabicName : firstMod.ModuleEnglishName)
                    : (!string.IsNullOrWhiteSpace(firstMod.ModuleEnglishName) ? firstMod.ModuleEnglishName : firstMod.ModuleArabicName);

                var moduleItem = new ModuleMenuItem
                {
                    ModuleID = firstMod.ModuleID,
                    ModuleCode = firstMod.ModuleCode,
                    Name = moduleName ?? firstMod.ModuleCode,
                    EnglishName = firstMod.ModuleEnglishName,
                    ArabicName = firstMod.ModuleArabicName,
                    IconClass = GetModuleIcon(firstMod.ModuleCode)
                };

                var subModuleGroups = moduleGroup.GroupBy(p => p.SubModuleID);
                foreach (var subGroup in subModuleGroups)
                {
                    var firstSub = subGroup.First();
                    var subName = isRtl
                        ? (!string.IsNullOrWhiteSpace(firstSub.SubModuleArabicName) ? firstSub.SubModuleArabicName : firstSub.SubModuleEnglishName)
                        : (!string.IsNullOrWhiteSpace(firstSub.SubModuleEnglishName) ? firstSub.SubModuleEnglishName : firstSub.SubModuleArabicName);

                    var subItem = new SubModuleMenuItem
                    {
                        SubModuleID = firstSub.SubModuleID,
                        SubModuleCode = firstSub.SubModuleCode,
                        Name = subName ?? firstSub.SubModuleCode,
                        EnglishName = firstSub.SubModuleEnglishName,
                        ArabicName = firstSub.SubModuleArabicName,
                        IconClass = GetSubModuleIcon(firstSub.SubModuleCode)
                    };

                    foreach (var form in subGroup)
                    {
                        var formName = isRtl
                            ? (!string.IsNullOrWhiteSpace(form.FormArabicName) ? form.FormArabicName : form.FormEnglishName)
                            : (!string.IsNullOrWhiteSpace(form.FormEnglishName) ? form.FormEnglishName : form.FormArabicName);

                        var formItem = new FormMenuItem
                        {
                            FormID = form.FormID,
                            FormCode = form.FormCode,
                            Name = formName ?? form.FormCode,
                            EnglishName = form.FormEnglishName,
                            ArabicName = form.FormArabicName,
                            IconClass = GetFormIcon(form.FormCode),
                            Url = GetFormUrl(form.FormCode),
                            CanSave = form.CanSave,
                            CanUpdate = form.CanUpdate,
                            CanDelete = form.CanDelete,
                            CanSearch = form.CanSearch,
                            CanPrint = form.CanPrint
                        };

                        subItem.Forms.Add(formItem);
                    }

                    moduleItem.SubModules.Add(subItem);
                }

                viewModel.Modules.Add(moduleItem);
            }

            return viewModel;
        }

        private static string GetModuleIcon(string moduleCode)
        {
            return moduleCode?.ToUpperInvariant() switch
            {
                "HIS" => "bi-hospital-fill",
                "FIN" or "ACC" => "bi-cash-coin",
                "INV" or "STK" => "bi-box-seam-fill",
                "HR" or "HRM" => "bi-people-fill",
                "REP" or "ANA" => "bi-pie-chart-fill",
                "SYS" or "ADM" => "bi-gear-wide-connected",
                _ => "bi-grid-1x2-fill"
            };
        }

        private static string GetSubModuleIcon(string subCode)
        {
            return subCode switch
            {
                "101" => "bi-person-vcard",
                "102" => "bi-shield-shaded",
                "103" => "bi-receipt-cutoff",
                "104" => "bi-journal-medical",
                "105" => "bi-calendar-event",
                "106" => "bi-person-badge-fill",
                "107" => "bi-activity",
                "108" => "bi-droplet-half",
                "109" => "bi-display",
                _ => "bi-folder2-open"
            };
        }

        private static string GetFormIcon(string formCode)
        {
            return formCode switch
            {
                var f when f.Contains("UserGroupsManagement", StringComparison.OrdinalIgnoreCase) => "bi-people-fill",
                var f when f.Contains("UsersManagement", StringComparison.OrdinalIgnoreCase) || f.Contains("SystemUsers", StringComparison.OrdinalIgnoreCase) => "bi-person-gear",
                var f when f.Contains("Patient", StringComparison.OrdinalIgnoreCase) => "bi-person-fill",
                var f when f.Contains("Lab", StringComparison.OrdinalIgnoreCase) => "bi-eyedropper",
                var f when f.Contains("RIS", StringComparison.OrdinalIgnoreCase) => "bi-cpu-fill",
                var f when f.Contains("Nursing", StringComparison.OrdinalIgnoreCase) => "bi-heart-pulse-fill",
                var f when f.Contains("Doctor", StringComparison.OrdinalIgnoreCase) => "bi-stethoscope",
                var f when f.Contains("Appointment", StringComparison.OrdinalIgnoreCase) => "bi-clock-history",
                var f when f.Contains("Invoice", StringComparison.OrdinalIgnoreCase) => "bi-file-earmark-spreadsheet-fill",
                _ => "bi-file-earmark-text"
            };
        }

        private static string GetFormUrl(string formCode)
        {
            return formCode?.Trim() switch
            {
                var code when code?.Contains("UserGroupsManagement", StringComparison.OrdinalIgnoreCase) == true
                    => "/Administration/UserGroups",
                var code when code?.Contains("UsersManagement", StringComparison.OrdinalIgnoreCase) == true
                           || code?.Contains("SystemUsers", StringComparison.OrdinalIgnoreCase) == true
                           || code?.Equals("101") == true
                    => "/Administration/Users",
                _ => "javascript:void(0);"
            };
        }
    }
}

