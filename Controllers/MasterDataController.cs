using AhmedRawdiBusinessPlatform.Models;
using AhmedRawdiBusinessPlatform.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace AhmedRawdiBusinessPlatform.Controllers;

[Authorize]
[Route("Administration")]
public sealed class MasterDataController : Controller
{
    private static readonly IReadOnlyDictionary<string, MasterDataPageViewModel> Pages = BuildPages();
    private static readonly HashSet<string> LookupEntities = new(StringComparer.OrdinalIgnoreCase) { "CostCentersLevels" };
    private readonly IMasterDataService _masterDataService;
    private readonly IPermissionService _permissionService;

    public MasterDataController(IMasterDataService masterDataService, IPermissionService permissionService)
    {
        _masterDataService = masterDataService;
        _permissionService = permissionService;
    }

    [HttpGet("{entity:regex(^(Companies|Branches|CostCentersGroup|CostCentersTypes|CostCenters|Nationalities|IdentityTypes|MaritalStatuses|Gender|MedicalSpecialties|PhysiciansLevels|Physicians)$)}")]
    public async Task<IActionResult> Index(string entity)
    {
        if (!Pages.TryGetValue(entity, out var page)) return NotFound();
        if (!await IsAllowedAsync(entity, "View")) return Forbid();
        return View(page);
    }

    [HttpGet("MasterData/{entity}/Search")]
    public async Task<IActionResult> Search(string entity, string? q, CancellationToken cancellationToken)
    {
        if (!Pages.ContainsKey(entity) && !LookupEntities.Contains(entity)) return NotFound();
        if (Pages.ContainsKey(entity) && !await IsAllowedAsync(entity, "Search")) return Forbid();
        return Json(await _masterDataService.SearchAsync(entity, q, cancellationToken));
    }

    [HttpGet("MasterData/{entity}/{id:long}")]
    public async Task<IActionResult> GetRecord(string entity, long id, CancellationToken cancellationToken)
    {
        if (!Pages.ContainsKey(entity) || id <= 0) return NotFound();
        if (!await IsAllowedAsync(entity, "View")) return Forbid();
        var json = await _masterDataService.GetRecordJsonAsync(entity, id, cancellationToken);
        return json is null ? NotFound() : Content(json, "application/json");
    }

