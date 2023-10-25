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
            AttachedSprite.Apply();
            On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
            On.PlayerGraphics.ApplyPalette += PlayerGraphics_ApplyPalette;
            On.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSprites;
            On.PlayerGraphics.ctor += PlayerGraphics_ctor;
            On.PlayerGraphics.Update += PlayerGraphics_Update;
            On.Player.Update += Player_Update;
            On.SlugcatHand.Update += SlugcatHand_Update;
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);
        }

        private static void PlayerGraphics_ApplyPalette(On.PlayerGraphics.orig_ApplyPalette orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig(self, sLeaser, rCam, palette);
            if(self.player.SlugCatClass.value == "BCPuppet")
            {
                self.player.BCPuppet().BCPuppetGown?.ApplyPalette(self.gownIndex, sLeaser, rCam, palette);
            }
        }

        public static void ShowArms(SlugcatHand hand, Player player, Vector2 armDist, Vector2 beamTipArmDist, float velEffect)
        {
            if (player.grasps[hand.limbNumber] == null)
            {
                if (hand.mode != Limb.Mode.HuntAbsolutePosition || hand.retractCounter > 0)
                {
                    if ((player.bodyMode != Player.BodyModeIndex.ClimbingOnBeam || player.animation == Player.AnimationIndex.BeamTip || player.animation == Player.AnimationIndex.StandOnBeam) && player.bodyMode != Player.BodyModeIndex.ZeroG && player.bodyMode != Player.BodyModeIndex.Swimming)
                    {
                        hand.mode = Limb.Mode.HuntRelativePosition;
                        if(player.animation == Player.AnimationIndex.BeamTip)
                        {
                            hand.relativeHuntPos = new Vector2((beamTipArmDist.x * -1 + beamTipArmDist.x * 2 * hand.limbNumber) * (1f - Mathf.Sin(player.switchHandsProcess * Mathf.PI)) + (velEffect * -1) * player.mainBodyChunk.vel.x, beamTipArmDist.y);
                        }
                        else
                        {
                            hand.relativeHuntPos = new Vector2((armDist.x * -1 + armDist.x * 2 * hand.limbNumber) * (1f - Mathf.Sin(player.switchHandsProcess * Mathf.PI)) + (velEffect * -1) * player.mainBodyChunk.vel.x, armDist.y);
                        }
                        hand.retractCounter = 0;
                    }
                }
            }
        }

        private static void SlugcatHand_Update(On.SlugcatHand.orig_Update orig, SlugcatHand self)
        {
            orig(self);
            if (self.owner.owner is Player player && player.SlugCatClass.value == "BCPuppet")
            {
                ShowArms(self, player, new Vector2(8, -20), new Vector2(20, -12), 4);
            }
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
                if(self.player.BCPuppet().BCPuppetGown != null)
                {
                    Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + 1);
                    self.gownIndex = sLeaser.sprites.Length - 1;
                    self.player.BCPuppet().BCPuppetGown.InitiateSprite(self.gownIndex, sLeaser, rCam);
                }
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
                self.player.BCPuppet().BCPuppetGown = new PuppetGown(self, "BCPuppetGownTex");
                self.player.BCPuppet().headAntennae = AttachedSprite.Create(self, AttachedSprite.AttachedSpriteType.Head, "BCPuppetAntennae_");
                self.player.BCPuppet().headPattern = AttachedSprite.Create(self, AttachedSprite.AttachedSpriteType.Head, "BCPuppetPattern_");
                self.player.BCPuppet().facePattern = AttachedSprite.Create(self, AttachedSprite.AttachedSpriteType.Face, "BCPuppetPattern_", false);
            }

            if (self.player.SlugCatClass.value == "MouthScug")
            {
                self.tail[0] = new TailSegment(self, 7.5f, 4f, null, 0.5f, 1f, 1f, true);
                self.tail[1] = new TailSegment(self, 8f, 7f, self.tail[0], 0.5f, 1f, 0.5f, true);
                self.tail[2] = new TailSegment(self, 7f, 7f, self.tail[1], 0.5f, 1f, 0.5f, true);
                self.tail[3] = new TailSegment(self, 4.5f, 7f, self.tail[2], 0.5f, 1f, 0.5f, true);
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
            try
            {
                BCPuppet_Update(self, eu);
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

        private static void BCPuppet_Update(Player self, bool eu)
        {
            if(self.SlugCatClass.value == "BCPuppet")
            {
                if (self.input[0].jmp && !self.input[1].jmp)
                {
                    
                }
                Debug.Log(self.eatCounter);
                if (self.eatCounter < 15)
                {
                    self.eatCounter = 15;
                }
                if (self.BCPuppet().grabbed)
                {
                    self.SlugCatSkill().ConcussiveBlast(self, false, false, 100, 40, 350, 60);
                    self.SubtractFood(1);
                    self.BCPuppet().grabbed = false;
                }
                self.aerobicLevel = 0;
                self.airInLungs = 1;
                self.slugcatStats.lungsFac = 0;
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

            mouthIndex = playerModule.MouthIndex(self);

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
                if (self.SlugCatSkill().ArtificerConsussionConditions(self))
                {
                    self.SlugCatSkill().ConcussiveBlast(self);
                }
            }
            if (playerModule.MouthContains(self, AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer) || playerModule.MouthContains(self, AbstractPhysicalObject.AbstractObjectType.Lantern))
            {
                self.glowing = true;
            }
            else
            {
                self.glowing = (self.room.game.session as StoryGameSession).saveState.theGlow;
            }
            if (self.BCPuppet().grabbed)
            {
                AbstractPhysicalObject.AbstractObjectType scavBomb = AbstractPhysicalObject.AbstractObjectType.ScavengerBomb;
                if (playerModule.MouthContains(self, scavBomb))
                {
                    int itemIndex = playerModule.FindMouthItemSlot(self, scavBomb);
                    if(itemIndex > -1)
                    {

                        mouthItems[itemIndex] = null;
                    }
                    self.SlugCatSkill().ConcussiveBlast(self, true, true, 80, 1, 200, 30, false, true);
                }
                AbstractPhysicalObject.AbstractObjectType beehive = AbstractPhysicalObject.AbstractObjectType.SporePlant;
                if (playerModule.MouthContains(self, beehive))
                {
                    int itemIndex = playerModule.FindMouthItemSlot(self, beehive);
                    if (itemIndex > -1)
                    {
                        mouthItems[itemIndex] = null;
                    }
                    //self.SlugCatSkill().ConcussiveBlast(self, true, true, 80, 1, 200, 30, false, true);
                }
                self.BCPuppet().grabbed = false;
            }
            //Debug.Log("Corpse stored: " + playerModule.mouthCreature);
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
                if(self.player.MouthScug().SpitCounter > 0)
                {
                    self.blink = 5;
                }
                self.swallowing = 0;
            }
            orig(self);
            if (self.player.SlugCatClass.value == "BCPuppet")
            {
                if (self.owner.room != null && self.owner.room.game.IsStorySession && !self.player.playerState.isPup)
                {
                    self.gown.visible = self.player.BCPuppet().wearingGown;
                }
                else
                {
                    self.gown.visible = false;
                }
                self.player.BCPuppet().BCPuppetGown.Update();
                self.swallowing = 0;
                SeaLegsPartTwo(self);
            }
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
            float num = 0f;
            if (graphics.player.room != null)
            {
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
                if (self.player.BCPuppet().BCPuppetGown != null)
                {
                    self.player.BCPuppet().BCPuppetGown.DrawSprite(self.gownIndex, sLeaser, rCam, timeStacker, camPos);
                }
                self.player.JadScug().DangerLevel = DangerLevelMath(self, 1562f, 9762f);
                bool nerv = (self.player.JadScug().DangerLevel > 0);
                self.player.BCPuppet().Draw(sLeaser, nerv);
                sLeaser.sprites[self.player.BCPuppet().headAntennae.SpriteIndex].color = (Color)self.GetColor(BCPuppetEnums.Color.Cloth);
                sLeaser.sprites[self.player.BCPuppet().headPattern.SpriteIndex].color = (Color)self.GetColor(BCPuppetEnums.Color.Pattern);
                sLeaser.sprites[self.player.BCPuppet().facePattern.SpriteIndex].color = (Color)self.GetColor(BCPuppetEnums.Color.Pattern);
                SeaLegsPartOne(self, sLeaser, camPos, timeStacker);
            }
        }

        private static void SeaLegsPartOne(PlayerGraphics pg, RoomCamera.SpriteLeaser sLeaser, Vector2 camPos, float timeStacker)
        {
            if (pg.player.animation == Player.AnimationIndex.DeepSwim || pg.player.animation == Player.AnimationIndex.SurfaceSwim)
            {
                float distance = 6;
                var head = Vector2.Lerp(pg.drawPositions[0, 1], pg.drawPositions[0, 0], timeStacker);
                var hips = Vector2.Lerp(pg.drawPositions[1, 1], pg.drawPositions[1, 0], timeStacker);
                var legs = hips + Custom.DirVec(head, hips).normalized * distance;
                sLeaser.sprites[4].isVisible = true;
                sLeaser.sprites[4].SetPosition(legs - camPos);
                
            }
        }
        private static void SeaLegsPartTwo(PlayerGraphics pg)
        {
            if (pg.player.animation == Player.AnimationIndex.DeepSwim || pg.player.animation == Player.AnimationIndex.SurfaceSwim)
            {
                pg.legs.ConnectToPoint(pg.owner.bodyChunks[1].pos + Custom.DirVec(pg.owner.bodyChunks[0].pos, pg.owner.bodyChunks[1].pos) * 4f, 4f, push: false, 0f, pg.owner.bodyChunks[1].vel, 0.2f, 0f);
                pg.legsDirection = Custom.DirVec(pg.owner.bodyChunks[0].pos, pg.owner.bodyChunks[1].pos);
                pg.legs.vel += pg.legsDirection * 0.2f;
                pg.legsDirection.Normalize();
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
