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
        public float needleCounter;


        public void Draw(RoomCamera.SpriteLeaser sLeaser, bool nerv)
        {
            foreach (var sprite in sLeaser.sprites)
            {
                if (nerv && Futile.atlasManager._allElementsByName.TryGetValue("MedicNerv_" + sprite.element.name, out var element)) { sprite.element = element; }
                else if (Futile.atlasManager._allElementsByName.TryGetValue("Medic_" + sprite.element.name, out element)) { sprite.element = element; }
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