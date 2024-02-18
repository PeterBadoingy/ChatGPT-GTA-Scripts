using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GTA;
using GTA.Native;
using GTA.Math;

public class DumpsterMod : Script
{
    // Define the range within which the player can interact with dumpsters
    private const float InteractRange = 2.0f;
    // Track whether the player is currently inside a dumpster
    private bool isInsideDumpster = false;
    // Track the currently entered dumpster
    private Prop currentDumpster = null;
    // Store the original position of the player before entering the dumpster
    private Vector3 originalPlayerPosition;
    // Store the dumpster that the player has entered
    private Prop dumpster;

    public DumpsterMod()
    {
        // Subscribe to key down and tick events
        KeyDown += OnKeyDown;
        Tick += OnTick;
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        // Check if the 'E' key is pressed
        if (e.KeyCode == Keys.E)
        {
            // If the player is not inside a dumpster, check for nearby dumpsters to enter
            if (!isInsideDumpster)
            {
                CheckForDumpsters();
            }
            // If the player is already inside a dumpster, trigger the exit action
            else
            {
                ExitDumpster();
            }
        }
    }

    private void OnTick(object sender, EventArgs e)
    {
        // If the player is inside a dumpster, set the camera view mode to third-person close
        if (isInsideDumpster)
        {
            Function.Call(Hash.SET_FOLLOW_PED_CAM_VIEW_MODE, 0);
        }
    }

    private void CheckForDumpsters()
    {
        // Get the player's position
        Vector3 playerPosition = Game.Player.Character.Position;

        // Iterate through nearby props to find dumpsters
        foreach (Prop nearbyDumpster in World.GetNearbyProps(playerPosition, InteractRange))
        {
            // Check if the nearby prop is a dumpster
            if (nearbyDumpster.Model.IsDumpster())
            {
                // If a dumpster is found, enter it
                EnterDumpster(nearbyDumpster);
                return;
            }
        }
    }

    private void EnterDumpster(Prop enteredDumpster)
    {
        // Assign the entered dumpster to the class-level variable
        dumpster = enteredDumpster;
        // Set the player as inside the dumpster
        isInsideDumpster = true;
        // Update the current dumpster
        currentDumpster = dumpster;
        // Store the original player position before entering the dumpster
        originalPlayerPosition = Game.Player.Character.Position;

        // Calculate the direction vector from the dumpster to the player
        Vector3 dumpsterToPlayer = Game.Player.Character.Position - dumpster.Position;
        dumpsterToPlayer.Normalize();
        // Calculate the dot product to determine if the player is facing the dumpster
        float dotProduct = Vector3.Dot(dumpster.ForwardVector, dumpsterToPlayer);

        // If the player is not facing the dumpster, adjust the player's heading
        if (dotProduct < 0.5f)
        {
            float dumpsterHeading = Function.Call<float>(Hash.GET_HEADING_FROM_VECTOR_2D, dumpster.ForwardVector.X, dumpster.ForwardVector.Y);
            Game.Player.Character.Heading = dumpsterHeading;
        }

        // Attach the player to the dumpster
        Function.Call(Hash.ATTACH_ENTITY_TO_ENTITY, Game.Player.Character, dumpster, 18905, 0f, -1.0f, 1.2f, 0f, 0f, 0f, true, true, true, true, 0, true);

        // Play the animation to climb up before teleporting the player inside the dumpster
        Game.Player.Character.Task.PlayAnimation("anim@veh@technical@rear@enter_exit", "climb_up", 8.0f, -8.0f, -1, AnimationFlags.None | AnimationFlags.None, 0.0f);
        Wait(2000);
        Function.Call(Hash.DETACH_ENTITY, Game.Player.Character, true, true);

        // Disable collision while the player is inside the dumpster
        dumpster.IsCollisionEnabled = false;

        // Teleport the player to the dumpster's position
        Game.Player.Character.Position = dumpster.Position;

        // Set the player's heading to match the dumpster's heading
        float dumpsterHeadingFinal = Function.Call<float>(Hash.GET_HEADING_FROM_VECTOR_2D, dumpster.ForwardVector.X, dumpster.ForwardVector.Y);
        Game.Player.Character.Heading = dumpsterHeadingFinal + 180.0f;

        // Set the player's task to cower as you did before
        Game.Player.Character.Task.Cower(-1);

        Wait(2000);

        // Hide the player from police and NPCs
        Function.Call(Hash.SET_ENTITY_VISIBLE, Game.Player.Character, false);

        // Re-enable collision while the player is inside
        dumpster.IsCollisionEnabled = true;
    }

    private void ExitDumpster()
    {
        // Set the player as not inside the dumpster
        isInsideDumpster = false;
        // Reset the current dumpster
        currentDumpster = null;

        // Clear player's animation tasks
        Game.Player.Character.Task.ClearAll();

        // Disable collision while the player is inside the dumpster
        dumpster.IsCollisionEnabled = false;

        // Make the player visible to police and NPCs
        Function.Call(Hash.SET_ENTITY_VISIBLE, Game.Player.Character, true);

        // Play the animation to climb up before teleporting the player out of the dumpster
        Game.Player.Character.Task.PlayAnimation("anim@veh@technical@rear@enter_exit", "climb_up", 8.0f, -8.0f, -1, AnimationFlags.None | AnimationFlags.None, 0.0f);
        Wait(2000);

        // Teleport player back to their original position
        //Game.Player.Character.Position = originalPlayerPosition;

        // Re-enable collision while the player is inside
        dumpster.IsCollisionEnabled = true;

        // Set Police ignore player
        Function.Call(Hash.SET_POLICE_IGNORE_PLAYER, Game.Player.Character, false);

        // Set Everyone ignore player
        Function.Call(Hash.SET_EVERYONE_IGNORE_PLAYER, Game.Player.Character, false);
    }
}

public static class ModelExtensions
{
    public static bool IsDumpster(this Model model)
    {
        // Define hashes for dumpster models
        int[] dumpsterHashes = new int[]
        {
            218085040, // prop_dumpster_01a
            666561306, // prop_dumpster_02a
            -58485588, // prop_dumpster_02b
            -206690185, // prop_dumpster_3a
            1511880420, // prop_dumpster_4a
            682791951 // prop_dumpster_4b
        };

        // Check if the model hash is in the list of dumpster hashes
        return Array.IndexOf(dumpsterHashes, model.Hash) != -1;
    }
}
