using System;
using System.Linq;
using System.Runtime.CompilerServices;
using RWCustom;
using SlugBase.DataTypes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace JadScugs
{

    public class ExtraInputs
    {
        public bool[] TriggersDown(Player self)
        {
            var n = self.playerState.playerNumber;
            var controller = RWInput.PlayerRecentController(n, self.room.game.rainWorld);
            float[] triggersRaw = TriggersRAW(self);
            bool[] triggerDown = new bool[2];
            triggerDown[0] = triggersRaw[0] > 0.5;
            triggerDown[1] = triggersRaw[1] > 0.5;
            return triggerDown;
        }
        public float[] TriggersRAW(Player self)
        {
            float[] triggers = new float[2];
            var n = self.playerState.playerNumber;
            var controller = RWInput.PlayerRecentController(n, self.room.game.rainWorld);
            if (controller is Rewired.Joystick)
            {
                var joystick = controller as Rewired.Joystick;

                var axisIDLS = -1;
                var axisIDRS = -1;

                for (int i = 0; i < joystick.AxisElementIdentifiers.Count; i++)
                {
                    if (joystick.AxisElementIdentifiers[i].name == "Left Trigger")
                    {
                        axisIDLS = joystick.AxisElementIdentifiers[i].id;
                    }
                    else if (joystick.AxisElementIdentifiers[i].name == "Right Trigger")
                    {
                        axisIDRS = joystick.AxisElementIdentifiers[i].id;
                    }
                }
                triggers[0] = joystick.GetAxis(axisIDLS); triggers[1] = joystick.GetAxis(axisIDRS);
            }
            return triggers;
        }
    }

    public static class InputPlayerExtension
    {
        private static readonly ConditionalWeakTable<Player, ExtraInputs> _cwt = new();

        public static ExtraInputs Input(this Player player) => _cwt.GetValue(player, _ => new ExtraInputs());
    }
}
