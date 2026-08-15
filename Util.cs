using LabApi.Features.Wrappers;
using PlayerRoles;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace SCPReplacer;

public static class Util
{
    /// <summary>
    /// Extracts the numerical SCP identifier from a vanilla role name.
    /// </summary>
    public static string ScpNumber(this RoleTypeId role) => Regex.Replace(role.ToString(), @"[^0-9]", string.Empty);

    /// <summary>
    /// Extracts the numerical SCP identifier from player input such as <c>SCP-079</c>.
    /// </summary>
    public static string ScpNumber(this string value) => Regex.Replace(value, @"[^0-9]", string.Empty);

    /// <summary>
    /// Gets a concise, player-facing name for the human roles this plugin can assign.
    /// </summary>
    public static string GetDisplayName(this RoleTypeId role) => role switch
    {
        RoleTypeId.ClassD => "Class-D Personnel",
        RoleTypeId.Scientist => "Scientist",
        RoleTypeId.FacilityGuard => "Facility Guard",
        _ => role.ToString(),
    };

    /// <summary>
    /// Gets a rich-text color for the human roles this plugin can assign.
    /// </summary>
    public static string GetDisplayColor(this RoleTypeId role) => role switch
    {
        RoleTypeId.ClassD => "#ff9900",
        RoleTypeId.Scientist => "#ffff00",
        RoleTypeId.FacilityGuard => "#3f9efc",
        _ => "#ffffff",
    };

    /// <summary>
    /// Resolves a volunteer lottery and assigns the selected eligible player to the original vanilla SCP role.
    /// </summary>
    public static void Replace(this ScpToReplace pendingRole)
    {
        Plugin? plugin = Plugin.Instance;
        if (plugin == null)
            return;

        Player[] eligibleVolunteers = pendingRole.Volunteers
            .Where(player => !player.IsDestroyed && player.IsAlive && !player.IsSCP)
            .ToArray();

        plugin.ScpsAwaitingReplacement.Remove(pendingRole);
        pendingRole.Volunteers.Clear();

        if (eligibleVolunteers.Length == 0)
        {
            LabApi.Features.Console.Logger.Info($"No eligible volunteers remained to replace SCP-{pendingRole.Name}.");
            return;
        }

        Player chosenPlayer = eligibleVolunteers[UnityEngine.Random.Range(0, eligibleVolunteers.Length)];

        // Remove effects before assigning the SCP role so a spectator's effects cannot carry over.
        chosenPlayer.DisableAllEffects();
        chosenPlayer.SetRole(pendingRole.Role);

        foreach (Player player in Player.List)
        {
            if (player == chosenPlayer)
            {
                player.SendBroadcast(
                    plugin.Config.BroadcastHeader + plugin.Config.ChangedSuccessfullySelfBroadcast.Replace("%NUMBER%", pendingRole.Name),
                    5,
                    shouldClearPrevious: true);
                continue;
            }

            player.SendBroadcast(
                plugin.Config.BroadcastHeader + plugin.Config.ChangedSuccessfullyEveryoneBroadcast.Replace("%NUMBER%", pendingRole.Name),
                5,
                shouldClearPrevious: true);
        }

        LabApi.Features.Console.Logger.Info($"{chosenPlayer.Nickname} has replaced SCP-{pendingRole.Name}.");
    }
}
