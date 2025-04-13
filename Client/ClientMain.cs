using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Mono.CSharp;
using static CitizenFX.Core.Native.API;

namespace Autopilot_NPC.Client
{
    public class ClientMain : BaseScript
    {
        // Store Heli, Driver, and Blip instances for later use
        private static Vehicle _vehicle;
        private static int _driver;
        private static Blip _destBlip;

        // Dummy entity for Heli mission workaround
        private static readonly Model _dummyEntityModel = PedHash.Pilot01SMY;
        private static int _dummyEntity;
        
        // TODO: Move to separate config file
        // Vehicle Model hash config
        private static readonly Model VehicleModel = VehicleHash.Volatus;
        private static readonly uint VehModelHash = (uint)VehicleModel;

        // TODO: Move to separate config file
        // Pilot ped hash config
        private static readonly Model PedModel = PedHash.Pilot01SMY;
        private static readonly uint PedModelHash = (uint)PedModel;

        // TODO: Move to separate config file
        // Starting position of the Heli
        private static Vector3 _homePosition = new Vector3(-1146.04f, -2864.61f, 14.0f);

        public ClientMain()
        {
            // Register commands ! DEV !
            RegisterCommand("extract", new Action(SpawnHelicopterLogic), false);
            RegisterCommand("cancelheli", new Action(RemoveHelicopterLogic), false);
        }
        
