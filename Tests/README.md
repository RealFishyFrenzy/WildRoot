# WildRoot automated verification

Run from the repository root with the .NET 10 SDK:

```powershell
dotnet run --project Tests/InventoryLogic/InventoryLogic.csproj
dotnet run --project Tests/GameClock/GameClock.Tests.csproj
dotnet run --project Tests/AnimalHunger/AnimalHunger.Tests.csproj
dotnet build Tests/ScriptCompilation/ScriptCompilation.csproj --no-incremental --verbosity minimal
```

After the first successful restore, append `--no-restore` to avoid restoring again.
For executable-lock avoidance, append `-p:UseAppHost=false` to the run commands.
Outputs go to the ignored Temp directory. These commands do not launch Unity,
change scenes, or modify assets and metadata.

The script compilation project references installed Unity 6000.5.0f1 engine DLLs
and this project's already imported UI/TMP DLLs under Library/ScriptAssemblies.
Override the editor path with `-p:UnityEditorDirectory="path/to/Editor"` if needed.
The check requires those DLLs to exist. It uses .NET 10 for a static compile and
does not replace Unity's own compilation, serialization, or player-build checks.
CS0649/CS0414 warnings for Inspector-configured fields are suppressed; other
compiler warnings remain enabled.

Inventory and hunger tests use minimal Unity stand-ins. The hunger suite exercises
the actual clock, animal, catch, storage and enclosure code with simulated lifecycle
calls. It covers missing data, distinct hunger rates, exact transfer preservation,
pause, disable/re-enable, storage freeze, and invalid numeric feeding inputs.
Unity object destruction semantics, physics, UI clicks, prefab loading, and scene
bindings still require Play Mode verification. See each suite's README for details.

The full compilation recursively includes Assets/Scripts, including AnimalInfoUI.
The hunger harness does not include the nested animal UI scripts: its passing
results are not automated coverage of Animal Info clicks, labels or lifecycle.
See Docs/WildRoot-0.0.4-status.md for the latest release-validation results and
manual acceptance checklist. Automated passes do not declare a release.
