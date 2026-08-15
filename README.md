# SCP Replacer — LabAPI Port

SCP Replacer lets eligible non-SCP players volunteer to replace an SCP that leaves near the beginning of a round. This branch is a **LabAPI-only** port: the project has no EXILED package reference, uses LabAPI wrappers and event handlers, and is built for **.NET Framework 4.8**.

> The port deliberately supports **vanilla SCP roles only**. EXILED CustomRoles was removed because it has no LabAPI-only equivalent. When a custom-role SCP leaves, this port does not offer that custom role as a replacement.

## Behaviour

| Situation | Result |
| --- | --- |
| A supported vanilla SCP dies or is C.A.S.S.I.E.-terminated within `death_cutoff` | Non-SCP players receive a broadcast and console message explaining how to volunteer. |
| A player uses `.volunteer <scp number>` before `replace_cutoff` | The player enters that SCP's one-time lottery. |
| The lottery period expires | One currently alive, non-SCP volunteer is selected and assigned the departed SCP's vanilla `RoleTypeId`. |
| An SCP uses `.human` close to round start | The player becomes Class-D, Scientist, or Facility Guard. This does not create a replacement opportunity. |
| The departing role is SCP-049-2 | It is not eligible for replacement, matching the original plugin's behaviour. |

## Requirements

The server must be running a compatible version of [Northwood's LabAPI](https://github.com/northwood-studios/LabAPI). The project targets `net48` and uses `Northwood.LabAPI` version `1.1.7`. No EXILED or EXILED CustomRoles assemblies are required.

A build machine must also have the **SCP: Secret Laboratory dedicated-server managed assemblies** available. The project references these assemblies through the `SL_REFERENCES` and `UNITY_REFERENCES` MSBuild properties.

## Build

Set both MSBuild properties to the dedicated server's `SCPSL_Data\Managed` folder, then build the Release configuration. The following PowerShell example assumes the server is installed in `C:\SCPSL`.

```powershell
dotnet build .\SCPReplacer.csproj -c Release `
  -p:SL_REFERENCES="C:\SCPSL\SCPSL_Data\Managed" `
  -p:UNITY_REFERENCES="C:\SCPSL\SCPSL_Data\Managed"
```

The output DLL is written to `bin\Release\net48\SCPReplacer.dll`. Copy only this plugin DLL to a plugin directory configured in LabAPI. LabAPI resolves its directories from `PluginPaths`; its default configuration includes the `global` and `$port` locations.

## PingPlayers Workflow

PingPlayers gives you server-file access through its panel File Manager or SFTP. Use that access to download the five server DLLs named below from `SCPSL_Data\Managed`, because the compiler must reference the same server build that PingPlayers is running. In this project folder, create a `References` directory and put the downloaded DLLs in it.

| Copy from PingPlayers `SCPSL_Data\Managed` | Save locally as |
| --- | --- |
| `Assembly-CSharp.dll` | `References\Assembly-CSharp.dll` |
| `Assembly-CSharp-firstpass.dll` | `References\Assembly-CSharp-firstpass.dll` |
| `CommandSystem.Core.dll` | `References\CommandSystem.Core.dll` |
| `Mirror.dll` | `References\Mirror.dll` |
| `UnityEngine.CoreModule.dll` | `References\UnityEngine.CoreModule.dll` |

Install the current [.NET SDK](https://dotnet.microsoft.com/download) on your Windows PC, then double-click `build_for_pingplayers.bat`. When the script reports success, upload `bin\Release\net48\SCPReplacer.dll` using the PingPlayers File Manager or SFTP. LabAPI’s standard target is the server AppData path `SCP Secret Laboratory\LabAPI\plugins\global`; if your server’s `PluginPaths` configuration differs, use the corresponding configured folder instead. Restart the server and inspect the startup console for `SCP Replacer v2.0.0 enabled.`

## Configuration

LabAPI creates the plugin configuration file the first time it loads the DLL. The main settings are shown below. All player-facing messages are also configurable in that file.

| Key | Default | Meaning |
| --- | ---: | --- |
| `is_enabled` | `true` | Enables or disables the plugin's event handlers. |
| `death_cutoff` | `60` | Maximum seconds after round start in which an SCP death or C.A.S.S.I.E. termination can create a volunteer opportunity. |
| `replace_cutoff` | `90` | Maximum seconds after round start in which players may use `.volunteer`. |
| `lottery_period_seconds` | `10` | Seconds to wait after the first volunteer before selecting a winner. |

## Commands

| Command | Aliases | Description |
| --- | --- | --- |
| `.volunteer <SCP number>` | `.v <SCP number>` | Enters the lottery for an eligible departed SCP. Inputs such as `079` and `SCP-079` are both accepted. |
| `.human` | `.no` | Allows an SCP to become a random human role early in the round; it does not create a replacement opportunity. |

## Migration Notes

| Previous EXILED dependency | LabAPI replacement |
| --- | --- |
| `Plugin<Config, Translations>` | `Plugin<Config>` with message strings consolidated into `Config`. |
| `Exiled.Events.Handlers.Server/Player` | `LabApi.Events.Handlers.ServerEvents` and `PlayerEvents`. |
| EXILED left-player detection | LabAPI's `PlayerEvents.Death`, using the preserved pre-death SCP role. |
| EXILED broadcast, console, role, item, and effect helpers | The corresponding LabAPI player wrapper methods. |
| EXILED CustomRoles | Removed to retain a LabAPI-only dependency surface. |

The lottery delay uses MEC's `Timing.CallDelayed`, which is provided by SCP: Secret Laboratory's server assemblies and is used in the official LabAPI examples. It is not an EXILED dependency.

## References

1. [LabAPI repository and official examples](https://github.com/northwood-studios/LabAPI)
2. [Northwood.LabAPI 1.1.7 package](https://www.nuget.org/packages/Northwood.LabAPI/1.1.7)
