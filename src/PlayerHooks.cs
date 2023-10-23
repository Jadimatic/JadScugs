using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace JadScugs
{
    public static class PlayerHooks
    {
        public static int mouthScugHeight = 14;

        public static void Init() 
        {
            On.SaveState.SessionEnded += SaveState_SessionEnded;
            IL.Player.GrabUpdate += Player_GrabUpdate;
            On.Player.GraspsCanBeCrafted += Player_GraspsCanBeCrafted;
            On.Player.SpitUpCraftedObject += Player_SpitUpCraftedObject;
            On.Player.SwallowObject += Player_SwallowObject;
            On.Player.CraftingResults += Player_CraftingResults;
            On.Player.EatMeatUpdate += Player_EatMeatUpdate;
            On.Player.HeavyCarry += Player_HeavyCarry;
            On.Player.ThrownSpear += Player_ThrownSpear;
            On.Player.DeathByBiteMultiplier += Player_DeathByBiteMultiplier;
            On.Player.Grabbed += Player_Grabbed;
            On.OverWorld.WorldLoaded += OverWorld_WorldLoaded;
            On.Player.Jump += Player_Jump;
        }

        private static void Player_GrabUpdate(ILContext il)
        {
            var cursor = new ILCursor(il);

            cursor.GotoNext(MoveType.After,
                i => i.MatchLdsfld<MoreSlugcatsEnums.SlugcatStatsName>(nameof(MoreSlugcatsEnums.SlugcatStatsName.Artificer)),
                i => i.MatchCallOrCallvirt(out _));

            cursor.MoveAfterLabels();
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate((Player self) => self.slugcatStats.name.value == "BCPuppet");//Injects this condition into the check for Arti spear crafting
            cursor.Emit(OpCodes.Or);
        }

        private static AbstractPhysicalObject.AbstractObjectType Player_CraftingResults(On.Player.orig_CraftingResults orig, Player self)
        {
            if (self.SlugCatClass.value == "BCPuppet")
            {
                AbstractPhysicalObject.AbstractObjectType abstractObjectType = GourmandCombos.CraftingResults_ObjectData(self.grasps[0], self.grasps[1], true);
                if (self.FoodInStomach < self.MaxFoodInStomach)
                {
                    if (abstractObjectType == AbstractPhysicalObject.AbstractObjectType.SlimeMold ||
                        abstractObjectType == AbstractPhysicalObject.AbstractObjectType.FlareBomb ||
                        abstractObjectType == AbstractPhysicalObject.AbstractObjectType.Lantern)
                    {
                        return orig(self);
                    }
                    Creature.Grasp[] grasps = self.grasps;
                    for (int i = 0; i < grasps.Length; i++)
                    {
                        if (grasps[i] != null && grasps[i].grabbed is IPlayerEdible && (grasps[i].grabbed as IPlayerEdible).Edible)
                        {
                            return null;
                        }
                    }
                    if (grasps[0] != null && grasps[0].grabbed is Spear)
                    {
                        return AbstractPhysicalObject.AbstractObjectType.Spear;
                    }
                    if (grasps[0] == null && grasps[1] != null && grasps[1].grabbed is Spear && !(grasps[1].grabbed as Spear).abstractSpear.explosive && self.objectInStomach == null)
                    {
                        return AbstractPhysicalObject.AbstractObjectType.Spear;
                    }
                }
                return null;
            }
            return orig(self);
        }

        private static void Player_Grabbed(On.Player.orig_Grabbed orig, Player self, Creature.Grasp grasp)
        {
            orig(self, grasp);
            if(self.SlugCatClass.value == "BCPuppet") { self.BCPuppet().grabbed = true; }
        }

        private static float Player_DeathByBiteMultiplier(On.Player.orig_DeathByBiteMultiplier orig, Player self)
        {
            orig(self);
            if(self.SlugCatClass.value == "BCPuppet" && self.FoodInStomach > 0)
            {
                return 0;
            }
            return orig(self);
        }

        private static void OverWorld_WorldLoaded(On.OverWorld.orig_WorldLoaded orig, OverWorld self)
        {
            orig(self);
            World world = self.activeWorld;
            for (int m = 0; m < self.game.Players.Count; m++)
            {
                if ((self.game.Players[m].realizedCreature as Player).SlugCatClass.value == "MouthScug")
                {
                    if (!(self.game.Players[m].realizedCreature as Player).TryGetMouthScugModule(out var playerModule))
                    {
                        return;
                    }
                    var mouthItems = playerModule.mouthItems;
                    for (int index = 0; index < mouthItems.Length; index++)
                    {//Ensures the region of the object matches the slugcat's current region.
                        if (self.game.Players[m].realizedCreature != null && mouthItems[index] != null)
                        {
                            mouthItems[index].world = world;
                        }
                    }
                }
            }
        }

        private static void Player_ThrownSpear(On.Player.orig_ThrownSpear orig, Player self, Spear spear)
        {
            orig(self, spear);
            if(self.SlugCatClass.value == "MouthScug")
            {
                if (!self.MouthScug().ProteinBoost)
                {
                    spear.spearDamageBonus = 0.3f;
                }
                else
                {
                    spear.spearDamageBonus = 1f;
                }
            }
        }

        private static bool Player_HeavyCarry(On.Player.orig_HeavyCarry orig, Player self, PhysicalObject obj)
        {
            var result = orig(self, obj);
            if (self.SlugCatClass.value == "MouthScug" && self.MouthScug().ProteinBoost)
            {
                return false;
            }
            return result;
        }

        private static void Player_EatMeatUpdate(On.Player.orig_EatMeatUpdate orig, Player self, int graspIndex)
        {
            if(self.SlugCatClass.value == "MouthScug")
            {
                if (!self.TryGetMouthScugModule(out var playerModule))
                {
                    return;
                }
                int mouthIndex = -1;
                var mouthItems = playerModule.mouthItems;
                mouthIndex = playerModule.MouthIndex(self);
                Debug.Log("Mouth Index is" + mouthIndex);
                /*if(mouthIndex == -2 && self.eatMeat > 40)
                {
                    if (!self.MouthScug().ProteinBoost)
                    {
                        self.MouthScug().ProteinBoost = true;
                    }
                    else
                    {
                        self.room.PlaySound(SoundID.Rock_Hit_Creature, self.mainBodyChunk, false, 1f, 1f);
                    }
                    self.ReleaseGrasp(graspIndex);
                    self.eatMeat = 0;
                }*/
                
                if (mouthIndex == -2 && self.eatMeat > 40)
                {
                    /*AbstractCreature abstractCreature = self.grasps[graspIndex].grabbed.abstractPhysicalObject as AbstractCreature;
                    self.ReleaseGrasp(graspIndex);
                    playerModule.mouthCreature = abstractCreature;
                    abstractCreature.pos = self.room.GetWorldCoordinate(self.firstChunk.pos);
                    abstractCreature.realizedObject.RemoveFromRoom();
                    self.room.abstractRoom.RemoveEntity(abstractCreature);
                    self.room.PlaySound(SoundID.Water_Nut_Swell, self.mainBodyChunk, false, 2.5f, 0.7f);*/
                    if(self.eatMeat == 41)
                    {
                        if (self.grasps[graspIndex].grabbed.abstractPhysicalObject is AbstractCreature abstractCreature)
                        {
                            if (!self.MouthScug().ProteinBoost && abstractCreature.state.meatLeft > 2 && self.FoodInStomach > 4)
                            {
                                self.AddFood(1);
                                abstractCreature.state.meatLeft -= abstractCreature.state.meatLeft;

                                self.room.PlaySound(SoundID.Slugcat_Eat_Meat_B, self.mainBodyChunk);
                                self.room.PlaySound(SoundID.Drop_Bug_Grab_Creature, self.mainBodyChunk, false, 1f, 0.76f);
                                self.MouthScug().ProteinBoost = true;
                            }
                            else
                            {
                                self.room.PlaySound(SoundID.Cicada_Bump_Attack_Hit_Player, self.mainBodyChunk, false, 3f, 1f);
                            }
                        }
                    }
                    self.ReleaseGrasp(graspIndex);
                    return;
                }
            }
            orig(self, graspIndex);
        }


        private static void Player_Jump(On.Player.orig_Jump orig, Player self)
        {
            orig(self);
            float power = 0.12f;
            if (self.SlugCatClass.value == "MouthScug")
            {
                if(self.MouthScug().ProteinBoost)
                {
                    self.jumpBoost *= 1f + power * 2;
                }
                else
                {
                    self.jumpBoost *= 1f + power;
                }
            }
        }

        private static void SaveState_SessionEnded(On.SaveState.orig_SessionEnded orig, SaveState self, RainWorldGame game, bool survived, bool newMalnourished)
        {
            foreach (var player in game.session.Players)
            {
                if (player != null)
                {
                    if (player?.realizedObject is Player realPlayer && realPlayer.TryGetMouthScugModule(out var module))
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

        private static void Player_SwallowObject(On.Player.orig_SwallowObject orig, Player self, int grasp)
        {
            AbstractPhysicalObject abstractPhysicalObject = self.grasps[grasp].grabbed.abstractPhysicalObject;
            if (self.SlugCatClass.value == "MouthScug")
            {
                if (!self.TryGetMouthScugModule(out var playerModule))
                {
                    return;
                }
                int mouthIndex = -1;
                var mouthItems = playerModule.mouthItems;


                if (grasp < 0 || self.grasps[grasp] == null)
                {
                    Debug.Log("Nothing was swallowed because hand empty");
                    return;
                }
                mouthIndex = playerModule.MouthIndex(self);
                self.ReleaseGrasp(grasp);
                if(mouthIndex > -1 && playerModule.mouthCreature == null)
                {
                    mouthItems[mouthIndex] = abstractPhysicalObject;
                    abstractPhysicalObject.realizedObject.RemoveFromRoom();
                    self.room.abstractRoom.RemoveEntity(abstractPhysicalObject);
                    self.MouthScug().StashDelayCounter = 20;
                    self.room.PlaySound(SoundID.Water_Nut_Swell, self.mainBodyChunk, false, 0.6f, 0.7f);
                }
                else
                {
                    self.room.PlaySound(SoundID.Rock_Bounce_Off_Creature_Shell, self.mainBodyChunk, false, 0.6f, 0.7f);
                }
            }
            else if (self.SlugCatClass.value == "BCPuppet" && self.FoodInStomach < self.MaxFoodInStomach)
            {
                if (abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.Rock)
                {
                    self.AddFood(1);
                    self.ReleaseGrasp(grasp);
                    abstractPhysicalObject.realizedObject.RemoveFromRoom();
                    self.room.abstractRoom.RemoveEntity(abstractPhysicalObject);
                    self.room.PlaySound(SoundID.Rock_Hit_Wall, self.firstChunk.pos, 1f, 1.5f + UnityEngine.Random.value * 1.5f);
                }
                else if (abstractPhysicalObject.type == MoreSlugcats.MoreSlugcatsEnums.AbstractObjectType.SingularityBomb)
                {
                    self.AddFood(1);
                    self.Stun(200);
                    self.ReleaseGrasp(grasp);
                    abstractPhysicalObject.realizedObject.RemoveFromRoom();
                    self.room.abstractRoom.RemoveEntity(abstractPhysicalObject);
                    self.room.PlaySound(SoundID.Zapper_Zap, self.firstChunk.pos, 1f, 1.5f + UnityEngine.Random.value * 1.5f);
                }
            }
            else
            {
                orig(self, grasp);
            }
        }

        /// <summary>Removes all objects from the hands of a specified slugcat.</summary>
        private static void RemoveHeldObjects(Player self)
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
        private static void AddHeldObject(AbstractPhysicalObject newObject, Player self)
        {
            self.room.abstractRoom.AddEntity(newObject);//Adds object into the room.
            newObject.pos = self.room.GetWorldCoordinate(self.firstChunk.pos);//Ensures it's placed at the slugcat's position
            newObject.RealizeInRoom();//Realizes the object in the room.
            if (self.FreeHand() != -1)
            {
                self.SlugcatGrab(newObject.realizedObject, self.FreeHand());//Places the object in a slugcat's hand.
            }
            else
            {
                if(newObject is AbstractSpear abstractSpear)
                {
                    (abstractSpear.realizedObject as Spear).SetRandomSpin();
                }
                Debug.Log(self.SlugCatClass.value + " failed to grab object because no hands were free."); return;
            }
        }

        private static void Player_SpitUpCraftedObject(On.Player.orig_SpitUpCraftedObject orig, Player self)
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
                if (self.grasps[0].grabbed.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.Rock && self.grasps[1].grabbed.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.Rock)
                {
                    AbstractSpear abstractSpear = new AbstractSpear(self.room.world, null, self.room.GetWorldCoordinate(self.firstChunk.pos), self.room.game.GetNewID(), false);
                    RemoveHeldObjects(self);
                    AddHeldObject(abstractSpear, self);
                    return;
                }
                if (self.grasps[0].grabbed.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.PuffBall && self.grasps[1].grabbed.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.PuffBall)
                {
                    AbstractConsumable abstractBatnip = new AbstractConsumable(self.room.world, AbstractPhysicalObject.AbstractObjectType.FlyLure, null, self.room.GetWorldCoordinate(self.firstChunk.pos), self.room.game.GetNewID(), -1, -1, null);
                    RemoveHeldObjects(self);
                    AddHeldObject(abstractBatnip, self);
                    return;
                }
                if (self.grasps[0].grabbed.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.FlyLure && self.grasps[1].grabbed.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.FlyLure)
                {
                    AbstractConsumable abstractSporePuff = new AbstractConsumable(self.room.world, AbstractPhysicalObject.AbstractObjectType.PuffBall, null, self.room.GetWorldCoordinate(self.firstChunk.pos), self.room.game.GetNewID(), -1, -1, null);
                    RemoveHeldObjects(self);
                    AddHeldObject(abstractSporePuff, self);
                    return;
                }
                if (self.grasps[0].grabbed.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.Lantern && self.grasps[1].grabbed.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.Lantern)
                {
                    AbstractPhysicalObject abstractGrenade = new AbstractPhysicalObject(self.room.world, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null, self.room.GetWorldCoordinate(self.firstChunk.pos), self.room.game.GetNewID());
                    RemoveHeldObjects(self);
                    AddHeldObject(abstractGrenade, self);
                    return;
                }
            }



            if (self.SlugCatClass.value == "BCPuppet")
            {
                for (int i = 0; i < self.grasps.Length; i++)
                {
                    if (self.grasps[i] != null)
                    {
                        AbstractPhysicalObject abstractPhysicalObject = self.grasps[i].grabbed.abstractPhysicalObject;
                        if (abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.Spear)
                        {
                            self.ReleaseGrasp(i);
                            abstractPhysicalObject.realizedObject.RemoveFromRoom();
                            self.room.abstractRoom.RemoveEntity(abstractPhysicalObject);
                            if (((abstractPhysicalObject as AbstractSpear).electric && (abstractPhysicalObject as AbstractSpear).electricCharge > 0) || (abstractPhysicalObject as AbstractSpear).explosive)
                            {
                                AbstractSpear abstractSpear = new AbstractSpear(self.room.world, null, self.abstractCreature.pos, self.room.game.GetNewID(), false);
                                self.room.abstractRoom.AddEntity(abstractSpear);
                                abstractSpear.RealizeInRoom();
                                if (self.FreeHand() != -1)
                                {
                                    self.SlugcatGrab(abstractSpear.realizedObject, self.FreeHand());
                                }
                                self.AddFood(1);
                                self.room.PlaySound(SoundID.Fire_Spear_Ignite, self.firstChunk.pos, 1f, 1.5f + UnityEngine.Random.value * 1.5f);
                            }
                            else
                            {
                                self.AddFood(1);
                                self.room.PlaySound(SoundID.Rock_Hit_Wall, self.firstChunk.pos, 1f, 1.5f + UnityEngine.Random.value * 1.5f);
                            }
                            return;
                        }
                    }
                }
            }













            if (self.SlugCatClass.value == "BCPuppet")
            {
                if (self.grasps[0].grabbed.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.Rock && self.grasps[1].grabbed.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.Rock)
                {
                    AbstractSpear abstractSpear = new AbstractSpear(self.room.world, null, self.room.GetWorldCoordinate(self.firstChunk.pos), self.room.game.GetNewID(), false);
                    RemoveHeldObjects(self);
                    AddHeldObject(abstractSpear, self);
                    return;
                }
                /*if (self.grasps[0].grabbed.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.Lantern && self.grasps[1].grabbed.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.Lantern)
                {
                    AbstractPhysicalObject abstractGrenade = new AbstractPhysicalObject(self.room.world, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null, self.room.GetWorldCoordinate(self.firstChunk.pos), self.room.game.GetNewID());
                    RemoveHeldObjects(self);
                    AddHeldObject(abstractGrenade, self);
                    return;
                }*/
            }
            orig(self);
        }

        private static bool oneHandCraft(Creature.Grasp[] grasps, AbstractPhysicalObject.AbstractObjectType obj)
        {
            bool canCraft = false;
            if (grasps[0].grabbed.abstractPhysicalObject.type == obj && grasps[1].grabbed.abstractPhysicalObject.type == null ||
                grasps[0].grabbed.abstractPhysicalObject.type == null && grasps[1].grabbed.abstractPhysicalObject.type == obj)
            {
                canCraft = true;
            }
            Debug.Log(canCraft);
            return canCraft;
        }

        private static bool Player_GraspsCanBeCrafted(On.Player.orig_GraspsCanBeCrafted orig, Player self)
        {
            orig(self);

            if (self.SlugCatClass.value == "MouthScug" && self.input[0].y == 1)
            {
                if(self.CraftingResults() != null)
                {
                    if (self.CraftingResults() == AbstractPhysicalObject.AbstractObjectType.DangleFruit)//DangleFruit is used for all super snacks
                    {
                        if (self.FoodInStomach < self.MaxFoodInStomach)
                        {
                            return true;
                        }
                        else return true;
                    }
                    else return true;
                }
                else
                {
                    if (self.grasps[0].grabbed.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.Rock && self.grasps[1].grabbed.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.Rock)
                    {//Rock + Rock = Spear
                        return true;
                    }
                    else if (self.grasps[0].grabbed.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.PuffBall && self.grasps[1].grabbed.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.PuffBall)
                    {//Spore Puff + Spore Puff = Batnip
                        return true;
                    }
                    else if (self.grasps[0].grabbed.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.FlyLure && self.grasps[1].grabbed.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.FlyLure)
                    {//Batnip + Batnip = Spore Puff
                        return true;
                    }
                    else if (self.grasps[0].grabbed.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.Lantern && self.grasps[1].grabbed.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.Lantern)
                    {//Lantern + Lantern = Grenade
                        return true;
                    }
                    else return orig(self);
                }
            }
            else if (self.SlugCatClass.value == "BCPuppet" && self.input[0].y == 1)
            {
                if (self.CraftingResults() != null)
                {
                    return true;
                }
                else
                {
                    if (self.grasps[0].grabbed.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.Rock && self.grasps[1].grabbed.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.Rock)
                    {//Rock + Rock = Spear
                        return true;
                    }
                    else if (self.grasps[0].grabbed.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.PuffBall && self.grasps[1].grabbed.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.PuffBall)
                    {//Spore Puff + Spore Puff = Batnip
                        return true;
                    }
                    else if (self.grasps[0].grabbed.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.FlyLure && self.grasps[1].grabbed.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.FlyLure)
                    {//Batnip + Batnip = Spore Puff
                        return true;
                    }
                    /*else if (self.grasps[0].grabbed.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.Lantern && self.grasps[1].grabbed.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.Lantern)
                    {//Lantern + Lantern = Grenade
                        return true;
                    }*/
                    else return orig(self);
                }
            }
            else return orig(self);
        }


    }
}
