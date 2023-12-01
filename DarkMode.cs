// Press U to enable Darkmode. 
//Dark mode switches off the vehicle lights if on and enables night vision inside a vehicle disables if player exists vehicle. 
//Need to adjust check on light status to prevent lights switching on when going into dark mode if the lights are currently off.
using System;
using System.Windows.Forms;
using GTA.Native;
using GTA.Math;
using GTA;

public class NightVisionScript : Script
{
    private bool nightVisionActive = false;
    private Vehicle lastVehicle = null;

    public NightVisionScript()
    {
        Tick += OnTick;
        KeyDown += OnKeyDown;
    }

    private void OnTick(object sender, EventArgs e)
    {
        // Check if the player is in a vehicle
        if (Game.Player.Character.IsInVehicle())
        {
            Vehicle vehicle = Game.Player.Character.CurrentVehicle;

            // Check if the player pressed "U"
            if (Game.IsKeyPressed(Keys.U))
            {
                ToggleNightVision(vehicle);
            }

            // Check if the player has exited the vehicle while night vision is active
            if (nightVisionActive && lastVehicle != null && !Game.Player.Character.IsInVehicle(lastVehicle))
            {
                ToggleNightVision(lastVehicle);
                lastVehicle = null; // Reset lastVehicle to null
            }
        }
        else
        {
            // Player is not in a vehicle, disable night vision and DarkMode
            if (nightVisionActive)
            {
                ToggleNightVision(null); // Pass null to indicate player, not a vehicle
                nightVisionActive = false;
            }
        }
    }

    private void OnKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
    {
        // Check if the player is in a vehicle
        if (Game.Player.Character.IsInVehicle())
        {
            Vehicle vehicle = Game.Player.Character.CurrentVehicle;

            // Check if the player pressed "U"
            if (e.KeyCode == Keys.U)
            {
                ToggleNightVision(vehicle);
            }
        }
    }

    private void ToggleNightVision(Vehicle vehicle)
    {
        // Toggle vehicle lights
        if (vehicle != null)
        {
            if (vehicle.LightsOn)
            {
                vehicle.LightsOn = false;
            }
            else
            {
                vehicle.LightsOn = true;
            }
        }

        // Toggle night vision effect
        if (nightVisionActive)
        {
            UI.Notify("DarkMode De-Activated!");
            Script.Wait(100);
            Function.Call(Hash.SET_NIGHTVISION, false);
            nightVisionActive = false;
        }
        else
        {
            UI.Notify("DarkMode Activated!");
            Script.Wait(100);
            Function.Call(Hash.SET_NIGHTVISION, true);
            nightVisionActive = true;
        }

        // Update lastVehicle if the night vision is active and it's a vehicle
        if (nightVisionActive && vehicle != null)
        {
            lastVehicle = vehicle;
        }
    }
}
