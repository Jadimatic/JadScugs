using SlugBase.SaveData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public  void Save()
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
