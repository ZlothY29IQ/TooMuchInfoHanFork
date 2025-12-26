using System;
using System.Collections.Generic;
using System.Text;
using Photon.Pun;
using TMPro;
using UnityEngine;

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
    public static           Dictionary<VRRig, GamePlatform> PlayerPlatforms      = new();
    public static           Dictionary<VRRig, List<string>> PlayerMods           = new();
    public static           Dictionary<string, DateTime>    AccountCreationDates = new();
    public static           List<VRRig>                     PlayersWithCosmetics = [];
    public static readonly  Dictionary<VRRig, int>          PlayerPing           = new();

    public static string AnimateGradient(this string text, Color32 baseColor, float speed = 2f)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        StringBuilder sb  = new();
        int           len = text.Length;

        for (int i = 0; i < len; i++)
        {
            float  offset  = (float)i                                                      / len;
            float  t       = (Mathf.Sin(Time.time * speed + (1f - offset) * Mathf.PI * 2) + 1f) * 0.5f;
            Color  blended = Color.Lerp(baseColor, Color.white, t);
            string hex     = ColorUtility.ToHtmlStringRGB(blended);
            sb.Append($"<color=#{hex}>{text[i]}</color>");
        }

        return sb.ToString();
    }
    
    public static GamePlatform GetPlatform(this VRRig rig) =>
            PlayerPlatforms.GetValueOrDefault(rig, GamePlatform.Unknown);

    public static string ParsePlatform(this GamePlatform gamePlatform)
    {
        return gamePlatform switch
               {
                       GamePlatform.Unknown    => "<color=#800000>UNKNOWN</color>",
                       GamePlatform.Steam      => "<color=#0091F7>STEAM</color>",
                       GamePlatform.OculusPC   => "<color=#0091F7>OCULUS PCVR</color>",
                       GamePlatform.PC         => "<color=#800000>PC</color>",
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