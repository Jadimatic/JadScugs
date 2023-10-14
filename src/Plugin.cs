using System;
using BepInEx;
using UnityEngine;
using SlugBase.Features;
using System.Security.Permissions;
using static SlugBase.Features.FeatureTypes;
using JadScugs;
using HUD;
using On;
using IL;
using System.IO;
using System.Collections.Generic;
using MoreSlugcats;
using RewiredConsts;
using MonoMod.Cil;
using Mono.Cecil.Cil;

#pragma warning disable CS0618 // Do not remove the following line.
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace JadScugs
{
    [BepInPlugin(MOD_ID, "Jad Scugs", "0.1.0")]
    class Plugin : BaseUnityPlugin
    {
        FAtlas atlas;
        public const string MOD_ID = "jadimatic.jadscugs";

        public static readonly PlayerFeature<float> JumpModifier = PlayerFloat("jadscugs/jump_modifier");
        public static readonly PlayerFeature<int> RollLength = PlayerInt("jadscugs/roll_length");
        public static readonly PlayerFeature<bool> ExplodeOnDeath = PlayerBool("jadscugs/explode_on_death");
        public static readonly GameFeature<float> MeanLizards = GameFloat("jadscugs/mean_lizards");
        public static readonly Player.AnimationIndex StuffFace = new Player.AnimationIndex("StuffFace", true);
        public static int mouthScugHeight = 14;
        private bool rolling = false;
        private int rollTimer = 0;
        private bool mouthStuffed = false;
        static bool assetsLoaded = false;
        static FAtlasManager[] atlasArray;

        // Add hooks
        public void OnEnable()
        {
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);

            // Put your custom hooks here!
            On.RainWorld.OnModsInit += Init;
            On.Player.checkInput += Player_checkInput;
            On.StoryGameSession.AddPlayer += StoryGameSession_AddPlayer;
            On.SaveState.SessionEnded += SaveState_SessionEnded;
            On.PlayerProgression.SaveToDisk += PlayerProgression_SaveToDisk;
            IL.Player.MovementUpdate += Player_MovementUpdate;
            On.Player.ctor += Player_ctor;
            On.Player.Update += Player_Update;
            On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
            On.PlayerGraphics.Update += PlayerGraphics_Update;
            On.Player.AnimationIndex.ctor += AnimationIndex_ctor;
            On.Player.UpdateBodyMode += Player_UpdateBodyMode;
            On.Player.Jump += Player_Jump;
            On.Player.GraspsCanBeCrafted += Player_GraspsCanBeCrafted;
            On.Player.SpitUpCraftedObject += Player_SpitUpCraftedObject;
            On.Player.SwallowObject += Player_SwallowObject;
            On.Player.Die += Player_Die;
            On.Lizard.ctor += Lizard_ctor;
            IL.Player.Update += Player_ILUpdate;
        }

        private void Player_ILUpdate(ILContext il)
        {
            
        }

        private void SaveState_SessionEnded(On.SaveState.orig_SessionEnded orig, SaveState self, RainWorldGame game, bool survived, bool newMalnourished)
        {
            foreach (var player in game.session.Players)
            {
                if (player != null)
                {
                    if(player?.realizedObject is Player realPlayer && realPlayer.TryGetMouthScugModule(out var module))
                    {
                        module.Save();
                    }
                }
                else
                {
                    //Shrug
                }
            }
            orig(self, game, survived, newMalnourished);
        }

        public void OnModsInit()
        {
            /*try
            {
                
            }
            catch (Exception e)
            {
                Debug.Log($"Exception at (hook name):  {e}");
            }*/
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

        private bool PlayerProgression_SaveToDisk(On.PlayerProgression.orig_SaveToDisk orig, PlayerProgression self, bool saveCurrentState, bool saveMaps, bool saveMiscProg)
        {
            
            return orig(self, saveCurrentState, saveMaps, saveMiscProg);
        }

        private void StoryGameSession_AddPlayer(On.StoryGameSession.orig_AddPlayer orig, StoryGameSession self, AbstractCreature player)
        {
            orig(self, player);
        }

        private void Player_checkInput(On.Player.orig_checkInput orig, Player self)
        {
            orig(self);//Not really sure what to do here yet
        }

        

        private void Player_SwallowObject(On.Player.orig_SwallowObject orig, Player self, int grasp)
        {
            if(self.SlugCatClass.value == "MouthScug")
            {
                if (!self.TryGetMouthScugModule(out var playerModule))
                {
                    return;
                }
                var mouthItems = playerModule.mouthItems;


                if (grasp < 0 || self.grasps[grasp] == null)
                {
                    Debug.Log("Nothing was swallowed because hand empty");
                    return;
                }

                
                /*self.objectInStomach = abstractPhysicalObject;
                self.objectInStomach.Abstractize(self.abstractCreature.pos); */
                /*BodyChunk mainBodyChunk = self.mainBodyChunk;
                mainBodyChunk.vel.y = mainBodyChunk.vel.y + 20f;*/

                AbstractPhysicalObject abstractPhysicalObject = self.grasps[grasp].grabbed.abstractPhysicalObject;
                self.ReleaseGrasp(grasp);
                if (self.input[0].y == 1)
                {
                    mouthItems[1] = abstractPhysicalObject;
                }
                else
                {
                    mouthItems[0] = abstractPhysicalObject;
                }
                abstractPhysicalObject.realizedObject.RemoveFromRoom();
                self.room.abstractRoom.RemoveEntity(abstractPhysicalObject);
                self.MouthScug().StashDelayCounter = 20;
                self.room.PlaySound(SoundID.Water_Nut_Swell, self.mainBodyChunk, false, 0.6f, 0.7f);
            }
            else
            {
                orig(self, grasp);
            }
        }

        private void AnimationIndex_ctor(On.Player.AnimationIndex.orig_ctor orig, Player.AnimationIndex self, string value, bool register)
        {
            orig(self, value, register);
        }

        private void PlayerGraphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
        {
            if(self.player.SlugCatClass.value == "MouthScug")
            {
                self.swallowing = 0;
            }
            orig(self);
        }

        /// <summary>Removes all objects from the hands of a specified slugcat.</summary>
        private void RemoveHeldObjects(Player self)
        {
            for (int i = 0; i < self.grasps.Length; i++)
            {
                if (self.grasps[i] != null)
                {
                    AbstractPhysicalObject abstractPhysicalObject = self.grasps[i].grabbed.abstractPhysicalObject;
                    self.ReleaseGrasp(i);//Makes the slugcat release an object.
                    abstractPhysicalObject.realizedObject.RemoveFromRoom();//Removes the object from the room.
                    self.room.abstractRoom.RemoveEntity(abstractPhysicalObject);//Removes the object from the world.
                    //Debug.Log("Removed "+ abstractPhysicalObject + " from hand "  + i + " of " + self.SlugCatClass.value + ".");
                }
            }
        }
        /// <summary>Puts an object in the free hand of a specified slugcat.</summary>
        private void AddHeldObject(AbstractPhysicalObject newObject, Player self)
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
        /// <summary>Generates a new object at the position of a specified slugcat.</summary>
        private void CreateObject(AbstractPhysicalObject newObject, Player self)
        {
            self.room.abstractRoom.AddEntity(newObject);//Adds object into the room.
            newObject.RealizeInRoom();//Realizes the object in the room.
            //Debug.Log("Added " + newObject + "at the position of " + self.SlugCatClass.value + ".");
        }

        private void Player_SpitUpCraftedObject(On.Player.orig_SpitUpCraftedObject orig, Player self)
        {
            if (self.SlugCatClass.value == "MouthScug")
            {
                if (self.CraftingResults() == AbstractPhysicalObject.AbstractObjectType.DangleFruit)
                {
                    if (self.FoodInStomach < self.MaxFoodInStomach) { }
                    else
                    {
                        AbstractConsumable abstractFlower = new AbstractConsumable(self.room.world, AbstractPhysicalObject.AbstractObjectType.KarmaFlower, null, self.room.GetWorldCoordinate(self.firstChunk.pos), self.room.game.GetNewID(), -1, -1, null);
                        AbstractPhysicalObject abstractRock = new AbstractPhysicalObject(self.room.world, AbstractPhysicalObject.AbstractObjectType.Rock, null, self.room.GetWorldCoordinate(self.firstChunk.pos), self.room.game.GetNewID());
                        RemoveHeldObjects(self);
                        AddHeldObject(abstractFlower, self);
                        return;
                    }
                }
                /*if (self.FreeHand() != -1 && !self.input[1].thrw && self.input[1].thrw)
                {
                    AbstractPhysicalObject abstractRock = new AbstractPhysicalObject(self.room.world, AbstractPhysicalObject.AbstractObjectType.Rock, null, self.room.GetWorldCoordinate(self.firstChunk.pos), self.room.game.GetNewID());
                    AddHeldObject(abstractRock, self);
                }*/
            }
            orig(self);
        }

        private bool Player_GraspsCanBeCrafted(On.Player.orig_GraspsCanBeCrafted orig, Player self)
        {
            orig(self);

            if (self.SlugCatClass.value == "MouthScug" && self.input[0].y == 1 && self.CraftingResults() != null) 
            {
                if(self.CraftingResults() == AbstractPhysicalObject.AbstractObjectType.DangleFruit)//DangleFruit is used for all super snacks
                {
                    if(self.FoodInStomach < self.MaxFoodInStomach)
                    {
                        return true;
                    }
                    else return true;
                }
                else return true;
            }
            else return orig(self);
        }

        private void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);
            try
            {
                // your hook's code
                MouthScug_Update(self, eu);
            }
            catch (Exception e)
            {
                Debug.Log($"Exception at (hook name):  {e}");
            }
        }

        private bool StashFlag(Player self)
        {
            return ((self.input[0].x == 0 /*&& self.input[0].y == 0*/ && !self.input[0].jmp && !self.input[0].thrw) ||
                (ModManager.MMF && self.input[0].x == 0 /*&& self.input[0].y == 1*/ && !self.input[0].jmp && !self.input[0].thrw &&
                (self.bodyMode != Player.BodyModeIndex.ClimbingOnBeam || self.animation == Player.AnimationIndex.BeamTip ||
                self.animation == Player.AnimationIndex.StandOnBeam))) && (self.mainBodyChunk.submersion < 0.5f);
        }

        private void MouthScug_Update(Player self, bool eu)
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
            if (self.input[0].y == 1)
            {
                mouthIndex = 1;
            }
            else if (self.input[0].y == 0)
            {
                mouthIndex = 0;
            }
            //ebug.Log(mouthIndex);
            Debug.Log("StashDelayCounter is "+self.MouthScug().StashDelayCounter);
            Debug.Log("SpitCounter is "+self.MouthScug().SpitCounter);
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
            if(self.MouthScug().StashDelayCounter > 0)
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
                if (RollLength.TryGet(self, out var length))
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
            if(self.MouthScug().ForceSwallowAnimCounter > 0)
            {
                self.swallowAndRegurgitateCounter = self.MouthScug().ForceSwallowAnimCounter;
            }
            Debug.Log("SwallowAndRegurgeCounter is " + self.swallowAndRegurgitateCounter);
            Debug.Log("ForceSwallowCounter is " + self.MouthScug().ForceSwallowAnimCounter);
        }


        public void UpdateMouthContents(Player self)
        {

        }

        private void Player_UpdateBodyMode(On.Player.orig_UpdateBodyMode orig, Player self)
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

        private void Init(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);


            if (!assetsLoaded)
            {
                assetsLoaded = true;
                Futile.atlasManager.LoadAtlas("atlases/MouthScug-head");
                Futile.atlasManager.LoadAtlas("atlases/StuffedMouthScug-head");
                Futile.atlasManager.LoadAtlas("atlases/MouthScug-body");
                Futile.atlasManager.LoadAtlas("atlases/MouthScug-hips");
                Futile.atlasManager.LoadAtlas("atlases/MouthScug-legs");
            }
        }

        private void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            if(self.player.SlugCatClass.value == "MouthScug")
            {
                float speed = 1.5f;
                if(self.player.MouthScug().SpitCounter > 0)
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
        

        // Load any resources, such as sprites or sounds
        private void LoadResources(RainWorld rainWorld)
        {
        }



        private void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);
            if (self.SlugCatClass.value == "MouthScug")
            {
                if(!self.playerState.isPup)
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

        // Implement MeanLizards
        private void Lizard_ctor(On.Lizard.orig_ctor orig, Lizard self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);

            if(MeanLizards.TryGet(world.game, out float meanness))
            {
                self.spawnDataEvil = Mathf.Min(self.spawnDataEvil, meanness);
            }
        }


        // Implement SuperJump
        private void Player_Jump(On.Player.orig_Jump orig, Player self)
        {
            orig(self);
            if(self.SlugCatClass.value == "MouthScug")
            {
                if (JumpModifier.TryGet(self, out var power))
                {
                    self.jumpBoost *= 1f + power;
                }
            }
        }

        // Implement ExlodeOnDeath
        private void Player_Die(On.Player.orig_Die orig, Player self)
        {
            bool wasDead = self.dead;

            orig(self);

            if(!wasDead && self.dead
                && ExplodeOnDeath.TryGet(self, out bool explode)
                && explode)
            {
                // Adapted from ScavengerBomb.Explode
                var room = self.room;
                var pos = self.mainBodyChunk.pos;
                var color = self.ShortCutColor();
                room.AddObject(new Explosion(room, self, pos, 7, 250f, 6.2f, 2f, 280f, 0.25f, self, 0.7f, 160f, 1f));
                room.AddObject(new Explosion.ExplosionLight(pos, 280f, 1f, 7, color));
                room.AddObject(new Explosion.ExplosionLight(pos, 230f, 1f, 3, new Color(1f, 1f, 1f)));
                room.AddObject(new ExplosionSpikes(room, pos, 14, 30f, 9f, 7f, 170f, color));
                room.AddObject(new ShockWave(pos, 330f, 0.045f, 5, false));

                room.ScreenMovement(pos, default, 1.3f);
                room.PlaySound(SoundID.Bomb_Explode, pos);
                room.InGameNoise(new Noise.InGameNoise(pos, 9000f, self, 1f));
            }
        }
    }
}