using System;
using GTA;
using GTA.Native;
using GTA.Math;
using System.Windows.Forms;

public class ZipTieScript : Script
{
    private bool isNpcZipTied = false;
    private Ped zipTiedPed;
    private static readonly string animDict = "anim@move_m@prisoner_cuffed_rc";
    private static readonly string animName = "settle_low";
    private const float followDistance = 3.0f;
    private Prop attachedProp;

    public ZipTieScript()
    {
        Tick += OnTick;
        KeyDown += OnKeyDown;

        // Preload the animation dictionary when the script starts
        Function.Call(Hash.REQUEST_ANIM_DICT, animDict);
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Z)
        {
            GTA.UI.Screen.ShowSubtitle("Attempting to zip-tie NPC...", 2000);
            ZipTieNearbyNpc();
        }
        else if (e.KeyCode == Keys.X && isNpcZipTied)
        {
            GTA.UI.Screen.ShowSubtitle("Untying NPC...", 2000);
            UntieNpc();
        }
    }

    private void ZipTieNearbyNpc()
    {
        Ped player = Game.Player.Character;
        float detectionRadius = 5.0f;
        Ped targetNpc = null;

        // Loop through nearby NPCs and find the first valid one
        foreach (Ped ped in World.GetNearbyPeds(player, detectionRadius))
        {
            if (ped != null && ped.IsHuman && !ped.IsPlayer && ped.IsAlive && !ped.IsInVehicle())
            {
                targetNpc = ped;
                break;
            }
        }

        // If a valid NPC is found, zip-tie them
        if (targetNpc != null && targetNpc.IsAlive)
        {
            isNpcZipTied = true;
            zipTiedPed = targetNpc;

            zipTiedPed.Task.ClearAllImmediately();
            zipTiedPed.BlockPermanentEvents = true;
            zipTiedPed.CanRagdoll = false;
            zipTiedPed.CanBeTargetted = false;

            Function.Call(Hash.SET_ENABLE_HANDCUFFS, zipTiedPed, true);
            zipTiedPed.Position = zipTiedPed.Position;

            // Check if the animation dictionary is loaded
            int startTime = Game.GameTime;
            while (!Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, animDict))
            {
                if (Game.GameTime > startTime + 5000)
                {
                    GTA.UI.Screen.ShowSubtitle("Failed to load animation.", 3000);
                    return;
                }
                Wait(100);
            }

            zipTiedPed.Task.PlayAnimation(animDict, animName, 8.0f, -8.0f, -1, (AnimationFlags)49, 0.0f);

            // Attach prop to the right hand (IK_R_Hand)
            AttachPropToHand(zipTiedPed);

            GTA.UI.Screen.ShowSubtitle("NPC has been zip-tied and is following!", 4000);
        }
        else
        {
            GTA.UI.Screen.ShowSubtitle("No valid NPC nearby to zip-tie.", 2000);
        }
    }

    private void AttachPropToHand(Ped ped)
    {
        // Load the prop model (example: a gun or another prop)
        int propModel = 623548567;  // Replace this with your specific model ID
        
        // Request the model
        Function.Call(Hash.REQUEST_MODEL, propModel);

        // Wait for the model to load
        while (!Function.Call<bool>(Hash.HAS_MODEL_LOADED, propModel))
        {
            Wait(100);  // Wait for 100ms before checking again
        }

        // Create the prop at the NPC's position
        attachedProp = World.CreateProp(propModel, ped.Position, new Vector3(0, 0, 0), true, true);

        // Adjust the prop's position and orientation using the provided quaternion and offset
        Function.Call(Hash.ATTACH_ENTITY_TO_ENTITY, attachedProp.Handle, ped.Handle, GetBoneIndex(ped, "IK_R_Hand"), 
            -0.030f,      // X offset (Right-Left)
            0.020f,      // Y offset (Up-Down)
            -0.05f,   // Z offset (Forward-Backward) from offz
            181.0502617f, // Rotation X (Roll) from quaternion
            94.441851f, // Rotation Y (Pitch) from quaternion
            90.859409f,  // Rotation Z (Yaw) from quaternion
            true,       // Collision enabled
            true,       // Keep position relative
            false,      // No soft attach
            false,      // No gravity
            2,          // Attach type (default)
            true        // Allow rotation
        );
    }

    private int GetBoneIndex(Ped ped, string boneName)
    {
        // Returns the bone index for the given bone name
        return Function.Call<int>(Hash.GET_ENTITY_BONE_INDEX_BY_NAME, ped.Handle, boneName);
    }

    private void UntieNpc()
    {
        if (zipTiedPed != null && zipTiedPed.IsAlive)
        {
            zipTiedPed.Task.ClearAllImmediately();
            zipTiedPed.BlockPermanentEvents = false;
            zipTiedPed.CanRagdoll = true;
            zipTiedPed.CanBeTargetted = true;

            Function.Call(Hash.SET_ENABLE_HANDCUFFS, zipTiedPed, false); // Disable cuffs
            zipTiedPed.Task.ClearAllImmediately(); // Stop the animation

            // Remove the attached prop
            if (attachedProp != null && attachedProp.Exists())
            {
                attachedProp.Delete();
            }

            GTA.UI.Screen.ShowSubtitle("NPC has been untied!", 2000);
            isNpcZipTied = false; // Reset the status
        }
    }

    private void GetInVehicle()
    {
        if (zipTiedPed != null && zipTiedPed.IsAlive)
        {
            Vehicle playerVehicle = Game.Player.Character.CurrentVehicle;

            if (playerVehicle != null && !zipTiedPed.IsInVehicle())
            {
                // Try to get in the vehicle as a passenger in the back left seat (seat index 1)
                zipTiedPed.Task.EnterVehicle(playerVehicle, VehicleSeat.LeftRear);
            }
            else
            {
                GTA.UI.Screen.ShowSubtitle("No vehicle nearby or NPC is already in a vehicle.", 2000);
            }
        }
    }

    private void OnTick(object sender, EventArgs e)
    {
        if (isNpcZipTied && zipTiedPed != null && zipTiedPed.IsAlive)
        {
            Ped player = Game.Player.Character;

            // If the NPC is too far from the player, make them follow
            if (zipTiedPed.Position.DistanceTo(player.Position) > followDistance)
            {
                zipTiedPed.Task.FollowToOffsetFromEntity(player, new Vector3(0, -2.0f, 0), 1.0f, -1, 1.0f, true);
            }

            // Reapply animation if it's not playing
            if (!Function.Call<bool>(Hash.IS_ENTITY_PLAYING_ANIM, zipTiedPed, animDict, animName, 3))
            {
                zipTiedPed.Task.PlayAnimation(animDict, animName, 8.0f, -8.0f, -1, (AnimationFlags)49, 0.0f);
            }

            // Automatically order the NPC to get into the vehicle when the player does
            if (player.IsInVehicle() && !zipTiedPed.IsInVehicle())
            {
                GetInVehicle();
            }
        }
    }
}
