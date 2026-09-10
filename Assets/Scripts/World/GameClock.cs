using System;
using UnityEngine;

/// <summary>Attach once to an active scene object to run the game clock.</summary>
public class GameClock : MonoBehaviour
{
    [Header("Clock configuration (applied on initialization)")]
    [SerializeField, Min(0.01f)] private float realMinutesPerDay = 25f;
    [SerializeField, Range(0, 23)] private int startingHour = 6;
    [SerializeField, Range(0, 59)] private int startingMinute = 0;

    private GameClockState clock;

    public GameTime CurrentTime
    {
        get
        {
            Initialize();
            return clock.CurrentTime;
        }
    }

    public long Day => CurrentTime.Day;
    public int Hour => CurrentTime.Hour;
    public int Minute => CurrentTime.Minute;

    public event Action<GameTime> MinuteChanged;
    public event Action<GameTime> HourChanged;
    public event Action<GameTime> DayChanged;

    private void Awake() => Initialize();

    private void Initialize()
    {
        if (clock != null)
            return;

        clock = new GameClockState(realMinutesPerDay, startingHour, startingMinute);
        clock.MinuteChanged += OnMinuteChanged;
        clock.HourChanged += time => HourChanged?.Invoke(time);
        clock.DayChanged += time => DayChanged?.Invoke(time);
    }

    private void Update()
    {
        // deltaTime is scaled by Unity: zero while timeScale is zero.
        clock.Advance(Time.deltaTime);
    }

    private void OnMinuteChanged(GameTime time)
    {
        Debug.Log(time.ToString(), this);
        MinuteChanged?.Invoke(time);
    }
}
