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
    private bool animationPlaying = false;
    private int currentAnimationIndex = -1;
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
        if (e.KeyCode == Keys.Multiply)
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
        int isWholeBodyFlag = int.Parse(animInfo.ContainsKey("IsWholeBodyFlag") ? animInfo["IsWholeBodyFlag"] : "0");
        int upperBodyOnlyFlag = int.Parse(animInfo.ContainsKey("UpperBodyOnlyFlag") ? animInfo["UpperBodyOnlyFlag"] : "0");

        Function.Call(Hash.REQUEST_ANIM_DICT, animationDictionary);

        if (Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, animationDictionary))
        {
            int flags = 0;

            if (Game.Player.Character.IsInVehicle())
            {
                // Use different flags for vehicle animations
                flags = isWholeBodyFlag != 0 ? isWholeBodyFlag : upperBodyOnlyFlag;
                if (setRepeat) flags |= 1;
            }
            else
            {
                // Apply appropriate flags for on-foot animations
                if (isWholeBodyFlag != 0)
                {
                    // Whole-body animation flag
                    flags = isWholeBodyFlag;
                }
                else if (upperBodyOnlyFlag != 0)
                {
                    // Upper-body animation with weapon visibility flag
                    flags = upperBodyOnlyFlag | 31;
                }
                else
                {
                    // Default to keeping weapon visible
                    flags = 31;
                }

                if (setRepeat) flags |= 1;
            }

            Function.Call(Hash.TASK_PLAY_ANIM, Game.Player.Character.Handle, animationDictionary, animationName, 3f, -3f, -1, flags, 0, false, false, false);

            animationPlaying = true;
            currentAnimationIndex = index;
        }
        else
        {
            //NotifyScriptStatus("Failed to load animation dictionary: " + animationDictionary);
        }
    }
    else
    {
        //NotifyScriptStatus("Animation not found for index: " + index);
    }
}

    private void CancelAnimation()
    {
        Function.Call(Hash.CLEAR_PED_TASKS, Game.Player.Character.Handle);
        animationPlaying = false;
        currentAnimationIndex = -1;
    }

    private void NotifyScriptStatus(string message)
    {
        Notification.Show(message, true);
    }
}