        private static async void SpawnHelicopterLogic()
        {
            // Check if an extraction is already in progress
            if (_vehicle != null)
            {
                Wrappers.Notify.SendNotification("An extraction is already in progress.");
                return;
            }
            
            // Create dummy entity for later use
            // Make it invisible, invincible, frozen
            await Tools.Model.LoadModel(_dummyEntityModel);
            _dummyEntity = CreatePed(4, _dummyEntityModel, _homePosition.X, _homePosition.Y, _homePosition.Z - 5, 0f, true, true);
            SetEntityVisible(_dummyEntity, false, false);
            SetEntityInvincible(_dummyEntity, true);
            SetEntityCollision(_dummyEntity, true, false);
            FreezeEntityPosition(_dummyEntity, true);
            SetBlockingOfNonTemporaryEvents(_dummyEntity, true);
            
            // Get player position for later use
            var playerPos = Game.Player.Character.Position;

            // Create Extraction point blip at player location
            _destBlip = Wrappers.Blip.CreateBlip("Extraction Point", 0, 161, playerPos);

            // Load and spawn Heli vehicle, set model as no longer needed, and set the rotor to full speed
            // Set custom primary, and secondary colors to black
            await Tools.Model.LoadModel(VehModelHash);
            _vehicle = await World.CreateVehicle(VehicleModel, _homePosition);
            SetHeliBladesFullSpeed(_vehicle.Handle);
            SetVehicleCustomPrimaryColour(_vehicle.Handle, 0, 0, 0);
            SetVehicleCustomSecondaryColour(_vehicle.Handle, 0, 0, 0);
            SetModelAsNoLongerNeeded(VehModelHash);
            
            // Load, and Create Driver ped inside the helicopter, set ped model as no longer needed
            // Some basic configuration to its behavior
            await Tools.Model.LoadModel(PedModelHash);
            _driver = CreatePedInsideVehicle(_vehicle.Handle, 0, PedModelHash, -1 ,true, true);
            SetPedCanBeTargetted(_driver, false);
            SetPedStayInVehicleWhenJacked(_driver, true);
            SetPedKeepTask(_driver, true);
            SetModelAsNoLongerNeeded(PedModelHash);
            SetPedCanBeDraggedOut(_driver, false);
            
            // Disable panic monologues on entering the helicopter
            StopPedSpeakingSynced(_driver, 1);

            // Set the driver and player Group relationship to 0 - Friendly, so the pilot won't attack the player
            SetRelationshipBetweenGroups(0, (uint)GetPedGroupIndex(Game.Player.Character.Handle),
                (uint)GetPedGroupIndex(_driver));
            SetRelationshipBetweenGroups(0, (uint)GetPedGroupIndex(_driver),
                (uint)GetPedGroupIndex(Game.Player.Character.Handle));

            // Only allow passenger seat, to fix hijacking problem
            SetPedVehicleForcedSeatUsage(Game.Player.Character.Handle, _vehicle.Handle, (int)VehicleSeat.RightFront, 0);
            SetPedVehicleForcedSeatUsage(Game.Player.Character.Handle, _vehicle.Handle, (int)VehicleSeat.RightRear, 0);
            
            // Add Helicopter blip to Extraction heli
            Wrappers.Blip.AddBlipForEntity(_vehicle.Handle, "Extraction Point", 40, 422);

            Wrappers.Notify.SendNotification("An extraction helicopter is inbound!");
            
            // Start a heli chase task (much more precise in terms of object, and terrain avoidance, then TaskHeliMission) towards the player
            // Wait until it's in range of the player (25 units)
            TaskHeliChase(_driver, Game.Player.Character.Handle, 0.0f, 0.0f, 20.0f);
            while (!_vehicle.IsInRangeOf(Game.Player.Character.Position, 25))
            {
                await Delay(1000);
            }
            
            // Initiate Heli mission with flag 20
            // Wait until heli is near the ground
            Wrappers.Task.TaskHeliMission(_driver, _vehicle.Handle, 0, 0, playerPos.X,
                playerPos.Y, playerPos.Z, 20, 60.0f);
            while (!_vehicle.IsInRangeOf(playerPos, 7))
            {
                await Delay(1000);
            }
            
            // Wait until the player enters the vehicle
            while (!Game.Player.Character.IsInVehicle(_vehicle))
            {
                Wrappers.Notify.SendNotification("The extraction helicopter is waiting for you!");
                await Delay(1500);
            }
            
            // Remove Extraction point blip
            Wrappers.Blip.RemoveBlip(_destBlip);
            
            Wrappers.Notify.SendNotification("Starting extraction!");
            
            // Start a heli chase task (much more precise in terms of object, and terrain avoidance, then TaskHeliMission) towards the player
            // Wait till it's in range of the player (25 units)
            TaskHeliChase(_driver, _dummyEntity, 0.0f, 0.0f, 20.0f);
            while (!_vehicle.IsInRangeOf(GetEntityCoords(_dummyEntity, true), 25))
            {
                await Delay(1000);
            }
            
            // Initiate Heli mission with flag 20
            // Wait until heli is near the ground
            Wrappers.Task.TaskHeliMission(_driver, _vehicle.Handle, 0, 0, GetEntityCoords(_dummyEntity, true).X,
                GetEntityCoords(_dummyEntity, true).Y, GetEntityCoords(_dummyEntity, true).Z, 20, 60.0f);
            while (!_vehicle.IsInRangeOf(GetEntityCoords(_dummyEntity, true), 7))
            {
                await Delay(1000);
            }
            
            Wrappers.Notify.SendNotification("Successful extraction!");
            
            // Wait till the player leaves the helicopter
            while (Game.Player.Character.IsInVehicle(_vehicle))
            {
                await Delay(3000);
            }
            
            // Cleanup logic
            RemoveHelicopterLogic();
        }

        /// <summary>
        /// Cleanup command. Deletes the Driver, Heli, and Blip and sets their values to null
        /// </summary>
        private static void RemoveHelicopterLogic()
        {
            if (_vehicle == null)
            {
                Debug.WriteLine("No helicopter spawned yet!");
                Debug.WriteLine("No pilot spawn yet!");
                return;
            }

            Wrappers.Ped.DeletePed(_driver);
            Wrappers.Ped.DeletePed(_dummyEntity);
            
            _vehicle.Delete();
            _vehicle = null;

            Wrappers.Blip.RemoveBlip(_destBlip);
        }

        [Tick]
        public Task OnTick()
        {
            return Task.FromResult(0);
        }
    }
}