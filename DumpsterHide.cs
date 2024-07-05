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
    private Ped playerPed;

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
            // Do a little Dance;
            // Roll a little Spliff;
            // Let's Get High Tonight, Get High Tonight;
        }
    }

    private Ped PlayerPed
    {
        get
        {
            if (playerPed == null || !playerPed.Exists())
            {
                playerPed = Game.Player.Character;
            }
            return playerPed;
        }
    }

    private void CheckForDumpsters()
    {
        Vector3 playerPosition = PlayerPed.Position;

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
            Function.Call(Hash.SET_POLICE_IGNORE_PLAYER, PlayerPed, true); // No Effect
            Function.Call(Hash.SET_EVERYONE_IGNORE_PLAYER, PlayerPed, true); // No Effect
            Function.Call(Hash.SET_ENTITY_VISIBLE, PlayerPed, false); // This works
            Function.Call(Hash.IS_ENTITY_ON_SCREEN, PlayerPed, false); // This ???
            Function.Call(Hash.SET_PLAYER_CAN_BE_HASSLED_BY_GANGS, PlayerPed, false);
            Function.Call(Hash.SET_IGNORE_LOW_PRIORITY_SHOCKING_EVENTS, PlayerPed, true);
        }
        else
        {
            Function.Call(Hash.SET_POLICE_IGNORE_PLAYER, PlayerPed, false); // No Effect
            Function.Call(Hash.SET_EVERYONE_IGNORE_PLAYER, PlayerPed, false); // No Effect
            Function.Call(Hash.SET_ENTITY_VISIBLE, PlayerPed, true); // This works
            Function.Call(Hash.IS_ENTITY_ON_SCREEN, PlayerPed, true); // This ???
            Function.Call(Hash.SET_PLAYER_CAN_BE_HASSLED_BY_GANGS, PlayerPed, true);
            Function.Call(Hash.SET_IGNORE_LOW_PRIORITY_SHOCKING_EVENTS, PlayerPed, false);
        }
    }

    private void EnterDumpster(Prop enteredDumpster)
    {
        dumpster = enteredDumpster;
        isInsideDumpster = true;
        currentDumpster = dumpster;
        originalPlayerPosition = PlayerPed.Position;

        // Calculate the direction vector from the dumpster to the player
        Vector3 dumpsterToPlayer = PlayerPed.Position - dumpster.Position;
        dumpsterToPlayer.Normalize();
        // Calculate the dot product to determine if the player is facing the dumpster
        float dotProduct = Vector3.Dot(dumpster.ForwardVector, dumpsterToPlayer);

        if (dotProduct < 0.5f)
        {
            // Player is not facing the dumpster, adjust player's heading to face it
            float dumpsterHeading = Function.Call<float>(Hash.GET_HEADING_FROM_VECTOR_2D, dumpster.ForwardVector.X, dumpster.ForwardVector.Y);
            PlayerPed.Heading = dumpsterHeading;
        }

        // Calculate the entry position in front of the dumpster
        Vector3 entryPosition = dumpster.Position + dumpster.ForwardVector * -1.2f; // Adjust distance as needed
        entryPosition += dumpster.UpVector * 0.2f; // Adjust height as needed

        // Teleport the player to the entry position
        PlayerPed.Position = entryPosition;

        // Play the dumpster enter animation
        PlayDumpsterEnterAnimation();
    }

    private void PlayDumpsterEnterAnimation()
    {
        PlayerPed.Task.PlayAnimation("move_climb", "standclimbup_80", 1.0f, -1, (AnimationFlags)512);

        Wait(800);

        dumpster.IsCollisionEnabled = false;

        PlayerPed.Position = dumpster.Position;

        float dumpsterHeadingFinal = Function.Call<float>(Hash.GET_HEADING_FROM_VECTOR_2D, dumpster.ForwardVector.X, dumpster.ForwardVector.Y);
        PlayerPed.Heading = dumpsterHeadingFinal + 180.0f;

        PlayerPed.Task.Cower(-1);

        Wait(800);

        HandlePlayerVisibilityAndAttention();

        dumpster.IsCollisionEnabled = true;
    }

    private void ExitDumpster()
    {
        isInsideDumpster = false;
        currentDumpster = null;

        PlayerPed.Task.ClearAll();
        Wait(1500);
 
        HandlePlayerVisibilityAndAttention();

        PlayDumpsterExitAnimation();
    }

    private void PlayDumpsterExitAnimation()
    {
        PlayerPed.Task.PlayAnimation("move_climb", "standclimbup_80", 1.0f, -1, (AnimationFlags)512);

        Wait(900);

        // Teleport player back to their original position
        PlayerPed.Position = originalPlayerPosition;
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
