using CommandSystem;
using LabApi.Features.Wrappers;
using PlayerRoles;
using System;
using System.Linq;

namespace SCPReplacer;

[CommandHandler(typeof(ClientCommandHandler))]
public sealed class Volunteer : ICommand
{
    public string Command => "volunteer";

    public string[] Aliases => new[] { "v" };

    public string Description => "Volunteer to become a vanilla SCP that left at the beginning of the round.";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        Plugin? plugin = Plugin.Instance;
        if (plugin == null)
        {
            response = "SCP Replacer is not enabled.";
            return false;
        }

        if (arguments.Count != 1)
        {
            response = plugin.Config.WrongUsageMessage;
            return false;
        }

        if (plugin.HasReplacementCutoffPassed())
        {
            response = plugin.Config.TooLateMessage;
            return false;
        }

        Player? player = Player.Get(sender);
        if (player == null)
        {
            response = "You must be a player to use this command.";
            return false;
        }

        string requestedScp = arguments.First().ScpNumber();
        ScpToReplace? pendingRole = plugin.ScpsAwaitingReplacement.FirstOrDefault(role => role.Name == requestedScp);
        if (pendingRole == null)
        {
            response = plugin.ScpsAwaitingReplacement.Count == 0
                ? plugin.Config.NoEligibleSCPsError
                : plugin.Config.InvalidSCPError + string.Join(", ", plugin.ScpsAwaitingReplacement);
            return false;
        }

        if (player.IsSCP && player.Role != RoleTypeId.Scp0492)
        {
            response = "SCPs cannot use this command.";
            return false;
        }

        if (!pendingRole.Volunteers.Add(player))
        {
            response = "You have already volunteered to replace this SCP.";
            return false;
        }

        plugin.ScheduleLottery(pendingRole);

        response = $"You have entered the lottery to become SCP-{pendingRole.Name}.";
        player.SendBroadcast(
            plugin.Config.BroadcastHeader + plugin.Config.EnteredLotteryBroadcast.Replace("%NUMBER%", pendingRole.Name),
            5,
            shouldClearPrevious: true);
        return true;
    }
}

[CommandHandler(typeof(ClientCommandHandler))]
public sealed class HumanCommand : ICommand
{
    public string Command => "human";

    public string[] Aliases => new[] { "no" };

    public string Description => "Forfeit being an SCP and become a random human class near the start of the round, creating a volunteer opportunity for that SCP role.";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        Plugin? plugin = Plugin.Instance;
        if (plugin == null)
        {
            response = "SCP Replacer is not enabled.";
            return false;
        }

        Player? scpPlayer = Player.Get(sender);
        if (scpPlayer == null)
        {
            response = "You must be a player to use this command.";
            return false;
        }

        if (!scpPlayer.IsSCP)
        {
            response = "You must be an SCP to use this command.";
            return false;
        }

        if (scpPlayer.Role == RoleTypeId.Scp0492)
        {
            response = "SCP-049-2 cannot use this command.";
            return false;
        }

        double elapsedSeconds = Round.Duration.TotalSeconds;
        if (elapsedSeconds > plugin.Config.DeathCutoff)
        {
            response = "This command must be used closer to the start of the round.";
            return false;
        }

        // Deaths, C.A.S.S.I.E. terminations, and this early .human opt-out create replacements.

        RoleTypeId forfeitedScpRole = scpPlayer.Role;
        RoleTypeId newRole = UnityEngine.Random.value switch
        {
            < 0.45f => RoleTypeId.ClassD,
            < 0.90f => RoleTypeId.Scientist,
            _ => RoleTypeId.FacilityGuard,
        };

        scpPlayer.DisableAllEffects();
        scpPlayer.SetRole(newRole);
        plugin.CreateReplacementOpportunity(scpPlayer.Nickname, forfeitedScpRole);

        if (newRole == RoleTypeId.ClassD)
        {
            scpPlayer.AddItem(ItemType.Flashlight);
            scpPlayer.AddItem(ItemType.Coin);
        }

        string roleName = newRole.GetDisplayName();
        response = $"You became a {roleName}.";
        scpPlayer.SendBroadcast(
            plugin.Config.BroadcastHeader + $"You became a <color={newRole.GetDisplayColor()}>{roleName}</color>.",
            10,
            shouldClearPrevious: true);
        return true;
    }
}
