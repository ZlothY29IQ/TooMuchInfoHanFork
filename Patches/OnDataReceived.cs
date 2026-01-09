using System;
using System.Linq;
using GorillaNetworking;
using HarmonyLib;
using Photon.Pun;
using TooMuchInfo.Tools;

namespace TooMuchInfo.Patches;

[HarmonyPatch(typeof(VRRig))]
[HarmonyPatch("SerializeReadShared", MethodType.Normal)]
public static class OnDataReceived
{
    private static void Postfix(VRRig __instance)
    {
        UpdatePlayerPing(__instance);
        UpdatePlayerCosmetXState(__instance);
        __instance.UpdateName();
    }

    private static void UpdatePlayerPing(VRRig rig)
    {
        double ping     = Math.Abs((rig.velocityHistoryList[0].time - PhotonNetwork.Time) * 1000);
        int    safePing = (int)Math.Clamp(Math.Round(ping), 0, int.MaxValue);

        Extensions.PlayerPing[rig] = safePing;
    }

    private static void UpdatePlayerCosmetXState(VRRig rig)
    {
        if (!Extensions.PlayerMods.ContainsKey(rig))
            Extensions.PlayerMods[rig] = [];

        if (!rig.HasCosmetics())
            return;

        CosmeticsController.CosmeticSet cosmeticSet = rig.cosmeticSet;
        bool hasCosmetx =
                cosmeticSet.items.Any(cosmetic => !cosmetic.isNullItem &&
                                                  !rig.rawCosmeticString.Contains(
                                                          cosmetic.itemName)) && !rig.inTryOnRoom;

        switch (hasCosmetx)
        {
            case true when !Extensions.PlayerMods[rig].Contains("[<color=red>COSMETX</color>]"):
                Extensions.PlayerMods[rig].Add("[<color=red>COSMETX</color>]");

                break;

            case false when Extensions.PlayerMods[rig].Contains("[<color=red>COSMETX</color>]"):
                Extensions.PlayerMods[rig].Remove("[<color=red>COSMETX</color>]");

                break;
        }
    }
}