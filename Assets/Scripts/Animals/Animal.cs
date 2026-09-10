using UnityEngine;

public class Animal : MonoBehaviour
{
    [Header("Animal Data")]
    [SerializeField] private AnimalData animalData;

    [Header("Identity")]
    [SerializeField] private string animalName = "Unnamed";

    [Header("Individual Stats")]
    [SerializeField] private float foodLevel = 50f;
    [SerializeField] private float health = 100f;
    [SerializeField] private float ageYears = 0f;
    [SerializeField] private AnimalSex sex = AnimalSex.Unknown;

    private GameClock subscribedClock;
    private GameTime lastClockTime;

    public float FoodLevel => foodLevel;
    public float Health => health;
    public float AgeYears => ageYears;
    public AnimalSex Sex => sex;

    private void OnEnable()
    {
        SubscribeToClock();
    }

    private void Start()
    {
        // Retry after scene Awake/OnEnable calls have completed.
        SubscribeToClock();
        if (subscribedClock == null)
            Debug.LogWarning($"{animalName}: no active GameClock found; hunger will not advance.", this);
    }

    private void SubscribeToClock()
    {
        if (subscribedClock != null)
            return;

        subscribedClock = FindAnyObjectByType<GameClock>();
        if (subscribedClock == null)
            return;

        lastClockTime = subscribedClock.CurrentTime;
        subscribedClock.MinuteChanged += OnGameMinuteChanged;
    }

    private void OnDisable()
    {
        if (subscribedClock != null)
            subscribedClock.MinuteChanged -= OnGameMinuteChanged;
        subscribedClock = null;
    }

    private void OnGameMinuteChanged(GameTime time)
    {
        double elapsedMinutes = (time.Day - lastClockTime.Day) * 1440.0 +
            (time.Hour - lastClockTime.Hour) * 60 + time.Minute - lastClockTime.Minute;
        lastClockTime = time;

        if (animalData != null && elapsedMinutes > 0)
            foodLevel = animalData.CalculateFoodAfterElapsedHours(foodLevel, elapsedMinutes / 60.0);
    }

    public string AnimalName => animalName;

    public AnimalData AnimalData => animalData;

    public SpeciesData Species =>
        animalData != null ? animalData.species : null;

    public void Feed(float amount)
    {
        // Reject invalid numeric inputs rather than permanently poisoning saved food state.
        if (animalData == null || float.IsNaN(amount) || float.IsInfinity(amount) ||
            float.IsNaN(foodLevel) || float.IsInfinity(foodLevel) ||
            float.IsNaN(animalData.baseMaxHunger) || float.IsInfinity(animalData.baseMaxHunger) ||
            animalData.baseMaxHunger < 0f)
            return;

        foodLevel += amount;

        foodLevel = Mathf.Clamp(
            foodLevel,
            0f,
            animalData.baseMaxHunger
        );

        Debug.Log(
            $"{animalName} ate! Food: " +
            $"{foodLevel}/{animalData.baseMaxHunger}"
        );
    }

    public AnimalInstance GetAnimalInstance()
    {
        return new AnimalInstance(
            animalData,
            animalName,
            foodLevel,
            health,
            ageYears,
            sex
        );
    }

    public void LoadFromInstance(AnimalInstance instance)
    {
        if (instance == null)
            return;

        animalData = instance.animalData;
        animalName = instance.animalName;
        foodLevel = instance.foodLevel;
        health = instance.health;
        ageYears = instance.ageYears;
        sex = instance.sex;

        // Loading is an exact transfer, not a needs tick or offline catch-up.
        if (subscribedClock == null)
            SubscribeToClock();

        if (subscribedClock != null)
            lastClockTime = subscribedClock.CurrentTime;
    }
}
