// Press Y to Drop Spike Stripes up to max:3 until 30-second recharge. Existing spikes are deleted on recharge or being far enough away to trigger them to delete automatically.
using GTA;
using GTA.Native;
using GTA.Math;
using System;
using System.Windows.Forms;
using System.Collections.Generic;

// Define the SpikeStripScript class that extends Script
public class SpikeStripScript : Script
{
    // Constants for spike strip limits and cooldown
    private const int MaxSpikeStrips = 3;
    private const int CooldownDurationSeconds = 30;
    private const float SpikeStripCollisionDistanceSquared = 1.0f * 3.0f;

    // Flags and variables for spike strip management
    private bool spikeStripsDeployed = false;
    private int deployedSpikeStripsCount = 0;
    private DateTime lastDeploymentTime = DateTime.MinValue;
    private DateTime cooldownEndTime = DateTime.MinValue;
    private bool cooldownFinishedNotified = true;
    private bool spikesUsedDuringCooldown = false;

    // List to store references to spike strip props
    private List<Prop> spikeStripsList = new List<Prop>();

    // Spike strip prop variable
    private Prop spikeStrip;

    // Constructor for the SpikeStripScript class
    public SpikeStripScript()
    {
        // Subscribe to KeyDown and Tick events
        KeyDown += OnKeyDown;
        Tick += OnTick;
    }

