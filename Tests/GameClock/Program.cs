using System;
using System.Collections.Generic;

static class Program
{
    static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    static void Main()
    {
        const double minuteSeconds = 1500.0 / 1440;
        var clock = new GameClockState();
        Check(clock.CurrentTime.ToString() == "Day 1 - 06:00", "Default start");
        int minutes = 0, hours = 0, days = 0;
        clock.MinuteChanged += time => minutes++;
        clock.HourChanged += time => hours++;
        clock.DayChanged += time => days++;
        clock.Advance(0);
        Check(minutes == 0, "Paused clock emitted event");
        clock.Advance(1500);
        Check(clock.CurrentTime.ToString() == "Day 2 - 06:00", "25 minute day");
        Check(minutes == 1440 && hours == 24 && days == 1, "Full-day event counts");

        clock = new GameClockState(25, 23, 59);
        var hourlyClock = new GameClockState(25, 23, 59);
        hourlyClock.Advance(62.5);
        Check(hourlyClock.CurrentTime.ToString() == "Day 2 - 00:59", "Exact hour boundary");
        var events = new List<string>();
        clock.MinuteChanged += time =>
        {
            Check(clock.CurrentTime.ToString() == time.ToString(), "Event snapshot/state mismatch");
            events.Add("minute " + time);
        };
        clock.HourChanged += time => events.Add("hour " + time);
        clock.DayChanged += time => events.Add("day " + time);
        clock.Advance(minuteSeconds / 2);
        Check(events.Count == 0, "Premature minute");
        clock.Advance(0);
        clock.Advance(minuteSeconds / 2);
        Check(string.Join("|", events) ==
            "minute Day 2 - 00:00|hour Day 2 - 00:00|day Day 2 - 00:00", "Midnight/event order");

        clock = new GameClockState(25, 8, 58);
        events.Clear();
        clock.MinuteChanged += time => events.Add(time.ToString());
        clock.Advance(minuteSeconds * 3 + minuteSeconds / 2);
        Check(string.Join("|", events) == "Day 1 - 08:59|Day 1 - 09:00|Day 1 - 09:01", "Catch-up minutes");
        clock.Advance(minuteSeconds / 2 + 1e-9);
        Check(clock.CurrentTime.Minute == 2, "Fraction retained");

        clock = new GameClockState(1, 0, 0);
        clock.Advance(180);
        Check(clock.CurrentTime.ToString() == "Day 4 - 00:00", "Configurable duration/multiple days");

        clock = new GameClockState();
        for (int i = 0; i < 90001; i++) clock.Advance(1.0 / 60);
        Check(clock.CurrentTime.ToString() == "Day 2 - 06:00", "60fps accumulation");

        ExpectInvalid(() => new GameClockState(0));
        ExpectInvalid(() => new GameClockState(25, 24));
        ExpectInvalid(() => new GameClockState(25, 0, 60));
        ExpectInvalid(() => clock.Advance(-1));
        ExpectInvalid(() => clock.Advance(double.NaN));
        Console.WriteLine("PASS: start, pause, 25-minute day, event counts/order, midnight, catch-up, fractions, custom duration, multiple days, frame accumulation, invalid input.");
    }

    static void ExpectInvalid(Action action)
    {
        try { action(); }
        catch (ArgumentOutOfRangeException) { return; }
        throw new Exception("Expected invalid input rejection");
    }
}
