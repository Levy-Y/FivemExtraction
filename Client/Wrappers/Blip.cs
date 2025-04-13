using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Autopilot_NPC.Client.Wrappers
{
    public static class Blip
    {
        public static CitizenFX.Core.Blip CreateBlip(string name, int color, int sprite, Vector3 position) 
        {
            CitizenFX.Core.Blip blip = World.CreateBlip(position);
            API.SetBlipColour(blip.Handle, color);
            API.SetBlipSprite(blip.Handle, sprite);
            API.BeginTextCommandSetBlipName("STRING");
            API.AddTextComponentString(name);
            API.EndTextCommandSetBlipName(blip.Handle);
            
            return blip;
        }
        
        public static int AddBlipForEntity(int entityId, string name, int color, int sprite) 
        {
            var entityBlip = API.AddBlipForEntity(entityId);
            API.SetBlipColour(entityBlip, color);
            API.SetBlipSprite(entityBlip, sprite);
            API.BeginTextCommandSetBlipName("STRING");
            API.AddTextComponentString(name);
            API.EndTextCommandSetBlipName(entityBlip);
            
            return entityBlip;
        }

        public static void RemoveBlip(CitizenFX.Core.Blip blip)
        {
            var blipHandle = blip.Handle;
            API.RemoveBlip(ref blipHandle);
        }
    }
}