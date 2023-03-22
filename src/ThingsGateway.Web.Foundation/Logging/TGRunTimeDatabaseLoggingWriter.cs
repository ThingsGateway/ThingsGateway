using Microsoft.Extensions.Logging;

using ThingsGateway.Core;

using TouchSocket.Core;

namespace ThingsGateway.Web.Foundation
{
    /// <summary>
    /// 数据库写入器
    /// </summary>
    public class TGRunTimeDatabaseLoggingWriter : IDatabaseLoggingWriter
    {
        private readonly SqlSugarScope _db;

        public TGRunTimeDatabaseLoggingWriter()
        {
            _db = DbContext.Db;

            Task.Factory.StartNew(LogInsert);
        }
        private IntelligentConcurrentQueue<RuntimeLog> _logQueues = new(10000);

        private async Task LogInsert()
        {
            var db = _db.CopyNew();
            while (true)
            {
                var data = _logQueues.ToListWithDequeue(10000);
                db.InsertableWithAttr(data).ExecuteCommand();//入库
                await Task.Delay(3000);
            }
        }
        public void Write(LogMessage logMsg, bool flush)
        {
            var customLevel = App.GetConfig<LogLevel?>("Logging:LogLevel:RunTimeLogCustom") ?? LogLevel.Trace;
            if (logMsg.LogLevel >= customLevel)
            {
                var logRuntime = new RuntimeLog
                {
                    LogLevel = logMsg.LogLevel,
                    LogMessage = logMsg.State.ToString(),
                    LogSource = logMsg.LogName,
                    LogTime = logMsg.LogDateTime,
                    Exception = logMsg.Exception?.ToString(),
                };
                //_db.InsertableWithAttr(logRuntime).ExecuteCommand();//入库
                _logQueues.Enqueue(logRuntime);
            }


        }

    }
}