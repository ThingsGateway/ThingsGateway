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

using Newtonsoft.Json.Linq;

using Opc.Ua;

using ThingsGateway.Core;

using TouchSocket.Core;

using Yitter.IdGenerator;

namespace ThingsGateway.OPCUA
{
    public partial class OPCUAClientDebugDriverPage
    {
        private ThingsGateway.Foundation.Adapter.OPCUA.OPCUAClient _plc;
        bool IsShowImportVariableList;
        private OPCUAClientPage tcpClientPage;
        private ImportVariable importVariable { get; set; }
        public override void Dispose()
        {
            _plc.SafeDispose();
            tcpClientPage.SafeDispose();
            base.Dispose();
        }

        public override async Task Read()
        {
            if (_plc.Connected)
            {
                try
                {
                    var data = await _plc.ReadJTokenValueAsync(new string[] { Address });
                    Messages.Add((Microsoft.Extensions.Logging.LogLevel.Debug, DateTime.Now.ToDateTimeF() + data.ToJson().FormatJson()));
                    if (Messages.Count > 2500)
                    {
                        Messages.Clear();
                    }
                }
                catch (Exception ex)
                {

                    Messages.Add((Microsoft.Extensions.Logging.LogLevel.Debug, DateTime.Now.ToDateTimeF() + ex.Message));
                }

            }
            else
            {
                Messages.Add((Microsoft.Extensions.Logging.LogLevel.Debug, DateTime.Now.ToDateTimeF() + "未连接"));
                if (Messages.Count > 2500)
                {
                    Messages.Clear();
                }
            }
        }

        public override string ToString()
        {
            return nameof(OPCUAClient);
        }

        public override async Task Write()
        {
            try
            {
                if (_plc.Connected)
                {
                    var data = await _plc.WriteNodeAsync(Address, JToken.Parse(WriteValue));
                    if (data.IsSuccess)
                    {
                        Messages.Add((Microsoft.Extensions.Logging.LogLevel.Information, DateTime.Now.ToDateTimeF() + " - 写入成功"));
                    }
                    else
                    {
                        Messages.Add((Microsoft.Extensions.Logging.LogLevel.Warning, DateTime.Now.ToDateTimeF() + " - 写入失败 " + data.Message));
                    }
                }
                else
                {
                    Messages.Add((Microsoft.Extensions.Logging.LogLevel.Debug, DateTime.Now.ToDateTimeF() + "未连接"));
                    if (Messages.Count > 2500)
                    {
                        Messages.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                Messages.Add((Microsoft.Extensions.Logging.LogLevel.Error, DateTime.Now.ToDateTimeF() + " - " + "写入失败：" + ex.Message));
            }
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                tcpClientPage.LogAction = LogOut;
                //载入配置
                _plc = tcpClientPage.OPC;
                _plc.DataChangedHandler += _plc_DataChangedHandler;
                StateHasChanged();
            }

            base.OnAfterRender(firstRender);
        }

        private void _plc_DataChangedHandler((NodeId id, DataValue dataValue, Newtonsoft.Json.Linq.JToken jToken) item)
        {
            Messages.Add((Microsoft.Extensions.Logging.LogLevel.Debug, DateTime.Now.ToDateTimeF() + (item.id + ":" + item.jToken)));
            if (Messages.Count > 2500)
            {
                Messages.Clear();
            }

        }

        private void Add()
        {
            if (_plc.Connected)
                _plc.AddSubscription(YitIdHelper.NextId().ToString(), new[] { Address });
            else
            {
                Messages.Add((Microsoft.Extensions.Logging.LogLevel.Debug, DateTime.Now.ToDateTimeF() + "未连接"));
                if (Messages.Count > 2500)
                {
                    Messages.Clear();
                }
            }
        }

        private async Task DownDeviceExport()
        {
            var data = await importVariable?.GetImportVariableList();
            await DownDeviceExport(data.Item1);
            await DownDeviceExport(data.Item2);
        }
        private void Remove()
        {
            if (_plc.Connected)
                _plc.RemoveSubscription("");
            else
            {
                Messages.Add((Microsoft.Extensions.Logging.LogLevel.Debug, DateTime.Now.ToDateTimeF() + "未连接"));
                if (Messages.Count > 2500)
                {
                    Messages.Clear();
                }
            }
        }
    }
}