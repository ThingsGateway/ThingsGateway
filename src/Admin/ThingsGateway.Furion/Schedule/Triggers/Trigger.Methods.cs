﻿// ------------------------------------------------------------------------
// 版权信息
// 版权归百小僧及百签科技（广东）有限公司所有。
// 所有权利保留。
// 官方网站：https://baiqian.com
//
// 许可证信息
// 项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。
// 许可证的完整文本可以在源代码树根目录中的 LICENSE-APACHE 和 LICENSE-MIT 文件中找到。
// ------------------------------------------------------------------------

using System.Collections.Concurrent;

using ThingsGateway.Templates;

namespace ThingsGateway.Schedule;

/// <summary>
/// 作业触发器基类
/// </summary>
public partial class Trigger
{
    /// <summary>
    /// 计算下一个触发时间
    /// </summary>
    /// <param name="startAt">起始时间</param>
    /// <returns><see cref="DateTime"/></returns>
    public virtual DateTime GetNextOccurrence(DateTime startAt) => throw new NotImplementedException();

    /// <summary>
    /// 执行条件检查
    /// </summary>
    /// <param name="jobDetail">作业信息</param>
    /// <param name="startAt">起始时间</param>
    /// <returns><see cref="bool"/></returns>
    public virtual bool ShouldRun(JobDetail jobDetail, DateTime startAt)
    {
        return NextRunTime.Value <= startAt
            && LastRunTime != NextRunTime;
    }

    /// <summary>
    /// 获取作业触发器构建器
    /// </summary>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public TriggerBuilder GetBuilder()
    {
        return TriggerBuilder.From(this);
    }

    /// <summary>
    /// 获取作业触发器最近运行时间
    /// </summary>
    /// <returns></returns>
    public IEnumerable<TriggerTimeline> GetTimelines()
    {
        return Timelines.OrderByDescending(u => u.CreatedTime).ToList();
    }

    /// <summary>
    /// 作业触发器转字符串输出
    /// </summary>
    /// <returns><see cref="string"/></returns>
    public override string ToString()
    {
        return $"<{JobId} {TriggerId}>{(string.IsNullOrWhiteSpace(Description) ? string.Empty : $" {Description.GetMaxLengthString()}")} {NumberOfRuns}ts";
    }

    /// <summary>
    /// 重置启动时最大触发次数等于一次的作业触发器
    /// </summary>
    /// <param name="startAt">起始时间</param>
    internal void ResetMaxNumberOfRunsEqualOnceOnStart(DateTime startAt)
    {
        if (StartNow
            && ResetOnlyOnce
            && MaxNumberOfRuns == 1
            && NumberOfRuns == 1)
        {
            NumberOfRuns = 0;
            SetStatus(TriggerStatus.Ready);
            NextRunTime = CheckRunOnStartAndReturnNextRunTime(startAt);

            if (MaxNumberOfErrors > 0 && NumberOfErrors >= MaxNumberOfErrors)
            {
                NumberOfErrors = MaxNumberOfErrors - 1;
            }
        }
    }

    /// <summary>
    /// 记录运行信息和计算下一个触发时间
    /// </summary>
    /// <param name="jobDetail">作业信息</param>
    /// <param name="startAt">起始时间</param>
    internal void Increment(JobDetail jobDetail, DateTime startAt)
    {
        // 阻塞状态并没有实际执行，此时忽略次数递增和最近运行时间赋值
        if (Status != TriggerStatus.Blocked)
        {
            NumberOfRuns++;
            // 处理立即执行的情况
            LastRunTime = NextRunTime == null
                ? startAt
                : (startAt < NextRunTime.Value
                    ? startAt
                    : NextRunTime);
        }

        NextRunTime = GetNextRunTime(startAt);

        // 检查下一次执行信息
        CheckAndFixNextOccurrence(jobDetail, startAt);
    }

    /// <summary>
    /// 记录错误信息，包含错误次数和运行状态
    /// </summary>
    /// <param name="jobDetail">作业信息</param>
    /// <param name="startAt">起始时间</param>
    internal void IncrementErrors(JobDetail jobDetail, DateTime startAt)
    {
        NumberOfErrors++;

        // 检查下一次执行信息
        if (CheckAndFixNextOccurrence(jobDetail, startAt)) SetStatus(TriggerStatus.ErrorToReady);
    }

