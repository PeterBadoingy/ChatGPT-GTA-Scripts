using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GTA;
using GTA.Native;
using GTA.Math;

public class DumpsterMod : Script
{
    private const float InteractRange = 2.0f;
    private bool isInsideDumpster = false;
    private Prop currentDumpster = null;
    private Vector3 originalPlayerPosition;

    // Define custom positions for dumpsters
    private Dictionary<int, Vector3> dumpsterPositions = new Dictionary<int, Vector3>()
    {
        { 218085040, new Vector3(0.0f, 0.0f, 0.0f) }, // prop_dumpster_01a these don't seem to apply
        { 666561306, new Vector3(0.0f, 0.0f, 0.0f) }, // prop_dumpster_02a
        { -58485588, new Vector3(0.0f, 0.0f, 0.0f) },  // prop_dumpster_02b
        { -206690185, new Vector3(0.0f, 0.0f, 0.0f) }, // prop_dumpster_3a
        { 1511880420, new Vector3(0.0f, 0.0f, 0.0f) }, // prop_dumpster_4a
        { 682791951, new Vector3(0.0f, 0.0f, 0.0f) }   // prop_dumpster_4b
    };

    public DumpsterMod()
    {
        KeyDown += OnKeyDown;
        Tick += OnTick;
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.E)
        {
            if (!isInsideDumpster)
            {
                CheckForDumpsters();
            }
            else
            {
                ExitDumpster();
            }
        }
    }

    private void OnTick(object sender, EventArgs e)
    {
        if (isInsideDumpster)
        {
            // Set the player's camera view mode to Third Person Close
            Function.Call(Hash.SET_FOLLOW_PED_CAM_VIEW_MODE, 4); // 0 - Third Person Close

            // Set Wanted Level
            Function.Call(Hash.FORCE_START_HIDDEN_EVASION, Game.Player.Character);
        }
    }

    private void CheckForDumpsters()
    {
        Vector3 playerPosition = Game.Player.Character.Position;

        foreach (Prop dumpster in World.GetNearbyProps(playerPosition, InteractRange))
        {
            if (dumpster.Model.IsDumpster())
            {
                EnterDumpster(dumpster);
                return;
            }
        }
    }

    private void EnterDumpster(Prop dumpster)
    {
        isInsideDumpster = true;
        currentDumpster = dumpster;
        originalPlayerPosition = Game.Player.Character.Position;

        // Get the dumpster's forward vector
        Vector3 dumpsterForward = dumpster.ForwardVector;

        // Calculate the angle between the dumpster's forward vector and the north direction
        float dumpsterHeading = Function.Call<float>(Hash.GET_HEADING_FROM_VECTOR_2D, dumpsterForward.X, dumpsterForward.Y);

        // Teleport the player to the dumpster's position
        Game.Player.Character.Position = dumpster.Position;

        // Set the player's heading to match the dumpster's heading
        Game.Player.Character.Heading = dumpsterHeading;

        // Flip the player's heading by 180 degrees
        Game.Player.Character.Heading += 180.0f;

        // Disable collision for the dumpster
        dumpster.IsCollisionEnabled = false;

        // Play animation or any other action suitable for being inside the dumpster
        Game.Player.Character.Task.Cower(-1);

        Wait(1500); // Adjust the time as needed

        // Reduce player visibility to police and NPCs
        Function.Call(Hash.SET_ENTITY_VISIBLE, Game.Player.Character, false);

        // Re-enable collision while the player is inside
        dumpster.IsCollisionEnabled = true;

        // Set Wanted Level
        Function.Call(Hash.SET_PLAYER_WANTED_LEVEL, Game.Player.Character, 0, true);

        // Set Wanted Level
        Function.Call(Hash.SET_PLAYER_WANTED_LEVEL_NOW, Game.Player.Character, false );

    }

    private void ExitDumpster()
    {
        isInsideDumpster = false;
        currentDumpster = null;

        // Reset player's animation
        Game.Player.Character.Task.ClearAll();

        // Restore player visibility to police and NPCs
        Function.Call(Hash.SET_ENTITY_VISIBLE, Game.Player.Character, true);

        // Teleport player back to their original position
        Game.Player.Character.Position = originalPlayerPosition;

        // If needed, you can re-enable collision for the dumpster here
    }
}

public static class ModelExtensions
{
    public static bool IsDumpster(this Model model)
    {
        // Define your dumpster models here
        // Adjust as per the actual model hashes of dumpsters in your game
        int[] dumpsterHashes = new int[]
        {
            218085040, // prop_dumpster_01a
            666561306, // prop_dumpster_02a
            -58485588, // prop_dumpster_02b
            -206690185, // prop_dumpster_3a
            1511880420, // prop_dumpster_4a
            682791951 // prop_dumpster_4b
        };

        return Array.IndexOf(dumpsterHashes, model.Hash) != -1;
    }
}