using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

public class FacialAnims : Script
{
    // Declare variables
    private Ped playerPed;
    private string currentFacialAnim = "";
    private bool showDisplay = false;
    private bool keysActive = false;
    private Color textColor = Color.White; // Set the desired text color here

    // Define a struct to represent a named facial animation
    private struct NamedFacialAnim
    {
        public string Caption;
        public string AnimName;

        // Constructor to initialize the struct
        public NamedFacialAnim(string caption, string animName)
        {
            Caption = caption;
            AnimName = animName;
        }
    }

    // List of named facial animations
    private List<NamedFacialAnim> vFacialAnims = new List<NamedFacialAnim>
    {
        new NamedFacialAnim("Aiming", "mood_aiming_1"),
        new NamedFacialAnim("Angry", "mood_angry_1"),
        new NamedFacialAnim("Burning", "burning_1"),
        new NamedFacialAnim("Dead", "dead_1"),
        new NamedFacialAnim("Drunk", "mood_drunk_1"),
        new NamedFacialAnim("Frustrated", "mood_frustrated_1"),
        new NamedFacialAnim("Happy", "mood_happy_1"),
        new NamedFacialAnim("Injured", "mood_injured_1"),
        new NamedFacialAnim("Normal", "mood_normal_1"),
        new NamedFacialAnim("Sleeping", "mood_sleeping_1"),
        new NamedFacialAnim("Smug", "mood_smug_1"),
        new NamedFacialAnim("Stressed", "mood_stressed_1"),
        new NamedFacialAnim("Sulk", "mood_sulk_1"),
    };

    // Constructor for the FacialAnims class
    public FacialAnims()
    {
        // Attach event handlers
        Tick += OnTick;
        KeyDown += OnKeyDown;

        // Initialize the playerPed variable with the player's character
        playerPed = Game.Player.Character;
    }

    // Function called every game tick
    private void OnTick(object sender, EventArgs e)
    {
        // Update playerPed to the current player character
        playerPed = Game.Player.Character;

        // Display current facial animation caption if showDisplay is true
        if (showDisplay)
        {
            GTA.UI.Screen.ShowSubtitle("Facial Expression: " + GetCaption(currentFacialAnim), 500);
        }
    }

    // Function called when a key is pressed
    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        // Check if keys are active
        if (keysActive)
        {
            // Handle left, right, and enter keys
            if (e.KeyCode == Keys.Left)
            {
                CycleFacialAnim(-1);
            }
            else if (e.KeyCode == Keys.Right)
            {
                CycleFacialAnim(1);
            }
            else if (e.KeyCode == Keys.Enter)
            {
                ApplySelectedFacialAnim();
            }
        }

        // Toggle keysActive and hide display on F9 key press
        if (e.KeyCode == Keys.F9)
        {
            keysActive = !keysActive;
            showDisplay = false;
        }
    }

    // Function to cycle through facial animations
    private void CycleFacialAnim(int direction)
    {
        int currentIndex = vFacialAnims.FindIndex(anim => anim.AnimName == currentFacialAnim);
        currentIndex = (currentIndex + direction + vFacialAnims.Count) % vFacialAnims.Count;

        NamedFacialAnim selectedAnim = vFacialAnims[currentIndex];
        SetFacialAnim(selectedAnim.AnimName);
    }

    // Function to set the current facial animation with error handling
    private void SetFacialAnim(string animName)
    {
        if (playerPed != null && playerPed.Exists())
        {
            playerPed.Task.ClearAll();
            currentFacialAnim = animName;
            showDisplay = true;
        }
        else
        {
            // Handle the case where the playerPed is not valid or doesn't exist
            // You might want to log or display an error message
            // For now, clear all tasks to prevent unexpected behavior
            if (playerPed != null)
            {
                playerPed.Task.ClearAll();
            }
        }
    }

    // Function to apply the selected facial animation
    private void ApplySelectedFacialAnim()
    {
        if (!string.IsNullOrEmpty(currentFacialAnim))
        {
            playerPed.Task.ClearAll();
            SetFacialMoodOverride(playerPed, currentFacialAnim);
            showDisplay = false;
        }
    }

    // Function to set the facial mood override with error handling
    private void SetFacialMoodOverride(Ped ped, string animName)
    {
        if (ped != null && ped.Exists())
        {
            int handle = ped.Handle;

            // Check if the ped has a valid handle before calling the function
            if (Function.Call<bool>(Hash.DOES_ENTITY_EXIST, handle))
            {
                Function.Call(Hash.SET_FACIAL_IDLE_ANIM_OVERRIDE, handle, animName, 0);
            }
            else
            {
                // Handle the case where the ped handle is not valid
                // You might want to log or display an error message
                // For now, clear all tasks to prevent unexpected behavior
                ped.Task.ClearAll();
            }
        }
        else
        {
            // Handle the case where the ped is not valid or doesn't exist
            // You might want to log or display an error message
            // For now, clear all tasks to prevent unexpected behavior
            ped.Task.ClearAll();
        }
    }

    // Function to get the caption of a facial animation
    private string GetCaption(string animName)
    {
        // Find the NamedFacialAnim with the given animName and return its Caption
        NamedFacialAnim anim = vFacialAnims.Find(namedAnim => namedAnim.AnimName == animName);
        return anim.Caption;
    }
}
