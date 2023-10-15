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

            On.Player.Jump += Player_Jump;
        }

        private static void Player_Jump(On.Player.orig_Jump orig, Player self)
        {
            orig(self);
            float power = 0.25f;
            if (self.SlugCatClass.value == "MouthScug")
            {
                self.jumpBoost *= 1f + power;
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
                /*if (self.FreeHand() != -1 && !self.input[1].thrw && self.input[1].thrw)
                {
                    AbstractPhysicalObject abstractRock = new AbstractPhysicalObject(self.room.world, AbstractPhysicalObject.AbstractObjectType.Rock, null, self.room.GetWorldCoordinate(self.firstChunk.pos), self.room.game.GetNewID());
                    AddHeldObject(abstractRock, self);
                }*/
            }
            orig(self);
        }

        private static bool Player_GraspsCanBeCrafted(On.Player.orig_GraspsCanBeCrafted orig, Player self)
        {
            orig(self);

            if (self.SlugCatClass.value == "MouthScug" && self.input[0].y == 1 && self.CraftingResults() != null)
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
            else return orig(self);
        }


    }
}
