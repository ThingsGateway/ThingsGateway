#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

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