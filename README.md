# CrabGameModList
A small BepInEx mod for Crab Game that allows hosts to display what mods they have to clients with ModList in the server browser.

## What is the point of this mod?
Any servers being hosted by someone with ModList will show that they are modded by replacing the Default/Modified server tag, as shown here:
![Server Listing](https://github.com/user-attachments/assets/bdd904e3-8916-4733-9428-99c8885a447b)

Additionally, when hovering over the server's tag, it will show the list of mods you and/or the host have on the left side of the screen (opposite of the list of maps and game modes) as shown here:
![Modded Server Listing)](https://github.com/user-attachments/assets/505c9e6a-3d6f-4d8e-89af-2a57feb4fc3c)
The color of the mod names mean different things, they go as follows:
- Blue: Both you and the host have the same mod, everything should work as intended.
- Green: The host has the mod but you do not, and the mod isn't required, so everything should still work as intended.
- Yellow: The host has the mod but you do not, and the mod is required. You can still join the lobby as normal, but you may run into some issues while playing without that mod.
- Red: You have the mod but the host does not. Everything should still work as intended, though that depends on the mod.
- Blue Name but Yellow Version: Both you and the host have the same mod, but don't have the same version of the mod (the version shown will be the hosts version), some issues may occur, though that depends on the mod.

If a server isn't using ModList, the Mod List text at the top will appear red and all of the mods you have will appear red as shown:
![Unmodded Server Listing](https://github.com/user-attachments/assets/9f7771c7-f87c-455b-88c0-58009fda045e)

## Wait, so everyone will be able to see what mods I have if I'm hosting?
No, in order for a mod to be added to your mod list, it must be added to the config, located at Crab Game/BepInEx/config/lammas123.ModList.SharedMods.txt, there are instructions there on how to add a mod to your mod list.

Only mods included in the shared mods list will be shared to other clients using ModList, though certain mods that are integrated with ModList (like [FloatingPlayerPatch](https://github.com/lammas321/CrabGameFloatingPlayerPatch)) will be added automatically.

# New as of v1.1.0 (besides some fixes I forgot about)
ModList will now keep Crab Game from hiding lobbies with 2 or less open spaces from the server list.
Crab Game would normally (for example) hide lobbies at 13 or 14 players out of 15 max players from the server list.
This would also hide lobbies with small max player counts (and completely hide lobbies with a max player count of 2 or 3), making it difficult for them to reach their max player count.
Though this is kind of a niche change, I think it's better to leave lobbies close to their maximum player count visible.