    /// <summary>
    /// 计算下一次运行时间
    /// </summary>
    /// <param name="startAt">起始时间</param>
    /// <returns><see cref="DateTime"/></returns>
    internal DateTime? GetNextRunTime(DateTime startAt)
    {
        // 如果未启动或不是正常的触发器状态，则返回 null
        if (StartNow == false || (Status != TriggerStatus.Backlog
            && Status != TriggerStatus.Ready
            && Status != TriggerStatus.ErrorToReady
            && Status != TriggerStatus.Running
            && Status != TriggerStatus.Blocked)) return null;

        // 如果已经设置了 NextRunTime 且其值大于当前时间，则返回当前 NextRunTime（可能因为其他方式修改了该值导致触发时间不是精准计算的时间）
        if (NextRunTime != null && NextRunTime.Value > startAt) return NextRunTime;

        var baseTime = GetStartAt(startAt);
        if (baseTime == null) return null;

        // 获取下一次执行时间
        return GetNextOccurrence(baseTime.Value);
    }

    /// <summary>
    /// 检查是否启动时执行一次并返回下一次执行时间
    /// </summary>
    /// <param name="startAt">起始时间</param>
    /// <returns><see cref="DateTime"/></returns>
    internal DateTime? CheckRunOnStartAndReturnNextRunTime(DateTime startAt)
    {
        // 检查是否启动时需要执行
        if (!(StartNow && RunOnStart))
        {
            return GetNextRunTime(startAt);
        }
        else
        {
            // 检查是否设置了 RunOnStartProvider 配置
            if (ScheduleOptionsBuilder.InternalRunOnStartProvider is null)
            {
                return (StartTime == null || StartTime.Value <= startAt)
                    ? startAt.AddSeconds(-1)
                    : StartTime;
            }

            return ScheduleOptionsBuilder.InternalRunOnStartProvider(this, startAt);
        }
    }

    /// <summary>
    /// 设置作业触发器状态
    /// </summary>
    /// <param name="status"><see cref="TriggerStatus"/></param>
    internal void SetStatus(TriggerStatus status)
    {
        if (Status == status) return;
        Status = status;
    }

    /// <summary>
    /// 是否是正常触发器状态
    /// </summary>
    /// <returns><see cref="bool"/></returns>
    internal bool IsNormalStatus()
    {
        var isNormalStatus = Status != TriggerStatus.Pause
            && Status != TriggerStatus.Archived
            && Status != TriggerStatus.Panic
            && Status != TriggerStatus.Overrun
            && Status != TriggerStatus.Unoccupied
            && Status != TriggerStatus.NotStart
            && Status != TriggerStatus.Unknown
            && Status != TriggerStatus.Unhandled;

        // 如果不是正常触发器状态，NextRunTime 强制设置为 null
        if (!isNormalStatus) NextRunTime = null;

        return isNormalStatus;
    }

    /// <summary>
    /// 检查下一次执行信息并修正 <see cref="NextRunTime"/> 和 <see cref="Status"/>
    /// </summary>
    /// <param name="jobDetail">作业信息</param>
    /// <param name="startAt">起始时间</param>
    /// <returns><see cref="bool"/></returns>
    internal bool CheckAndFixNextOccurrence(JobDetail jobDetail, DateTime startAt)
    {
        // 检查作业信息运行时类型
        if (jobDetail.RuntimeJobType == null)
        {
            SetStatus(TriggerStatus.Unhandled);
            NextRunTime = null;
            return false;
        }

        // 检查作业触发器运行时类型
        if (RuntimeTriggerType == null)
        {
            SetStatus(TriggerStatus.Unknown);
            NextRunTime = null;
            return false;
        }

        // 检查是否立即启动
        if (StartNow == false)
        {
            SetStatus(TriggerStatus.NotStart);
            NextRunTime = null;
            return false;
        }

        // 开始时间检查
        if (StartTime != null)
        {
            if (StartTime.Value > startAt)
            {
                SetStatus(TriggerStatus.Backlog);
                return true;
            }
        }

        // 结束时间检查
        if (EndTime != null)
        {
            var compareTime = NextRunTime != null ? NextRunTime.Value : startAt;
            if (EndTime.Value < compareTime)
            {
                SetStatus(TriggerStatus.Archived);
                NextRunTime = null;
                return false;
            }
        }

        // 最大次数判断
        if (MaxNumberOfRuns > 0 && NumberOfRuns >= MaxNumberOfRuns)
        {
            SetStatus(TriggerStatus.Overrun);
            NextRunTime = null;
            return false;
        }

        // 最大错误数判断
        if (MaxNumberOfErrors > 0 && NumberOfErrors >= MaxNumberOfErrors)
        {
            SetStatus(TriggerStatus.Panic);
            NextRunTime = null;
            return false;
        }

        // 状态检查
        if (!IsNormalStatus())
        {
            return false;
        }

        // 下一次运行时间空判断
        if (NextRunTime == null)
        {
            if (IsNormalStatus()) SetStatus(TriggerStatus.Unoccupied);
            return false;
        }

        return true;
    }

