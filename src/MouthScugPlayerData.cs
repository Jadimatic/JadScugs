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
        public int ForceSwallowAnimCounter;
    }

    public static class PlayerExtension
    {
        private static readonly ConditionalWeakTable<Player, MouthScugPlayerData> _cwt = new();

        public static MouthScugPlayerData MouthScug(this Player player) => _cwt.GetValue(player, _ => new MouthScugPlayerData());
    }
}
