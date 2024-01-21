#region copyright

//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

#endregion

using NewLife.Configuration;

namespace ThingsGateway.Components
{
    /// <summary>
    /// BlazorAppInfoConfigs
    /// </summary>
    [Config("blazor.appinfo.json", "json")]
    public class BlazorAppInfoConfigs : Config<BlazorAppInfoConfigs>
    {
        #region Logo

        /// <summary>标题内容</summary>
        public string Title { get; set; } = "ThingsGateway";

        /// <summary>标题内容</summary>
        public string Remark { get; set; } = "边缘采集网关";

        #endregion Logo

        #region 属性

        /// <summary>页脚内容</summary>
        public string FooterString { get; set; } = $"Support By Diego";

        /// <summary>页脚跳转</summary>
        public string FooterUri { get; set; } = "https://gitee.com/diego2098/ThingsGateway";

        #endregion 属性

        #region 详情页面

        /// <summary>QQ群号</summary>
        public string QQGroupNumber { get; set; } = "605534569";

        /// <summary>QQ群跳转</summary>
        public string QQGroupUri { get; set; } = "http://qm.qq.com/cgi-bin/qm/qr?_wv=1027&k=NnBjPO-8kcNFzo_RzSbdICflb97u2O1i&authKey=V1MI3iJtpDMHc08myszP262kDykbx2Yev6ebE4Me0elTe0P0IFAmtU5l7Sy5w0jx&noverify=0&group_code=605534569";

        /// <summary>文档链接</summary>
        public string DocsUri { get; set; } = "https://diego2098.gitee.io/thingsgateway-docs/";

        #endregion 详情页面
    }
}