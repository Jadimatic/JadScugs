using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JadScugs
{
    public static class WipHooks
    { 
        public static void Init()
        {
            On.Player.checkInput += Player_checkInput;
            On.StoryGameSession.AddPlayer += StoryGameSession_AddPlayer;
            On.PlayerProgression.SaveToDisk += PlayerProgression_SaveToDisk;
            On.Player.AnimationIndex.ctor += AnimationIndex_ctor;
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);

            IL.Player.Update += Player_ILUpdate;
            IL.Player.MovementUpdate += Player_MovementUpdate;
        }

        private static void LoadResources(RainWorld rainWorld)
        {
        }
        private static void AnimationIndex_ctor(On.Player.AnimationIndex.orig_ctor orig, Player.AnimationIndex self, string value, bool register)
        {
            orig(self, value, register);
        }

        private static bool PlayerProgression_SaveToDisk(On.PlayerProgression.orig_SaveToDisk orig, PlayerProgression self, bool saveCurrentState, bool saveMaps, bool saveMiscProg)
        {

            return orig(self, saveCurrentState, saveMaps, saveMiscProg);
        }

        private static void StoryGameSession_AddPlayer(On.StoryGameSession.orig_AddPlayer orig, StoryGameSession self, AbstractCreature player)
        {
            orig(self, player);
        }

        private static void Player_checkInput(On.Player.orig_checkInput orig, Player self)
        {
            orig(self);
        }

        private static void Player_ILUpdate(ILContext il)
        {
            // Empty
        }

        private static void Player_MovementUpdate(ILContext il)
        {
            //==--This entire method has been commented due to developmental difficulties, it will be revisited at a future time.--==
            /*var cursor = new ILCursor(il);

            cursor.GotoNext(MoveType.After,
                i => i.MatchCallOrCallvirt<Player>("get_playerState"),
                i => i.MatchLdfld<PlayerState>(nameof(PlayerState.isPup)),
                i => i.MatchBrtrue(out _),
                i => i.MatchLdcI4(out _));

            cursor.MoveAfterLabels();

            cursor.Emit(OpCodes.Ldarg_0);
            //-- Distance when not pup
            cursor.EmitDelegate((int originalDistance, Player self) => self.SlugCatClass.value == "MouthScug" ? mouthScugHeight : originalDistance);

            cursor.GotoNext(MoveType.After, i => i.MatchLdcI4(out _));

            cursor.MoveAfterLabels();

            cursor.Emit(OpCodes.Ldarg_0);
            //-- Distance when pup
            cursor.EmitDelegate((int originalDistance, Player self) => self.SlugCatClass.value == "MouthScug" ? mouthScugHeight / 2 : originalDistance);

            cursor.GotoNext(MoveType.After,
                i => i.MatchLdfld<PhysicalObject>(nameof(PhysicalObject.bodyChunkConnections)),
                i => i.MatchLdcI4(out _),
                i => i.MatchLdelemRef(),
                i => i.MatchLdcR4(out _));

            cursor.MoveAfterLabels();

            cursor.Emit(OpCodes.Ldarg_0);
            //-- Distance when rolling
            cursor.EmitDelegate((float originalDistance, Player self) => self.SlugCatClass.value == "MouthScug" ? mouthScugHeight : originalDistance);*/
        }
    }
}
