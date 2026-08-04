using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;

namespace AhmedRawdiBusinessPlatform.Services
{
    public class LanguageService : ILanguageService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        private static readonly Dictionary<string, string> EnDict = new(StringComparer.OrdinalIgnoreCase)
        {
            { "LangName", "English" },
            { "SwitchLang", "العربية" },
            { "SwitchLangCulture", "ar-SA" },
            { "BrandTitle", "Ahmed Rawdi Platform" },
            { "BrandShort", "AR Business Platform" },
            { "SearchPlaceholder", "Search platform records..." },
            { "Core", "Core" },
            { "Dashboard", "Dashboard" },
            { "BusinessModules", "Business Modules" },
            { "SystemUsers", "System Users" },
            { "SystemGroups", "System Groups" },
            { "BusinessAnalytics", "Business Analytics" },
            { "SystemAndData", "System & Data" },
            { "SqlServer", "SQL Server (ARBP)" },
            { "PrivacyAndSecurity", "Privacy & Security" },
            { "LoggedInAs", "Logged in as:" },
            { "WelcomeBack", "Welcome back" },
            { "BusinessPlatformDashboard", "Business Platform Dashboard" },
            { "DashboardSubtitle", "Overview of SQL Server ARBP database & platform activity." },
            { "RefreshData", "Refresh Data" },
            { "LoggedUser", "Logged User" },
            { "SystemGroup", "System Group" },
            { "SqlDatabase", "SQL Database" },
            { "AuthStoredProc", "Auth Stored Proc" },
            { "AuthenticationActive", "Authentication Active" },
            { "ExecutedSuccessfully", "Executed Successfully" },
            { "AuthenticatedUserDetails", "Authenticated User Account Details" },
            { "ActiveSession", "Active Session" },
            { "UserInfo", "User Info" },
            { "EmailAndMobile", "Email & Mobile" },
            { "GroupAndRole", "Group / Role" },
            { "AccountStatus", "Account Status" },
            { "Active", "Active" },
            { "SystemArchitecture", "System Architecture & Integration" },
            { "Framework", "Framework" },
            { "DatabaseEngine", "Database Engine" },
            { "OrmProvider", "ORM / Data Provider" },
            { "GitHubIntegration", "GitHub Integration" },
            { "SignIn", "Sign In" },
            { "SignInSubtitle", "Please enter your credentials to access the business platform." },
            { "SignOut", "Sign Out" },
            { "MyProfile", "My Profile" },
            { "AccountSettings", "Account Settings" },
            { "UserCode", "User Code" },
            { "Password", "Password" },
            { "RememberMe", "Keep me signed in" },
            { "EnterpriseBadge", "Enterprise ERP & Business Intelligence" },
            { "EmpoweringHeadline", "Empowering Business with Precision & Insight" },
            { "EmpoweringDesc", "Streamlined workflow management, real-time analytics, and secure centralized access tailored for modern business execution." },
            { "SecureAccess", "Secure Access" },
            { "RoleBasedAuth", "Role-based SQL procedure auth" },
            { "RealTimeData", "Real-Time Data" },
            { "HighPerformanceAnalytics", "High performance analytics" },
            { "EncryptedBadge", "256-Bit Encrypted • Enterprise Authentication" },
            { "OnePlatform", "One platform." },
            { "ClearerDecisions", "Clearer decisions." },
            { "Secure", "Secure" },
            { "RoleBasedAccess", "Role-based access" },
            { "Live", "Live" },
            { "BusinessIntelligence", "Business intelligence" },
            { "Unified", "Unified" },
            { "CentralizedWorkflows", "Centralized workflows" },
            { "PrecisionQuote", "Precision in operations creates confidence in every decision." },
            { "SecureWorkspace", "Secure workspace" },
            { "CaseSensitive", "Case sensitive" },
            { "UserCodePlaceholder", "Enter your user code" },
            { "PasswordPlaceholder", "Enter your password" },
            { "ShowPassword", "Show password" },
            { "HidePassword", "Hide password" },
            { "Copyright", "Copyright" },
            { "PrivacyPolicy", "Privacy Policy" },
            { "TermsAndConditions", "Terms & Conditions" }
        };

