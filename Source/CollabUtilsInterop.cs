using System;
using MonoMod.ModInterop;

namespace CelesteDeathTracker
{
    [ModImportName("CollabUtils2.LobbyHelper")]
    internal static class CollabUtilsInterop
    {
        public static Func<string, string>? GetLobbyLevelSet;
    }
}
