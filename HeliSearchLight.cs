// Searches for Police Helicopters within the player radius and enables the searchlight to follow the player while wanted.
// Having the Annihilator in the list enables the Weapons.

using System;
using GTA;
using GTA.Native;
using GTA.Math;

public class PoliceHelicopterMod : Script
{
    private Vehicle policeHelicopter;

    public PoliceHelicopterMod()
    {
        Tick += OnTick;
    }

    private void OnTick(object sender, EventArgs e)
    {
        // Get all vehicles near the player
        Vehicle[] nearbyVehicles = World.GetNearbyVehicles(Game.Player.Character.Position, 200.0f);

        // Find the nearest police helicopter
        policeHelicopter = FindNearestPoliceHelicopter(nearbyVehicles);

        if (policeHelicopter != null && policeHelicopter.Exists())
        {
            // The rest of your script logic here
            Ped heliPilot = policeHelicopter.GetPedOnSeat(VehicleSeat.Driver);
            if (heliPilot != null && heliPilot.Exists())
            {
                // Get the position of the player
                Vector3 playerPosition = Game.Player.Character.Position;

                // Set the searchlight target for the police helicopter
                Function.Call(Hash.SET_MOUNTED_WEAPON_TARGET, heliPilot.Handle, 0, 0, playerPosition.X, playerPosition.Y, playerPosition.Z, 2, 0);
            }
        }
    }

    private Vehicle FindNearestPoliceHelicopter(Vehicle[] vehicles)
    {
        Vehicle nearestPoliceHelicopter = null;
        float minDistance = float.MaxValue;

        foreach (Vehicle vehicle in vehicles)
        {
            if (IsPoliceHelicopter(vehicle))
            {
                float distance = vehicle.Position.DistanceToSquared(Game.Player.Character.Position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestPoliceHelicopter = vehicle;
                }
            }
        }

        return nearestPoliceHelicopter;
    }

    private bool IsPoliceHelicopter(Vehicle vehicle)
    {
        return
            vehicle.Model.Hash == (uint)VehicleHash.Annihilator ||
            vehicle.Model.Hash == (uint)VehicleHash.Buzzard2 ||
            vehicle.Model.Hash == (uint)VehicleHash.Frogger2 ||
            vehicle.Model.Hash == (uint)VehicleHash.Polmav;
        // Add more hashes if needed
    }
}
