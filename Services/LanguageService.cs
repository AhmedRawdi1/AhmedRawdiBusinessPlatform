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
            { "AccessConfiguration", "Access configuration" },
            { "GroupAccessWorkspace", "Group access and membership" },
            { "GroupScopedSettings", "Selected group settings" },
            { "FormsPermissions", "Forms Permissions" },
            { "ReportsPermissions", "Reports Permissions" },
            { "GroupMembers", "Group Members" },
            { "SelectGroupToManageAccess", "Select a saved group to continue" },
            { "FormsPermissionsHint", "Review and configure access to system modules, submodules, and forms." },
            { "ReportsPermissionsHint", "Control which reports the selected group can view, export, and print." },
            { "GroupMembersHint", "View and manage the users assigned to the selected group." },
            { "NewGroupPermissions", "New group form permissions" },
            { "SelectedGroupPermissions", "Selected group form permissions" },
            { "AllFormsDefaultHint", "All system forms are loaded with permissions disabled by default." },
            { "ExistingPermissionsHint", "Existing permissions and unassigned system forms are shown together." },
            { "Forms", "forms" },
            { "LoadingFormsPermissions", "Loading system forms and permissions..." },
            { "FormsPermissionsLoadError", "Unable to load forms permissions from the database." },
            { "SystemForm", "System form" },
            { "CanView", "Can View" },
            { "CanSave", "Can Save" },
            { "CanDelete", "Can Delete" },
            { "CanSearch", "Can Search" },
            { "CanPrint", "Can Print" },
            { "NoSystemFormsFound", "No system forms found" },
            { "EnabledPermissions", "permissions enabled" },
            { "FilterSystemForms", "Find a form, module, or code..." },
            { "PermissionReadingGuide", "A blue check means this group has the permission." },
            { "DeleteGroupNoSelectionTitle", "Unable to delete group" },
            { "DeleteGroupNoSelectionMessage", "Select a group from Search before using Delete." },
            { "DeleteGroupConfirmTitle", "Delete selected group?" },
            { "DeleteGroupConfirmMessage", "This group will be marked inactive. You can still find it in the group directory." },
            { "DeleteGroupNotFoundMessage", "The selected group no longer exists. Refresh the group directory and try again." },
            { "DeleteGroupFailedMessage", "The group could not be deleted. Please try again or contact your system administrator." },
            { "Cancel", "Cancel" },
            { "OK", "OK" },
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
            { "OwnerCopyright", "Ahmed Rawdi. All rights reserved." },
            { "OperationalWorkspace", "Workspace online" },
            { "CapsLockOn", "Caps Lock is on" },
            { "SigningIn", "Signing in..." },
            { "EncryptedSession", "Encrypted session" },
            { "AuthorizedAccessOnly", "Authorized access only" },
            { "LightMode", "Use light theme" },
            { "DarkMode", "Use dark theme" },
            { "SecureWorkspace", "Secure workspace" },
            { "CaseSensitive", "Case sensitive" },
            { "UserCodePlaceholder", "Enter your user code" },
            { "PasswordPlaceholder", "Enter your password" },
            { "ShowPassword", "Show password" },
            { "HidePassword", "Hide password" },
            { "Copyright", "Copyright" },
            { "PrivacyPolicy", "Privacy Policy" },
            { "TermsAndConditions", "Terms & Conditions" }
            ,{ "UsersGroupsManagement", "Users Groups Management" }
            ,{ "UsersGroupsSubtitle", "Create, organize, and maintain user access groups from one secure workspace." }
            ,{ "Administration", "Administration" }
            ,{ "AccessControl", "Access Control" }
            ,{ "PageActions", "Page actions" }
            ,{ "New", "New" }
            ,{ "Save", "Save" }
            ,{ "Delete", "Delete" }
            ,{ "Search", "Search" }
            ,{ "Export", "Export" }
            ,{ "Import", "Import" }
            ,{ "Print", "Print" }
            ,{ "GroupRecord", "Group record" }
            ,{ "GroupDetails", "Group details" }
            ,{ "NewRecord", "New record" }
            ,{ "GroupCode", "Group code" }
            ,{ "GroupCodePlaceholder", "e.g. FINANCE_ADMIN" }
            ,{ "EnglishName", "English name" }
            ,{ "ArabicName", "Arabic name" }
            ,{ "Description", "Description" }
            ,{ "DescriptionPlaceholder", "Describe this group's responsibilities and access scope..." }
            ,{ "ActiveGroup", "Active group" }
            ,{ "ActiveGroupHint", "Members can use permissions assigned to this group." }
            ,{ "Directory", "Directory" }
            ,{ "UserGroups", "User groups" }
            ,{ "Records", "records" }
            ,{ "SearchGroupsPlaceholder", "Search by group code or name..." }
            ,{ "NoGroupsLoaded", "No groups loaded" }
            ,{ "NoGroupsLoadedHint", "Use Search to load existing groups or create a new group." }
            ,{ "CreateFirstGroup", "Create new group" }
            ,{ "LoadingGroups", "Loading user groups..." }
            ,{ "GroupsLoadError", "Unable to load user groups from the database." }
            ,{ "Retry", "Retry" }
            ,{ "Status", "Status" }
            ,{ "Registered", "Registered" }
            ,{ "Inactive", "Inactive" }
            ,{ "NoGroupsFound", "No groups found" }
            ,{ "NoGroupsFoundHint", "Try another search term or create a new group." }
            ,{ "UsersManagement", "Users Management" }
            ,{ "UsersSubtitle", "Create, organize, and maintain system user accounts from one secure workspace." }
            ,{ "UserRecord", "User record" }
            ,{ "UserDetails", "User details" }
            ,{ "AssignedGroup", "Assigned group" }
            ,{ "SelectGroup", "Select group..." }
            ,{ "EmailAddress", "Email address" }
            ,{ "EmailPlaceholder", "e.g. user@domain.com" }
            ,{ "MobileNumber", "Mobile number" }
            ,{ "MobileNumPlaceholder", "e.g. +966500000000" }
            ,{ "AccountExpiration", "Account expiration" }
            ,{ "ActiveUser", "Active user" }
            ,{ "ActiveUserHint", "Active users can log in and perform actions based on their group permissions." }
            ,{ "UserConfiguration", "User Account Scope" }
            ,{ "UserAccessWorkspace", "User Profile & Permissions" }
            ,{ "UserScopedSettings", "User-level settings" }
            ,{ "UserInformation", "User Profile" }
            ,{ "AssignedGroupPermissions", "Group Permissions" }
            ,{ "UserDirectory", "System Users Directory" }
            ,{ "SearchUsersPlaceholder", "Search by code, name, email, or group..." }
            ,{ "LoadingUsers", "Loading system users..." }
            ,{ "UsersLoadError", "Unable to load system users from the database." }
            ,{ "NoUsersFound", "No users found" }
            ,{ "NoUsersFoundHint", "Try another search term or create a new system user." }
            ,{ "CreateFirstUser", "Create new user" }
            ,{ "DeleteUserNoSelectionTitle", "No User Selected" }
            ,{ "DeleteUserNoSelectionMessage", "Please select a user from the directory before attempting deletion." }
            ,{ "DeleteUserConfirmTitle", "Deactivate User" }
            ,{ "DeleteUserConfirmMessage", "Are you sure you want to deactivate this user? This action can be reversed by editing the user." }
            ,{ "DeleteUserNotFoundMessage", "The selected user could not be found in the database." }
            ,{ "DeleteUserFailedMessage", "Failed to deactivate user. Please check database connectivity and try again." }
            ,{ "SaveUserSuccess", "User saved successfully." }
            ,{ "SaveUserFailed", "Failed to save user details." }
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
            { "AccessConfiguration", "إعدادات الوصول" },
            { "GroupAccessWorkspace", "صلاحيات المجموعة وأعضاؤها" },
            { "GroupScopedSettings", "إعدادات المجموعة المحددة" },
            { "FormsPermissions", "صلاحيات النماذج" },
            { "ReportsPermissions", "صلاحيات التقارير" },
            { "GroupMembers", "أعضاء المجموعة" },
            { "SelectGroupToManageAccess", "اختر مجموعة محفوظة للمتابعة" },
            { "FormsPermissionsHint", "راجع واضبط الوصول إلى وحدات النظام والوحدات الفرعية والنماذج." },
            { "ReportsPermissionsHint", "تحكم في التقارير التي يمكن للمجموعة المحددة عرضها وتصديرها وطباعتها." },
            { "GroupMembersHint", "اعرض وأدر المستخدمين المسندين إلى المجموعة المحددة." },
            { "NewGroupPermissions", "صلاحيات نماذج المجموعة الجديدة" },
            { "SelectedGroupPermissions", "صلاحيات نماذج المجموعة المحددة" },
            { "AllFormsDefaultHint", "تم تحميل جميع نماذج النظام بصلاحيات غير مفعلة افتراضيًا." },
            { "ExistingPermissionsHint", "تظهر الصلاحيات الحالية ونماذج النظام غير المسندة معًا." },
            { "Forms", "نموذج" },
            { "LoadingFormsPermissions", "جارٍ تحميل نماذج النظام والصلاحيات..." },
            { "FormsPermissionsLoadError", "تعذر تحميل صلاحيات النماذج من قاعدة البيانات." },
            { "SystemForm", "نموذج النظام" },
            { "CanView", "عرض" },
            { "CanSave", "حفظ" },
            { "CanDelete", "حذف" },
            { "CanSearch", "بحث" },
            { "CanPrint", "طباعة" },
            { "NoSystemFormsFound", "لا توجد نماذج نظام" },
            { "EnabledPermissions", "صلاحية مفعلة" },
            { "FilterSystemForms", "ابحث عن نموذج أو وحدة أو رمز..." },
            { "PermissionReadingGuide", "علامة الاختيار الزرقاء تعني أن الصلاحية ممنوحة للمجموعة." },
            { "DeleteGroupNoSelectionTitle", "تعذر حذف المجموعة" },
            { "DeleteGroupNoSelectionMessage", "يرجى اختيار مجموعة من البحث قبل استخدام زر الحذف." },
            { "DeleteGroupConfirmTitle", "هل تريد حذف المجموعة المحددة؟" },
            { "DeleteGroupConfirmMessage", "سيتم تعيين المجموعة كغير نشطة، وستظل ظاهرة في دليل المجموعات." },
            { "DeleteGroupNotFoundMessage", "المجموعة المحددة لم تعد موجودة. حدّث دليل المجموعات ثم حاول مرة أخرى." },
            { "DeleteGroupFailedMessage", "تعذر حذف المجموعة. حاول مرة أخرى أو تواصل مع مسؤول النظام." },
            { "Cancel", "إلغاء" },
            { "OK", "حسنًا" },
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
            { "OwnerCopyright", "أحمد روضي. جميع الحقوق محفوظة." },
            { "OperationalWorkspace", "مساحة العمل متصلة" },
            { "CapsLockOn", "مفتاح الأحرف الكبيرة مفعّل" },
            { "SigningIn", "جارٍ تسجيل الدخول..." },
            { "EncryptedSession", "جلسة مشفرة" },
            { "AuthorizedAccessOnly", "دخول المصرح لهم فقط" },
            { "LightMode", "استخدام الوضع الفاتح" },
            { "DarkMode", "استخدام الوضع الداكن" },
            { "SecureWorkspace", "مساحة عمل آمنة" },
            { "CaseSensitive", "حساسة لحالة الأحرف" },
            { "UserCodePlaceholder", "أدخل كود المستخدم" },
            { "PasswordPlaceholder", "أدخل كلمة المرور" },
            { "ShowPassword", "إظهار كلمة المرور" },
            { "HidePassword", "إخفاء كلمة المرور" },
            { "Copyright", "جميع الحقوق محفوظة" },
            { "PrivacyPolicy", "سياسة الخصوصية" },
            { "TermsAndConditions", "الشروط والأحكام" }
            ,{ "UsersGroupsManagement", "إدارة مجموعات المستخدمين" }
            ,{ "UsersGroupsSubtitle", "إنشاء مجموعات صلاحيات المستخدمين وتنظيمها وإدارتها من مساحة عمل آمنة." }
            ,{ "Administration", "إدارة النظام" }
            ,{ "AccessControl", "التحكم في الوصول" }
            ,{ "PageActions", "إجراءات الصفحة" }
            ,{ "New", "جديد" }
            ,{ "Save", "حفظ" }
            ,{ "Delete", "حذف" }
            ,{ "Search", "بحث" }
            ,{ "Export", "تصدير" }
            ,{ "Import", "استيراد" }
            ,{ "Print", "طباعة" }
            ,{ "GroupRecord", "سجل المجموعة" }
            ,{ "GroupDetails", "تفاصيل المجموعة" }
            ,{ "NewRecord", "سجل جديد" }
            ,{ "GroupCode", "كود المجموعة" }
            ,{ "GroupCodePlaceholder", "مثال: FINANCE_ADMIN" }
            ,{ "EnglishName", "الاسم بالإنجليزية" }
            ,{ "ArabicName", "الاسم بالعربية" }
            ,{ "Description", "الوصف" }
            ,{ "DescriptionPlaceholder", "صف مسؤوليات المجموعة ونطاق صلاحياتها..." }
            ,{ "ActiveGroup", "مجموعة نشطة" }
            ,{ "ActiveGroupHint", "يمكن للأعضاء استخدام الصلاحيات المعينة لهذه المجموعة." }
            ,{ "Directory", "الدليل" }
            ,{ "UserGroups", "مجموعات المستخدمين" }
            ,{ "Records", "سجلات" }
            ,{ "SearchGroupsPlaceholder", "البحث بكود المجموعة أو اسمها..." }
            ,{ "NoGroupsLoaded", "لم يتم تحميل مجموعات" }
            ,{ "NoGroupsLoadedHint", "استخدم البحث لتحميل المجموعات الحالية أو أنشئ مجموعة جديدة." }
            ,{ "CreateFirstGroup", "إنشاء مجموعة جديدة" }
            ,{ "LoadingGroups", "جارٍ تحميل مجموعات المستخدمين..." }
            ,{ "GroupsLoadError", "تعذر تحميل مجموعات المستخدمين من قاعدة البيانات." }
            ,{ "Retry", "إعادة المحاولة" }
            ,{ "Status", "الحالة" }
            ,{ "Registered", "تاريخ التسجيل" }
            ,{ "Inactive", "غير نشط" }
            ,{ "NoGroupsFound", "لم يتم العثور على مجموعات" }
            ,{ "NoGroupsFoundHint", "جرّب مصطلح بحث آخر أو أنشئ مجموعة جديدة." }
            ,{ "UsersManagement", "إدارة المستخدمين" }
            ,{ "UsersSubtitle", "إنشاء حسابات مستخدمي النظام وتنظيمها وإدارتها من مساحة عمل آمنة." }
            ,{ "UserRecord", "سجل المستخدم" }
            ,{ "UserDetails", "تفاصيل المستخدم" }
            ,{ "AssignedGroup", "المجموعة المعينة" }
            ,{ "SelectGroup", "اختر المجموعة..." }
            ,{ "EmailAddress", "البريد الإلكتروني" }
            ,{ "EmailPlaceholder", "مثال: user@domain.com" }
            ,{ "MobileNumber", "رقم الجوال" }
            ,{ "MobileNumPlaceholder", "مثال: +966500000000" }
            ,{ "AccountExpiration", "تاريخ انتهاء الحساب" }
            ,{ "ActiveUser", "مستخدم نشط" }
            ,{ "ActiveUserHint", "المستخدمون النشطون يمكنهم تسجيل الدخول واستخدام الصلاحيات المعينة لمجموعتهم." }
            ,{ "UserConfiguration", "نطاق حساب المستخدم" }
            ,{ "UserAccessWorkspace", "ملف المستخدم والصلاحيات" }
            ,{ "UserScopedSettings", "إعدادات مستوى المستخدم" }
            ,{ "UserInformation", "ملف المستخدم" }
            ,{ "AssignedGroupPermissions", "صلاحيات المجموعة" }
            ,{ "UserDirectory", "دليل مستخدمي النظام" }
            ,{ "SearchUsersPlaceholder", "البحث بكود المستخدم أو الاسم أو البريد أو المجموعة..." }
            ,{ "LoadingUsers", "جارٍ تحميل مستخدمي النظام..." }
            ,{ "UsersLoadError", "تعذر تحميل مستخدمي النظام من قاعدة البيانات." }
            ,{ "NoUsersFound", "لم يتم العثور على مستخدمين" }
            ,{ "NoUsersFoundHint", "جرّب مصطلح بحث آخر أو أنشئ مستخدم نظام جديد." }
            ,{ "CreateFirstUser", "إنشاء مستخدم جديد" }
            ,{ "DeleteUserNoSelectionTitle", "لم يتم تحديد مستخدم" }
            ,{ "DeleteUserNoSelectionMessage", "يرجى تحديد مستخدم من الدليل قبل محاولة الحذف." }
            ,{ "DeleteUserConfirmTitle", "إلغاء تفعيل المستخدم" }
            ,{ "DeleteUserConfirmMessage", "هل أنت تأكد من أنك تريد إلغاء تفعيل هذا المستخدم؟ يمكن التراجع عن هذا الإجراء تعديل المستخدم." }
            ,{ "DeleteUserNotFoundMessage", "تعذر العثور على المستخدم المحدد في قاعدة البيانات." }
            ,{ "DeleteUserFailedMessage", "فشل إلغاء تفعيل المستخدم. يرجى التحقق من الاتصال بقاعدة البيانات والمحاولة مرة أخرى." }
            ,{ "SaveUserSuccess", "تم حفظ بيانات المستخدم بنجاح." }
            ,{ "SaveUserFailed", "فشل حفظ بيانات المستخدم." }
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
