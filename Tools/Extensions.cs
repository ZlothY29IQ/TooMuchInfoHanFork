using System;
using System.Collections.Generic;
using Photon.Pun;

namespace TooMuchInfo.Tools;

public enum GamePlatform
{
    Steam,
    OculusPC,
    PC,
    Standalone,
    Unknown,
}

public static class Extensions
{
    public static          Dictionary<VRRig, GamePlatform> PlayerPlatforms      = new();
    public static          Dictionary<VRRig, List<string>> PlayerMods           = new();
    public static          Dictionary<string, DateTime>    AccountCreationDates = new();
    public static          List<VRRig>                     PlayersWithCosmetics = [];
    public static readonly Dictionary<VRRig, int>          PlayerPing           = new();

    public static GamePlatform GetPlatform(this VRRig rig) =>
            PlayerPlatforms.GetValueOrDefault(rig, GamePlatform.Unknown);

    public static string ParsePlatform(this GamePlatform gamePlatform)
    {
        return gamePlatform switch
               {
                       GamePlatform.Unknown    => "<color=#000000>UNKNOWN</color>",
                       GamePlatform.Steam      => "<color=#0091F7>STEAM</color>",
                       GamePlatform.OculusPC   => "<color=#0091F7>OCULUS PCVR</color>",
                       GamePlatform.PC         => "<color=#000000>PC</color>",
                       GamePlatform.Standalone => "<color=#26A6FF>STANDALONE</color>",
                       var _                   => throw new ArgumentOutOfRangeException(),
               };
    }

    public static DateTime GetAccountCreationDate(this VRRig rig) => AccountCreationDates[rig.OwningNetPlayer.UserId];

    public static string[] GetPlayerMods(this VRRig rig) => PlayerMods[rig].ToArray();

    public static bool HasCosmetics(this VRRig rig) => PlayersWithCosmetics.Contains(rig);

    public static int GetPing(this VRRig rig) =>
            PlayerPing.TryGetValue(rig, out int ping) ? ping : PhotonNetwork.GetPing();
}