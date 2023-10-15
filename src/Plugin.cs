using System;
using BepInEx;
using System.Security.Permissions;

#pragma warning disable CS0618 // Do not remove the following line.
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace JadScugs
{
    [BepInPlugin(MOD_ID, PLUGIN_NAME, PLUGIN_VERSION)]
    class Plugin : BaseUnityPlugin
    {
        public const string MOD_ID = "jadimatic.jadscugs";
        public const string PLUGIN_NAME = "Jad Scugs";
        public const string PLUGIN_VERSION = "0.1.0";

        private bool _initialized;

        private void LogInfo(object ex) => Logger.LogInfo(ex);

        public void OnEnable()
        {
            LogInfo("Loading " + PLUGIN_NAME + " " + PLUGIN_VERSION);
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
        }

        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            try
            {
                if (_initialized) return;
                _initialized = true;

                WipHooks.Init();
                PlayerGraphicsHooks.Init();
                PlayerHooks.Init();

                Futile.atlasManager.LoadAtlas("atlases/MouthScug-head");
                Futile.atlasManager.LoadAtlas("atlases/StuffedMouthScug-head");
                Futile.atlasManager.LoadAtlas("atlases/MouthScug-body");
                Futile.atlasManager.LoadAtlas("atlases/MouthScug-hips");
                Futile.atlasManager.LoadAtlas("atlases/MouthScug-legs");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                Logger.LogMessage("FAIL trying to load " + PLUGIN_NAME);
            }
        }
    }
}