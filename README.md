# SCP Replacer — LabAPI Port

SCP Replacer lets eligible non-SCP players volunteer to replace a supported SCP role that becomes available near the beginning of a round. This is a **LabAPI-only** port: it has no EXILED package reference, uses LabAPI wrappers and events, and targets **.NET Framework 4.8**.

> The plugin supports **vanilla SCP roles only**. EXILED CustomRoles was removed because it has no LabAPI-only equivalent.

## Behaviour

| Trigger during the early-round window | Result |
| --- | --- |
| A supported SCP dies | Non-SCP players receive a broadcast and console message explaining how to volunteer. |
| A supported SCP is C.A.S.S.I.E.-terminated | The same replacement opportunity is created. |
| A supported SCP uses `.human` / `.no` | The player becomes a random human role and a replacement opportunity is created for their former SCP role. |
| A player uses `.volunteer <SCP number>` before `replace_cutoff` | The player enters that SCP role's one-time lottery. |
| The lottery period expires | One currently alive, non-SCP volunteer is selected and assigned the corresponding vanilla SCP role. |

The supported roles are **SCP-049, SCP-079, SCP-096, SCP-106, SCP-173, SCP-939, and SCP-3114**. SCP-049-2 is intentionally excluded.


## Installation

Install `SCPReplacer.dll` at https://github.com/kyomuteki/SCPReplacer/releases/tag/1.1.0 put it in LabAPI/plugins/global

## Configuration

LabAPI creates the plugin configuration file the first time it loads the DLL. All player-facing messages can be configured there.

| Key | Default | Meaning |
| --- | ---: | --- |
| `is_enabled` | `true` | Enables or disables the plugin's event handlers. |
| `death_cutoff` | `60` | Maximum seconds after round start in which an SCP death, C.A.S.S.I.E. termination, or `.human` / `.no` opt-out can create a volunteer opportunity. |
| `replace_cutoff` | `90` | Maximum seconds after round start in which players may use `.volunteer`. |
| `lottery_period_seconds` | `10` | Seconds to wait after the first volunteer before selecting a winner. |

For example, set `death_cutoff: 120` to enable all three replacement triggers during the first two minutes of a round.

## Commands

| Command | Aliases | Description |
| --- | --- | --- |
| `.volunteer <SCP number>` | `.v <SCP number>` | Enters the lottery for an available SCP role. Inputs such as `079` and `SCP-079` are both accepted. |
| `.human` | `.no` | Lets an eligible SCP become a random human role early in the round and creates a replacement opportunity for that former SCP role. |

