# WildRoot game clock

## Unity setup

SampleScene already has GameClock on TimeManager, configured for a 25-minute day
starting at 06:00. Verify exactly one active clock; do not add another for validation.
Other scenes require their own deliberate setup.

Defaults: Day 1, 06:00, with 25 real minutes per game day at timeScale 1.
Starting Hour, Starting Minute, and Real Minutes Per Day are configurable in the
Inspector before play. Configuration is read once when the clock initializes.
One game minute takes approximately 1.041667 scaled seconds at the default rate.

Update supplies Time.deltaTime to the clock. timeScale 0 pauses it; other timeScale
values scale its speed. Disabling the component also stops advancement. There is
no offline catch-up, save/load, or cross-scene persistence yet.

The initial time is available immediately; notifications/logs begin at the next
minute boundary. Each crossed minute logs a line such as `Day 1 - 06:01`.

## Observing the clock

Future components can hold a reference to GameClock and subscribe to
MinuteChanged, HourChanged, or DayChanged in OnEnable, then unsubscribe in
OnDisable. Each event receives an immutable GameTime with Day, Hour, and Minute.
Read CurrentTime for the initial state; subscribing does not replay past events.
On a shared boundary notifications occur in minute, hour, day order, after the
clock state has been updated. Long frames emit every crossed boundary in order.
Observers should not call Advance recursively from clock-state callbacks.

## Automated checks

With the .NET 10 SDK, run from the repository root:

```powershell
dotnet run --project Tests/GameClock/GameClock.Tests.csproj
```

Tests compile the actual clock arithmetic without Unity stand-ins. They cover
start configuration, zero elapsed time, 25-minute days, hour/day rollover,
notification order/counts, frame accumulation, catch-up, and invalid input.

## Play Mode checks

- Verify the existing clock emits minute logs once per boundary.
- Confirm 23:59 rolls over to the next day at 00:00. Discard any temporary test
  settings rather than saving changes to the release scene.
- Set Time.timeScale to zero through a debugger or temporary test script: no
  clock logs should occur during the pause. Restore it to 1 and verify resumption.
- At timeScale 1, confirm a full 24-hour cycle takes approximately 25 real minutes.

World-animal hunger now observes this clock, and Animal Info observes minute
events as well as refreshing live values. Crops, weather, lighting and sleeping
are not implemented by the clock. There is no player-facing clock display yet.
