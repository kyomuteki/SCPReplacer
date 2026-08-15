using LabApi.Features.Wrappers;
using PlayerRoles;
using System.Collections.Generic;

namespace SCPReplacer;

/// <summary>
/// Represents a vanilla SCP role that became available after its player left early in the round.
/// </summary>
public sealed class ScpToReplace
{
    public ScpToReplace(string name, RoleTypeId role)
    {
        Name = name;
        Role = role;
    }

    /// <summary>
    /// Gets the numerical SCP identifier presented to players, such as <c>079</c>.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the vanilla SCP role to assign to the lottery winner.
    /// </summary>
    public RoleTypeId Role { get; }

    /// <summary>
    /// Gets players who entered this replacement lottery.
    /// </summary>
    public HashSet<Player> Volunteers { get; } = new();

    /// <summary>
    /// Gets or sets whether the one-time lottery delay has already been scheduled.
    /// </summary>
    public bool LotteryScheduled { get; set; }

    public override string ToString() => $"SCP-{Name}";
}
