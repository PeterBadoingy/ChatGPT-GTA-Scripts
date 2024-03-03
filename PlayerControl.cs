//  = Quick Hotkeys for Animations = Numpad 1-9 = * to enable/disable =

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using GTA;
using GTA.UI;
using GTA.Native;

public class AnimationScript : Script
{
    private bool scriptEnabled = true;
    private bool animationPlaying = false; // Track if an animation is currently playing
    private int currentAnimationIndex = -1; // Track the index of the current animation
    private Dictionary<int, Dictionary<string, string>> animationData;

    public AnimationScript()
    {
        animationData = new Dictionary<int, Dictionary<string, string>>();
        LoadAnimationData("animation_data.ini");
        
        Tick += OnTick;
        KeyDown += OnKeyDown;
    }

    private void LoadAnimationData(string filePath)
    {
        string animationDataFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath);

        if (!File.Exists(animationDataFilePath))
        {
            NotifyScriptStatus("Error: INI file not found!");
            return;
        }

        string[] lines = File.ReadAllLines(animationDataFilePath);

        int currentKey = 1;
        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();
            if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
            {
                int animationKey;
                if (int.TryParse(trimmedLine.Substring(10, 1), out animationKey))
                {
                    currentKey = animationKey;
                    animationData[currentKey] = new Dictionary<string, string>();
                }
            }
            else if (animationData.ContainsKey(currentKey) && trimmedLine.Contains("="))
            {
                string[] parts = trimmedLine.Split(new char[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    string key = parts[0].Trim();
                    string value = parts[1].Trim();
                    animationData[currentKey][key] = value;
                }
            }
        }
    }

    private void OnTick(object sender, EventArgs e)
    {
        if (!scriptEnabled)
            return;

        // Your main logic for handling animations/scenarios goes here
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Multiply) // Numpad * key
        {
            scriptEnabled = !scriptEnabled;
            NotifyScriptStatus("Hotkeys " + (scriptEnabled ? "Enabled" : "Disabled"));
        }
        else if (scriptEnabled)
        {
            if (e.KeyCode >= Keys.NumPad1 && e.KeyCode <= Keys.NumPad9)
            {
                int animationIndex = (int)(e.KeyCode - Keys.NumPad1) + 1;
                if (animationPlaying && currentAnimationIndex == animationIndex)
                {
                    // If the animation is playing and the same key is pressed again, cancel the animation
                    CancelAnimation();
                }
                else
                {
                    PlayAnimation(animationIndex);
                }
            }
        }
    }

    private void PlayAnimation(int index)
    {
        if (animationData.ContainsKey(index))
        {
            Dictionary<string, string> animInfo = animationData[index];
            string animationDictionary = animInfo["AnimationDictionary"];
            string animationName = animInfo["AnimationName"];
            bool setRepeat = bool.Parse(animInfo.ContainsKey("SetRepeat") ? animInfo["SetRepeat"] : "false");
            bool isWholeBody = bool.Parse(animInfo.ContainsKey("IsWholeBody") ? animInfo["IsWholeBody"] : "false");
            bool upperBodyOnly = bool.Parse(animInfo.ContainsKey("UpperBodyOnly") ? animInfo["UpperBodyOnly"] : "false");

            // Request the animation dictionary
            Function.Call(Hash.REQUEST_ANIM_DICT, animationDictionary);

            // Check if the animation dictionary is loaded
            if (Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, animationDictionary))
            {
                // Play the animation
                int flags = 0;
                if (setRepeat) flags |= 1;
                if (isWholeBody) flags |= 15;
                if (upperBodyOnly) flags |= 49;
                
                Function.Call(Hash.TASK_PLAY_ANIM, Game.Player.Character.Handle, animationDictionary, animationName, 5f, -5f, -1, flags, 0, false, false, false);
                
                // Notify that the animation is being played
                //NotifyScriptStatus("Playing Animation: " + animInfo["Name"]);
                
                animationPlaying = true;
                currentAnimationIndex = index;
            }
            else
            {
                // Notify if the animation dictionary failed to load
                //NotifyScriptStatus("Failed to load animation dictionary: " + animationDictionary);
            }
        }
        else
        {
            // Notify if the animation index is not found
            //NotifyScriptStatus("Animation not found for index: " + index);
        }
    }

    private void CancelAnimation()
    {
        Function.Call(Hash.CLEAR_PED_TASKS, Game.Player.Character.Handle);
        //NotifyScriptStatus("Animation Canceled");
        
        animationPlaying = false;
        currentAnimationIndex = -1;
    }

    private void NotifyScriptStatus(string message)
    {
        Notification.Show(message, true);
    }
}
