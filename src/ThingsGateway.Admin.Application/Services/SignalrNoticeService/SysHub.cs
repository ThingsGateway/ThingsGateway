//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Core.Extension;

namespace ThingsGateway.Admin.Application;

public class SysHub : ISysHub
{
    private readonly IVerificatInfoService _verificatInfoService;

    public SysHub(IVerificatInfoService verificatInfoService)
    {
        _verificatInfoService = verificatInfoService;
    }

    #region 方法

    /// <summary>
    /// 更新cache
    /// </summary>
    /// <param name="clientId">用户id</param>
    /// <param name="verificatId">上线时的验证id</param>
    /// <param name="isConnect">上线</param>
    public void UpdateVerificat(long clientId, long verificatId = 0, bool isConnect = true)
    {
        if (clientId != 0)
        {
            //获取cache当前用户的verificat信息列表
            if (isConnect)
            {
                //获取cache中当前verificat
                var verificatInfo = _verificatInfoService.GetOne(verificatId);
                if (verificatInfo != null)
                {
                    verificatInfo.ClientIds.Add(clientId);//添加到客户端列表
                    _verificatInfoService.Update(verificatInfo);//更新Cache
                }
            }
            else
            {
                //获取当前客户端ID所在的verificat信息
                var verificatInfo = _verificatInfoService.GetOne(verificatId);
                if (verificatInfo != null)
                {
                    verificatInfo.ClientIds.RemoveWhere(it => it == clientId);//从客户端列表删除
                    _verificatInfoService.Update(verificatInfo);//更新Cache
                }
            }
        }
    }

    #endregion 方法

}
