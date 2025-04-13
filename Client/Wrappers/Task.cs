using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Autopilot_NPC.Client.Wrappers
{
    public class Task
    {
        public static void TaskHeliMission(
              int pilot,
              int aircraft,
              int targetVehicle,
              int targetPed,
              float destinationX,
              float destinationY,
              float destinationZ,
              int missionFlag,
              float maxSpeed)
        {
            Function.Call(
                Hash.TASK_HELI_MISSION,
                pilot,
                aircraft,
                targetVehicle,
                targetPed,
                destinationX,
                destinationY,
                destinationZ,
                missionFlag,
                maxSpeed,
                -1.0f,
                -1.0f,
                80,
                -1.0f,
                -1.0f,
                8192
            );
        }
    }
}