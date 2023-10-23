using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.CompilerServices;
using UnityEngine;

namespace JadScugs;

public class AttachedSprite
{
    private static readonly ConditionalWeakTable<PlayerGraphics, List<AttachedSprite>> _cwt = new();

    public readonly AttachedSpriteType SpriteType;
    public string SpritePrefix;
    public int SpriteIndex;
    public bool InFront;
    public string Container;

    private bool ScheduledForRemoval;

    private AttachedSprite(AttachedSpriteType type, string spritePrefix, bool inFront, string container)
    {
        SpriteType = type;
        SpritePrefix = spritePrefix;
        InFront = inFront;
        Container = container;
    }

    public static AttachedSprite Create(PlayerGraphics playerGraphics, AttachedSpriteType type, string spritePrefix, bool inFront = true, string container = "Midground")
    {
        var attachedSprite = new AttachedSprite(type, spritePrefix, inFront, container);
        _cwt.GetValue(playerGraphics, _ => new()).Add(attachedSprite);
        return attachedSprite;
    }

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        SpriteIndex = sLeaser.sprites.Length;
        var sprites = SpriteType == AttachedSpriteType.Arms ? 4 : 1;
        Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + sprites);
        for (var i = 0; i < sprites; i++)
        {
            sLeaser.sprites[SpriteIndex + i] = new FSprite("pixel");
        }
    }

    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        if (sLeaser.sprites.Length <= (SpriteType == AttachedSpriteType.Arms ? SpriteIndex + 3 : SpriteIndex)) return;

        newContatiner ??= rCam.ReturnFContainer(Container);

        for (var i = 0; i < (SpriteType == AttachedSpriteType.Arms ? 4 : 1); i++)
        {
            var sprite = sLeaser.sprites[SpriteIndex + i];
            sprite.RemoveFromContainer();
            newContatiner.AddChild(sprite);

            if (InFront)
            {
                sprite.MoveInFrontOfOtherNode(sLeaser.sprites[(int)SpriteType + i]);
            }
            else
            {
                sprite.MoveBehindOtherNode(sLeaser.sprites[(int)SpriteType + i]);
            }
        }
    }

    public void RemoveFromContainer()
    {
        ScheduledForRemoval = true;
    }

    #region Hooks
    public static void Apply()
    {
        On.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSprites;
        On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
        On.PlayerGraphics.AddToContainer += PlayerGraphics_AddToContainer;
    }

    private static void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);
        if (!_cwt.TryGetValue(self, out var attachedSprites) || attachedSprites.Count == 0) return;

        foreach (var attachedSprite in attachedSprites)
        {
            attachedSprite.InitiateSprites(sLeaser, rCam);
        }

        self.AddToContainer(sLeaser, rCam, null);
    }

    private static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (!_cwt.TryGetValue(self, out var attachedSprites) || attachedSprites.Count == 0) return;

        foreach (var attachedSprite in attachedSprites.ToList())
        {
            if (sLeaser.sprites.Length <= (attachedSprite.SpriteType == AttachedSpriteType.Arms ? attachedSprite.SpriteIndex + 3 : attachedSprite.SpriteIndex)) continue;

            if (attachedSprite.ScheduledForRemoval)
            {
                for (var i = 0; i < (attachedSprite.SpriteType == AttachedSpriteType.Arms ? 4 : 1); i++)
                {
                    sLeaser.sprites[attachedSprite.SpriteIndex + i].RemoveFromContainer();
                }

                attachedSprites.Remove(attachedSprite);
                continue;
            }

            for (var i = 0; i < (attachedSprite.SpriteType == AttachedSpriteType.Arms ? 4 : 1); i++)
            {
                var sprite = sLeaser.sprites[attachedSprite.SpriteIndex + i];
                var originalSprite = sLeaser.sprites[(int)attachedSprite.SpriteType + i];

                if (Futile.atlasManager._allElementsByName.TryGetValue(attachedSprite.SpritePrefix + originalSprite.element.name, out var newElement))
                {
                    sprite.element = newElement;
                }
                sprite.SetPosition(originalSprite.GetPosition());
                sprite.rotation = originalSprite.rotation;
                sprite.scaleX = originalSprite.scaleX;
                sprite.scaleY = originalSprite.scaleY;
                sprite.isVisible = originalSprite.isVisible;
                sprite.alpha = originalSprite.alpha;
            }
        }
    }

    private static void PlayerGraphics_AddToContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        orig(self, sLeaser, rCam, newContatiner);
        if (!_cwt.TryGetValue(self, out var attachedSprites) || attachedSprites.Count == 0) return;

        foreach (var attachedSprite in attachedSprites)
        {
            attachedSprite.AddToContainer(sLeaser, rCam, newContatiner);
        }
    }
    #endregion

    public enum AttachedSpriteType
    {
        Body = 0,
        Hips = 1,
        Head = 3,
        Legs = 4,
        Arms = 5,
        Face = 9,
        Mark = 11
    }
}