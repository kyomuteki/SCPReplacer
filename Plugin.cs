using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Plugins;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SCPReplacer;

/// <summary>
/// Lets non-SCP players volunteer to replace a vanilla SCP eliminated early in a round.
/// </summary>
public sealed class Plugin : Plugin<Config>
{
    /// <summary>
    /// Gets the active plugin instance, allowing commands to use the shared pending-replacement state.
    /// </summary>
    public static Plugin? Instance { get; private set; }

    /// <summary>
    /// Gets SCP roles that are eligible to be claimed by a volunteer.
    /// </summary>
    public List<ScpToReplace> ScpsAwaitingReplacement { get; } = new();

    private bool _isEnabled;

    public override string Name => "SCP Replacer";

    public override string Description => "Allows players to volunteer to replace vanilla SCPs eliminated early in a round.";

    public override string Author => "Jon M";

    public override Version RequiredApiVersion { get; } = new(LabApiProperties.CompiledVersion);

    public override void Enable()
    {
        if (!Config.IsEnabled)
        {
            Logger.Info($"{Name} is disabled by configuration.");
            return;
        }

        Instance = this;
        _isEnabled = true;

        ServerEvents.RoundStarted += OnRoundStarted;
        PlayerEvents.Death += OnPlayerDeath;

        Logger.Info($"{Name} v{Version} enabled with death-based replacement detection.");
    }

    public override void Disable()
    {
        ServerEvents.RoundStarted -= OnRoundStarted;
        PlayerEvents.Death -= OnPlayerDeath;

        _isEnabled = false;
        ScpsAwaitingReplacement.Clear();
        Instance = null;

        Logger.Info($"{Name} disabled.");
    }

    private void OnRoundStarted()
    {
        ScpsAwaitingReplacement.Clear();
    }

    /// <summary>
    /// Opens a volunteer opportunity when a vanilla SCP dies or is terminated during the configurable early-round window.
    /// PlayerDeathEventArgs preserves OldRole even after the player becomes a spectator.
    /// </summary>
    private void OnPlayerDeath(PlayerDeathEventArgs ev)
    {
        if (!IsReplaceableScp(ev.OldRole))
            return;

        CreateReplacementOpportunity(ev.Player.Nickname, ev.OldRole);
    }

    /// <summary>
    /// Creates a volunteer opportunity for a supported SCP role within the configurable early-round window.
    /// Used by SCP death, C.A.S.S.I.E. termination, and the .human command.
    /// </summary>
    public void CreateReplacementOpportunity(string nickname, RoleTypeId role)
    {
        double elapsedSeconds = Round.Duration.TotalSeconds;
        string scpNumber = role.ScpNumber();

        Logger.Info($"Creating a replacement opportunity for SCP-{scpNumber} from {nickname} at {elapsedSeconds:F1} seconds into the round.");

        if (elapsedSeconds > Config.DeathCutoff)
        {
            Logger.Info("The replacement opportunity is not eligible because the early-round cutoff has passed.");
            return;
        }

        if (ScpsAwaitingReplacement.Any(pendingRole => pendingRole.Role == role))
        {
            Logger.Info($"SCP-{scpNumber} already has an open volunteer opportunity.");
            return;
        }

        foreach (Player player in Player.List.Where(player => !player.IsSCP))
        {
            string message = Config.ReplaceBroadcast.Replace("%NUMBER%", scpNumber);
            player.SendBroadcast(Config.BroadcastHeader + message, 16, shouldClearPrevious: true);
            player.SendConsoleMessage(message, "yellow");
        }

        ScpsAwaitingReplacement.Add(new ScpToReplace(scpNumber, role));
        Logger.Info($"SCP-{scpNumber} is now available for volunteer replacement.");
    }

    /// <summary>
    /// Starts a single, delayed lottery for the supplied role.
    /// </summary>
    public void ScheduleLottery(ScpToReplace role)
    {
        if (role.LotteryScheduled)
            return;

        role.LotteryScheduled = true;
        Timing.CallDelayed(Config.LotteryPeriodSeconds, () => ResolveLottery(role));
    }

    private void ResolveLottery(ScpToReplace role)
    {
        if (!_isEnabled || !ScpsAwaitingReplacement.Contains(role))
            return;

        role.Replace();
    }

    /// <summary>
    /// Gets whether the player-volunteer period is over.
    /// </summary>
    public bool HasReplacementCutoffPassed() => Round.Duration.TotalSeconds > Config.ReplaceCutoff;

    private static bool IsReplaceableScp(RoleTypeId role)
    {
        return role is RoleTypeId.Scp049
            or RoleTypeId.Scp079
            or RoleTypeId.Scp096
            or RoleTypeId.Scp106
            or RoleTypeId.Scp173
            or RoleTypeId.Scp939
            or RoleTypeId.Scp3114;
    }
}
