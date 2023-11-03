using System;
using System.Linq;
using System.Runtime.CompilerServices;
using RWCustom;
using SlugBase.DataTypes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace JadScugs
{

    public class MedicPlayerData
    {
        public bool grabbed;
        public int needles;
        public int maxNeedles = 16;
        public float needleCounter;
        public bool hasNeedles;
        public int needleThreshhold = 3;

        public void TailUpdate(PlayerGraphics self, bool initialized)
        {
            Vector2[] tailSegmentPositions = new Vector2[4];
            tailSegmentPositions[0] = self.tail[0].pos; tailSegmentPositions[1] = self.tail[1].pos;
            tailSegmentPositions[2] = self.tail[2].pos; tailSegmentPositions[3] = self.tail[3].pos;
            if (self.player.Medic().needles > self.player.Medic().needleThreshhold && (!self.player.Medic().hasNeedles || !initialized))
            {
                Debug.Log("PLAYER SPRITES UPDATED!"); Debug.Log("PLAYER SPRITES UPDATED!"); Debug.Log("PLAYER SPRITES UPDATED!"); Debug.Log("PLAYER SPRITES UPDATED!"); Debug.Log("PLAYER SPRITES UPDATED!"); Debug.Log("PLAYER SPRITES UPDATED!"); Debug.Log("PLAYER SPRITES UPDATED!"); Debug.Log("PLAYER SPRITES UPDATED!");
                self.tail = new TailSegment[4];
                self.tail[0] = new TailSegment(self, 7.5f, 4f, null, 0.5f, 1f, 1f, true); self.tail[0].pos = tailSegmentPositions[0];
                self.tail[1] = new TailSegment(self, 7.4f, 7f, self.tail[0], 0.5f, 1f, 0.5f, true); self.tail[1].pos = tailSegmentPositions[1];
                self.tail[2] = new TailSegment(self, 7.3f, 7f, self.tail[1], 0.5f, 1f, 0.5f, true); self.tail[2].pos = tailSegmentPositions[2];
                self.tail[3] = new TailSegment(self, 4.5f, 7f, self.tail[2], 0.5f, 1f, 0.5f, true); self.tail[3].pos = tailSegmentPositions[3];
                var bp = self.bodyParts.ToList();
                bp.RemoveAll(x => x is TailSegment);
                bp.AddRange(self.tail);

                self.bodyParts = bp.ToArray();
                self.player.Medic().hasNeedles = true;
            }
            else if (self.player.Medic().needles < self.player.Medic().needleThreshhold + 1 && (self.player.Medic().hasNeedles || !initialized))
            {
                self.tail = new TailSegment[4];
                self.tail[0] = new TailSegment(self, 6f, 4f, null, 0.5f, 1f, 1f, true); self.tail[0].pos = tailSegmentPositions[0];
                self.tail[1] = new TailSegment(self, 4f, 7f, self.tail[0], 0.5f, 1f, 0.5f, true); self.tail[1].pos = tailSegmentPositions[1];
                self.tail[2] = new TailSegment(self, 2.5f, 7f, self.tail[1], 0.5f, 1f, 0.5f, true); self.tail[2].pos = tailSegmentPositions[2];
                self.tail[3] = new TailSegment(self, 1f, 7f, self.tail[2], 0.5f, 1f, 0.5f, true); self.tail[3].pos = tailSegmentPositions[3];
                var bp = self.bodyParts.ToList();
                bp.RemoveAll(x => x is TailSegment);
                bp.AddRange(self.tail);

                self.bodyParts = bp.ToArray();
                self.player.Medic().hasNeedles = false;
            }
        }
        public void Draw(RoomCamera.SpriteLeaser sLeaser, PlayerGraphics self, bool nerv)
        {
            foreach (var sprite in sLeaser.sprites)
            {
                if (nerv && Futile.atlasManager._allElementsByName.TryGetValue("MedicNerv_" + sprite.element.name, out var element)) { sprite.element = element; }
                else if (Futile.atlasManager._allElementsByName.TryGetValue("Medic_" + sprite.element.name, out element)) { sprite.element = element; }
            }
            //FAtlasElement tailNeedlesFull = Futile.atlasManager.GetElementWithName("MedicNeedlesFull_Tail");
            //FAtlasElement tailNeedlesHalf = Futile.atlasManager.GetElementWithName("MedicNeedlesHalf_Tail");
            //FAtlasElement tailNeedlesEmpty = Futile.atlasManager.GetElementWithName("MedicNeedlesEmpty_Tail");
            if (self.player.Medic().needles < self.player.Medic().needleThreshhold + 1)
            {
                //Debug.Log("Tail-Empty");
            }
            if (self.player.Medic().needles > self.player.Medic().needleThreshhold && self.player.Medic().needles < (self.player.Medic().maxNeedles / 2) + 1)
            {
                //Debug.Log("Tail-Half");
            }
            if (self.player.Medic().needles > self.player.Medic().needleThreshhold && self.player.Medic().needles > (self.player.Medic().maxNeedles / 2))
            {
                //Debug.Log("Tail-Full");
            }
        }
        public int NeedleTypeIndex(Player self)
        {
            int index = -1;
            bool[] triggersDown = self.Input().TriggersDown(self);
            if (triggersDown[0] && triggersDown[1]) { index = -2; }
            else if (triggersDown[0]) { index = 0; }
            else if (triggersDown[1]) { index = 1; }
            return index;
        }
    }

    public static class MedicPlayerExtension
    {
        private static readonly ConditionalWeakTable<Player, MedicPlayerData> _cwt = new();

        public static MedicPlayerData Medic(this Player player) => _cwt.GetValue(player, _ => new MedicPlayerData());
    }
}