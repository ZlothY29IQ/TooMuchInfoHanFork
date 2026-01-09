using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TooMuchInfo.Tools;
using UnityEngine;

namespace TooMuchInfo.Patches;

[HarmonyPatch(typeof(VRRig), "UpdateName", typeof(bool))]
public static class NamePatch
{
    private static void Postfix(VRRig __instance, bool isNamePermissionEnabled)
    {
        if (__instance.isLocal || !__instance.HasCosmetics())
            return;

        List<string> specialCosmetics = [];
        specialCosmetics.AddRange(from key in Plugin.KnownCosmetics.Keys
                                  where __instance.rawCosmeticString.Contains(key)
                                  select Plugin.KnownCosmetics[key]);

        string fpsColour  = __instance.fps       < 72 ? __instance.fps        < 60 ? "red" : "orange" : "green";
        string pingColour = __instance.GetPing() > 100 ? __instance.GetPing() > 250 ? "red" : "orange" : "green";

        List<string> lines = [];
        

        string nameLine =
                __instance.OwningNetPlayer.SanitizedNickName
                          .AnimateGradient(__instance.playerColor) +
                (Plugin.ShowKnownPeople &&
                 Plugin.KnownPeople.TryGetValue(__instance.OwningNetPlayer.UserId, out string known)
                         ? $" : <color=green>{known}</color>"
                         : "");

        string fpsLine =
                $"<color={fpsColour}>{__instance.fps}</color> FPS : " +
                $"{__instance.GetPlatform().ParsePlatform()} : "      +
                $"<color={pingColour}>{__instance.GetPing()}</color> MS";

        string dateLine = __instance.GetAccountCreationDate().ToString("dd/MM/yyyy");
        string modsLine = __instance.GetPlayerMods().Join(" ");

        Dictionary<string, string> sections = new()
        {
                { "SpecialCosmetics", specialCosmetics.Count > 0 ? specialCosmetics.Join(" ").Trim() : "" },
                { "Name", nameLine },
                { "FpsPlatformPing", fpsLine },
                { "CreationDate", dateLine },
                { "Mods", modsLine },
        };

        foreach (string key in Plugin.LineOrder.Where(key => key != "SpecialCosmetics"))
        {
            if (!sections.TryGetValue(key, out string value))
                continue;

            switch (key)
            {
                case "SpecialCosmetics" when !Plugin.ShowSpecialCosmetics:
                case "FpsPlatformPing" when !Plugin.ShowFpsPlatformPing:
                case "CreationDate" when !Plugin.ShowCreationDate:
                case "Mods" when !Plugin.ShowMods:
                    continue;

                default:
                    lines.Add(value);

                    break;
            }
        }

        __instance.playerText1.transform.localPosition = new Vector3(0.0085f, 0.2f, 0.1408f);
        __instance.playerText1.text                    = lines.Join("\n").Trim();
    }
}