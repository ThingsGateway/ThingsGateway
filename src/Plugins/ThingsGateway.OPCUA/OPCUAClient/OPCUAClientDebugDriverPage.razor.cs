#region copyright
//------------------------------------------------------------------------------
//  �˴����Ȩ����Ϊȫ�ļ����ǣ�����ԭ�����ر������������·��ֶ�����
//  �˴����Ȩ�����ر�������Ĵ��룩�����߱���Diego����
//  Դ����ʹ��Э����ѭ���ֿ�Ŀ�ԴЭ�鼰����Э��
//  GiteeԴ����ֿ⣺https://gitee.com/diego2098/ThingsGateway
//  GithubԴ����ֿ⣺https://github.com/kimdiego2098/ThingsGateway
//  ʹ���ĵ���https://diego2098.gitee.io/thingsgateway-docs/
//  QQȺ��605534569
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
                Messages.Add((Microsoft.Extensions.Logging.LogLevel.Debug, DateTime.Now.ToDateTimeF() + "δ����"));
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
                        Messages.Add((Microsoft.Extensions.Logging.LogLevel.Information, DateTime.Now.ToDateTimeF() + " - д��ɹ�"));
                    }
                    else
                    {
                        Messages.Add((Microsoft.Extensions.Logging.LogLevel.Warning, DateTime.Now.ToDateTimeF() + " - д��ʧ�� " + data.Message));
                    }
                }
                else
                {
                    Messages.Add((Microsoft.Extensions.Logging.LogLevel.Debug, DateTime.Now.ToDateTimeF() + "δ����"));
                    if (Messages.Count > 2500)
                    {
                        Messages.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                Messages.Add((Microsoft.Extensions.Logging.LogLevel.Error, DateTime.Now.ToDateTimeF() + " - " + "д��ʧ�ܣ�" + ex.Message));
            }
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                tcpClientPage.LogAction = LogOut;
                //��������
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
                Messages.Add((Microsoft.Extensions.Logging.LogLevel.Debug, DateTime.Now.ToDateTimeF() + "δ����"));
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
                Messages.Add((Microsoft.Extensions.Logging.LogLevel.Debug, DateTime.Now.ToDateTimeF() + "δ����"));
                if (Messages.Count > 2500)
                {
                    Messages.Clear();
                }
            }
        }
    }
}