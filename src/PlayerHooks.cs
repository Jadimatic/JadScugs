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
            
            On.Player.GraspsCanBeCrafted += Player_GraspsCanBeCrafted;
            On.Player.SpitUpCraftedObject += Player_SpitUpCraftedObject;
            On.Player.SwallowObject += Player_SwallowObject;
            On.Player.EatMeatUpdate += Player_EatMeatUpdate;
            On.Player.HeavyCarry += Player_HeavyCarry;
            On.Player.ThrownSpear += Player_ThrownSpear;
            On.OverWorld.WorldLoaded += OverWorld_WorldLoaded;
            On.Player.Jump += Player_Jump;
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


                /*self.objectInStomach = abstractPhysicalObject;
                self.objectInStomach.Abstractize(self.abstractCreature.pos); */
                            /*BodyChunk mainBodyChunk = self.mainBodyChunk;
                            mainBodyChunk.vel.y = mainBodyChunk.vel.y + 20f;*/
                            mouthIndex = playerModule.MouthIndex(self);

                AbstractPhysicalObject abstractPhysicalObject = self.grasps[grasp].grabbed.abstractPhysicalObject;
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
            orig(self);
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
                    else return false;
                }
            }
            else return orig(self);
        }


    }
}