        private static readonly Dictionary<string, string> ArDict = new(StringComparer.OrdinalIgnoreCase)
        {
            { "LangName", "العربية" },
            { "SwitchLang", "English" },
            { "SwitchLangCulture", "en-US" },
            { "BrandTitle", "منصة أحمد روضي للأعمال" },
            { "BrandShort", "منصة أحمد روضي للأعمال" },
            { "SearchPlaceholder", "البحث في سجلات المنصة..." },
            { "Core", "الرئيسية" },
            { "Dashboard", "لوحة التحكم" },
            { "BusinessModules", "وحدات الأعمال" },
            { "SystemUsers", "مستخدمو النظام" },
            { "SystemGroups", "مجموعات النظام" },
            { "BusinessAnalytics", "تحليلات الأعمال" },
            { "SystemAndData", "النظام والبيانات" },
            { "SqlServer", "خادم البيانات (ARBP)" },
            { "PrivacyAndSecurity", "الخصوصية والأمان" },
            { "LoggedInAs", "مسجل الدخول باسم:" },
            { "WelcomeBack", "أهلاً وسهلاً بك" },
            { "BusinessPlatformDashboard", "لوحة تحكم منصة الأعمال" },
            { "DashboardSubtitle", "نظرة عامة على قاعدة بيانات SQL Server ونشاط المنصة." },
            { "RefreshData", "تحديث البيانات" },
            { "LoggedUser", "المستخدم الحالي" },
            { "SystemGroup", "مجموعة النظام" },
            { "SqlDatabase", "قاعدة البيانات" },
            { "AuthStoredProc", "الإجراء المخزن" },
            { "AuthenticationActive", "التوثيق نشط" },
            { "ExecutedSuccessfully", "تم التنفيذ بنجاح" },
            { "AuthenticatedUserDetails", "تفاصيل حساب المستخدم الموثق" },
            { "ActiveSession", "جلسة نشطة" },
            { "UserInfo", "معلومات المستخدم" },
            { "EmailAndMobile", "البريد والجوّال" },
            { "GroupAndRole", "المجموعة / الصلاحية" },
            { "AccountStatus", "حالة الحساب" },
            { "Active", "نشط" },
            { "SystemArchitecture", "بنية ونظام التكامل" },
            { "Framework", "إطار العمل" },
            { "DatabaseEngine", "محرك قاعدة البيانات" },
            { "OrmProvider", "مزود البيانات" },
            { "GitHubIntegration", "تكامل جيت هاب" },
            { "SignIn", "تسجيل الدخول" },
            { "SignInSubtitle", "يرجى إدخال بيانات الدخول الخاصة بك للوصول إلى منصة الأعمال." },
            { "SignOut", "تسجيل الخروج" },
            { "MyProfile", "ملفي الشخصي" },
            { "AccountSettings", "إعدادات الحساب" },
            { "UserCode", "كود المستخدم" },
            { "Password", "كلمة المرور" },
            { "RememberMe", "تذكر بيانات دخولي" },
            { "EnterpriseBadge", "نظام تخطيط الموارد وذكاء الأعمال" },
            { "EmpoweringHeadline", "تمكين الأعمال بدقة ورؤية مستقبليّة" },
            { "EmpoweringDesc", "إدارة سلسة لسير العمل، تحليلات في الوقت الفعلي، ووصول آمن وممركز مصمم لتنفيذ الأعمال الحديثة." },
            { "SecureAccess", "وصول آمن" },
            { "RoleBasedAuth", "توثيق إجراءات SQL على مستوى الأدوار" },
            { "RealTimeData", "بيانات فورية" },
            { "HighPerformanceAnalytics", "تحليلات عالية الأداء" },
            { "EncryptedBadge", "تشفير 256 بت • توثيق للمؤسسات" },
            { "OnePlatform", "منصة واحدة." },
            { "ClearerDecisions", "قرارات أكثر وضوحاً." },
            { "Secure", "آمنة" },
            { "RoleBasedAccess", "وصول حسب الصلاحيات" },
            { "Live", "فورية" },
            { "BusinessIntelligence", "ذكاء الأعمال" },
            { "Unified", "موحّدة" },
            { "CentralizedWorkflows", "سير عمل مركزي" },
            { "PrecisionQuote", "الدقة في العمليات تمنح الثقة في كل قرار." },
            { "SecureWorkspace", "مساحة عمل آمنة" },
            { "CaseSensitive", "حساسة لحالة الأحرف" },
            { "UserCodePlaceholder", "أدخل كود المستخدم" },
            { "PasswordPlaceholder", "أدخل كلمة المرور" },
            { "ShowPassword", "إظهار كلمة المرور" },
            { "HidePassword", "إخفاء كلمة المرور" },
            { "Copyright", "جميع الحقوق محفوظة" },
            { "PrivacyPolicy", "سياسة الخصوصية" },
            { "TermsAndConditions", "الشروط والأحكام" }
        };

        public LanguageService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string CurrentCulture
        {
            get
            {
                var context = _httpContextAccessor.HttpContext;
                if (context != null)
                {
                    var feature = context.Features.Get<IRequestCultureFeature>();
                    if (feature != null)
                    {
                        return feature.RequestCulture.Culture.Name;
                    }
                }
                return CultureInfo.CurrentUICulture.Name;
            }
        }

        public bool IsRightToLeft => CurrentCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase);

        public string Get(string key)
        {
            var dict = IsRightToLeft ? ArDict : EnDict;
            if (dict.TryGetValue(key, out var val))
            {
                return val;
            }
            return key;
        }

        public void SetCulture(string culture)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null)
            {
                context.Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                    new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true, SameSite = SameSiteMode.Lax }
                );
            }
        }
    }
}
