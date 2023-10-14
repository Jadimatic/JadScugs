using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace JadScugs
{
    public static class ModuleManager
    {
        public static readonly ConditionalWeakTable<Player, MouthScugModule> mouthScugData = new();
        public static bool TryGetMouthScugModule(this Player self, out MouthScugModule playerModule)
        {
            if (self.SlugCatClass.value != "MouthScug")
            {
                playerModule = null!;
                return false;
            }

            if (!mouthScugData.TryGetValue(self, out playerModule))
            {
                Debug.Log($"self: {self}");
                Debug.Log($"abstractCreature: {self.abstractCreature}");
                Debug.Log($"world: {self.abstractCreature.world}");
                Debug.Log($"game: {self.abstractCreature.world.game}");
                Debug.Log($"story session: {self.abstractCreature.world.game.GetStorySession}");
                Debug.Log($"save state: {self.abstractCreature.world.game.GetStorySession?.saveState}");
                playerModule = MouthScugModule.Load(self.abstractCreature.world.game.GetStorySession?.saveState.miscWorldSaveData, self);
                mouthScugData.Add(self, playerModule);
            }

            return true;
        }
    }
}
