//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Debug;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Json.Extension;

using TouchSocket.Core;

/// <summary>
/// 调试UI
/// </summary>
public abstract class AdapterDebugBase : ComponentBase, IDisposable
{
    /// <inheritdoc/>
    ~AdapterDebugBase()
    {
        this.SafeDispose();
    }

    /// <summary>
    /// 长度
    /// </summary>
    public int ArrayLength { get; set; } = 1;

    /// <summary>
    /// 默认读写设备
    /// </summary>
    [Parameter]
    public IProtocol Plc { get; set; }

    /// <summary>
    /// 变量地址
    /// </summary>
    public string RegisterAddress { get; set; } = "400001";

    /// <summary>
    /// 写入值
    /// </summary>
    public string WriteValue { get; set; }

    /// <summary>
    /// 数据类型
    /// </summary>
    protected DataTypeEnum DataType { get; set; } = DataTypeEnum.Int16;

    [Inject]
    private IStringLocalizer<AdapterDebugBase> Localizer { get; set; }

    /// <inheritdoc/>
    public void Dispose()
    {
        Plc?.SafeDispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public virtual async Task ReadAsync()
    {
        if (Plc != null)
        {
            try
            {
                var data = await Plc.ReadAsync(RegisterAddress, ArrayLength, DataType);
                if (data.IsSuccess)
                {
                    Plc.Logger?.LogInformation(data.Content.ToJsonNetString());
                }
                else
                {
                    Plc.Logger?.Warning(data.ToString());
                }
            }
            catch (Exception ex)
            {
                Plc.Logger?.Exception(ex);
            }
        }
    }

    /// <inheritdoc/>
    public virtual async Task WriteAsync()
    {
        if (Plc != null)
        {
            try
            {
                var data = await Plc.WriteAsync(RegisterAddress, WriteValue.GetJTokenFromString(), DataType);
                if (data.IsSuccess)
                {
                    Plc.Logger?.LogInformation($" {WriteValue.GetJTokenFromString()} {Localizer["WriteSuccess"]}");
                }
                else
                {
                    Plc.Logger?.Warning(data.ToString());
                }
            }
            catch (Exception ex)
            {
                Plc.Logger?.Exception(ex);
            }
        }
    }
}
