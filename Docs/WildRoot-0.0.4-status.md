# WildRoot 0.0.4 release validation status

The user confirmed manual Unity Play Mode acceptance passed and explicitly
approved committing, tagging v0.0.4 and pushing the complete release candidate.
Automated validation passed as recorded below. No 0.0.5 work is included.

## Current configuration (2026-09-09)

- The audited 0.0.3 baseline is 6e3e9fcfca50bff04a1ce77e066f5ab601e7c629,
  identified by the remote v0.0.3 tag and version-change history.
- ProjectSettings currently declares bundleVersion 0.0.4, unlike the earlier
  audit snapshot (0.0.3). This validation task did not change that setting.
- SampleScene already contains TimeManager/GameClock: Day 1, 06:00, with a
  25-real-minute day at timeScale 1. Do not add a second clock.
- AnimalData's hourly food-loss code default is zero. Goldfish explicitly uses
  5 food per game hour, with maximum food/health of 100. Earlier documentation
  claiming the current Goldfish rate is zero is superseded.
- Inventory and Animal Info panels are implemented and assigned in SampleScene.
  Animal Info displays name, species, remaining food (labelled Hunger), and health.
  Stored health, age and sex do not imply damage, aging or breeding systems.

## Automated verification

Fresh validation on 2026-09-09 using .NET SDK 10.0.301:

- Inventory/crafting: PASS, all 11 scenarios.
- GameClock: PASS, including pause, 25-minute day, rollover, event order,
  accumulation and invalid inputs.
- Animal hunger/transfer: PASS, including rates, simulated capture/storage,
  exact transfers, lifecycle, null data and invalid numeric inputs.
- Full non-incremental script compilation: PASS, zero warnings/errors. All 52
  Assets C# scripts are under the recursive compilation include, including the
  newest AnimalInfoUI, PlayerActions and Animal changes.
- First attempts were blocked by sandbox access to the local NuGet configuration;
  authorized retries succeeded. No gameplay/source changes were needed.

Commands: the three dotnet run commands in Tests/README.md with
-p:UseAppHost=false, and dotnet build Tests/ScriptCompilation/ScriptCompilation.csproj
--no-incremental --verbosity minimal. Outputs are in ignored Temp folders.

Compilation uses installed Unity/UI/TMP assemblies, not Unity's Editor build
pipeline. Inspector-field warnings CS0649/CS0414 are suppressed as configured.
Inventory/hunger tests use Unity stand-ins; Animal Info UI is compiled but has no
automated interaction test. These passes do not verify physics, real destruction,
serialized bindings, UI input or player builds. Manual Play Mode acceptance was
subsequently confirmed by the user; no standalone player-build verification is claimed.

## Implemented behavior and limits

- World animals observe clock minute events. Net/tank records do not tick.
- Transfers copy food and other individual state exactly, without storage catch-up.
- Missing AnimalData skips hunger; stored species names have safe fallbacks.
- Invalid non-finite hunger/feeding inputs are guarded. Already invalid food is
  not repaired by inventing replacement state.
- Inventory/crafting transactions protect against partial failures; pickups retain
  unaccepted quantities. Inventory and hotbar refresh from change notifications.
- Inventory blocks gameplay input. Net right-click priority, cancellation after
  switching away from the net, and tenth-slot selection with 0 are implemented.
- World placement no longer enforces the baseline water-only restriction.
  Validation did not introduce habitat, obstacle or placement-range rules.

## Manual Unity acceptance checklist

Acceptance passed, as reported by the user. The checklist below is retained as
the release acceptance scope, not a claim that the agent performed these checks.

- Let Unity import/compile; check Console, scene/prefab/data references, sprites
  and UI after asset moves. Confirm no missing scripts or references.
- Check minute logs, pause/resume, midnight rollover and approximately 25 real
  minutes per full day at timeScale 1. Discard temporary test settings.
- Check Goldfish food loss at 5/game hour and world -> catch -> net -> tank ->
  remove to net -> world -> catch again. Wait in storage; confirm frozen food,
  exact transfer values, pause behavior and no duplicate capture/subscription.
- Inspect Animal Info: non-net right-click opening, live feeding/time updates,
  close button/Escape, missing-data handling and animal removal.
- Check inventory open/close, movement/action blocking, UI click-through,
  E interaction, net right-click priority, tool-switch cancellation and keys 1-0.
- Check full/partial pickups and capacity-limited crafting: leftovers remain,
  failed crafts consume nothing, and inventory/hotbar update.
- Smoke-test tanks, resource tools, furnace, cave transitions and camera following.
  These existing systems need regression checks, not new features.

## Remaining/deferred work (not completed release features)

- Tank feeding/hunger, starvation, health loss, death, aging, breeding, additional
  needs, offline progression, visible clock, lighting, weather and sleep.
- Full-animal food consumption, action distance, habitat/obstacle placement rules,
  resource depletion and additional UI behavior require gameplay decisions.
- Multi-animal net selection is absent; the net currently exposes its first animal.
- Placement uses a single overlapping collider for tank detection.
- LoadFromInstance may subscribe a disabled Animal; targeted coverage is pending.
- Animal Info does not block gameplay like inventory and does not explicitly
  detect a disabled Animal component on an otherwise active GameObject.
- Late clock creation/replacement and persistence are not comprehensively supported.
  Normal setup expects an existing scene clock; no fallback timer was added.

## Completed asset reorganization

The approved moves preserved original asset/meta bytes and GUIDs. The historical
move audit verified 189 original metadata files plus 11 new folder metadata files
(200 at that time). The later release audit found 202, with no missing pairs,
orphans or duplicate GUIDs. These counts describe different snapshots.
See AssetReorganization.md and AssetReorganizationAudit.json. The user has now
confirmed manual Unity Play Mode acceptance passed for the release candidate.
