# World animal hunger foundation

AnimalData exposes Food Loss Per Game Hour. Its code default is zero, but the
current Goldfish asset is configured at 5 food per game hour. Hunger changes on GameClock minute
events (rate / 60 for each elapsed game minute). Values are clamped to 0 through
baseMaxHunger. Health, age, sex and every other stat are untouched.

Normal world lifecycle subscription occurs on enable. Catching disables the component
before copying its state so deferred destruction cannot apply another needs tick.
Net inventory and tank AnimalInstance records have no ticking behavior. Loading
copies food exactly and starts observing from the clock's current minute, with no
catch-up for time in storage. Timing is quantized to the clock's minute boundaries.
LoadFromInstance also attempts subscription when disconnected, even on a disabled
component; that edge case remains an audit follow-up, not a verified guarantee.

Use one active GameClock in the scene before animals start. Missing clock means
no progression and a warning; enabling/re-enabling the animal connects it again.
There is no fallback timer. SpeciesData is not required for hunger: rates belong
to AnimalData. Missing AnimalData means hunger is skipped; missing species names
display Unknown Species. Transfers themselves never clamp or recalculate food.

Run from the project root with .NET 10:

```powershell
dotnet run --project Tests/AnimalHunger/AnimalHunger.Tests.csproj
```

The tests compile the actual GameClock, clock arithmetic, Animal, CatchableAnimal,
AnimalInventory, AnimalEnclosure and animal data code. Minimal Unity stand-ins
simulate lifecycle calls and elapsed time. Test rates are fixtures only.

## Unity Play Mode checks

- With the current Goldfish asset, verify world food drops by approximately 5 per
  game hour (until clamped at zero) and stops when Time.timeScale is zero.
- Zero-rate behavior is covered by automated fixtures; no rebalance is needed.
- Verify world -> catch -> net -> tank -> remove -> world -> catch again. Food
  must remain exact during transfers and unchanged while waiting in net/tank.
- Verify cancelling placement does not modify stored food.
- Verify missing data/species does not produce null-reference exceptions.
- Verify enabled world animals subscribe once and stop after capture/disable.

Automated tests do not exercise Unity physics, actual prefab instantiation,
NetUI/EnclosureUI clicks, or serialized scene bindings. Those require Play Mode.
