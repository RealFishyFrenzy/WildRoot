using System;
using System.Collections.Generic;

// Bounded, temporary display state. Repeated keyed failures cannot flood the feed
// or extend an existing message forever (e.g. trigger-stay on a full inventory).
public sealed class NotificationFeed
{
    private sealed class Entry
    {
        public string Text;
        public string Key;
        public double Expires;
    }

    private readonly List<Entry> entries = new List<Entry>();
    private readonly int capacity;
    private readonly double lifetime;

    public NotificationFeed(int capacity = 3, double lifetime = 3)
    {
        if (capacity < 1 || double.IsNaN(lifetime) || double.IsInfinity(lifetime) || lifetime <= 0)
            throw new ArgumentOutOfRangeException();
        this.capacity = capacity;
        this.lifetime = lifetime;
    }

    public bool Post(string message, string key, double now)
    {
        if (!IsValidTime(now) || string.IsNullOrWhiteSpace(message))
            return false;
        Expire(now);
        if (!string.IsNullOrEmpty(key) && entries.Exists(entry => entry.Key == key))
            return false;
        if (entries.Count == capacity)
            entries.RemoveAt(0);
        entries.Add(new Entry { Text = message, Key = key, Expires = now + lifetime });
        return true;
    }

    public string GetText(double now)
    {
        if (IsValidTime(now))
            Expire(now);
        return string.Join("\n", entries.ConvertAll(entry => entry.Text));
    }

    public void Clear() => entries.Clear();

    private void Expire(double now) => entries.RemoveAll(entry => entry.Expires <= now);
    private static bool IsValidTime(double time) => !double.IsNaN(time) && !double.IsInfinity(time) && time >= 0;
}
