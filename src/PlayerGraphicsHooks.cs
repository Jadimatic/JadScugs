using MoreSlugcats;
using Noise;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static MonoMod.InlineRT.MonoModRule;

namespace JadScugs
{
    public static class PlayerGraphicsHooks
    {
        static int rollTimer = 0;

        public static readonly Player.AnimationIndex StuffFace = new Player.AnimationIndex("StuffFace", true);
        public static void Init()
        {
            On.Player.ctor += Player_ctor;
            On.Player.UpdateBodyMode += Player_UpdateBodyMode;
            On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
            On.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSprites;
            On.PlayerGraphics.ctor += PlayerGraphics_ctor;
            On.PlayerGraphics.Update += PlayerGraphics_Update;
            On.Player.Update += Player_Update;
            AttachedSprite.Apply();
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);
        }

        private static void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            orig(self, sLeaser, rCam);
            //TAIL
            if(self.player.SlugCatClass.value == "MouthScug")
            {
                if (sLeaser.sprites[2] is TriangleMesh tail && Futile.atlasManager.DoesContainElementWithName("MouthScug_Tail"))
                {
                    tail.element = Futile.atlasManager.GetElementWithName("MouthScug_Tail");
                    for (var i = tail.vertices.Length - 1; i >= 0; i--)
                    {
                        var perc = i / 2 / (float)(tail.vertices.Length / 2);
                        Vector2 uv;
                        if (i % 2 == 0)
                            uv = new Vector2(perc, 0f);
                        else if (i < tail.vertices.Length - 1)
                            uv = new Vector2(perc, 1f);
                        else
                            uv = new Vector2(1f, 0f);

                        // Map UV values to the element
                        uv.x = Mathf.Lerp(tail.element.uvBottomLeft.x, tail.element.uvTopRight.x, uv.x);
                        uv.y = Mathf.Lerp(tail.element.uvBottomLeft.y, tail.element.uvTopRight.y, uv.y);

                        tail.UVvertices[i] = uv;
                    }
                }
            }
            if (self.player.SlugCatClass.value == "BCPuppet")
            {
                if (sLeaser.sprites[2] is TriangleMesh tail && Futile.atlasManager.DoesContainElementWithName("BCPuppet_Tail"))
                {
                    tail.element = Futile.atlasManager.GetElementWithName("BCPuppet_Tail");
                    for (var i = tail.vertices.Length - 1; i >= 0; i--)
                    {
                        var perc = i / 2 / (float)(tail.vertices.Length / 2);
                        Vector2 uv;
                        if (i % 2 == 0)
                            uv = new Vector2(perc, 0f);
                        else if (i < tail.vertices.Length - 1)
                            uv = new Vector2(perc, 1f);
                        else
                            uv = new Vector2(1f, 0f);

                        // Map UV values to the element
                        uv.x = Mathf.Lerp(tail.element.uvBottomLeft.x, tail.element.uvTopRight.x, uv.x);
                        uv.y = Mathf.Lerp(tail.element.uvBottomLeft.y, tail.element.uvTopRight.y, uv.y);

                        tail.UVvertices[i] = uv;
                    }
                }
            }
        }

        private static void PlayerGraphics_ctor(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
        {
            orig(self, ow);


            if(self.player.SlugCatClass.value == "BCPuppet")
            {
                var headSprite = AttachedSprite.Create(self, AttachedSprite.AttachedSpriteType.Head, "BCPuppet2_");
                for (int i = 0; i < self.hands.Length; i++)
                {
                    self.hands[i].mode = Limb.Mode.Dangle;
                    self.hands[i].retractCounter = 0;
                }
            }

            if (self.player.SlugCatClass.value == "MouthScug")
            {
                self.tail[0] = new TailSegment(self, 7.5f, 4f, null, 0.5f, 1f, 1f, true);
                self.tail[1] = new TailSegment(self, 8f, 7f, self.tail[0], 0.5f, 1f, 0.5f, true);
                self.tail[2] = new TailSegment(self, 7f, 7f, self.tail[1], 0.5f, 1f, 0.5f, true);
                self.tail[3] = new TailSegment(self, 4.5f, 7f, self.tail[2], 0.5f, 1f, 0.5f, true);
            }
        }











        public static bool ArtificerConsussionConditions(Player self)
        {
            bool flag = self.wantToJump > 0 && self.input[0].pckp;
            bool flag2 = self.eatMeat >= 20 || self.maulTimer >= 15;
            return (flag && !self.submerged && !flag2 && (self.input[0].y < 0 || self.bodyMode == Player.BodyModeIndex.Crawl) && (self.canJump > 0 || self.input[0].y < 0) && self.Consious && !self.pyroJumpped && self.pyroParryCooldown <= 0f);
        }

        public static void ConcussiveBlast(Player self, bool smoke, bool light)
        {
            self.pyroParryCooldown = 40f;
            Vector2 pos2 = self.firstChunk.pos;
            for (int k = 0; k < 8; k++)
            {
                if(smoke)
                {
                    self.room.AddObject(new Explosion.ExplosionSmoke(pos2, Custom.RNV() * 5f * UnityEngine.Random.value, 1f));
                }
            }
            if(light)
            {
                self.room.AddObject(new Explosion.ExplosionLight(pos2, 160f, 1f, 3, Color.white));
            }
            for (int l = 0; l < 10; l++)
            {
                Vector2 a2 = Custom.RNV();
                self.room.AddObject(new Spark(pos2 + a2 * UnityEngine.Random.value * 40f, a2 * Mathf.Lerp(4f, 30f, UnityEngine.Random.value), Color.white, null, 4, 18));
            }
            self.room.AddObject(new ShockWave(pos2, 200f, 0.2f, 6, false));
            self.room.PlaySound(SoundID.Fire_Spear_Explode, pos2, 0.3f + UnityEngine.Random.value * 0.3f, 0.5f + UnityEngine.Random.value * 2f);
            self.room.InGameNoise(new InGameNoise(pos2, 8000f, self, 1f));
            List<Weapon> list = new List<Weapon>();
            for (int m = 0; m < self.room.physicalObjects.Length; m++)
            {
                for (int n = 0; n < self.room.physicalObjects[m].Count; n++)
                {
                    if (self.room.physicalObjects[m][n] is Weapon)
                    {
                        Weapon weapon = self.room.physicalObjects[m][n] as Weapon;
                        if (weapon.mode == Weapon.Mode.Thrown && Custom.Dist(pos2, weapon.firstChunk.pos) < 300f)
                        {
                            list.Add(weapon);
                        }
                    }
                    bool flag3;
                    if (ModManager.CoopAvailable && !Custom.rainWorld.options.friendlyFire)
                    {
                        Player player = self.room.physicalObjects[m][n] as Player;
                        flag3 = (player == null || player.isNPC);
                    }
                    else
                    {
                        flag3 = true;
                    }
                    bool flag4 = flag3;
                    if (self.room.physicalObjects[m][n] is Creature && self.room.physicalObjects[m][n] != self && flag4)
                    {
                        Creature creature = self.room.physicalObjects[m][n] as Creature;
                        if (Custom.Dist(pos2, creature.firstChunk.pos) < 200f && (Custom.Dist(pos2, creature.firstChunk.pos) < 60f || self.room.VisualContact(self.abstractCreature.pos, creature.abstractCreature.pos)))
                        {
                            self.room.socialEventRecognizer.WeaponAttack(null, self, creature, true);
                            creature.SetKillTag(self.abstractCreature);
                            if (creature is Scavenger)
                            {
                                (creature as Scavenger).HeavyStun(80);
                            }
                            else
                            {
                                creature.Stun(80);
                            }
                            creature.firstChunk.vel = Custom.DegToVec(Custom.AimFromOneVectorToAnother(pos2, creature.firstChunk.pos)) * 30f;
                            if (creature is TentaclePlant)
                            {
                                for (int num5 = 0; num5 < creature.grasps.Length; num5++)
                                {
                                    creature.ReleaseGrasp(num5);
                                }
                            }
                        }
                    }
                }
            }
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
        private static bool MouthStuffed(Player self)
        {
            if (!self.TryGetMouthScugModule(out var playerModule))
            {
                return false;
            }
            if(playerModule.mouthItems[0] != null || playerModule.mouthItems[1] != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static void MouthScug_Update(Player self, bool eu)
        {
            if (!self.TryGetMouthScugModule(out var playerModule))
            {
                return;
            }
            var mouthItems = playerModule.mouthItems;
            int mouthIndex = -1;
            int length = 600;
            /*if (self.input[0].y == 1)
            {
                mouthIndex = 1;
            }
            else if (self.input[0].y == 0)
            {
                mouthIndex = 0;
            }*/

            mouthIndex = playerModule.MouthIndex(self);
            //Debug.Log("StashDelayCounter is " + self.MouthScug().StashDelayCounter);
            //Debug.Log("Protein Boost? "+self.MouthScug().ProteinBoost);
            Debug.Log("Meat eating counter is "+self.eatMeat);
            //Debug.Log("SpitCounter is " + self.MouthScug().SpitCounter);

            Debug.Log("The current danger level is" + self.JadScug().DangerLevel);

            if (self.input[0].pckp && StashFlag(self) && self.MouthScug().StashDelayCounter == 0 && self.eatCounter == 40 && self.eatMeat == 0)
            {
                if (mouthIndex > -1)
                {
                    if (mouthItems[mouthIndex] != null)
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
                }
                else if(playerModule.mouthCreature != null)
                {
                    self.MouthScug().SpitCounter++;
                    if (self.MouthScug().SpitCounter > 60)
                    {
                        AbstractCreature abstractCreature = playerModule.mouthCreature;
                        playerModule.mouthCreature = null;
                        abstractCreature.pos = self.room.GetWorldCoordinate(self.firstChunk.pos);
                        //abstractCreature.Die();
                        self.room.abstractRoom.AddEntity(abstractCreature);
                        abstractCreature.RealizeInRoom();
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

            self.pyroParryCooldown -= 1f;
            if (playerModule.MouthContains(self, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant))
            {
                if (ArtificerConsussionConditions(self))
                {
                    ConcussiveBlast(self, true, true);
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
            Debug.Log("Corpse stored: " + playerModule.mouthCreature);
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
            newObject.pos = self.abstractCreature.pos;
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
            {//This fixes jank arms when small while hanging from poles by repositioning the body
                /*if (self.animation == Player.AnimationIndex.HangUnderVerticalBeam)
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
                }*/
            }
        }

        private static float DangerLevelMath(PlayerGraphics graphics, float minRange, float maxRange)
        {
            //ThreatDetermination test = new ThreatDetermination(self.playerState.playerNumber);

            //test.currentMusicAgnosticThreat
            float num = 0f;
            if (graphics.player.Consious && graphics.objectLooker.currentMostInteresting != null && graphics.objectLooker.currentMostInteresting is Creature)
            {
                CreatureTemplate.Relationship relationship = graphics.player.abstractCreature.creatureTemplate.CreatureRelationship((graphics.objectLooker.currentMostInteresting as Creature).abstractCreature.creatureTemplate);
                if (relationship.type == CreatureTemplate.Relationship.Type.Afraid && !(graphics.objectLooker.currentMostInteresting as Creature).dead)
                {
                    num = Mathf.InverseLerp(Mathf.Lerp(minRange, maxRange, relationship.intensity), 10f, Vector2.Distance(graphics.player.mainBodyChunk.pos, graphics.objectLooker.mostInterestingLookPoint) * (graphics.player.room.VisualContact(graphics.player.mainBodyChunk.pos, graphics.objectLooker.mostInterestingLookPoint) ? 1f : 1.5f));
                    if ((graphics.objectLooker.currentMostInteresting as Creature).abstractCreature.abstractAI != null && (graphics.objectLooker.currentMostInteresting as Creature).abstractCreature.abstractAI.RealAI != null)
                    {
                        num *= (graphics.objectLooker.currentMostInteresting as Creature).abstractCreature.abstractAI.RealAI.CurrentPlayerAggression(graphics.player.abstractCreature);
                    }
                }
            }
            return num;
        }

        private static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            self.player.JadScug().DangerLevel = DangerLevelMath(self, 250f, 1562f);
            if (self.player.SlugCatClass.value == "MouthScug")
            {
                self.player.JadScug().DangerLevel = DangerLevelMath(self, 1562f, 9762f);
                bool nerv = (self.player.JadScug().DangerLevel > 0);
                float speed = 1.5f;
                if (self.player.MouthScug().SpitCounter > 0)
                {
                    //Debug.Log("Shaking!");
                    self.player.Blink(5);
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
                self.player.MouthScug().Draw(sLeaser, MouthStuffed(self.player), nerv);
            }
            if(self.player.SlugCatClass.value == "BCPuppet")
            {
                self.player.JadScug().DangerLevel = DangerLevelMath(self, 1562f, 9762f);
                bool nerv = (self.player.JadScug().DangerLevel > 0);
                self.player.BCPuppet().Draw(sLeaser, nerv);
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
