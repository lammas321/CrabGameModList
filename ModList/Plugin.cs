using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using SteamworksNative;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Schema;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ModList
{
    [BepInPlugin($"lammas123.{MyPluginInfo.PLUGIN_NAME}", MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class ModList : BasePlugin
    {
        internal static ModList Instance;
        internal Dictionary<string, bool> sharedMods = [];

        public override void Load()
        {
            Instance = this;

            string path = Path.Combine(Paths.ConfigPath, "lammas123.ModList.SharedMods.txt");
            if (File.Exists(path))
            {
                foreach (string str in File.ReadAllLines(path))
                {
                    string line = str.Trim();
                    if (line.Length == 0 || line.StartsWith("#")) continue;
                    string[] split = line.Split('=', StringSplitOptions.RemoveEmptyEntries);
                    if (split.Length == 0 || sharedMods.ContainsKey(split[0])) continue;

                    if (split.Length == 1)
                        sharedMods.Add(split[0], false);
                    else
                        sharedMods.Add(split[0].Trim(), split[1].Trim() == "1");
                }
            }
            else
                File.WriteAllLines(path, [
                    "# The list of additional mods you want to show to clients with the ModList mod, or to have show up in your mod list whether the host has that mod or not.",
                    "# Format: ModGuid=Required",
                    "# Example: lammas123.FloatingPlayerPatch=0",
                    "",
                    "# ModList will assume that the mod is not required if the equals sign is not included or if any value besides '1' for required is present.",
                    "# Adding mods like FloatingPlayerPatch here is not required as FloatingPlayerPatch soft depends on ModList, and thus will add itself to the list automatically if ModList is installed.",
                    "# However, if you want to make FloatingPlayerPatch appear to be required, as it is not marked as so by default, then you can add that line with a '1' for required.",
                    "# If you need a mod's guid, ModList will log the guids, names, and versions of every enabled mod you have in your BepInEx/plugins folder on startup to make finding them easier.",
                    "",
                    ""
                ]);

            Harmony.CreateAndPatchAll(typeof(Patches));
            Log.LogInfo($"Loaded [{MyPluginInfo.PLUGIN_NAME} {MyPluginInfo.PLUGIN_VERSION}]");
        }

        public static bool AddMod(string guid, bool required = false)
        {
            if (Instance.sharedMods.ContainsKey(guid) || !IL2CPPChainloader.Instance.Plugins.ContainsKey(guid))
                return false;

            Instance.sharedMods.Add(guid, required);
            return true;
        }


        internal void OnStartLobby()
        {
            SteamMatchmaking.SetLobbyData(LobbyManager.Instance.field_Private_CSteamID_0, "Modded", "1");
            foreach (string guid in sharedMods.Keys)
                if (IL2CPPChainloader.Instance.Plugins.ContainsKey(guid))
                    SteamMatchmaking.SetLobbyData(LobbyManager.Instance.field_Private_CSteamID_0, $"Mod:{guid}:{IL2CPPChainloader.Instance.Plugins[guid].Metadata.Name}:{IL2CPPChainloader.Instance.Plugins[guid].Metadata.Version}:{(sharedMods[guid] ? "1" : "0")}", "1");
        }

        internal void CreateModListGameObject()
        {
            if (MenuUiServerListingGameModesAndMapsInfo.Instance.transform.childCount != 1) return; // The mod list game object has aleady been created

            // List all mods the client has enabled in the console
            Log.LogInfo("Enabled Mods:");
            foreach (PluginInfo info in IL2CPPChainloader.Instance.Plugins.Values)
                Log.LogInfo($"{info.Metadata.GUID}: {info.Metadata.Name} v{info.Metadata.Version}");

            // Create it
            Transform modList = UnityEngine.Object.Instantiate(MenuUiServerListingGameModesAndMapsInfo.Instance.parent, MenuUiServerListingGameModesAndMapsInfo.Instance.transform).transform;
            modList.name = "Mod List";
            modList.localPosition *= -1;

            UnityEngine.Object.DestroyImmediate(modList.GetChild(0).gameObject); // Destroy 'Header'
            UnityEngine.Object.DestroyImmediate(modList.GetChild(2).gameObject); // Destroy 'MapText'
            UnityEngine.Object.DestroyImmediate(modList.GetChild(2).gameObject); // Destroy 'MapContainer'

            Transform modListText = modList.GetChild(0); // Previously 'GameModeText'
            modListText.name = "Mod List Text";
            modListText.GetComponent<TextMeshProUGUI>().text = "Mod List";

            Transform modListContainer = modList.GetChild(1); // Previously 'GameModeContainer'
            modListContainer.name = "Mod List Container";
            modListContainer.GetComponent<GridLayoutGroup>().cellSize = new(390, 18);
        }

        internal void CheckModded(MenuUiServerListing listing)
        {
            if (SteamMatchmaking.GetLobbyData(listing.field_Private_CSteamID_0, "Modded") != "1") return;
            listing.modifiedText.text = "Modded\n<size=60%>(Hover for info)";
            listing.modifiedImg.color = new Color(0f, 0.5f, 1f);
        }

        internal void ClearModList()
        {
            Transform modListContainer = MenuUiServerListingGameModesAndMapsInfo.Instance.transform.GetChild(1).GetChild(1);
            foreach (Transform transform in modListContainer.GetComponentsInChildren<Transform>())
                if (transform != modListContainer)
                    UnityEngine.Object.DestroyImmediate(transform.gameObject);
        }

        internal void FillModList(CSteamID lobbyId)
        {
            // Set header
            Transform modList = MenuUiServerListingGameModesAndMapsInfo.Instance.transform.GetChild(1);
            modList.gameObject.SetActive(true);
            modList.GetChild(0).GetComponent<TextMeshProUGUI>().text = SteamMatchmaking.GetLobbyData(lobbyId, "Modded") == "1"
                ? "Mod List"
                : $"<color={MenuUiServerListingGameModesAndMapsInfo.Instance.redCol}>Mod List";

            // Get mods the host has (that are compatible with ModList)
            int count = SteamMatchmaking.GetLobbyDataCount(lobbyId);
            Dictionary<string, CrabGameMod> foundMods = [];

            for (int i = 0; i < count; i++)
            {
                SteamMatchmaking.GetLobbyDataByIndex(lobbyId, i, out string key, 255, out string value, 255);
                if (value != "1") continue;

                try
                {
                    string[] modData = key.Split(':', StringSplitOptions.RemoveEmptyEntries);
                    if (modData.Length < 4 || modData[1] == $"lammas123.{MyPluginInfo.PLUGIN_NAME}") continue;

                    CrabGameMod mod = new(modData[1], modData[3]);
                    if (IL2CPPChainloader.Instance.Plugins.ContainsKey(mod.guid))
                    {
                        if (IL2CPPChainloader.Instance.Plugins[mod.guid].Metadata.Version.ToString() == mod.version)
                            mod.state = CrabGameModState.BothHaveMod;
                        else
                            mod.state = CrabGameModState.ModVersionMismatch;
                    }
                    else
                    {
                        mod.name = modData[2];
                        if (modData[4] == "1")
                            mod.state = CrabGameModState.RequiredModNotOnClient;
                        else
                            mod.state = CrabGameModState.ModNotOnClient;
                    }

                    foundMods.Add(modData[1], mod);
                }
                catch (Exception) { }
            }

            // Add all mods on client that are not already added
            foreach (string guid in sharedMods.Keys)
                if (guid != $"lammas123.{MyPluginInfo.PLUGIN_NAME}" && !foundMods.ContainsKey(guid) && IL2CPPChainloader.Instance.Plugins.ContainsKey(guid))
                    foundMods.Add(guid, new(guid, IL2CPPChainloader.Instance.Plugins[guid].Metadata.Version.ToString()));

            Transform modListContainer = modList.GetChild(1);
            foreach (CrabGameMod mod in foundMods.Values)
                UnityEngine.Object.Instantiate(MenuUiServerListingGameModesAndMapsInfo.Instance.prefabText, modListContainer).GetComponent<TextMeshProUGUI>().text = mod.GetDisplayName();
        }

        internal void HideModList()
            => MenuUiServerListingGameModesAndMapsInfo.Instance.transform.GetChild(1).gameObject.SetActive(false);


        internal enum CrabGameModState
        {
            ModNotOnHost,
            ModNotOnClient,
            RequiredModNotOnClient,
            ModVersionMismatch,
            BothHaveMod
        }

        internal struct CrabGameMod(string guid, string version, CrabGameModState state = CrabGameModState.ModNotOnHost)
        {
            internal string guid = guid;
            internal string version = version;
            internal CrabGameModState state = state;
            internal string name;

            internal readonly string GetDisplayName()
            {
                string color;
                switch(state)
                {
                    case CrabGameModState.BothHaveMod or CrabGameModState.ModVersionMismatch:
                        {
                            color = MenuUiServerListingGameModesAndMapsInfo.Instance.blueCol;
                            break;
                        }
                    case CrabGameModState.RequiredModNotOnClient:
                        {
                            color = $"#{ColorUtility.ToHtmlStringRGB(Color.yellow)}";
                            break;
                        }
                    case CrabGameModState.ModNotOnClient:
                        {
                            color = $"#{ColorUtility.ToHtmlStringRGB(Color.green)}";
                            break;
                        }
                    default:
                        {
                            color = MenuUiServerListingGameModesAndMapsInfo.Instance.redCol;
                            break;
                        }
                }

                string name = IL2CPPChainloader.Instance.Plugins.ContainsKey(guid)
                    ? IL2CPPChainloader.Instance.Plugins[guid].Metadata.Name
                    : this.name;
                if (state == CrabGameModState.ModVersionMismatch)
                    name += $"<color=#{ColorUtility.ToHtmlStringRGB(Color.yellow)}>";

                return $"<color={color}>{name}<size=75%> v{version}";
            }
        }
    }
}