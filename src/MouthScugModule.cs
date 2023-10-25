using SlugBase.SaveData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace JadScugs
{
    public class MouthScugModule
    {
        public Player player;
        public AbstractPhysicalObject[] mouthItems = new AbstractPhysicalObject[2];
        public AbstractCreature mouthCreature;
        

        public static MouthScugModule Load(MiscWorldSaveData data, Player player)
        {
            MouthScugModule mouth = new MouthScugModule();
            mouth.player = player;
            if (data != null && data.GetSlugBaseData().TryGet($"MouthScug_Items_{player.playerState.playerNumber}", out string[] savedItems))
            {
                if (savedItems[0] == null){mouth.mouthItems[0] = null;}
                else{ mouth.mouthItems[0] = SaveState.AbstractPhysicalObjectFromString(player.room.world, savedItems[0]); }
                if (savedItems[1] == null){mouth.mouthItems[1] = null;}
                else{ mouth.mouthItems[1] = SaveState.AbstractPhysicalObjectFromString(player.room.world, savedItems[1]); }

                if (savedItems[2] == null) {mouth.mouthCreature = null;}
                else { mouth.mouthCreature = SaveState.AbstractCreatureFromString(player.room.world, savedItems[2], false); }
            }
            return mouth;
        }

        public bool MouthContains(Player self, AbstractPhysicalObject.AbstractObjectType obj)
        {
            if (!self.TryGetMouthScugModule(out var playerModule))
            {
                return false;
            }
            var mouthItems = playerModule.mouthItems;
            for (int i = 0; i < mouthItems.Length; i++)
            {
                if (mouthItems[i] != null)
                {
                    if (mouthItems[i].type == obj)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public int FindMouthItemSlot(Player self, AbstractPhysicalObject.AbstractObjectType objType)
        {
            if (!self.TryGetMouthScugModule(out var playerModule))
            {
                return -1;
            }
            var mouthItems = playerModule.mouthItems;
            int itemIndex = -1;
            for (int i = 0; i < mouthItems.Length; i++)
            {
                if (mouthItems[i] != null && itemIndex < 0)
                {
                    if (mouthItems[i].type == objType)
                    {
                        itemIndex = i;
                    }
                }
            }
            return itemIndex;
        }

        public int MouthIndex(Player self)
        {
            int index = -1;
            bool[] triggersDown = self.Input().TriggersDown(self);
            if (triggersDown[0] && triggersDown[1]) { index = -2; }
            else if (triggersDown[0]) { index = 0; }
            else if (triggersDown[1]) { index = 1; }
            return index;
        }

        public void Save()
        {
            var data = player.abstractCreature.world.game.GetStorySession.saveState.miscWorldSaveData;
            data.GetSlugBaseData().Set($"MouthScug_Items_{player.playerState.playerNumber}", new string[] {
            mouthItems[0]?.ToString(),
            mouthItems[1]?.ToString(),
            mouthCreature == null ? null : SaveState.AbstractCreatureToStringStoryWorld(mouthCreature)
        });
        }
    }
}
