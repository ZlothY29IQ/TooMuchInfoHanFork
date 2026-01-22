using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using BepInEx;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using Photon.Pun;
using TooMuchInfo.Tools;

namespace TooMuchInfo;

[BepInPlugin(Constants.PluginGuid, Constants.PluginName, Constants.PluginVersion)]
public class Plugin : BaseUnityPlugin
{
    public static bool ShowSpecialCosmetics;
    public static bool ShowKnownPeople;
    public static bool ShowFpsPlatformPing;
    public static bool ShowCreationDate;
    public static bool ShowMods;

    public static List<string> LineOrder;

    public static Dictionary<string, string> KnownMods;
    public static Dictionary<string, string> KnownCheats;
    public static Dictionary<string, string> KnownPeople;
    public static Dictionary<string, string> KnownCosmetics;

    private void Awake()
    {
        ShowSpecialCosmetics = Config.Bind("Display", "SpecialCosmetics", true).Value;
        ShowKnownPeople      = Config.Bind("Display", "KnownPeople",      true).Value;
        ShowFpsPlatformPing  = Config.Bind("Display", "FpsPlatformPing",  true).Value;
        ShowCreationDate     = Config.Bind("Display", "CreationDate",     true).Value;
        ShowMods             = Config.Bind("Display", "Mods",             true).Value;

        LineOrder = Config.Bind(
                "Order",
                "LineOrder",
                "SpecialCosmetics,Name,FpsPlatformPing,CreationDate,Mods"
        ).Value.Split(',').Select(x => x.Trim()).ToList();

        new Harmony(Constants.PluginGuid).PatchAll();
    }

    private void Start() => GorillaTagger.OnPlayerSpawned(() =>
                                                          {
                                                              using HttpClient httpClient = new();
                                                              HttpResponseMessage response =
                                                                      httpClient.GetAsync(
                                                                              "https://data.hamburbur.org/").Result;

                                                              using (Stream hamburburDataStream =
                                                                     response.Content.ReadAsStreamAsync().Result)
                                                              {
                                                                  using (StreamReader reader = new(hamburburDataStream))
                                                                  {
                                                                      JObject root = JObject.Parse(reader.ReadToEnd());

                                                                      KnownCheats =
                                                                              root["Known Cheats"]?
                                                                                     .ToObject<Dictionary<string,
                                                                                              string>>();

                                                                      KnownMods =
                                                                              root["Known Mods"]?
                                                                                     .ToObject<Dictionary<string,
                                                                                              string>>();

                                                                      KnownPeople =
                                                                              root["Known People"]?
                                                                                     .ToObject<Dictionary<string,
                                                                                              string>>();

                                                                      KnownCosmetics =
                                                                              root["Special Cosmetics"]?
                                                                                     .ToObject<Dictionary<string,
                                                                                              string>>();
                                                                  }
                                                              }
                                                          });

    private void Update()
    {
        if (VRRig.LocalRig.playerText1 == null || PhotonNetwork.LocalPlayer == null)
            return;

        VRRig.LocalRig.playerText1.text =
                PhotonNetwork.LocalPlayer.NickName.AnimateGradient(VRRig.LocalRig.playerColor);
    }
}