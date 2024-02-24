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
    private Prop dumpster;

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
            // Function.Call(Hash.SET_FOLLOW_PED_CAM_VIEW_MODE, 2);
        }
    }

    private void CheckForDumpsters()
    {
        Vector3 playerPosition = Game.Player.Character.Position;

        foreach (Prop nearbyDumpster in World.GetNearbyProps(playerPosition, InteractRange))
        {
            if (nearbyDumpster.Model.IsDumpster())
            {
                EnterDumpster(nearbyDumpster);
                return;
            }
        }
    }

    private void HandlePlayerVisibilityAndAttention()
    {
        if (isInsideDumpster)
        {
            Function.Call(Hash.SET_POLICE_IGNORE_PLAYER, Game.Player.Character, true);
            Function.Call(Hash.SET_EVERYONE_IGNORE_PLAYER, Game.Player.Character, true);
            Function.Call(Hash.SET_ENTITY_VISIBLE, Game.Player.Character, false);
        }
        else
        {
            Function.Call(Hash.SET_POLICE_IGNORE_PLAYER, Game.Player.Character, false);
            Function.Call(Hash.SET_EVERYONE_IGNORE_PLAYER, Game.Player.Character, false);
            Function.Call(Hash.SET_ENTITY_VISIBLE, Game.Player.Character, true);
        }
    }

    private void EnterDumpster(Prop enteredDumpster)
    {
        dumpster = enteredDumpster;
        isInsideDumpster = true;
        currentDumpster = dumpster;
        originalPlayerPosition = Game.Player.Character.Position;

        // Calculate the direction vector from the dumpster to the player
        Vector3 dumpsterToPlayer = Game.Player.Character.Position - dumpster.Position;
        dumpsterToPlayer.Normalize();
        // Calculate the dot product to determine if the player is facing the dumpster
        float dotProduct = Vector3.Dot(dumpster.ForwardVector, dumpsterToPlayer);

        if (dotProduct < 0.5f)
        {
            // Player is not facing the dumpster, adjust player's heading to face it
            float dumpsterHeading = Function.Call<float>(Hash.GET_HEADING_FROM_VECTOR_2D, dumpster.ForwardVector.X, dumpster.ForwardVector.Y);
            Game.Player.Character.Heading = dumpsterHeading;
        }

        // Calculate the entry position in front of the dumpster
        Vector3 entryPosition = dumpster.Position + dumpster.ForwardVector * -1.2f; // Adjust distance as needed
        entryPosition += dumpster.UpVector * 0.2f; // Adjust height as needed

        // Teleport the player to the entry position
        Game.Player.Character.Position = entryPosition;

        // Play the dumpster enter animation
        PlayDumpsterEnterAnimation();
    }

    private void PlayDumpsterEnterAnimation()
    {
        Game.Player.Character.Task.PlayAnimation("move_climb", "standclimbup_80", 1.0f, -1, (AnimationFlags)512);

        Wait(800);

        dumpster.IsCollisionEnabled = false;

        Game.Player.Character.Position = dumpster.Position;

        float dumpsterHeadingFinal = Function.Call<float>(Hash.GET_HEADING_FROM_VECTOR_2D, dumpster.ForwardVector.X, dumpster.ForwardVector.Y);
        Game.Player.Character.Heading = dumpsterHeadingFinal + 180.0f;

        Game.Player.Character.Task.Cower(-1);

        Wait(800);

        // Call HandlePlayerVisibilityAndAttention() to make the player invisible and ignore by police
        HandlePlayerVisibilityAndAttention();

        dumpster.IsCollisionEnabled = true;
    }

    private void ExitDumpster()
    {
        isInsideDumpster = false;
        currentDumpster = null;

        Game.Player.Character.Task.ClearAll();
        Wait(1500);
        // Call HandlePlayerVisibilityAndAttention() to make the player visible and reset police attention
        HandlePlayerVisibilityAndAttention();

        PlayDumpsterExitAnimation();
    }

    private void PlayDumpsterExitAnimation()
    {
        Game.Player.Character.Task.PlayAnimation("move_climb", "standclimbup_80", 1.0f, -1, (AnimationFlags)512);

        Wait(900);

        // Teleport player back to their original position
        Game.Player.Character.Position = originalPlayerPosition;
    }
}

public static class ModelExtensions
{
    public static bool IsDumpster(this Model model)
    {
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
