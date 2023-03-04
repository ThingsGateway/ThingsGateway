namespace ThingsGateway.Web.Rcl.Core
{
    public class APPThemeOptions
    {
        public string Accent { get; set; }

        public string Error { get; set; }

        public string Info { get; set; }

        public string Primary { get; set; }

        public string Secondary { get; set; }

        public string Success { get; set; }

        public Dictionary<string, string> UserDefined { get; set; } = new();

        public string Warning { get; set; }

        public void SetMasaThemesOp(ThemeOptions masaOp)
        {
            masaOp.Primary = Primary; masaOp.Secondary = Secondary;
            masaOp.Accent = Accent; masaOp.Success = Success;
            masaOp.Error = Error; masaOp.Info = Info;
            masaOp.Warning = Warning;
            masaOp.UserDefined = UserDefined;
        }
    }

    public class APPThemes
    {
        //public APPThemeOptions Dark { get; set; } = new();

        public bool IsDark { get; set; }

        public LayoutPrpo LayoutPrpo { get; set; } = new();

        //public APPThemeOptions Light { get; set; } = new();

        //public void DefalutThemes()
        //{
        //    Dark.UserDefined.Add("barColor", "#1e1e1e");
        //    Dark.Accent = "#FF4081";
        //    Dark.Error = "#FF5252";
        //    Dark.Info = "#2196F3";
        //    Dark.Primary = "#2196F3";
        //    Dark.Secondary = "#424242";
        //    Dark.Success = "#4CAF50";
        //    Dark.Warning = "#FB8C00";

        //    Light.UserDefined.Add("barColor", "#fff");
        //    Light.Accent = "#82B1FF";
        //    Light.Error = "#FF5252";
        //    Light.Info = "#2196F3";
        //    Light.Primary = "#1976D2";
        //    Light.Secondary = "#424242";
        //    Light.Success = "#4CAF50";
        //    Light.Warning = "#FB8C00";
        //}
    }

    public class LayoutPrpo
    {
        public int AppBarHeight = 48;
        public int FooterBarHeight = 36;
        public int PageTabsHeight = 36;
    }
}