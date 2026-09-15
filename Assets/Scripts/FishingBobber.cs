using System.Runtime.CompilerServices;
using UnityEngine;

public class FishingBobber : MonoBehaviour
{
    [Header("Bite Timing")]
    [SerializeField] private float minBitDelay = 2f;
    [SerializeField] private float maxBitDelay = 6f;
    [SerializeField] private float biteDuration = 1.5f;

    [Header("Bite Movement")]
    [SerializeField] private float bounceHeight = 0.12f;
    [SerializeField] private float bounceSpeed = 12f;
    [SerializeField] private float shakeAmount = 0.04f;

    private Vector3 restingPos;
    private float biteTimer;
    private float biteTimeRemaining;

    public bool HasBite {  get; private set; }

    private void Start()
    {
        restingPos = transform.position;
        ScheduleNextBite();
    }

    private void Update()
    {
        if (!HasBite)
        {
            biteTimer -= Time.deltaTime;

            if (biteTimer <= 0f)
                StartBite();

            return;
        }

        biteTimeRemaining -= Time.deltaTime;

        float bounce = Mathf.Sin(Time.time * bounceSpeed) * bounceHeight;

        float shake = Mathf.Sin(Time.time * bounceSpeed * 1.7f) * shakeAmount;

        transform.position = restingPos + new Vector3 (shake, bounce, 0f);
        
        if (biteTimeRemaining <= 0f)
            EndBite();
    }

    private void ScheduleNextBite()
    {
        HasBite = false;

        biteTimer = Random.Range(minBitDelay, maxBitDelay);

        transform.position = restingPos;
    }

    private void StartBite()
    {
        HasBite = true;
        biteTimeRemaining = biteDuration;
    }

    private void EndBite()
    {
        transform.position = restingPos;
        ScheduleNextBite();
    }
}
