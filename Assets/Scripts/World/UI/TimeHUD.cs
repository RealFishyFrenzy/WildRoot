using UnityEngine;
using TMPro;

/// <summary>
/// HUD component that displays the current Day and Time from the authoritative GameClock.
/// </summary>
public class TimeHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameClock gameClock;
    [SerializeField] private TMP_Text timeText;

    private void Awake()
    {
        if (gameClock == null)
        {
            gameClock = FindAnyObjectByType<GameClock>();
        }
    }

    private void OnEnable()
    {
        if (gameClock == null)
        {
            gameClock = FindAnyObjectByType<GameClock>();
        }

        if (gameClock != null)
        {
            gameClock.MinuteChanged += OnMinuteChanged;
            gameClock.DayChanged += OnDayChanged;
        }

        UpdateDisplay();
    }

    private void Start()
    {
        if (gameClock == null)
        {
            gameClock = FindAnyObjectByType<GameClock>();
            if (gameClock != null)
            {
                gameClock.MinuteChanged -= OnMinuteChanged;
                gameClock.MinuteChanged += OnMinuteChanged;
                gameClock.DayChanged -= OnDayChanged;
                gameClock.DayChanged += OnDayChanged;
            }
        }

        UpdateDisplay();
    }

    private void OnDisable()
    {
        if (gameClock != null)
        {
            gameClock.MinuteChanged -= OnMinuteChanged;
            gameClock.DayChanged -= OnDayChanged;
        }
    }

    private void OnMinuteChanged(GameTime time)
    {
        UpdateTimeText(time);
    }

    private void OnDayChanged(GameTime time)
    {
        UpdateTimeText(time);
    }

    public void UpdateDisplay()
    {
        if (gameClock != null)
        {
            UpdateTimeText(gameClock.CurrentTime);
        }
    }

    private void UpdateTimeText(GameTime time)
    {
        if (timeText == null)
            return;

        timeText.text = FormatTime(time);
    }

    public static string FormatTime(GameTime time)
    {
        return FormatTime(time.Day, time.Hour, time.Minute);
    }

    public static string FormatTime(long day, int hour, int minute)
    {
        string amPm = hour < 12 ? "AM" : "PM";
        int displayHour = hour % 12;
        if (displayHour == 0)
            displayHour = 12;

        return $"Day {day} / {displayHour}:{minute:D2} {amPm}";
    }
}
