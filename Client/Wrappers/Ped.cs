using CitizenFX.Core.Native;

namespace Autopilot_NPC.Client.Wrappers
{
    public class Ped
    {
        public static void DeletePed(int pedId)
        {
            var id = pedId;
            API.DeletePed(ref id);
        }
    }
}