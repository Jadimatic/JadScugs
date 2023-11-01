using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Noise;
using RWCustom;
using SlugBase.DataTypes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace JadScugs
{

    public class SlugCatSkills
    {
        public void SpawnOverseer(Player self, int ownerIterator = 1)
        {
            AbstractCreature abstractOverseer = new AbstractCreature(self.room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Overseer), null, self.room.GetWorldCoordinate(self.firstChunk.pos), self.room.game.GetNewID());
            (abstractOverseer.abstractAI as OverseerAbstractAI).ownerIterator = ownerIterator;
            abstractOverseer.pos = self.room.GetWorldCoordinate(self.firstChunk.pos);
            self.room.abstractRoom.AddEntity(abstractOverseer);
            abstractOverseer.RealizeInRoom();
            self.room.PlaySound(SoundID.Zapper_Zap, self.mainBodyChunk, false, 0.38f, 2.5f);
        }
        /// <summary>Puts an object in the free hand of a specified slugcat.</summary>
        public void AddHeldObject(AbstractPhysicalObject newObject, Player self)
        {
            self.room.abstractRoom.AddEntity(newObject);//Adds object into the room.
            newObject.pos = self.abstractCreature.pos;//Ensures it's placed at the slugcat's position
            newObject.RealizeInRoom();//Realizes the object in the room.
            if (self.FreeHand() != -1)
            {
                self.SlugcatGrab(newObject.realizedObject, self.FreeHand());//Places the object in a slugcat's hand.
            }
            else
            {
                if (newObject is AbstractSpear abstractSpear)
                {//Speeeeeeeeen!
                    (abstractSpear.realizedObject as Spear).SetRandomSpin();
                }
                Debug.Log(self.SlugCatClass.value + " failed to grab object because no hands were free."); return;
            }
        }

        public bool ArtificerConsussionConditions(Player self)
        {
            bool flag = self.wantToJump > 0 && self.input[0].pckp;
            bool flag2 = self.eatMeat >= 20 || self.maulTimer >= 15;
            return (flag && !self.submerged && !flag2 && (self.input[0].y < 0 || self.bodyMode == Player.BodyModeIndex.Crawl) && (self.canJump > 0 || self.input[0].y < 0) && self.Consious && !self.pyroJumpped && self.pyroParryCooldown <= 0f);
        }

        public void ConcussiveBlast(Player self, bool smoke = true, bool light = true, int stunDuration = 80, float coolDown = 40f, float radius = 200f, float intensity = 30f, bool stunSelf = false, bool doesDamage = false)
        {
            if(stunSelf)
            {
                self.Stun((stunDuration / 4) * 3);
            }
            self.pyroParryCooldown = coolDown;
            Vector2 pos2 = self.firstChunk.pos;
            for (int k = 0; k < 8; k++)
            {
                if (smoke)
                {
                    self.room.AddObject(new Explosion.ExplosionSmoke(pos2, Custom.RNV() * 5f * UnityEngine.Random.value, 1f));
                }
            }
            if (light)
            {
                self.room.AddObject(new Explosion.ExplosionLight(pos2, 160f, 1f, 3, Color.white));
            }
            for (int l = 0; l < 10; l++)
            {
                Vector2 a2 = Custom.RNV();
                self.room.AddObject(new Spark(pos2 + a2 * UnityEngine.Random.value * 40f, a2 * Mathf.Lerp(4f, 30f, UnityEngine.Random.value), Color.white, null, 4, 18));
            }
            self.room.AddObject(new ShockWave(pos2, radius, 0.2f, 6, false));
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
                        if (Custom.Dist(pos2, creature.firstChunk.pos) < radius && (Custom.Dist(pos2, creature.firstChunk.pos) < 60f || self.room.VisualContact(self.abstractCreature.pos, creature.abstractCreature.pos)))
                        {
                            self.room.socialEventRecognizer.WeaponAttack(null, self, creature, true);
                            creature.SetKillTag(self.abstractCreature);
                            if (creature is Scavenger)
                            {
                                (creature as Scavenger).HeavyStun(stunDuration);
                            }
                            else
                            {
                                creature.Stun(stunDuration);
                            }
                            if(doesDamage)
                            {
                                creature.Violence(self.bodyChunks[1], null, creature.bodyChunks[0], null, Creature.DamageType.Explosion, intensity / 25, stunDuration);
                            }
                            creature.firstChunk.vel = Custom.DegToVec(Custom.AimFromOneVectorToAnother(pos2, creature.firstChunk.pos)) * intensity;
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
    }

    public static class SlugCatSkillExtension
    {
        private static readonly ConditionalWeakTable<Player, SlugCatSkills> _cwt = new();

        public static SlugCatSkills SlugCatSkill(this Player player) => _cwt.GetValue(player, _ => new SlugCatSkills());
    }
}
