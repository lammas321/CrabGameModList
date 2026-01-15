using BepInEx.IL2CPP;
using static ModList.ModList;

namespace ModList
{
    public static class Api
    {
        public static bool AddMod(string guid, bool required = false)
        {
            if (Instance.sharedMods.ContainsKey(guid) || !IL2CPPChainloader.Instance.Plugins.ContainsKey(guid))
                return false;

            Instance.sharedMods.Add(guid, required);
            return true;
        }

        public static bool IsModShared(string guid)
            => Instance.sharedMods.ContainsKey(guid);

        public static bool IsModRequired(string guid)
            => Instance.sharedMods.ContainsKey(guid) && Instance.sharedMods[guid];
    }
}