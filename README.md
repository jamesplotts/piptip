# PipTip

Source: https://github.com/jamesplotts/piptip

A Railroader mod for the 0.6 "Maintenance of Way" built-in mod loader. It works on the row of car pips below the selected engine's name, in the controls panel at the bottom of the screen.

- **Hover a pip** to see that car in a callout styled like the Tab tags, colored by the car's destination. The callout shows:
  - the car's destination or train;
  - its freight (or Empty), or its passengers;
  - its condition and wheel oil, in amber under 70% and red under 40%;
  - its handbrake, hotbox and derailed warnings.

  Hovering an engine or its tender shows the pair together: the engine's info, the coal and water (or fuel), and the condition and oil of both cars.
- **Right-click a pip** to open the game's own car menu: Inspect, Select/Deselect, Bleed, and Apply/Release Handbrake. The menu adds one more item, **Disconnect from Train**, which uncouples the car from its neighbor on the engine side. It closes both angle cocks first, the same as holding the game's smart-air modifier while you click a coupler, so the train doesn't go into emergency. An engine, or a steam engine's own tender, gets **Disconnect Cars from Engine** in its place. It uncouples the cars from the engine and tender the same way, and sets the handbrake on the first car. It leaves other engines coupled so an MU lash-up stays together. If the engine has cars on both sides, the item isn't shown.

## Install

1. In Railroader, turn on **Preferences > Features > Experimental > Allow Unsafe DLL Mods**, then restart the game.
2. Unzip `PipTip_<version>.zip` into `Documents\Railroader\Mods\`. You should end up with `Mods\PipTip\` holding `Info.json`, `PipTip.dll` and `0Harmony.dll`.
   - On Linux with Proton, that's the same folder inside the game's prefix: `steamapps/compatdata/1683150/pfx/drive_c/users/steamuser/Documents/Railroader/Mods/`.
3. On the main menu, open **Mods** and enable PipTip.

## Build

You need the .NET SDK (6 or later) and the game installed. The project compiles against the game's own DLLs, and none of them are committed.

```sh
cp Paths.user.example Paths.user   # then set GameDir (and ModDevDir) for your machine
dotnet build PipTip/PipTip.csproj              # Debug: copies the mod to ModDevDir
dotnet build PipTip/PipTip.csproj -c Release   # writes bin/PipTip_<version>.zip
```

Debug builds go to `Documents\Railroader\ModDev\PipTip`, which the game loads as a development pack. The game only loads a rebuilt DLL after a restart. The version number is set in `Directory.Build.props`.

## How it works

- The strip is drawn by `UI.TrainBrakeDisplay`. A Harmony postfix on its `GetCarImage` adds a `PipHover` component to each pip, which handles the pointer events. The air-joint markers overlap the pips, so they're made click-through.
- The callout is a copy of the Tab tag's callout prefab, drawn on the game's tooltip canvas. If that prefab ever goes missing, the mod falls back to the game's plain tooltip callout.
- Everything else uses the game's own calls: `CarInspector.Show`, `SetBleed`, `SetHandbrake`, and the coupler's cut lever and angle cocks. That keeps it working in multiplayer the same way the game's own buttons do.
- The patched methods aren't part of the supported modding API. If a game update changes them, PipTip logs `patching failed, mod inactive` in Player.log and leaves the game alone.

## License

MIT. Harmony (`0Harmony.dll`) is MIT-licensed, by Andreas Pardeike; its license is in `LICENSE-Harmony.txt`.
