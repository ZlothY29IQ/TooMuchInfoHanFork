using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExitGames.Client.Photon;
using HarmonyLib;
using PlayFab;
using PlayFab.ClientModels;
using TooMuchInfo.Tools;
using UnityEngine;

namespace TooMuchInfo.Patches;

[HarmonyPatch(typeof(VRRig))]
internal static class OnCosmeticsLoadedPatch
{
    private static readonly DateTime OculusPayDay = new(2023, 02, 06);

    [HarmonyPatch("IUserCosmeticsCallback.OnGetUserCosmetics")]
    [HarmonyPostfix]
    private static void OnGetRigCosmetics(VRRig __instance) => OnLoad(__instance);

    private static async void OnLoad(VRRig rig)
    {
        Extensions.PlayersWithCosmetics.Add(rig);

        DateTime playerCreationDate;

        if (!Extensions.AccountCreationDates.TryGetValue(rig.creator.UserId, out playerCreationDate))
        {
            try
            {
                TaskCompletionSource<GetAccountInfoResult> tcs = new();

                PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest { PlayFabId = rig.creator.UserId, },
                        result => tcs.SetResult(result),
                        error =>
                        {
                            Debug.LogError("Failed to get account info: " + error.ErrorMessage);
                            tcs.SetException(new Exception(error.ErrorMessage));
                        });

                GetAccountInfoResult result = await tcs.Task;
                Extensions.AccountCreationDates[rig.creator.UserId] = result.AccountInfo.Created;
                playerCreationDate                                  = result.AccountInfo.Created;
            }
            catch
            {
                // ignored
            }
        }

        Hashtable    properties = rig.creator.GetPlayerRef().CustomProperties;
        List<string> mods       = [];

        foreach (string key in properties.Keys)
        {
            if (Plugin.KnownCheats.TryGetValue(key, out string cheat))
                mods.Add($"[<color=red>{cheat.ToUpper()}</color>]");

            if (Plugin.KnownMods.TryGetValue(key, out string mod))
                mods.Add($"[<color=green>{mod.ToUpper()}</color>]");
        }

        Extensions.PlayerMods[rig] = mods;

        string cosmeticsAllowed = string.Concat(rig._playerOwnedCosmetics).ToLower();

        if (cosmeticsAllowed.Contains("s. first login"))
        {
            Extensions.PlayerPlatforms[rig] = GamePlatform.Steam;

            return;
        }

        if (cosmeticsAllowed.Contains("first login") || cosmeticsAllowed.Contains("game-purchase"))
        {
            Extensions.PlayerPlatforms[rig] = GamePlatform.OculusPC;

            return;
        }

        if (rig.creator.GetPlayerRef().CustomProperties.Count > 1)
        {
            Extensions.PlayerPlatforms[rig] = GamePlatform.PC;

            return;
        }

        if (playerCreationDate > OculusPayDay)
        {
            Extensions.PlayerPlatforms[rig] = GamePlatform.Standalone;

            return;
        }

        Extensions.PlayerPlatforms[rig] = GamePlatform.Unknown;
    }
}