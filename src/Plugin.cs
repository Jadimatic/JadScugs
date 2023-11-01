using System;
using BepInEx;
using System.Security.Permissions;
using System.Runtime.CompilerServices;
using UnityEngine;
using System.IO;

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
        public static Texture2D HeadTexture;
        public static Texture2D BodyTexture;
        public static Texture2D HipTexture;
        public static Texture2D LegTexture;
        public static Texture2D TailTexture;
        public static Texture2D ArmTexture;

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
                BCPuppetEnums.RegisterValues();

                Futile.atlasManager.LoadAtlas("atlases/MouthScugAtlases/MouthScug-face"); Futile.atlasManager.LoadAtlas("atlases/MouthScugAtlases/MouthScugNerv-face");
                Futile.atlasManager.LoadAtlas("atlases/MouthScugAtlases/MouthScug-head");
                Futile.atlasManager.LoadAtlas("atlases/MouthScugAtlases/StuffedMouthScug-head");
                Futile.atlasManager.LoadAtlas("atlases/MouthScugAtlases/MouthScug-body");
                Futile.atlasManager.LoadAtlas("atlases/MouthScugAtlases/MouthScug-hips");
                Futile.atlasManager.LoadAtlas("atlases/MouthScugAtlases/MouthScug-legs");
                Futile.atlasManager.LoadAtlas("atlases/MouthScugAtlases/MouthScug-tail"); Futile.atlasManager.LoadAtlas("atlases/MouthScugAtlases/MouthScugNerv-tail");

                Futile.atlasManager.LoadAtlas("atlases/MedicAtlases/Medic-face"); Futile.atlasManager.LoadAtlas("atlases/MedicAtlases/MedicNerv-face");
                Futile.atlasManager.LoadAtlas("atlases/MedicAtlases/Medic-head");
                Futile.atlasManager.LoadAtlas("atlases/MedicAtlases/Medic-body");
                Futile.atlasManager.LoadAtlas("atlases/MedicAtlases/Medic-hips");
                Futile.atlasManager.LoadAtlas("atlases/MedicAtlases/Medic-legs");
                Futile.atlasManager.LoadAtlas("atlases/MedicAtlases/Medic-tail"); Futile.atlasManager.LoadAtlas("atlases/MedicAtlases/MedicNerv-tail");

                Futile.atlasManager.LoadAtlas("atlases/BCPuppetAtlases/BCPuppet-face"); Futile.atlasManager.LoadAtlas("atlases/BCPuppetAtlases/BCPuppetNerv-face"); Futile.atlasManager.LoadAtlas("atlases/BCPuppetAtlases/Layers/BCPuppetPattern-face");
                Futile.atlasManager.LoadAtlas("atlases/BCPuppetAtlases/BCPuppet-head"); Futile.atlasManager.LoadAtlas("atlases/BCPuppetAtlases/Layers/BCPuppetAntennae-head"); Futile.atlasManager.LoadAtlas("atlases/BCPuppetAtlases/Layers/BCPuppetPattern-head");
                Futile.atlasManager.LoadAtlas("atlases/BCPuppetAtlases/BCPuppet-arm"); Futile.atlasManager.LoadAtlas("atlases/BCPuppetAtlases/Layers/BCPuppetGown-arm");
                Futile.atlasManager.LoadAtlas("atlases/BCPuppetAtlases/BCPuppet-body"); Futile.atlasManager.LoadAtlas("atlases/BCPuppetAtlases/BCPuppetDressed-body"); Futile.atlasManager.LoadAtlas("atlases/BCPuppetAtlases/Layers/BCPuppetGown-body");
                Futile.atlasManager.LoadAtlas("atlases/BCPuppetAtlases/Layers/BCPuppetGownTex");
                Futile.atlasManager.LoadAtlas("atlases/BCPuppetAtlases/BCPuppet-hips"); Futile.atlasManager.LoadAtlas("atlases/BCPuppetAtlases/BCPuppetDressed-hips"); Futile.atlasManager.LoadAtlas("atlases/BCPuppetAtlases/Layers/BCPuppetGown-hips");
                Futile.atlasManager.LoadAtlas("atlases/BCPuppetAtlases/BCPuppet-legs");
                Futile.atlasManager.LoadAtlas("atlases/BCPuppetAtlases/BCPuppet-tail");
                
                HeadTexture = new Texture2D(150, 75, TextureFormat.ARGB32, false);
                var headTextureFile = AssetManager.ResolveFilePath("atlases/BCPuppetAtlases/BCPuppet-head.png");
                if (File.Exists(headTextureFile))
                {
                    var rawData = File.ReadAllBytes(headTextureFile);
                    HeadTexture.LoadImage(rawData);
                }
                ArmTexture = new Texture2D(150, 75, TextureFormat.ARGB32, false);
                var armTextureFile = AssetManager.ResolveFilePath("atlases/BCPuppetAtlases/BCPuppet-arm.png");
                if (File.Exists(armTextureFile))
                {
                    var rawData = File.ReadAllBytes(armTextureFile);
                    ArmTexture.LoadImage(rawData);
                }
                BodyTexture = new Texture2D(150, 75, TextureFormat.ARGB32, false);
                var bodyTextureFile = AssetManager.ResolveFilePath("atlases/BCPuppetAtlases/BCPuppet-body.png");
                if (File.Exists(bodyTextureFile))
                {
                    var rawData = File.ReadAllBytes(bodyTextureFile);
                    BodyTexture.LoadImage(rawData);
                }
                HipTexture = new Texture2D(150, 75, TextureFormat.ARGB32, false);
                var hipTextureFile = AssetManager.ResolveFilePath("atlases/BCPuppetAtlases/BCPuppet-hip.png");
                if (File.Exists(hipTextureFile))
                {
                    var rawData = File.ReadAllBytes(hipTextureFile);
                    HipTexture.LoadImage(rawData);
                }
                TailTexture = new Texture2D(150, 75, TextureFormat.ARGB32, false);
                var tailTextureFile = AssetManager.ResolveFilePath("atlases/BCPuppetAtlases/BCPuppet-hip.png");
                if (File.Exists(tailTextureFile))
                {
                    var rawData = File.ReadAllBytes(tailTextureFile);
                    TailTexture.LoadImage(rawData);
                }





            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Logger.LogMessage("FAIL trying to load " + PLUGIN_NAME);
            }
        }
    }
    public class JadScugExtraPlayerData
    {
        public float DangerLevel;
    }

    public static class JadScugPlayerExtension
    {
        private static readonly ConditionalWeakTable<Player, JadScugExtraPlayerData> _cwt = new();

        public static JadScugExtraPlayerData JadScug(this Player player) => _cwt.GetValue(player, _ => new JadScugExtraPlayerData());
    }
}