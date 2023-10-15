using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace JadScugs
{
    public static class PlayerGraphicsHooks
    {
        static bool mouthStuffed = false;
        static int rollTimer = 0;

        public static readonly Player.AnimationIndex StuffFace = new Player.AnimationIndex("StuffFace", true);
        public static void Init()
        {
            On.Player.ctor += Player_ctor;
            On.Player.UpdateBodyMode += Player_UpdateBodyMode;
            On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
            On.PlayerGraphics.Update += PlayerGraphics_Update;
            On.Player.Update += Player_Update;

            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);
        }

        private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);
            try
            {
                MouthScug_Update(self, eu);
            }
            catch (Exception e)
            {
                Debug.Log($"Exception at (hook name):  {e}");
            }
        }

        private static void MouthScug_Update(Player self, bool eu)
        {
            if (!self.TryGetMouthScugModule(out var playerModule))
            {
                return;
            }
            //Debug.Log("eatCounter is "+self.eatCounter);
            //Debug.Log("eatMeat is " + self.eatMeat);
            //Debug.Log("swallowAndRegurgitateCounter value is "+self.swallowAndRegurgitateCounter);
            //self.swallowAndRegurgitateCounter++;
            var mouthItems = playerModule.mouthItems;
            int mouthIndex = 0;
            int length = 600;
            if (self.input[0].y == 1)
            {
                mouthIndex = 1;
            }
            else if (self.input[0].y == 0)
            {
                mouthIndex = 0;
            }
            //ebug.Log(mouthIndex);
            Debug.Log("StashDelayCounter is " + self.MouthScug().StashDelayCounter);
            Debug.Log("SpitCounter is " + self.MouthScug().SpitCounter);
            if (StashFlag(self) && self.MouthScug().StashDelayCounter == 0 && self.eatCounter == 40 && self.eatMeat == 0)
            {
                if (self.input[0].pckp && mouthItems[mouthIndex] != null)
                {
                    self.swallowAndRegurgitateCounter = 0;
                    self.MouthScug().SpitCounter++;
                    if (self.MouthScug().SpitCounter > 60)
                    {
                        AddHeldObject(mouthItems[mouthIndex], self);
                        mouthItems[mouthIndex] = null;
                        self.room.PlaySound(SoundID.Water_Nut_Swell, self.mainBodyChunk, false, 0.6f, 0.7f);
                        self.MouthScug().SpitCounter = 0;
                    }
                }
                else
                {
                    self.MouthScug().SpitCounter = 0;
                }
            }
            else
            {
                self.MouthScug().SpitCounter = 0;
            }
            if (self.MouthScug().StashDelayCounter > 0)
            {
                self.MouthScug().StashDelayCounter--;
            }

            if (StashFlag(self) && self.input[0].y == 1 && self.input[0].pckp && mouthItems[1] == null)
            {
                self.MouthScug().ForceSwallowAnimCounter++;
            }
            else
            {
                self.MouthScug().ForceSwallowAnimCounter = 0;
            }
            if (self.MouthScug().ForceSwallowAnimCounter > 90)
            {
                for (int num13 = 0; num13 < 2; num13++)
                {
                    if (self.grasps[num13] != null && self.CanBeSwallowed(self.grasps[num13].grabbed))
                    {
                        self.SwallowObject(num13);
                        self.MouthScug().ForceSwallowAnimCounter = 0;
                    }
                }
            }




            if (self.input[0].jmp && !self.input[1].jmp)
            {
                /*if(playerModule.mouthCreature == null)
                {
                    playerModule.mouthCreature = new AbstractCreature(self.room.world, StaticWorld.GetCreatureTemplate(MoreSlugcatsEnums.CreatureTemplateType.FireBug), null, self.room.GetWorldCoordinate(self.firstChunk.pos), self.room.game.GetNewID());
                }
                else 
                {
                    playerModule.mouthCreature = null;
                }*/
                /*mouthCreature.Die();
                self.room.abstractRoom.AddEntity(mouthCreature);
                mouthCreature.RealizeInRoom();*/
            }
            //Debug.Log("The current spawn queue is: " + playerModule.mouthCreature);
            Debug.Log(mouthItems[0] + ", " + mouthItems[1]);

            if (self.animation == Player.AnimationIndex.Roll)
            {
                rollTimer += 1;
                if (rollTimer < length)
                {
                    if (self.input[0].y < 0)
                    {
                        self.rollCounter = 14;
                    }
                }
            }
            else { rollTimer = 0; }

            if (Input.GetKeyDown(KeyCode.X))
            {
                if (mouthStuffed)
                {
                    mouthStuffed = false;
                }
                else
                {
                    mouthStuffed = true;
                }
            }
            if (self.MouthScug().ForceSwallowAnimCounter > 0)
            {
                self.swallowAndRegurgitateCounter = self.MouthScug().ForceSwallowAnimCounter;
            }
            Debug.Log("SwallowAndRegurgeCounter is " + self.swallowAndRegurgitateCounter);
            Debug.Log("ForceSwallowCounter is " + self.MouthScug().ForceSwallowAnimCounter);
        }
        private static void CreateObject(AbstractPhysicalObject newObject, Player self)
        {
            self.room.abstractRoom.AddEntity(newObject);//Adds object into the room.
            newObject.RealizeInRoom();//Realizes the object in the room.
            //Debug.Log("Added " + newObject + "at the position of " + self.SlugCatClass.value + ".");
        }
        private static bool StashFlag(Player self)
        {
            return ((self.input[0].x == 0 /*&& self.input[0].y == 0*/ && !self.input[0].jmp && !self.input[0].thrw) ||
                (ModManager.MMF && self.input[0].x == 0 /*&& self.input[0].y == 1*/ && !self.input[0].jmp && !self.input[0].thrw &&
                (self.bodyMode != Player.BodyModeIndex.ClimbingOnBeam || self.animation == Player.AnimationIndex.BeamTip ||
                self.animation == Player.AnimationIndex.StandOnBeam))) && (self.mainBodyChunk.submersion < 0.5f);
        }

        private static void AddHeldObject(AbstractPhysicalObject newObject, Player self)
        {
            self.room.abstractRoom.AddEntity(newObject);//Adds object into the room.
            newObject.pos = self.room.GetWorldCoordinate(self.firstChunk.pos);
            newObject.RealizeInRoom();//Realizes the object in the room.
            if (self.FreeHand() != -1)
            {
                self.SlugcatGrab(newObject.realizedObject, self.FreeHand());//Places the object in a slugcat's hand.
            }
            else
            {
                Debug.Log(self.SlugCatClass.value + " failed to grab object because no hands were free."); return;
            }
        }

        public static void UpdateMouthContents(Player self)
        {

        }

        private static void PlayerGraphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
        {
            if (self.player.SlugCatClass.value == "MouthScug")
            {
                self.swallowing = 0;
            }
            orig(self);
        }

        private static void Player_UpdateBodyMode(On.Player.orig_UpdateBodyMode orig, Player self)
        {
            orig(self);
            if (self.SlugCatClass.value == "MouthScug")
            {//This fixes jank arms while hanging from poles by repositioning the body
                if (self.animation == Player.AnimationIndex.HangUnderVerticalBeam)
                {
                    self.bodyChunks[0].vel.y = 1.25f;
                    if (self.input[0].y > 0)
                    {
                        self.bodyChunks[0].vel.y += 1.25f;
                    }
                }
                if (self.animation == Player.AnimationIndex.BellySlide)
                {
                    //Put code to change the slide here or something
                }
            }
        }

        private static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            if (self.player.SlugCatClass.value == "MouthScug")
            {
                float speed = 1.5f;
                if (self.player.MouthScug().SpitCounter > 0)
                {
                    //Debug.Log("Shaking!");
                    self.PlayerBlink();
                    sLeaser.sprites[3].x += Mathf.Sin(Mathf.PI * Mathf.Lerp(self.player.MouthScug().SpitCounter, self.player.MouthScug().SpitCounter - 1, timeStacker) * speed);
                    sLeaser.sprites[9].x += Mathf.Sin(Mathf.PI * Mathf.Lerp(self.player.MouthScug().SpitCounter, self.player.MouthScug().SpitCounter - 1, timeStacker) * speed);
                }

                if (!self.player.playerState.isPup)
                {

                    //Debug.Log("Is Not Baby");
                }
                else
                {
                    //Debug.Log("Is Baby");
                }
                //self.playerState.isPup = true;

                //HEAD
                string headName = sLeaser.sprites[3].element.name;
                if (mouthStuffed)
                {
                    if (headName.StartsWith("HeadA") && Futile.atlasManager.DoesContainElementWithName("StuffedMouthScug_" + headName))
                    {
                        sLeaser.sprites[3].SetElementByName("StuffedMouthScug_" + headName);
                    }
                }
                else
                {
                    if (headName.StartsWith("HeadA") && Futile.atlasManager.DoesContainElementWithName("MouthScug_" + headName))
                    {
                        sLeaser.sprites[3].SetElementByName("MouthScug_" + headName);
                    }
                }

                //FACE
                string faceName = sLeaser.sprites[9].element.name;
                if (faceName.StartsWith("FaceA") && Futile.atlasManager.DoesContainElementWithName("MouthScug_" + faceName))
                {
                    sLeaser.sprites[9].SetElementByName("MouthScug_" + faceName);
                }
                if (faceName.StartsWith("FaceB") && Futile.atlasManager.DoesContainElementWithName("MouthScug_" + faceName))
                {
                    sLeaser.sprites[9].SetElementByName("MouthScug_" + faceName);
                }
                if (faceName.StartsWith("FaceStunned") && Futile.atlasManager.DoesContainElementWithName("MouthScug_" + faceName))
                {
                    sLeaser.sprites[9].SetElementByName("MouthScug_" + faceName);
                }
                if (faceName.StartsWith("FaceDead") && Futile.atlasManager.DoesContainElementWithName("MouthScug_" + faceName))
                {
                    sLeaser.sprites[9].SetElementByName("MouthScug_" + faceName);
                }
                //TORSO
                string bodyName = sLeaser.sprites[0].element.name;
                if (bodyName.StartsWith("BodyA") && Futile.atlasManager.DoesContainElementWithName("MouthScug_" + bodyName))
                {
                    sLeaser.sprites[0].SetElementByName("MouthScug_" + bodyName);
                }
                string hipsName = sLeaser.sprites[1].element.name;
                if (hipsName.StartsWith("HipsA") && Futile.atlasManager.DoesContainElementWithName("MouthScug_" + hipsName))
                {
                    sLeaser.sprites[1].SetElementByName("MouthScug_" + hipsName);
                }
                //LEGS
                string legsName = sLeaser.sprites[4].element.name;
                if (legsName.StartsWith("LegsA") && Futile.atlasManager.DoesContainElementWithName("MouthScug_" + legsName))
                {
                    sLeaser.sprites[4].SetElementByName("MouthScug_" + legsName);
                }
            }
        }

        private static void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);
            if (self.SlugCatClass.value == "MouthScug")
            {
                if (!self.playerState.isPup)
                {
                    //self.bodyChunkConnections[0] = new PhysicalObject.BodyChunkConnection(self.bodyChunks[0], self.bodyChunks[1], 34f,
                    //PhysicalObject.BodyChunkConnection.Type.Normal, 1f, 0.5f);
                    //Debug.Log("Is Not Baby");
                }
                else
                {
                    //self.bodyChunkConnections[0] = new PhysicalObject.BodyChunkConnection(self.bodyChunks[0], self.bodyChunks[1], 8f,
                    //PhysicalObject.BodyChunkConnection.Type.Normal, 1f, 0.5f);
                    //Debug.Log("Is Baby");
                }
                //self.playerState.isPup = true;
            }
        }

        private static void LoadResources(RainWorld rainWorld)
        {
        }
    }
}
