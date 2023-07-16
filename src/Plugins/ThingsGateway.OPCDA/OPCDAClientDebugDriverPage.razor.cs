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


using ThingsGateway.Core;
using ThingsGateway.Foundation.Adapter.OPCDA.Da;

using TouchSocket.Core;

using Yitter.IdGenerator;

namespace ThingsGateway.OPCDA
{
    public partial class OPCDAClientDebugDriverPage
    {
        private ThingsGateway.Foundation.Adapter.OPCDA.OPCDAClient _plc;
        bool IsShowImportVariableList;
        private OPCDAClientPage tcpClientPage;
        private ImportVariable importVariable { get; set; }

        public override void Dispose()
        {
            _plc.SafeDispose();
            tcpClientPage.SafeDispose();
            base.Dispose();
        }

        public override Task Read()
        {
            var data = _plc.ReadGroup();
            if (data.IsSuccess)
            {
            }
            else
            {
                Messages.Add((Microsoft.Extensions.Logging.LogLevel.Warning, DateTime.Now.ToDateTimeF() + " - " + data.Message));
            }

            return Task.CompletedTask;
        }

        public override string ToString()
        {
            return nameof(OPCDAClient);
        }

        public override Task Write()
        {
            try
            {
                var data = _plc.Write(Address, WriteValue);
                if (data.IsSuccess)
                {
                    Messages.Add((Microsoft.Extensions.Logging.LogLevel.Information, DateTime.Now.ToDateTimeF() + " - 写入" + data.Message));
                }
                else
                {
                    Messages.Add((Microsoft.Extensions.Logging.LogLevel.Warning, DateTime.Now.ToDateTimeF() + " - " + data.Message));
                }
            }
            catch (Exception ex)
            {
                Messages.Add((Microsoft.Extensions.Logging.LogLevel.Error, DateTime.Now.ToDateTimeF() + " - " + "写入失败：" + ex.Message));
            }

            return Task.CompletedTask;
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                tcpClientPage.LogAction = LogOut;
                tcpClientPage.ValueAction = ValueOut;
                //载入配置
                _plc = tcpClientPage.OPC;
                StateHasChanged();
            }

            base.OnAfterRender(firstRender);
        }

        private void Add()
        {
            var tags = new Dictionary<string, List<OpcItem>>();
            var tag = new OpcItem(Address);
            tags.Add(YitIdHelper.NextId().ToString(), new List<OpcItem>() { tag });
            var result = _plc.AddTags(tags);
            if (!result.IsSuccess)
            {
                Messages.Add((Microsoft.Extensions.Logging.LogLevel.Debug, DateTime.Now.ToDateTimeF() + result.Message));
            }
        }

        private async Task DownDeviceExport()
        {
            var data = importVariable?.GetImportVariableList();
            if (data != null)
            {
                await DownDeviceExport(data?.Item1);
                await DownDeviceExport(data?.Item2);
            }
        }
        private void Remove()
        {
            _plc.RemoveTags(new List<string>() { Address });
        }

        private void ValueOut(List<ItemReadResult> values)
        {
            Messages.Add((Microsoft.Extensions.Logging.LogLevel.Debug, DateTime.Now.ToDateTimeF() + Environment.NewLine + values.ToJson().FormatJson()));
            if (Messages.Count > 2500)
            {
                Messages.Clear();
            }
        }
    }
}