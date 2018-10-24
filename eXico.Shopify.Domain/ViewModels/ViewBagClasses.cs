namespace Exico.Shopify.Data.Domain.ViewModels
{
    public class Versions
    {
        private string _FrameworkVersion;

        public Versions()
        {
            NugetVersion = FrameWorkVersion;
        }

        public string AppVersion { get; set; }
        
        public string FrameWorkVersion
        {
            get
            {
                return _FrameworkVersion;
            }
            set
            {
                NugetVersion = _FrameworkVersion = value;
            }
        }
        public string DataSeederFrameworkVersion { get; set; }
        public string NugetVersion { get; set; }


    }

    public class ControllerNames
    {
        public string DashboardController { get; set; }
        public string MyProfileController { get; set; }
        public string AccountController { get; set; }
        public string UninstallController { get; set; }
        public string HomeController { get; set; }
    }

    public class ViewNames
    {
        public const string SCRIPT_SECTION = "scripts";
        public const string STYLE_SECTION = "styles";
        public string ErrorPage = "XError";

        public DashboardViews Dashboard { get; set; } = new DashboardViews();
        public MetaViews Meta { get; set; } = new MetaViews();
        public MyProfileViews MyProfile { get; set; } = new MyProfileViews();
        public ShopifyViews Shopify { get; set; } = new ShopifyViews();
        public WebMsgViews WebMsg { get; set; } = new WebMsgViews();
        public NavBarViews NavBar { get; set; } = new NavBarViews();
        public AccountViews Account { get; set; } = new AccountViews();
        public LogoViews Logo { get; set; } = new LogoViews();
        public HomeViews Home { get; set; } = new HomeViews();
        public FooterViews Footer = new FooterViews();
    }

    #region Views classes
    public class HomeViews
    {
        public string Index { get; set; } = "HomeIndex";
    }
    public class LogoViews
    {
        public string LogoLoader { get; set; } = "LogoLoader";
        public string DefaultBase64Content { get; set; } = "_LogoBase64Data";
    }

    public class AccountViews
    {
        public string Login { get; set; } = "AccountLogin";
        public string LoginPartial { get; set; } = "_LoginPartial";
    }

    public class NavBarViews
    {
        public string NavBar { get; set; } = "DefaultNavBar";
        public string DefaultNavBarContent { get; set; } = "_DefaultNavBarContent";
        public string DefaultNavBarHeader { get; set; } = "_DefaultNavBarHeader";
        public string DefaultNavBarLinks { get; set; } = "_DefaultNavBarLinks";
        public string OptionalNavMenu { get; set; } = "_OptionalNavMenu";
    }

    public class DashboardViews
    {
        public string Index { get; set; } = "DashboardIndex";
        public string IndexDefaultContent { get; set; } = "_DashboardIndexDefaultContent";
        public string IndexOptionalContent { get; set; } = "_DashboardIndexOptionalContent";
        public string PlanDoesNotAllow { get; set; } = "DashboardPlanDoesNotAllow";
        public string Support { get; set; } = "DashboardSupport";
        public string SupportDefaultContent { get; set; } = "_DashboardSupportDefaultContent";
        public string SupportOptionalContent { get; set; } = "_DashboardSupportOptionalContent";
        public string SupportContactusForm { get; set; } = "_DashboardSupportContactUsForm";
        public string SupportOurAddress { get; set; } = "_DashboardSupportOurAddress";
        public string ConsiderUpgradingPlan { get; set; } = "DashboardConsiderUpgradingPlan";
    }

    public class MetaViews
    {
        public string DefaultMetaFields { get; set; } = "_DefaultMetaFields";
        public string DefaultHeaderIncludes { get; set; } = "_DefaultHeaderIncludes";
        public string DefaultAfterBodyIncludes { get; set; } = "_DefaultAfterBodyIncludes";
        public string DefaultPageTitle { get; set; } = "_DefaultPageTitle";

        public string CustomMetaFields { get; set; } = "_CustomMetaFields";
        public string CustomHeaderIncludes { get; set; } = "_CustomHeaderIncludes";
        public string CustomAfterBodyIncludes { get; set; } = "_CustomAfterBodyIncludes";


    }

    public class MyProfileViews
    {
        public string Index { get; set; } = "MyProfileIndex";
        public string IndexDefaultContent { get; set; } = "_MyProfileIndexDefaultContent";
        public string IndexOptionalContent { get; set; } = "_MyProfileIndexOptionalContent";
        public string ChangePlan { get; set; } = "MyProfileChangePlan";
    }

    public class ShopifyViews
    {
        public string ChoosePlan { get; set; } = "ShopifyChoosePlan";
        public string ChoosePlanRenderingCss { get; set; } = "_ShopifyChoosePlanRenderingCss";
        public string ChoosePlanRenderer { get; set; } = "_ShopifyChoosePlanRenderer";
    }

    public class WebMsgViews
    {
        public string DefaultWebMsgLoader { get; set; } = "DefaultWebMsgLoader";
        public string DefaultWebMsgrenderer { get; set; } = "_DefaultWebMsgRenderer";
    }

    public class FooterViews
    {
        public string DefaultFooter = "DefaultFooter";
        public string DefaultFooterContent = "_DefaultFooterContent";
    }

    #endregion
}



