using HarmonyLib;
using TooMuchInfo.Tools;

namespace TooMuchInfo.Patches;

[HarmonyPatch(typeof(VRRig), "UpdateName", typeof(bool))]
public static class NamePatch
{
    private static void Postfix(VRRig __instance, bool isNamePermissionEnabled)
    {
        if (__instance.isLocal || !__instance.HasCosmetics())
            return;

        string fpsColour = __instance.fps < 72 ? __instance.fps < 60 ? "red" : "orange" : "green";
        string pingColour = __instance.GetPing() > 100 ? __instance.GetPing() > 250 ? "red" : "orange" : "green";
        
        string text = $"{__instance.OwningNetPlayer.SanitizedNickName}{(Plugin.KnownPeople.TryGetValue(__instance.OwningNetPlayer.UserId, out string value) ? $" : <color=green>{value}</color>" : "")}\n" +
                      $"<color={fpsColour}>{__instance.fps}</color> FPS : <color={pingColour}>{__instance.GetPing()}</color> MS\n" +
                      $"{__instance.GetAccountCreationDate().ToShortDateString()}\n" +
                      $"{__instance.GetPlayerMods().Join(" ")}";
        
        __instance.playerText1.text = text;
    }
}