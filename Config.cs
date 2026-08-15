using System.ComponentModel;

namespace SCPReplacer;

/// <summary>
/// Configurable behavior and player-facing messages for the LabAPI plugin.
/// LabAPI serializes this class to the plugin configuration file.
/// </summary>
public sealed class Config
{
    [Description("Whether SCP Replacer should register its event handlers.")]
    public bool IsEnabled { get; set; } = true;

    [Description("The maximum seconds after round start in which an SCP death, C.A.S.S.I.E. termination, or .human opt-out can create a volunteer opportunity.")]
    public int DeathCutoff { get; set; } = 60;

    [Description("The maximum seconds after round start in which a player can use .volunteer.")]
    public int ReplaceCutoff { get; set; } = 90;

    [Description("The number of seconds after the first volunteer before the replacement lottery is resolved.")]
    public float LotteryPeriodSeconds { get; set; } = 10f;

    public string WrongUsageMessage { get; set; } = "Usage: .volunteer <SCP number>. Example: .volunteer 079 or .v 079";

    public string TooLateMessage { get; set; } = "It is too late in the game to replace an SCP.";

    public string ChangedSuccessfullySelfBroadcast { get; set; } = "You have replaced <color=red>SCP-%NUMBER%</color>";

    public string EnteredLotteryBroadcast { get; set; } = "You have entered the lottery to replace <color=red>SCP-%NUMBER%</color>";

    public string ChangedSuccessfullyEveryoneBroadcast { get; set; } = "<color=red>SCP-%NUMBER%</color> has been replaced";

    public string NoEligibleSCPsError { get; set; } = "No SCPs are currently eligible for replacement.";

    public string InvalidSCPError { get; set; } = "The SCP number you entered is not available. Currently available SCP numbers are: ";

    public string BroadcastHeader { get; set; } = "<color=yellow>[SCP Replacer]</color>\n";

    public string ReplaceBroadcast { get; set; } = "<color=red>SCP-%NUMBER%</color> was eliminated. Enter <color=green>.volunteer %NUMBER%</color> in the <color=orange>~</color> console to replace it.";
}