    [HttpPost("MasterData/{entity}/Save")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(string entity, [FromBody] JsonElement payload, CancellationToken cancellationToken)
    {
        if (!Pages.ContainsKey(entity) || payload.ValueKind != JsonValueKind.Object) return BadRequest(new { success = false });
        long? id = payload.TryGetProperty("ID", out var idProperty) && idProperty.TryGetInt64(out var parsedId) && parsedId > 0 ? parsedId : null;
        if (!await IsAllowedAsync(entity, id.HasValue ? "Update" : "Save")) return Forbid();
        var userId = CurrentUserId();
        try
        {
            var savedId = await _masterDataService.SaveAsync(entity, id, payload.GetRawText(), userId, cancellationToken);
            return Json(new { success = true, id = savedId });
        }
        catch (Microsoft.Data.SqlClient.SqlException exception)
        {
            return BadRequest(new { success = false, message = exception.Number is 2601 or 2627 ? "DuplicateCode" : exception.Message });
        }
    }

    [HttpPost("MasterData/{entity}/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string entity, [FromForm] long id, CancellationToken cancellationToken)
    {
        if (!Pages.ContainsKey(entity) || id <= 0) return BadRequest(new { success = false });
        if (!await IsAllowedAsync(entity, "Delete")) return Forbid();
        await _masterDataService.DeleteAsync(entity, id, CurrentUserId(), cancellationToken);
        return Json(new { success = true });
    }

    private long? CurrentUserId() => long.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
    private long? CurrentGroupId() => long.TryParse(User.FindFirstValue("GroupID"), out var id) ? id : null;
    private Task<bool> IsAllowedAsync(string entity, string permission) =>
        _permissionService.HasFormPermissionAsync(CurrentUserId(), CurrentGroupId(), $"Frm_{entity}", permission);

    private static IReadOnlyDictionary<string, MasterDataPageViewModel> BuildPages()
    {
        static MasterDataFieldViewModel F(string name, string en, string ar, bool required = false,
            string type = "text", int? max = null, bool full = false, string? enPlaceholder = null,
            string? arPlaceholder = null) => new()
            {
                Name = name, EnglishLabel = en, ArabicLabel = ar, Required = required, Type = type,
                MaxLength = max, FullWidth = full, EnglishPlaceholder = enPlaceholder, ArabicPlaceholder = arPlaceholder
            };
        static MasterDataFieldViewModel Lookup(string name, string en, string ar, string source) => new()
        {
            Name = name, EnglishLabel = en, ArabicLabel = ar, Required = true, Type = "lookup", LookupEntity = source
        };
        static MasterDataPageViewModel Page(string key, string en, string ar, string enSub, string arSub,
            string icon, params MasterDataFieldViewModel[] fields) => new()
            {
                EntityKey = key, EnglishTitle = en, ArabicTitle = ar, EnglishSubtitle = enSub,
                ArabicSubtitle = arSub, IconClass = icon, Fields = fields
            };
        static MasterDataFieldViewModel[] ReferenceFields(bool remarks = true)
        {
            var fields = new List<MasterDataFieldViewModel>
            {
                F("Code", "Code", "الكود", true, max: 100, enPlaceholder: "Enter a unique code", arPlaceholder: "أدخل كوداً فريداً"),
                F("EngName", "English name", "الاسم بالإنجليزية", true, max: 200),
                F("ArbName", "Arabic name", "الاسم بالعربية", true, max: 200)
            };
            if (remarks) fields.Add(F("Remarks", "Remarks", "ملاحظات", type: "textarea", full: true));
            return [.. fields];
        }

        var pages = new[]
        {
            Page("Companies", "Companies", "الشركات", "Maintain legal entities and their official contact and registration details.", "إدارة الكيانات القانونية وبيانات التسجيل والتواصل الرسمية.", "bi-buildings",
                F("Code", "Company code", "كود الشركة", true, max: 100), F("EngName", "English name", "الاسم بالإنجليزية", true, max: 100),
                F("ArbName", "Arabic name", "الاسم بالعربية", true, max: 100), F("CRN", "Commercial registration no.", "رقم السجل التجاري", true, max: 100),
                F("VATRN", "VAT registration no.", "الرقم الضريبي", true, max: 100), F("PhoneNo", "Phone number", "رقم الهاتف", true, "tel", 100),
                F("Email", "Email address", "البريد الإلكتروني", true, "email", 100), Lookup("LocationCountryID", "Country", "الدولة", "Nationalities"),
                F("AddressEngName", "English address", "العنوان بالإنجليزية", true, "textarea", 200, true),
                F("AddressArbName", "Arabic address", "العنوان بالعربية", true, "textarea", 200, true), F("IsActive", "Active company", "شركة نشطة", type: "checkbox", full: true)),
            Page("Branches", "Branches", "الفروع", "Organize company branches, locations and operational contact details.", "تنظيم فروع الشركات ومواقعها وبيانات التواصل التشغيلية.", "bi-diagram-3",
                Lookup("CompanyID", "Company", "الشركة", "Companies"), F("Code", "Branch code", "كود الفرع", true, max: 150),
                F("EngName", "English name", "الاسم بالإنجليزية", true, max: 200), F("ArbName", "Arabic name", "الاسم بالعربية", true, max: 200),
                F("CRN", "Commercial registration no.", "رقم السجل التجاري", max: 100), F("VATRN", "VAT registration no.", "الرقم الضريبي", max: 100),
                F("PhoneNo", "Phone number", "رقم الهاتف", type: "tel", max: 100), F("Email", "Email address", "البريد الإلكتروني", type: "email", max: 100),
                Lookup("LocationCountryID", "Country", "الدولة", "Nationalities"), F("LocationCityID", "City record ID", "معرّف المدينة", type: "number"),
                F("AddressEngName", "English address", "العنوان بالإنجليزية", type: "textarea", max: 400, full: true),
                F("AddressArbName", "Arabic address", "العنوان بالعربية", type: "textarea", max: 400, full: true), F("IsActive", "Active branch", "فرع نشط", type: "checkbox", full: true)),
            Page("CostCentersGroup", "Cost center groups", "مجموعات مراكز التكلفة", "Define the main grouping structure for financial cost centers.", "تعريف هيكل التجميع الرئيسي لمراكز التكلفة المالية.", "bi-collection", ReferenceFields()),
            Page("CostCentersTypes", "Cost center types", "أنواع مراكز التكلفة", "Classify cost centers by their operational and financial purpose.", "تصنيف مراكز التكلفة حسب الغرض التشغيلي والمالي.", "bi-tags", ReferenceFields()),
            Page("CostCenters", "Cost centers", "مراكز التكلفة", "Manage the organization’s financial allocation and reporting centers.", "إدارة مراكز التخصيص وإعداد التقارير المالية للمنشأة.", "bi-bullseye",
                Lookup("CCGID", "Cost center group", "مجموعة مركز التكلفة", "CostCentersGroup"), Lookup("CCLID", "Cost center level", "مستوى مركز التكلفة", "CostCentersLevels"),
                Lookup("CCTID", "Cost center type", "نوع مركز التكلفة", "CostCentersTypes"), F("Code", "Cost center code", "كود مركز التكلفة", true, max: 100),
                F("EngName", "English name", "الاسم بالإنجليزية", true, max: 200), F("ArbName", "Arabic name", "الاسم بالعربية", true, max: 200),
                F("Remarks", "Remarks", "ملاحظات", type: "textarea", full: true)),
            Page("Nationalities", "Nationalities", "الجنسيات", "Maintain standardized country and nationality names for patient and workforce records.", "إدارة أسماء الدول والجنسيات المعيارية لسجلات المرضى والعاملين.", "bi-globe2",
                F("Code", "Country code", "كود الدولة", true, max: 50), F("Country_EngName", "Country name (English)", "اسم الدولة بالإنجليزية", true, max: 200),
                F("Country_ArbName", "Country name (Arabic)", "اسم الدولة بالعربية", true, max: 200), F("Nat_EngName", "Nationality (English)", "الجنسية بالإنجليزية", true, max: 200),
                F("Nat_ArbName", "Nationality (Arabic)", "الجنسية بالعربية", true, max: 200)),
            Page("IdentityTypes", "Identity types", "أنواع مستندات الهوية", "Configure accepted identity documents and supporting notes.", "تهيئة أنواع مستندات الهوية المعتمدة والملاحظات المرتبطة بها.", "bi-person-vcard", ReferenceFields()),
            Page("MaritalStatuses", "Marital statuses", "الحالات الاجتماعية", "Maintain the standardized marital status reference list.", "إدارة القائمة المرجعية المعيارية للحالات الاجتماعية.", "bi-people", ReferenceFields()),
            Page("Gender", "Gender", "الجنس", "Maintain the gender reference values used across clinical and administrative records.", "إدارة القيم المرجعية للجنس المستخدمة في السجلات الطبية والإدارية.", "bi-gender-ambiguous", ReferenceFields(false)),
            Page("MedicalSpecialties", "Medical specialties", "التخصصات الطبية", "Maintain the clinical specialty catalog used for physician classification.", "إدارة دليل التخصصات الطبية المستخدم في تصنيف الأطباء.", "bi-heart-pulse", ReferenceFields()),
            Page("PhysiciansLevels", "Physician levels", "مستويات الأطباء", "Define professional grades and levels for physicians.", "تعريف الدرجات والمستويات المهنية للأطباء.", "bi-award", ReferenceFields()),
            Page("Physicians", "Physicians", "الأطباء", "Maintain physician profiles, clinical assignments and appointment parameters.", "إدارة ملفات الأطباء والتعيينات السريرية وإعدادات المواعيد.", "bi-person-badge",
                Lookup("CostCenterID", "Cost center", "مركز التكلفة", "CostCenters"), Lookup("MedicalSpecialtyID", "Medical specialty", "التخصص الطبي", "MedicalSpecialties"),
                Lookup("PhysicianLevelID", "Physician level", "مستوى الطبيب", "PhysiciansLevels"), Lookup("NationalityID", "Nationality", "الجنسية", "Nationalities"),
                F("Code", "Physician code", "كود الطبيب", true, max: 100), F("EngName", "English name", "الاسم بالإنجليزية", true, max: 200),
                F("ArbName", "Arabic name", "الاسم بالعربية", true, max: 200), F("PrimaryMobile", "Primary mobile", "رقم الجوال الأساسي", true, "tel", 11),
                F("SecondaryMobile", "Secondary mobile", "رقم الجوال الإضافي", type: "tel", max: 11), F("PrimaryEmail", "Primary email", "البريد الإلكتروني الأساسي", true, "email", 200),
                F("SecondaryEmail", "Secondary email", "البريد الإلكتروني الإضافي", type: "email", max: 200), F("FollowUpCount", "Follow-up count", "عدد المتابعات", true, "number"),
                F("FollowUpPeriod", "Follow-up period (days)", "فترة المتابعة بالأيام", true, "number"), F("ConsDuration", "Consultation duration (min)", "مدة الاستشارة بالدقائق", true, "number"),
                F("FollowUpDuration", "Follow-up duration (min)", "مدة المتابعة بالدقائق", true, "number"), F("Remarks", "Remarks", "ملاحظات", type: "textarea", full: true),
                F("IsActive", "Active physician", "طبيب نشط", type: "checkbox", full: true))
        };

        return pages.ToDictionary(page => page.EntityKey, StringComparer.OrdinalIgnoreCase);
    }
}
