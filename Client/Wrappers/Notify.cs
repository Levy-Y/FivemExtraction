using CitizenFX.Core.Native;

namespace Autopilot_NPC.Client.Wrappers
{
    public class Notify
    {
        public static void SendNotification(string text)
        {
            API.SetNotificationTextEntry("STRING");
            API.AddTextComponentString(text);
            API.DrawNotification(true, false);
        }
    }
}