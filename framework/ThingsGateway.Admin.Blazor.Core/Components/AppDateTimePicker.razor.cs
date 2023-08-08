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

using BlazorComponent;

using Masa.Blazor;

using Microsoft.AspNetCore.Components;

namespace ThingsGateway.Admin.Blazor.Core;
/// <summary>
/// DateTimePicker
/// </summary>
public partial class AppDateTimePicker
{
    private static readonly int[] ValidHours = Enumerable.Range(0, 24).ToArray();
    private static readonly int[] ValidMinutes = Enumerable.Range(0, 60).ToArray();
    private static readonly int[] ValidSeconds = Enumerable.Range(0, 60).ToArray();

    [Inject]
    InitTimezone InitTimezone { get; set; }

    /// <summary>
    /// max time  [utc]
    /// </summary>
    [Parameter]
    public DateTime? Max { get; set; }

    /// <summary>
    /// min time  [utc]
    /// </summary>
    [Parameter]
    public DateTime? Min { get; set; }
    /// <summary>
    /// NoTitle
    /// </summary>
    [Parameter]
    public bool NoTitle { get; set; }

    /// <summary>
    /// selected datetime[utc]
    /// </summary>
    [Parameter]
    public DateTime? Value { get; set; }

    /// <summary>
    /// ValueChanged
    /// </summary>
    [Parameter]
    public EventCallback<DateTime?> ValueChanged { get; set; }
    /// <summary>
    /// ChildContent
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; }

    /// <summary>
    /// OutputTimezoneOffset
    /// </summary>
    [Parameter]
    public TimeSpan OutputTimezoneOffset { get; set; } = TimeSpan.FromMinutes(0);
    /// <summary>
    /// DisplayTimezoneOffset
    /// </summary>
    [Parameter]
    public TimeSpan DisplayTimezoneOffset { get; set; } = TimeSpan.FromMinutes(0);

    private DateTime MaxOffset
    {
        get
        {
            if (Max == null || Max == DateTime.MaxValue) return DateTime.MaxValue;
            try
            {
                return Max.Value.Add(-OutputTimezoneOffset).Add(DisplayTimezoneOffset);
            }
            catch
            {
                // ignored
            }

            return DateTime.MaxValue;
        }
    }

    private DateTime MinOffset
    {
        get
        {
            if (Min == null || Min == DateTime.MinValue) return DateTime.MinValue;
            try
            {
                return Min.Value.ToUniversalTime().Add(DisplayTimezoneOffset);
            }
            catch
            {
            }

            return DateTime.MinValue;
        }
    }

    private DateTime? _internalDateTime = null;
    private DateOnly? _internalDate = null;
    private readonly bool _getValidSelectItems = false;

    private TimeOnly InternalTime
    {
        get
        {
            if (_internalDateTime is null) return new TimeOnly(0, 0, 0);
            return TimeOnly.FromDateTime(_internalDateTime.Value);
        }
    }

    private DateTime ClientNow => DateTime.UtcNow.Add(DisplayTimezoneOffset);
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        DisplayTimezoneOffset = InitTimezone.TimezoneOffset;
        await base.SetParametersAsync(parameters);
        if (Max is not null && Min is not null && Max < Min)
        {
            await PopupService.EnqueueSnackbarAsync(("The maximum time cannot be less than the minimum time"), AlertTypes.Error);
            Max = null;
        }
    }

    private int[] GetHours()
    {
        var validHours = ValidHours;
        if (!_getValidSelectItems) return validHours;
        var hours = validHours;
        if (MaxOffset.Subtract(MinOffset).TotalDays < 1)
        {
            hours = FilterAvailableValues(validHours, MinOffset.Hour, MaxOffset.Hour);
        }
        else if (_internalDateTime.HasValue)
        {
            var sameWithMax = _internalDateTime.Value.Date == MaxOffset.Date;
            var sameWithMin = _internalDateTime.Value.Date == MinOffset.Date;
            if (sameWithMin && sameWithMax)
            {
                hours = FilterAvailableValues(validHours, MinOffset.Hour, MaxOffset.Hour);
            }
            else if (sameWithMax)
            {
                hours = FilterAvailableValues(validHours, validHours.First(), MaxOffset.Hour);
            }
            else if (sameWithMin)
            {
                hours = FilterAvailableValues(validHours, MinOffset.Hour, validHours.Last());
            }
        }
        return hours;
    }

    private int[] GetMinutes()
    {
        var validMinutes = ValidMinutes;
        if (!_getValidSelectItems) return validMinutes;
        var minutes = validMinutes;
        if (MaxOffset.Subtract(MinOffset).TotalHours < 1)
        {
            minutes = FilterAvailableValues(validMinutes, MinOffset.Minute, MaxOffset.Minute);
        }
        else if (_internalDateTime.HasValue)
        {
            var sameWithMax = _internalDateTime.Value.Date == MaxOffset.Date
                              && _internalDateTime.Value.Hour == MaxOffset.Hour;
            var sameWithMin = _internalDateTime.Value.Date == MinOffset.Date
                              && _internalDateTime.Value.Hour == MinOffset.Hour;
            if (sameWithMin && sameWithMax)
            {
                minutes = FilterAvailableValues(validMinutes, MinOffset.Minute, MaxOffset.Minute);
            }
            else if (sameWithMax)
            {
                minutes = FilterAvailableValues(validMinutes, validMinutes.First(), MaxOffset.Minute);
            }
            else if (sameWithMin)
            {
                minutes = FilterAvailableValues(validMinutes, MinOffset.Minute, validMinutes.Last());
            }
        }
        return minutes;
    }


    private int[] GetSeconds()
    {
        var validSeconds = ValidSeconds;
        if (!_getValidSelectItems) return validSeconds;
        var seconds = validSeconds;

        if (MaxOffset.Subtract(MinOffset).TotalMinutes < 1)
        {
            seconds = FilterAvailableValues(validSeconds,
                MinOffset == DateTime.MinValue ? validSeconds.First() : MinOffset.Second,
                MaxOffset == DateTime.MaxValue ? validSeconds.Last() : MaxOffset.Second);
        }
        else if (_internalDateTime.HasValue)
        {
            var sameWithMax = _internalDateTime.Value.Date == MaxOffset.Date
                                    && _internalDateTime.Value.Hour == MaxOffset.Hour
                                    && _internalDateTime.Value.Minute == MaxOffset.Minute;
            var sameWithMin = _internalDateTime.Value.Date == MinOffset.Date
                                    && _internalDateTime.Value.Hour == MinOffset.Hour
                                    && _internalDateTime.Value.Minute == MinOffset.Minute;
            if (sameWithMin && sameWithMax)
            {
                seconds = FilterAvailableValues(validSeconds, MinOffset.Second, MaxOffset.Second);
            }
            else if (sameWithMax)
            {
                seconds = FilterAvailableValues(validSeconds, validSeconds.First(), MaxOffset.Second);
            }
            else if (sameWithMin)
            {
                seconds = FilterAvailableValues(validSeconds, MinOffset.Second, validSeconds.Last());
            }
        }
        return seconds;
    }

    private int[] FilterAvailableValues(int[] validValues, int min, int max)
    {
        Func<int, bool> whereFunc = min <= max ? h => h >= min && h <= max : h => h <= min && h >= max;
        return validValues.Where(whereFunc).ToArray();
    }

    private bool GetNowClickState()
    {
        return ClientNow < MinOffset || ClientNow > MaxOffset;
    }

    private DateOnly GetMinDateOnly()
    {
        return DateOnly.FromDateTime(MinOffset);
    }

    private DateOnly GetMaxDateOnly()
    {
        return DateOnly.FromDateTime(MaxOffset.Date);
    }

    private async Task DateChangedAsync(DateOnly? date)
    {
        await UpdateValueAsync(date?.ToDateTime(InternalTime));
    }

    private async Task HourChangedAsync(int hour)
    {
        var time = new TimeOnly(hour, InternalTime.Minute, InternalTime.Second);
        await UpdateTimeValueAsync(time);
    }

    private async Task MinuteChangedAsync(int minute)
    {
        var time = new TimeOnly(InternalTime.Hour, minute, InternalTime.Second);
        await UpdateTimeValueAsync(time);
    }

    private async Task SecondChangedAsync(int second)
    {
        var time = new TimeOnly(InternalTime.Hour, InternalTime.Minute, second);
        await UpdateTimeValueAsync(time);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dateTime">accept the time using display time zone</param>
    /// <returns></returns>
    private async Task UpdateValueAsync(DateTime? dateTime)
    {
        dateTime = TryFixDateTime(dateTime);
        _internalDateTime = dateTime;
        _internalDate = _internalDateTime is null ? null : DateOnly.FromDateTime(_internalDateTime.Value);
        dateTime = dateTime?.Add(-DisplayTimezoneOffset).Add(OutputTimezoneOffset); //to utc time
        Value = dateTime;
        if (ValueChanged.HasDelegate)
        {
            await ValueChanged.InvokeAsync(DateTime.SpecifyKind(dateTime.Value, DateTimeKind.Utc));
        }
    }

    private DateTime? TryFixDateTime(DateTime? dateTime)
    {
        if (dateTime != null)
        {
            if (dateTime > MaxOffset)
            {
                dateTime = MaxOffset;
            }
            else if (dateTime < MinOffset)
            {
                dateTime = MinOffset;
            }
        }
        return dateTime;
    }

    private async Task UpdateTimeValueAsync(TimeOnly time)
    {
        DateTime? dateTime;
        if (_internalDate is null)
        {
            if (MinOffset > ClientNow)
            {
                dateTime = DateOnly.FromDateTime(MinOffset).ToDateTime(time);
            }
            else if (MaxOffset < ClientNow)
            {
                dateTime = DateOnly.FromDateTime(MaxOffset).ToDateTime(time);
            }
            else
            {
                dateTime = DateOnly.FromDateTime(ClientNow).ToDateTime(time);
            }
        }
        else
        {
            dateTime = _internalDate.Value.ToDateTime(time);
        }

        await UpdateValueAsync(dateTime);
    }

    private async Task OnNowAsync()
    {
        await UpdateValueAsync(ClientNow);
    }

    private async Task OnResetAsync()
    {
        await UpdateValueAsync(null);
    }
}