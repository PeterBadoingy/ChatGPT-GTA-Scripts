using System;
using GTA;
using GTA.Native;
using GTA.Math;

public class RadioLoudScript : Script
{
    public RadioLoudScript()
    {
        // Subscribe to the tick event, which is called every frame
        Tick += OnTick;
    }

    private void OnTick(object sender, EventArgs e)
    {
        // Get the player's current vehicle
        Vehicle playerVehicle = Game.Player.Character.CurrentVehicle;

        // Check if the player is in a vehicle
        if (playerVehicle != null)
        {
            // Set the vehicle radio to loud
            Function.Call(Hash.SET_VEHICLE_RADIO_LOUD, playerVehicle.Handle, true);
        }
    }
}
