using System;
using System.Linq;
using System.Runtime.CompilerServices;
using BeeWorld;
using RWCustom;
using SlugBase.DataTypes;
using UnityEngine;
using UnityEngine.Diagnostics;
using Random = UnityEngine.Random;

namespace JadScugs
{

    public class BCPuppetPlayerData
    {
        public bool grabbed;
        public AttachedSprite headAntennae;
        public AttachedSprite headPattern;
        public AttachedSprite facePattern;



        public FAtlas HeadAtlas;
        public FAtlas ArmAtlas;
        public FAtlas BodyAtlas;
        public FAtlas HipAtlas;
        public FAtlas LegAtlas;
        public FAtlas TailAtlas;
        public WeakReference<Player> playerRef;

        public BCPuppetPlayerData(Player player)
        {
            var isBCPuppet = player.SlugCatClass.value == "BCPuppet";
            playerRef = new WeakReference<Player>(player);

            if (!isBCPuppet)
            {
                return;
            }
        }

        ~BCPuppetPlayerData()
        {
            try
            {
                HeadAtlas.Unload();
                ArmAtlas.Unload();
                BodyAtlas.Unload();
                HipAtlas.Unload();
                LegAtlas.Unload();
                TailAtlas.Unload();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }



        public Color BodyColor;
        public Color ClothColor;
        public void SetupColors(PlayerGraphics pg)
        {
            ClothColor = pg.GetColor(BCPuppetEnums.Color.Cloth) ?? Custom.hexToColor("fd7a02");
        }

        public void LoadHeadAtlas()
        {
            var headTexture = new Texture2D(Plugin.HeadTexture.width, Plugin.HeadTexture.height, TextureFormat.ARGB32, false);
            Graphics.CopyTexture(Plugin.HeadTexture, headTexture);
            JadScugUtils.MapTextureColor(headTexture, 0, BodyColor);
            JadScugUtils.MapTextureColor(headTexture, 0, ClothColor);

            if (playerRef.TryGetTarget(out var player))
            {
                HeadAtlas = Futile.atlasManager.LoadAtlasFromTexture("bcpuppetheadtexture_" + player.playerState.playerNumber + Time.time + Random.value, headTexture, false);
            }
        }
        public void LoadArmAtlas()
        {
            var armTexture = new Texture2D(Plugin.ArmTexture.width, Plugin.ArmTexture.height, TextureFormat.ARGB32, false);
            Graphics.CopyTexture(Plugin.TailTexture, armTexture);
            JadScugUtils.MapTextureColor(armTexture, 0, BodyColor);
            JadScugUtils.MapTextureColor(armTexture, 0, ClothColor);

            if (playerRef.TryGetTarget(out var player))
            {
                BodyAtlas = Futile.atlasManager.LoadAtlasFromTexture("bcpuppetlegtexture_" + player.playerState.playerNumber + Time.time + Random.value, armTexture, false);
            }
        }
        public void LoadBodyAtlas()
        {
            var bodyTexture = new Texture2D(Plugin.BodyTexture.width, Plugin.BodyTexture.height, TextureFormat.ARGB32, false);
            Graphics.CopyTexture(Plugin.BodyTexture, bodyTexture);
            JadScugUtils.MapTextureColor(bodyTexture, 0, BodyColor);
            JadScugUtils.MapTextureColor(bodyTexture, 0, ClothColor);

            if (playerRef.TryGetTarget(out var player))
            {
                BodyAtlas = Futile.atlasManager.LoadAtlasFromTexture("bcpuppetbodytexture_" + player.playerState.playerNumber + Time.time + Random.value, bodyTexture, false);
            }
        }
        public void LoadHipAtlas()
        {
            var hipTexture = new Texture2D(Plugin.HipTexture.width, Plugin.HipTexture.height, TextureFormat.ARGB32, false);
            Graphics.CopyTexture(Plugin.HipTexture, hipTexture);
            JadScugUtils.MapTextureColor(hipTexture, 0, BodyColor);
            JadScugUtils.MapTextureColor(hipTexture, 0, ClothColor);


            if (playerRef.TryGetTarget(out var player))
            {
                BodyAtlas = Futile.atlasManager.LoadAtlasFromTexture("bcpuppethiptexture_" + player.playerState.playerNumber + Time.time + Random.value, hipTexture, false);
            }
        }
        public void LoadLegAtlas()
        {
            var legTexture = new Texture2D(Plugin.LegTexture.width, Plugin.LegTexture.height, TextureFormat.ARGB32, false);
            Graphics.CopyTexture(Plugin.LegTexture, legTexture);
            JadScugUtils.MapTextureColor(legTexture, 0, BodyColor);
            JadScugUtils.MapTextureColor(legTexture, 0, ClothColor);

            if (playerRef.TryGetTarget(out var player))
            {
                BodyAtlas = Futile.atlasManager.LoadAtlasFromTexture("bcpuppetlegtexture_" + player.playerState.playerNumber + Time.time + Random.value, legTexture, false);
            }
        }
        public void LoadTailAtlas()
        {
            var tailTexture = new Texture2D(Plugin.TailTexture.width, Plugin.TailTexture.height, TextureFormat.ARGB32, false);
            Graphics.CopyTexture(Plugin.TailTexture, tailTexture);
            JadScugUtils.MapTextureColor(tailTexture, 0, BodyColor);
            JadScugUtils.MapTextureColor(tailTexture, 0, ClothColor);

            if (playerRef.TryGetTarget(out var player))
            {
                BodyAtlas = Futile.atlasManager.LoadAtlasFromTexture("bcpuppettailtexture_" + player.playerState.playerNumber + Time.time + Random.value, tailTexture, false);
            }
        }



        public void Draw(RoomCamera.SpriteLeaser sLeaser, bool nerv)
        {
            foreach (var sprite in sLeaser.sprites)
            {
                if (nerv && Futile.atlasManager._allElementsByName.TryGetValue("BCPuppetNerv_" + sprite.element.name, out var element)) { sprite.element = element; }
                else if (Futile.atlasManager._allElementsByName.TryGetValue("BCPuppet_" + sprite.element.name, out element)) { sprite.element = element; }
            }
            /*FAtlasElement tailNerv = Futile.atlasManager.GetElementWithName("BCPuppetNerv_Tail");
            FAtlasElement tailNormal = Futile.atlasManager.GetElementWithName("BCPuppet_Tail");
            sLeaser.sprites[2].element = nerv ? tailNerv : tailNormal;*/
        }

    }






    public static class BCPuppetPlayerExtension
    {
        public static Color? GetColor(this PlayerGraphics pg, PlayerColor color) => color.GetColor(pg);

        public static Color? GetColor(this Player player, PlayerColor color) => (player.graphicsModule as PlayerGraphics)?.GetColor(color);

        public static Player Get(this WeakReference<Player> weakRef)
        {
            weakRef.TryGetTarget(out var result);
            return result;
        }

        public static PlayerGraphics PlayerGraphics(this Player player) => (PlayerGraphics)player.graphicsModule;

        public static TailSegment[] Tail(this Player player) => player.PlayerGraphics().tail;


        private static readonly ConditionalWeakTable<Player, BCPuppetPlayerData> _cwt = new();

        public static BCPuppetPlayerData BCPuppet(this Player player) => _cwt.GetValue(player, _ => new BCPuppetPlayerData(player));
    }
}