    // Event handler for KeyDown event
    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        // Check if the 'Y' key is pressed
        if (e.KeyCode == Keys.Y)
        {
            // Toggle spike strip deployment on 'Y' key press
            if (Game.Player.Character.IsInVehicle())
            {
                // Check if the player is on cooldown
                if (DateTime.Now < cooldownEndTime)
                {
                    // Notify the player about the cooldown
                    cooldownFinishedNotified = false;
                    NotifyCooldown();
                }
                else
                {
                    // Notify the player about successful deployment
                    spikesUsedDuringCooldown = true;
                    DeploySpikeStrip();
                }
            }
            else
            {
                // Notify the player if they are not in a vehicle
                UI.Notify("You must be in a vehicle to deploy spike strips!");
            }
        }
    }

    // Event handler for Tick event
    private void OnTick(object sender, EventArgs e)
    {
        // Check if the cooldown has finished and spikes were used during the cooldown
        if (DateTime.Now >= cooldownEndTime && spikesUsedDuringCooldown && !cooldownFinishedNotified)
        {
            // Notify the player that they can use spikes again
            UI.Notify("Cooldown finished. You can use spikes again!");

            // Set the flag to true to prevent repeated notifications
            cooldownFinishedNotified = true;

            // Reset the spikesUsedDuringCooldown flag
            spikesUsedDuringCooldown = false;

            // Clear existing spikes
            DeleteSpikeStrips();
        }

        // Check conditions if spike strips are deployed during each tick
        if (spikeStripsDeployed)
        {
            // Check for collisions with vehicles
            CheckCollisions();

            // Check distance from the deployed spike strips and delete if too far
            CheckDistance();
        }
    }

    // Method to deploy a single spike strip
    private void DeploySpikeStrip()
    {
        // Get the current vehicle
        Vehicle currentVehicle = Game.Player.Character.CurrentVehicle;

        // Calculate position and rotation for the spike strip relative to the vehicle
        Vector3 vehiclePos = currentVehicle.Position;
        Vector3 offset = currentVehicle.ForwardVector * -5.0f;
        Vector3 spikePos = vehiclePos + offset + new Vector3(0, 0, -0.5f);
        Vector3 spikeRot = currentVehicle.Rotation + new Vector3(0, 0, 90.0f);

        // Create the spike strip prop
        spikeStrip = World.CreateProp("p_ld_stinger_s", spikePos, spikeRot, false, false);

        // Add the spike strip to the list
        spikeStripsList.Add(spikeStrip);

        // Update the deployment flags and count
        spikeStripsDeployed = true;
        deployedSpikeStripsCount++;

        // Notify the player about successful deployment
        UI.Notify("Spike strip deployed!");

        // Check if the last spike is used
        if (deployedSpikeStripsCount == MaxSpikeStrips)
        {
            // Notify the player about maximum spike strips deployed and initiate cooldown
            UI.Notify("Maximum spike strips deployed. Cooldown initiated.");
            InitiateCooldown();
        }
    }

    // Method to check collisions between vehicles and spike strips
    private void CheckCollisions()
    {
        // Iterate through nearby vehicles
        foreach (Vehicle vehicle in World.GetNearbyVehicles(Game.Player.Character.Position, 250f))
        {
            // Check if the vehicle is colliding with any of the spike strips
            if (vehicle.Exists() && IsCollidingWithSpikeStrips(vehicle))
            {
                // Burst tires of the colliding vehicle
                BurstCollidingTires(vehicle);
            }
        }
    }

    // Method to check if a vehicle is colliding with any of the spike strips
    private bool IsCollidingWithSpikeStrips(Vehicle vehicle)
    {
        foreach (Prop spikeStrip in spikeStripsList)
        {
            // Check for collision between the vehicle and each spike strip
            if (IsCollidingWithSpikeStrip(vehicle, spikeStrip))
            {
                return true;
            }
        }

        return false;
    }

    // Method to check if a vehicle is colliding with a specific spike strip
    private bool IsCollidingWithSpikeStrip(Vehicle vehicle, Prop spikeStrip)
    {
        Vector3 propDimensions = spikeStrip.Model.GetDimensions();

        // Calculate a threshold distance based on prop dimensions
        float thresholdDistance = Math.Max(propDimensions.X, Math.Max(propDimensions.Y, propDimensions.Z));

        // Check for collision based on dimensions
        return vehicle.Position.DistanceToSquared(spikeStrip.Position) < thresholdDistance * thresholdDistance;
    }

    // Method to burst tires of a vehicle colliding with the spike strips
    private void BurstCollidingTires(Vehicle vehicle)
    {
        for (int i = 0; i < 4; i++)
        {
            if (!IsVehicleTyreBurst(vehicle, i) && IsTireCollidingWithSpikeStrips(vehicle, i))
            {
                SetVehicleTyreBurst(vehicle, i, true, 1000.0f);
            }
        }
    }

    // Method to check if a specific tire of a vehicle is colliding with the spike strips
    private bool IsTireCollidingWithSpikeStrips(Vehicle vehicle, int index)
    {
        foreach (Prop spikeStrip in spikeStripsList)
        {
            if (IsTireCollidingWithSpikeStrip(vehicle, index, spikeStrip))
            {
                return true;
            }
        }

        return false;
    }

    // Method to check if a specific tire of a vehicle is colliding with a specific spike strip
    private bool IsTireCollidingWithSpikeStrip(Vehicle vehicle, int index, Prop spikeStrip)
    {
        // Get the bone index for the specified wheel
        int boneIndex = Function.Call<int>(Hash.GET_ENTITY_BONE_INDEX_BY_NAME, vehicle.Handle, GetWheelBoneName(index));

        // Get the position of the tire
        Vector3 tirePos = Function.Call<Vector3>(Hash.GET_WORLD_POSITION_OF_ENTITY_BONE, vehicle.Handle, boneIndex);

        // Check for collision between the tire and the spike strip
        return tirePos.DistanceToSquared(spikeStrip.Position) < SpikeStripCollisionDistanceSquared;
    }

    // Method to check if a specific tire of a vehicle is burst
    private bool IsVehicleTyreBurst(Vehicle vehicle, int index)
    {
        int wheelID = index < 2 ? index : index + 2;
        return Function.Call<bool>(Hash.IS_VEHICLE_TYRE_BURST, vehicle.Handle, wheelID, false);
    }

    // Method to set the burst status of a specific tire of a vehicle
    private void SetVehicleTyreBurst(Vehicle vehicle, int index, bool burst, float damage)
    {
        int wheelID = index < 2 ? index : index + 2;
        Function.Call(Hash.SET_VEHICLE_TYRE_BURST, vehicle.Handle, wheelID, burst, damage);
    }

    // Method to check the distance between the player's vehicle and spike strips, and delete if too far
    private void CheckDistance()
    {
        // Check if the spike strips exist and the player is in a vehicle
        if (spikeStripsList.Count > 0 && Game.Player.Character.IsInVehicle() && Game.Player.Character.CurrentVehicle.Position.DistanceToSquared(spikeStripsList[0].Position) > 20000)
        {
            // Delete spike strips if too far from the player's vehicle
            DeleteSpikeStrips();
        }
    }

    // Method to delete spike strips
    private void DeleteSpikeStrips()
    {
        // Check if the spike strips exist and delete them
        if (spikeStripsList.Count > 0)
        {
            foreach (Prop spikeStrip in spikeStripsList)
            {
                if (spikeStrip != null && spikeStrip.Exists())
                {
                    spikeStrip.Delete();
                }
            }

            spikeStripsList.Clear();
            spikeStripsDeployed = false;
            deployedSpikeStripsCount = 0;

            // Notify the player about successful deletion
            // UI.Notify("Spike strip deleted!");
        }
    }

    // Method to initiate cooldown
    private void InitiateCooldown()
    {
        lastDeploymentTime = DateTime.Now;
        cooldownEndTime = DateTime.Now.AddSeconds(CooldownDurationSeconds);
        cooldownFinishedNotified = false;
    }

    // Method to notify the cooldown
    private void NotifyCooldown()
    {
        double remainingSeconds = CooldownDurationSeconds - (DateTime.Now - lastDeploymentTime).TotalSeconds;
        int roundedSeconds = (int)Math.Round(remainingSeconds);

        string message = (roundedSeconds == 1)
            ? String.Format("{0} second", roundedSeconds)
            : String.Format("{0} seconds", roundedSeconds);

        UI.Notify(String.Format("Cooldown active. Time remaining: {0}", message));
    }

    // Helper method to get the wheel bone name based on the tire index
    private string GetWheelBoneName(int index)
    {
        switch (index)
        {
            case 0: return "wheel_lf";
            case 1: return "wheel_rf";
            case 2: return "wheel_lr";
            case 3: return "wheel_rr";
            default: return "wheel_lf";
        }
    }
}
