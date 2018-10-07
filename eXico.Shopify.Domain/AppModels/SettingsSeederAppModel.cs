namespace Exico.Shopify.Data.Domain.AppModels
{
    public class SettingsSeederAppModel
    {
        public string API_KEY { get; set; } = null;
        public string SECRET_KEY { get; set; } = null;
        public string APP_BASE_URL { get; set; } = null;
        public string SHOPIFY_CONTROLLER { get; set; } = null;
        public string UNINSTALL_CONTROLLER { get; set; } = null;
        public string DASHBOARD_CONTOLLER { get; set; } = null;
        public string ACCOUNT_CONTOLLER { get; set; } = null;
        public string APP_NAME { get; set; } = null;
        public string WELCOME_EMAIL_TEMPLATE { get; set; } = null;
        public string SHOPIFY_EVENT_EMAIL_SUBSCRIBERS { get; set; } = null;
        public string SHOPIFY_EMAILS_FROM_ADDRESS { get; set; } = null;
        public string PRIVILEGED_IPS { get; set; } = null;
        public string USES_EMBEDED_SDK { get; set; } = null;
        public string APP_SUPPORT_EMAIL_ADDRESS { get; set; } = null;
        public string APP_VERSION { get; set; } = null;
        //public string SEEDER_FRAMEWORK_VERSION { get; set; } = null; /*should only entered by system*/
        public string SHOPIFY_APP_STOER_URL { get; set; } = null;
        public string MY_PROFILE_CONTOLLER { get; set; } = null;
        public string SEND_GRID_API_KEY { get; set; } = null;

    }
}
