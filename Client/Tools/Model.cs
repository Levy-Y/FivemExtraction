using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Autopilot_NPC.Client.Tools
{
    public class Model
    {
        public static async Task<bool> LoadModel(uint model)
        {
            if (!API.IsModelInCdimage(model))
            {
                Debug.WriteLine($"Invalid model {model} was supplied to LoadModel.");
                return false;
            }

            API.RequestModel(model);
            while (!API.HasModelLoaded(model))
            {
                Debug.WriteLine($"Waiting for model {model} to load");
                await BaseScript.Delay(100);
            }

            return true;
        }
    }
}