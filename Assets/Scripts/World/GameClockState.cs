using System;

/// <summary>An immutable snapshot supplied to clock observers.</summary>
public readonly struct GameTime
{
    public long Day { get; }
    public int Hour { get; }
    public int Minute { get; }

    public GameTime(long day, int hour, int minute)
    {
        if (day < 1) throw new ArgumentOutOfRangeException(nameof(day));
        if (hour < 0 || hour > 23) throw new ArgumentOutOfRangeException(nameof(hour));
        if (minute < 0 || minute > 59) throw new ArgumentOutOfRangeException(nameof(minute));
        Day = day;
        Hour = hour;
        Minute = minute;
    }

    public override string ToString() => $"Day {Day} - {Hour:00}:{Minute:00}";
}

/// <summary>Clock arithmetic independent of Unity and gameplay systems.</summary>
public sealed class GameClockState
{
    private const int MinutesPerDay = 24 * 60;
    private readonly double gameMinutesPerSecond;
    private double remainingGameMinutes;

    public GameTime CurrentTime { get; private set; }
    public event Action<GameTime> MinuteChanged;
    public event Action<GameTime> HourChanged;
    public event Action<GameTime> DayChanged;

    public GameClockState(double dayLengthMinutes = 25, int startingHour = 6, int startingMinute = 0)
    {
        if (double.IsNaN(dayLengthMinutes) || double.IsInfinity(dayLengthMinutes) || dayLengthMinutes <= 0)
            throw new ArgumentOutOfRangeException(nameof(dayLengthMinutes));

        gameMinutesPerSecond = MinutesPerDay / (dayLengthMinutes * 60);
        CurrentTime = new GameTime(1, startingHour, startingMinute);
    }

    /// <summary>
    /// Supply elapsed scaled seconds. Emits every crossed minute, including after a long frame.
    /// State is updated before notifications; order is minute, then hour, then day.
    /// </summary>
    public void Advance(double scaledSeconds)
    {
        if (double.IsNaN(scaledSeconds) || double.IsInfinity(scaledSeconds) || scaledSeconds < 0)
            throw new ArgumentOutOfRangeException(nameof(scaledSeconds));
        if (scaledSeconds == 0)
            return;

        remainingGameMinutes += scaledSeconds * gameMinutesPerSecond;
        long minutesElapsed = (long)Math.Floor(remainingGameMinutes);
        remainingGameMinutes -= minutesElapsed;

        for (long i = 0; i < minutesElapsed; i++)
        {
            int minute = CurrentTime.Minute + 1;
            int hour = CurrentTime.Hour;
            long day = CurrentTime.Day;
            bool hourChanged = minute == 60;
            if (hourChanged)
            {
                minute = 0;
                hour++;
            }
            bool dayChanged = hour == 24;
            if (dayChanged)
            {
                hour = 0;
                day++;
            }

            CurrentTime = new GameTime(day, hour, minute);
            MinuteChanged?.Invoke(CurrentTime);
            if (hourChanged) HourChanged?.Invoke(CurrentTime);
            if (dayChanged) DayChanged?.Invoke(CurrentTime);
        }
    }
}