    /// <summary>
    /// 下一次可执行检查
    /// </summary>
    /// <param name="startAt">起始时间</param>
    /// <returns><see cref="bool"/></returns>
    internal bool NextShouldRun(DateTime startAt)
    {
        return IsNormalStatus()
            && NextRunTime != null
            && NextRunTime.Value >= startAt;
    }

    /// <summary>
    /// 当前可执行检查
    /// </summary>
    /// <param name="jobDetail">作业信息</param>
    /// <param name="startAt">起始时间</param>
    /// <returns><see cref="bool"/></returns>
    internal bool CurrentShouldRun(JobDetail jobDetail, DateTime startAt)
    {
        return CheckAndFixNextOccurrence(jobDetail, startAt)
            // 调用派生类 ShouldRun 方法
            && ShouldRun(jobDetail, startAt);
    }

    /// <summary>
    /// 记录作业触发器运行信息
    /// </summary>
    /// <param name="schedulerFactory">作业计划工厂</param>
    /// <param name="jobId">作业 Id</param>
    /// <param name="exception">异常信息</param>
    /// <returns><see cref="Task"/></returns>
    internal async Task RecordTimelineAsync(ISchedulerFactory schedulerFactory, string jobId, string exception = null)
    {
        Timelines ??= new();

        // 只保留 5 条记录
        if (Timelines.Count >= 5)
        {
            var _timelines = Timelines.Dequeue();
            _timelines?.Dispose();
        }

        var timeline = new TriggerTimeline
        {
            JobId = jobId,
            TriggerId = TriggerId,
            LastRunTime = LastRunTime,
            NumberOfRuns = NumberOfRuns,
            NextRunTime = NextRunTime,
            Status = Status,
            Result = Result,
            ElapsedTime = ElapsedTime,
            CreatedTime = DateTime.Now,
            Mode = Mode,
            Exception = exception
        };

        Timelines.Enqueue(timeline);

        // 调用事件委托（记录作业触发器运行信息）
        if (schedulerFactory is SchedulerFactory schedulerFactoryInstance)
        {
            // 初始化作业执行记录持久上下文
            var context = new PersistenceExecutionRecordContext(
                schedulerFactoryInstance.GetJob(jobId).GetJobDetail()
                , this
                , timeline.Mode
                , timeline);

            await schedulerFactoryInstance.RecordTimelineAsync(context).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 带命名规则的数据库列名
    /// </summary>
    private readonly ConcurrentDictionary<NamingConventions, string[]> _namingColumnNames = new();

    /// <summary>
    /// 获取数据库列名
    /// </summary>
    /// <param name="naming">命名法</param>
    /// <returns>string[]</returns>
    private string[] ColumnNames(NamingConventions naming = NamingConventions.CamelCase)
    {
        // 如果字典中已经存在过，则直接返回
        var contains = _namingColumnNames.TryGetValue(naming, out var columnNames);
        if (contains) return columnNames;

        // 否则创建新的
        var nameColumnNames = new[]
        {
            Penetrates.GetNaming(nameof(TriggerId), naming)    // 第一个是标识，禁止移动位置
            , Penetrates.GetNaming(nameof(JobId), naming)   // 第二个是作业标识，禁止移动位置
            , Penetrates.GetNaming(nameof(TriggerType), naming)
            , Penetrates.GetNaming(nameof(AssemblyName), naming)
            , Penetrates.GetNaming(nameof(Args), naming)
            , Penetrates.GetNaming(nameof(Description), naming)
            , Penetrates.GetNaming(nameof(Status), naming)
            , Penetrates.GetNaming(nameof(StartTime), naming)
            , Penetrates.GetNaming(nameof(EndTime), naming)
            , Penetrates.GetNaming(nameof(LastRunTime), naming)
            , Penetrates.GetNaming(nameof(NextRunTime), naming)
            , Penetrates.GetNaming(nameof(NumberOfRuns), naming)
            , Penetrates.GetNaming(nameof(MaxNumberOfRuns), naming)
            , Penetrates.GetNaming(nameof(NumberOfErrors), naming)
            , Penetrates.GetNaming(nameof(MaxNumberOfErrors), naming)
            , Penetrates.GetNaming(nameof(NumRetries), naming)
            , Penetrates.GetNaming(nameof(RetryTimeout), naming)
            , Penetrates.GetNaming(nameof(StartNow), naming)
            , Penetrates.GetNaming(nameof(RunOnStart), naming)
            , Penetrates.GetNaming(nameof(ResetOnlyOnce), naming)
            , Penetrates.GetNaming(nameof(Result), naming)
            , Penetrates.GetNaming(nameof(ElapsedTime), naming)
            , Penetrates.GetNaming(nameof(UpdatedTime), naming)
        };
        _ = _namingColumnNames.TryAdd(naming, nameColumnNames);

        return nameColumnNames;
    }

    /// <summary>
    /// 获取触发器初始化时间
    /// </summary>
    /// <param name="startAt">起始时间</param>
    /// <returns><see cref="DateTime"/> 或者 null</returns>
    private DateTime? GetStartAt(DateTime startAt)
    {
        if (StartTime == null)
        {
            if (EndTime == null) return startAt;
            else return EndTime.Value < startAt ? null : startAt;
        }
        else return StartTime.Value < startAt ? startAt : StartTime.Value;
    }

    /// <summary>
    /// 转换成 Sql 语句
    /// </summary>
    /// <param name="tableName">数据库表名</param>
    /// <param name="behavior">持久化行为</param>
    /// <param name="naming">命名法</param>
    /// <returns><see cref="string"/></returns>
    public string ConvertToSQL(string tableName, PersistenceBehavior behavior, NamingConventions naming = NamingConventions.CamelCase)
    {
        // 判断是否自定义了 SQL 输出程序
        if (TriggerOptions.ConvertToSQLConfigure != null)
        {
            return TriggerOptions.ConvertToSQLConfigure(tableName, ColumnNames(naming), this, behavior, naming);
        }

        // 生成新增 SQL
        if (behavior == PersistenceBehavior.Appended)
        {
            return ConvertToInsertSQL(tableName, naming);
        }
        // 生成更新 SQL
        else if (behavior == PersistenceBehavior.Updated)
        {
            return ConvertToUpdateSQL(tableName, naming);
        }
        // 生成删除 SQL
        else if (behavior == PersistenceBehavior.Removed)
        {
            return ConvertToDeleteSQL(tableName, naming);
        }

        return string.Empty;
    }

    /// <summary>
    /// 转换成 Sql 新增语句
    /// </summary>
    /// <param name="tableName">数据库表名</param>
    /// <param name="naming">命名法</param>
    /// <returns><see cref="string"/></returns>
    public string ConvertToInsertSQL(string tableName, NamingConventions naming = NamingConventions.CamelCase)
    {
        // 不使用反射生成，为了使顺序可控，生成 SQL 可控，性能损耗最小
        var columnNames = ColumnNames(naming);

        return $@"INSERT INTO {Penetrates.WrapDatabaseFieldName(tableName)}(
    {Penetrates.WrapDatabaseFieldName(columnNames[0])},
    {Penetrates.WrapDatabaseFieldName(columnNames[1])},
    {Penetrates.WrapDatabaseFieldName(columnNames[2])},
    {Penetrates.WrapDatabaseFieldName(columnNames[3])},
    {Penetrates.WrapDatabaseFieldName(columnNames[4])},
    {Penetrates.WrapDatabaseFieldName(columnNames[5])},
    {Penetrates.WrapDatabaseFieldName(columnNames[6])},
    {Penetrates.WrapDatabaseFieldName(columnNames[7])},
    {Penetrates.WrapDatabaseFieldName(columnNames[8])},
    {Penetrates.WrapDatabaseFieldName(columnNames[9])},
    {Penetrates.WrapDatabaseFieldName(columnNames[10])},
    {Penetrates.WrapDatabaseFieldName(columnNames[11])},
    {Penetrates.WrapDatabaseFieldName(columnNames[12])},
    {Penetrates.WrapDatabaseFieldName(columnNames[13])},
    {Penetrates.WrapDatabaseFieldName(columnNames[14])},
    {Penetrates.WrapDatabaseFieldName(columnNames[15])},
    {Penetrates.WrapDatabaseFieldName(columnNames[16])},
    {Penetrates.WrapDatabaseFieldName(columnNames[17])},
    {Penetrates.WrapDatabaseFieldName(columnNames[18])},
    {Penetrates.WrapDatabaseFieldName(columnNames[19])},
    {Penetrates.WrapDatabaseFieldName(columnNames[20])},
    {Penetrates.WrapDatabaseFieldName(columnNames[21])},
    {Penetrates.WrapDatabaseFieldName(columnNames[22])}
)
VALUES(
    {Penetrates.GetNoNumberSqlValueOrNull(TriggerId)},
    {Penetrates.GetNoNumberSqlValueOrNull(JobId)},
    {Penetrates.GetNoNumberSqlValueOrNull(TriggerType)},
    {Penetrates.GetNoNumberSqlValueOrNull(AssemblyName)},
    {Penetrates.GetNoNumberSqlValueOrNull(Args)},
    {Penetrates.GetNoNumberSqlValueOrNull(Description)},
    {((int)Status)},
    {Penetrates.GetNoNumberSqlValueOrNull(StartTime.ToFormatString())},
    {Penetrates.GetNoNumberSqlValueOrNull(EndTime.ToFormatString())},
    {Penetrates.GetNoNumberSqlValueOrNull(LastRunTime.ToFormatString())},
    {Penetrates.GetNoNumberSqlValueOrNull(NextRunTime.ToFormatString())},
    {NumberOfRuns},
    {MaxNumberOfRuns},
    {NumberOfErrors},
    {MaxNumberOfErrors},
    {NumRetries},
    {RetryTimeout},
    {Penetrates.GetBooleanSqlValue(StartNow)},
    {Penetrates.GetBooleanSqlValue(RunOnStart)},
    {Penetrates.GetBooleanSqlValue(ResetOnlyOnce)},
    {Penetrates.GetNoNumberSqlValueOrNull(Result)},
    {ElapsedTime},
    {Penetrates.GetNoNumberSqlValueOrNull(UpdatedTime.ToFormatString())}
);";
    }

    /// <summary>
    /// 转换成 Sql 更新语句
    /// </summary>
    /// <param name="tableName">数据库表名</param>
    /// <param name="naming">命名法</param>
    /// <returns><see cref="string"/></returns>
    public string ConvertToUpdateSQL(string tableName, NamingConventions naming = NamingConventions.CamelCase)
    {
        // 不使用反射生成，为了使顺序可控，生成 SQL 可控，性能损耗最小
        var columnNames = ColumnNames(naming);

        return $@"UPDATE {Penetrates.WrapDatabaseFieldName(tableName)}
SET
    {Penetrates.WrapDatabaseFieldName(columnNames[0])} = {Penetrates.GetNoNumberSqlValueOrNull(TriggerId)},
    {Penetrates.WrapDatabaseFieldName(columnNames[1])} = {Penetrates.GetNoNumberSqlValueOrNull(JobId)},
    {Penetrates.WrapDatabaseFieldName(columnNames[2])} = {Penetrates.GetNoNumberSqlValueOrNull(TriggerType)},
    {Penetrates.WrapDatabaseFieldName(columnNames[3])} = {Penetrates.GetNoNumberSqlValueOrNull(AssemblyName)},
    {Penetrates.WrapDatabaseFieldName(columnNames[4])} = {Penetrates.GetNoNumberSqlValueOrNull(Args)},
    {Penetrates.WrapDatabaseFieldName(columnNames[5])} = {Penetrates.GetNoNumberSqlValueOrNull(Description)},
    {Penetrates.WrapDatabaseFieldName(columnNames[6])} = {((int)Status)},
    {Penetrates.WrapDatabaseFieldName(columnNames[7])} = {Penetrates.GetNoNumberSqlValueOrNull(StartTime.ToFormatString())},
    {Penetrates.WrapDatabaseFieldName(columnNames[8])} = {Penetrates.GetNoNumberSqlValueOrNull(EndTime.ToFormatString())},
    {Penetrates.WrapDatabaseFieldName(columnNames[9])} = {Penetrates.GetNoNumberSqlValueOrNull(LastRunTime.ToFormatString())},
    {Penetrates.WrapDatabaseFieldName(columnNames[10])} = {Penetrates.GetNoNumberSqlValueOrNull(NextRunTime.ToFormatString())},
    {Penetrates.WrapDatabaseFieldName(columnNames[11])} = {NumberOfRuns},
    {Penetrates.WrapDatabaseFieldName(columnNames[12])} = {MaxNumberOfRuns},
    {Penetrates.WrapDatabaseFieldName(columnNames[13])} = {NumberOfErrors},
    {Penetrates.WrapDatabaseFieldName(columnNames[14])} = {MaxNumberOfErrors},
    {Penetrates.WrapDatabaseFieldName(columnNames[15])} = {NumRetries},
    {Penetrates.WrapDatabaseFieldName(columnNames[16])} = {RetryTimeout},
    {Penetrates.WrapDatabaseFieldName(columnNames[17])} = {Penetrates.GetBooleanSqlValue(StartNow)},
    {Penetrates.WrapDatabaseFieldName(columnNames[18])} = {Penetrates.GetBooleanSqlValue(RunOnStart)},
    {Penetrates.WrapDatabaseFieldName(columnNames[19])} = {Penetrates.GetBooleanSqlValue(ResetOnlyOnce)},
    {Penetrates.WrapDatabaseFieldName(columnNames[20])} = {Penetrates.GetNoNumberSqlValueOrNull(Result)},
    {Penetrates.WrapDatabaseFieldName(columnNames[21])} = {ElapsedTime},
    {Penetrates.WrapDatabaseFieldName(columnNames[22])} = {Penetrates.GetNoNumberSqlValueOrNull(UpdatedTime.ToFormatString())}
WHERE {Penetrates.WrapDatabaseFieldName(columnNames[0])} = {Penetrates.GetNoNumberSqlValueOrNull(TriggerId)} AND {Penetrates.WrapDatabaseFieldName(columnNames[1])} = {Penetrates.GetNoNumberSqlValueOrNull(JobId)};";
    }

    /// <summary>
    /// 转换成 Sql 删除语句
    /// </summary>
    /// <param name="tableName">数据库表名</param>
    /// <param name="naming">命名法</param>
    /// <returns><see cref="string"/></returns>
    public string ConvertToDeleteSQL(string tableName, NamingConventions naming = NamingConventions.CamelCase)
    {
        // 不使用反射生成，为了使顺序可控，生成 SQL 可控，性能损耗最小
        var columnNames = ColumnNames(naming);

        return $@"DELETE FROM {Penetrates.WrapDatabaseFieldName(tableName)}
WHERE {Penetrates.WrapDatabaseFieldName(columnNames[0])} = {Penetrates.GetNoNumberSqlValueOrNull(TriggerId)} AND {Penetrates.WrapDatabaseFieldName(columnNames[1])} = {Penetrates.GetNoNumberSqlValueOrNull(JobId)};";
    }

    /// <summary>
    /// 转换成 JSON 字符串
    /// </summary>
    /// <param name="naming">命名法</param>
    /// <returns><see cref="string"/></returns>
    public string ConvertToJSON(NamingConventions naming = NamingConventions.CamelCase)
    {
        return Penetrates.Write(writer =>
        {
            writer.WriteStartObject();

            writer.WriteString(Penetrates.GetNaming(nameof(TriggerId), naming), TriggerId);
            writer.WriteString(Penetrates.GetNaming(nameof(JobId), naming), JobId);
            writer.WriteString(Penetrates.GetNaming(nameof(TriggerType), naming), TriggerType);
            writer.WriteString(Penetrates.GetNaming(nameof(AssemblyName), naming), AssemblyName);
            writer.WriteString(Penetrates.GetNaming(nameof(Args), naming), Args);
            writer.WriteString(Penetrates.GetNaming(nameof(Description), naming), Description);
            writer.WriteNumber(Penetrates.GetNaming(nameof(Status), naming), (int)Status);
            writer.WriteString(Penetrates.GetNaming(nameof(StartTime), naming), StartTime.ToFormatString());
            writer.WriteString(Penetrates.GetNaming(nameof(EndTime), naming), EndTime.ToFormatString());
            writer.WriteString(Penetrates.GetNaming(nameof(LastRunTime), naming), LastRunTime.ToFormatString());
            writer.WriteString(Penetrates.GetNaming(nameof(NextRunTime), naming), NextRunTime.ToFormatString());
            writer.WriteNumber(Penetrates.GetNaming(nameof(NumberOfRuns), naming), NumberOfRuns);
            writer.WriteNumber(Penetrates.GetNaming(nameof(MaxNumberOfRuns), naming), MaxNumberOfRuns);
            writer.WriteNumber(Penetrates.GetNaming(nameof(NumberOfErrors), naming), NumberOfErrors);
            writer.WriteNumber(Penetrates.GetNaming(nameof(MaxNumberOfErrors), naming), MaxNumberOfErrors);
            writer.WriteNumber(Penetrates.GetNaming(nameof(NumRetries), naming), NumRetries);
            writer.WriteNumber(Penetrates.GetNaming(nameof(RetryTimeout), naming), RetryTimeout);
            writer.WriteBoolean(Penetrates.GetNaming(nameof(StartNow), naming), StartNow);
            writer.WriteBoolean(Penetrates.GetNaming(nameof(RunOnStart), naming), RunOnStart);
            writer.WriteBoolean(Penetrates.GetNaming(nameof(ResetOnlyOnce), naming), ResetOnlyOnce);
            writer.WriteString(Penetrates.GetNaming(nameof(Result), naming), Result);
            writer.WriteNumber(Penetrates.GetNaming(nameof(ElapsedTime), naming), ElapsedTime);
            writer.WriteString(Penetrates.GetNaming(nameof(UpdatedTime), naming), UpdatedTime.ToFormatString());

            writer.WriteEndObject();
        });
    }

    /// <summary>
    /// 转换成 Monitor 字符串
    /// </summary>
    /// <param name="naming">命名法</param>
    /// <returns><see cref="string"/></returns>
    public string ConvertToMonitor(NamingConventions naming = NamingConventions.CamelCase)
    {
        return TP.Wrapper(nameof(Trigger), Description ?? TriggerType, new[]
        {
            $"##{Penetrates.GetNaming(nameof(TriggerId), naming)}## {TriggerId}"
            , $"##{Penetrates.GetNaming(nameof(JobId), naming)}## {JobId}"
            , $"##{Penetrates.GetNaming(nameof(TriggerType), naming)}## {TriggerType}"
            , $"##{Penetrates.GetNaming(nameof(AssemblyName), naming)}## {AssemblyName}"
            , $"##{Penetrates.GetNaming(nameof(Args), naming)}## {Args}"
            , $"##{Penetrates.GetNaming(nameof(Description), naming)}## {Description}"
            , $"##{Penetrates.GetNaming(nameof(Status), naming)}## {Status}"
            , $"##{Penetrates.GetNaming(nameof(StartTime), naming)}## {StartTime.ToFormatString()}"
            , $"##{Penetrates.GetNaming(nameof(EndTime), naming)}## {EndTime.ToFormatString()}"
            , $"##{Penetrates.GetNaming(nameof(LastRunTime), naming)}## {LastRunTime.ToFormatString()}"
            , $"##{Penetrates.GetNaming(nameof(NextRunTime), naming)}## {NextRunTime.ToFormatString()}"
            , $"##{Penetrates.GetNaming(nameof(NumberOfRuns), naming)}## {NumberOfRuns}"
            , $"##{Penetrates.GetNaming(nameof(MaxNumberOfRuns), naming)}## {MaxNumberOfRuns}"
            , $"##{Penetrates.GetNaming(nameof(NumberOfErrors), naming)}## {NumberOfErrors}"
            , $"##{Penetrates.GetNaming(nameof(MaxNumberOfErrors), naming)}## {MaxNumberOfErrors}"
            , $"##{Penetrates.GetNaming(nameof(NumRetries), naming)}## {NumRetries}"
            , $"##{Penetrates.GetNaming(nameof(RetryTimeout), naming)}## {RetryTimeout}"
            , $"##{Penetrates.GetNaming(nameof(StartNow), naming)}## {StartNow}"
            , $"##{Penetrates.GetNaming(nameof(RunOnStart), naming)}## {RunOnStart}"
            , $"##{Penetrates.GetNaming(nameof(ResetOnlyOnce), naming)}## {ResetOnlyOnce}"
            , $"##{Penetrates.GetNaming(nameof(Result), naming)}## {Result}"
            , $"##{Penetrates.GetNaming(nameof(ElapsedTime), naming)}## {ElapsedTime}"
            , $"##{Penetrates.GetNaming(nameof(UpdatedTime), naming)}## {UpdatedTime.ToFormatString()}"
        });
    }
}