using HarmonyLib;
using static ModList.ModList;

namespace ModList
{
    internal static class Patches
    {
        //   Anti Bepinex detection (Thanks o7Moon: https://github.com/o7Moon/CrabGame.AntiAntiBepinex)
        [HarmonyPatch(typeof(EffectManager), nameof(EffectManager.Method_Private_Void_GameObject_Boolean_Vector3_Quaternion_0))] // Ensures effectSeed is never set to 4200069 (if it is, modding has been detected)
        [HarmonyPatch(typeof(LobbyManager), nameof(LobbyManager.Method_Private_Void_0))] // Ensures connectedToSteam stays false (true means modding has been detected)
        //[HarmonyPatch(typeof(SnowSpeedModdingDetector), nameof(SnowSpeedModdingDetector.Method_Private_Void_0))] // Would ensure snowSpeed is never set to Vector3.zero (though it is immediately set back to Vector3.one due to an accident on Dani's part lol)
        [HarmonyPrefix]
        public static bool PreBepinexDetection()
            => false;


        //   Server List Mods List
        // A list of your mods (that are compatible with this) will appear on the left side of the server list upon hovering your cursor over a server listing's Default/Modified tag.
        // If the server host also has the mod, your mod will appear in blue, otherwise it'll appear red.
        [HarmonyPatch(typeof(LobbyManager), nameof(LobbyManager.StartLobby))]
        [HarmonyPostfix]
        internal static void PostLobbyManagerStartLobby()
            => Instance.OnStartLobby();

        [HarmonyPatch(typeof(MenuUiServerListingGameModesAndMapsInfo), nameof(MenuUiServerListingGameModesAndMapsInfo.Awake))]
        [HarmonyPostfix]
        internal static void PostMenuUiServerListingGameModesAndMapsInfoAwake()
            => Instance.CreateModListGameObject();

        [HarmonyPatch(typeof(MenuUiServerListing), nameof(MenuUiServerListing.Method_Private_Void_1))]
        [HarmonyPostfix]
        internal static void PostMenuUiServerListingCheckModified(MenuUiServerListing __instance)
            => Instance.CheckModded(__instance);

        [HarmonyPatch(typeof(MenuUiServerListingGameModesAndMaps), nameof(MenuUiServerListingGameModesAndMaps.OnPointerEnter))]
        [HarmonyPrefix]
        internal static void PreMenuUiServerListingGameModesAndMapsOnPointerEnter()
            => Instance.ClearModList();

        [HarmonyPatch(typeof(MenuUiServerListingGameModesAndMaps), nameof(MenuUiServerListingGameModesAndMaps.OnPointerEnter))]
        [HarmonyPostfix]
        internal static void PostMenuUiServerListingGameModesAndMapsOnPointerEnter(MenuUiServerListingGameModesAndMaps __instance)
            => Instance.FillModList(__instance.serverUi.field_Private_CSteamID_0);

        [HarmonyPatch(typeof(MenuUiServerListingGameModesAndMaps), nameof(MenuUiServerListingGameModesAndMaps.OnPointerExit))]
        [HarmonyPostfix]
        internal static void PostMenuUiServerListingGameModesAndMapsOnPointerExit()
            => Instance.HideModList();
    }
}