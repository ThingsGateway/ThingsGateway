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

namespace ThingsGateway.Web.Rcl.Core
{
    /// <summary>
    /// masa.stack
    /// </summary>
    public partial class AppDateTimePicker
    {
        private static readonly int[] _hours = Enumerable.Range(0, 24).ToArray();
        private static readonly int[] _minutes = Enumerable.Range(0, 60).ToArray();
        private static readonly int[] _seconds = Enumerable.Range(0, 60).ToArray();
        [Inject]
        public JsInitVariables JsInitVariables { get; set; } = default!;
        [Parameter]
        public DateTime? Max { get; set; }

        [Parameter]
        public DateTime? Min { get; set; }

        [Parameter]
        public bool NoTitle { get; set; } = true;

        [Parameter]
        public DateTime? Value { get; set; }

        [Parameter]
        public EventCallback<DateTime?> ValueChanged { get; set; }

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        [Parameter]
        public TimeSpan OutputTimezoneOffset { get; set; } = TimeSpan.FromMinutes(0);
        [Parameter]
        public TimeSpan DisplayTimezoneOffset { get; set; }

        private DateOnly? Date
        {
            get
            {
                if (Value is null)
                    return null;
                return DateOnly.FromDateTime(Value.Value.Add(DisplayTimezoneOffset));
            }
        }

        private TimeOnly Time
        {
            get
            {
                if (Value is null)
                    return new(GetHours()[0], GetMinutes()[0], GetSeconds()[0]);
                return TimeOnly.FromDateTime(Value.Value.Add(DisplayTimezoneOffset));
            }
        }

        public override async Task SetParametersAsync(ParameterView parameters)
        {
            DisplayTimezoneOffset = JsInitVariables.TimezoneOffset;
            await base.SetParametersAsync(parameters);
            if (Max is not null && Min is not null && Max < Min)
            {
                await PopupService.EnqueueSnackbarAsync(new(T("The maximum time cannot be less than the minimum time"), AlertTypes.Error));
                Max = null;
            }
        }

        private int[] GetHours()
        {
            if (Min is null && Max is null)
                return _hours;
            else
            {
                var hours = _hours;
                if (Min is not null && Value is not null && Min.Value.Date >= Value.Value.Date)
                    hours = hours.Where(h => h >= Min.Value.Hour).ToArray();
                else if (Max is not null && Value is not null && Max.Value.Date <= Value.Value.Date)
                    hours = hours.Where(h => h <= Max.Value.Hour).ToArray();
                return hours;
            }
        }

        private int[] GetMinutes()
        {
            if (Min is null && Max is null)
                return _minutes;
            {
                if (Min is not null && Value is not null && Min.Value.Date >= Value.Value.Date)
                    return _minutes.Where(h => h >= Min.Value.Minute).ToArray();
                else if (Max is not null && Value is not null && Max.Value.Date <= Value.Value.Date)
                    return _minutes.Where(h => h <= Max.Value.Minute).ToArray();
                return _minutes;
            }
        }

        private int[] GetSeconds()
        {
            if (Min is null && Max is null)
                return _seconds;
            {
                if (Min is not null && Value is not null && Min.Value.Date >= Value.Value.Date)
                    return _seconds.Where(h => h >= Min.Value.Second).ToArray();
                else if (Max is not null && Value is not null && Max.Value.Date <= Value.Value.Date)
                    return _seconds.Where(h => h <= Max.Value.Second).ToArray();
                else
                    return _seconds;
            }
        }

        private bool GetNowClickState()
        {
            if (Min is not null)
                return DateTime.UtcNow < Min;
            else if (Max is not null)
                return DateTime.UtcNow > Max;
            else
                return false;
        }

        private DateOnly? GetMinDateOnly()
        {
            if (Min is not null)
            {
                if (Value is null)
                    return DateOnly.FromDateTime(Min.Value);
                else if (Min.Value.TimeOfDay < Value.Value.TimeOfDay)
                    return DateOnly.FromDateTime(Min.Value.AddDays(1));
                else
                    return DateOnly.FromDateTime(Min.Value);
            }
            else
                return null;
        }

        private DateOnly? GetMaxDateOnly()
        {
            if (Max is not null)
            {
                if (Value is null)
                    return DateOnly.FromDateTime(Max.Value);
                else if (Max.Value.TimeOfDay < Value.Value.TimeOfDay)
                    return DateOnly.FromDateTime(Max.Value.AddDays(-1));
                else
                    return DateOnly.FromDateTime(Max.Value);
            }

            return null;
        }

        private async Task DateChangedAsync(DateOnly? date)
        {
            await UpdateValueAsync(date?.ToDateTime(Time));
        }

        private async Task HourChangedAsync(int hour)
        {
            var time = new TimeOnly(hour, Time.Minute, Time.Second);
            await UpdateValueAsync(time);
        }

        private async Task MinuteChangedAsync(int minute)
        {
            var time = new TimeOnly(Time.Hour, minute, Time.Second);
            await UpdateValueAsync(time);
        }

        private async Task SecondChangedAsync(int second)
        {
            var time = new TimeOnly(Time.Hour, Time.Minute, second);
            await UpdateValueAsync(time);
        }

        private async Task UpdateValueAsync(DateTime? dateTime)
        {
            dateTime = dateTime?.Add(-DisplayTimezoneOffset).Add(OutputTimezoneOffset);
            if (ValueChanged.HasDelegate)
            {
                await ValueChanged.InvokeAsync(dateTime);
            }
            else
            {
                Value = dateTime;
            }
        }

        private async Task UpdateValueAsync(TimeOnly time)
        {
            DateTime? dateTime = default;
            if (Date is null)
            {
                var now = DateTime.UtcNow;
                if (Min is not null)
                {
                    if (Min < now)
                        dateTime = DateOnly.FromDateTime(now).ToDateTime(time);
                    else
                        dateTime = DateOnly.FromDateTime(Min.Value).ToDateTime(time);
                }
                else if (Max is not null)
                {
                    if (Max < now)
                        dateTime = DateOnly.FromDateTime(Max.Value).ToDateTime(time);
                    else
                        dateTime = DateOnly.FromDateTime(now).ToDateTime(time);
                }
            }
            else
                dateTime = Date.Value.ToDateTime(time);
            await UpdateValueAsync(dateTime);
        }

        private async Task OnNowAsync()
        {
            await UpdateValueAsync(DateTime.UtcNow.Add(JsInitVariables.TimezoneOffset));
        }

        private async Task OnResetAsync()
        {
            await UpdateValueAsync(null);
        }
    }
}