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
                                  where __instance.concatStringOfCosmeticsAllowed.Contains(key)
                                  select Plugin.KnownCosmetics[key]);

        string fpsColour  = __instance.fps       < 72 ? __instance.fps        < 60 ? "red" : "orange" : "green";
        string pingColour = __instance.GetPing() > 100 ? __instance.GetPing() > 250 ? "red" : "orange" : "green";

        string text = $"{(specialCosmetics.Count > 0 ? specialCosmetics.Join(" ").Trim() + "\n" : "")}" +
                      $"{__instance.OwningNetPlayer.SanitizedNickName.AnimateGradient(__instance.playerColor)}{(Plugin.KnownPeople.TryGetValue(__instance.OwningNetPlayer.UserId, out string value) ? $" : <color=green>{value}</color>" : "")}\n" +
                      $"<color={fpsColour}>{__instance.fps}</color> FPS : {__instance.GetPlatform().ParsePlatform()} : <color={pingColour}>{__instance.GetPing()}</color> MS\n" +
                      $"{__instance.GetAccountCreationDate():dd/MM/yyyy}\n" +
                      $"{__instance.GetPlayerMods().Join(" ")}";

        __instance.playerText1.transform.localPosition = new Vector3(0.0085f, 0.2f, 0.1408f);
        __instance.playerText1.text                    = text.Trim();
    }
}