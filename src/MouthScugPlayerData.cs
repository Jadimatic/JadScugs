using System;
using System.Linq;
using System.Runtime.CompilerServices;
using RWCustom;
using SlugBase.DataTypes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace JadScugs
{
    
    public class MouthScugPlayerData
    {
        public int SpitCounter;
        public int StashDelayCounter;
        public bool ProteinBoost;
        public bool mouthStuffed = false;

        public void Draw(RoomCamera.SpriteLeaser sLeaser, bool mouthStuffed, bool nerv)
        {
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
            if (nerv)
            {
                if (faceName.StartsWith("FaceA") && Futile.atlasManager.DoesContainElementWithName("MouthScugNerv_" + faceName))
                {
                    sLeaser.sprites[9].SetElementByName("MouthScugNerv_" + faceName);
                }
                if (faceName.StartsWith("FaceB") && Futile.atlasManager.DoesContainElementWithName("MouthScugNerv_" + faceName))
                {
                    sLeaser.sprites[9].SetElementByName("MouthScugNerv_" + faceName);
                }
                if (faceName.StartsWith("FaceStunned") && Futile.atlasManager.DoesContainElementWithName("MouthScugNerv_" + faceName))
                {
                    sLeaser.sprites[9].SetElementByName("MouthScugNerv_" + faceName);
                }
                if (faceName.StartsWith("FaceDead") && Futile.atlasManager.DoesContainElementWithName("MouthScugNerv_" + faceName))
                {
                    sLeaser.sprites[9].SetElementByName("MouthScugNerv_" + faceName);
                }
            }
            else
            {
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
            //TAIL
            FAtlasElement tailNerv = Futile.atlasManager.GetElementWithName("MouthScugNerv_Tail");
            FAtlasElement tailNormal = Futile.atlasManager.GetElementWithName("MouthScug_Tail");
            sLeaser.sprites[2].element = nerv ? tailNerv : tailNormal;
        }



    }

    public static class MouthScugPlayerExtension
    {
        private static readonly ConditionalWeakTable<Player, MouthScugPlayerData> _cwt = new();

        public static MouthScugPlayerData MouthScug(this Player player) => _cwt.GetValue(player, _ => new MouthScugPlayerData());
    }
}
