using System;
using GTA;
using GTA.Native;
using System.Windows.Forms;

public class HatGlassesScript : Script
{
    bool hatsOn = false;
    int currentHats = -1;
    int myHats = -1;
    int sgTexture = -1; // Hat texture index
    bool hatsSet = false;
    bool noHats = false;

    bool glassesOn = false;
    int currentGlasses = -1;
    int myGlasses = -1;
    int sgTextureGlasses = -1; // Glasses texture index
    bool glassesSet = false;
    bool noGlasses = false;

    public HatGlassesScript()
    {
        Tick += OnTick;
        KeyDown += OnKeyDown;
    }

    private void OnTick(object sender, EventArgs e)
    {
        // Your main logic can go here if needed.
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.H)
        {
            HatsEventHandler();
        }
        else if (e.KeyCode == Keys.G)
        {
            SunglassesEventHandler();
        }
    }

    private void HatsEventHandler()
    {
        Ped player = Game.Player.Character;
        currentHats = Function.Call<int>(Hash.GET_PED_PROP_INDEX, player, 0);

        if (currentHats == -1 && !hatsSet)
        {
            noHats = true;
            hatsSet = false;
        }
        else if (currentHats != -1 && !hatsSet)
        {
            myHats = Function.Call<int>(Hash.GET_PED_PROP_INDEX, player, 0);
            sgTexture = Function.Call<int>(Hash.GET_PED_PROP_TEXTURE_INDEX, player, 0);
            noHats = false;
            hatsSet = true;
            hatsOn = true;
        }
        else if (currentHats == -1 && hatsSet)
        {
            hatsOn = false;
        }
        else if (hatsSet && currentHats != -1 && myHats != currentHats)
        {
            myHats = Function.Call<int>(Hash.GET_PED_PROP_INDEX, player, 0);
            sgTexture = Function.Call<int>(Hash.GET_PED_PROP_TEXTURE_INDEX, player, 0);
            hatsSet = true;
            noHats = false;
            hatsOn = true;
        }

        // Takes hat off / Puts hat On
        if (!noHats)
        {
            hatsOn = !hatsOn;
            PlayAnimation(player, hatsOn ? "missheistdockssetup1hardhat@" : "missheist_agency2ahelmet", hatsOn ? "put_on_hat" : "take_off_helmet_stand");
            if (hatsOn)
            {
                Wait(1200); // Adjust the time as needed
                Function.Call(Hash.SET_PED_PROP_INDEX, player, 0, myHats, sgTexture, 2);
                ShowNotification("Hat is on");
            }
            else
            {
                Wait(500); // Adjust the time as needed
                Function.Call(Hash.CLEAR_PED_PROP, player, 0);
                ShowNotification("Hat is off");
            }
        }
        else
        {
            ShowNotification("You are not wearing a Hat");
        }
    }

    private void SunglassesEventHandler()
    {
        Ped player = Game.Player.Character;
        currentGlasses = Function.Call<int>(Hash.GET_PED_PROP_INDEX, player, 1);

        if (currentGlasses == -1 && !glassesSet)
        {
            noGlasses = true;
            glassesSet = false;
        }
        else if (currentGlasses != -1 && !glassesSet)
        {
            myGlasses = Function.Call<int>(Hash.GET_PED_PROP_INDEX, player, 1);
            sgTextureGlasses = Function.Call<int>(Hash.GET_PED_PROP_TEXTURE_INDEX, player, 1);
            noGlasses = false;
            glassesSet = true;
            glassesOn = true;
        }
        else if (currentGlasses == -1 && glassesSet)
        {
            glassesOn = false;
        }
        else if (glassesSet && currentGlasses != -1 && myGlasses != currentGlasses)
        {
            myGlasses = Function.Call<int>(Hash.GET_PED_PROP_INDEX, player, 1);
            sgTextureGlasses = Function.Call<int>(Hash.GET_PED_PROP_TEXTURE_INDEX, player, 1);
            glassesSet = true;
            noGlasses = false;
            glassesOn = true;
        }

        // Takes Glasses off / Puts Glasses On
        if (!noGlasses)
        {
            glassesOn = !glassesOn;
            PlayAnimation(player, glassesOn ? "clothingspecs" : "clothingspecs", glassesOn ? "put_on" : "take_off");
            if (glassesOn)
            {
                Wait(3500); // Adjust the time as needed
                Function.Call(Hash.SET_PED_PROP_INDEX, player, 1, myGlasses, sgTextureGlasses, 2);
                ShowNotification("Glasses are on");
            }
            else
            {
                Wait(1000); // Adjust the time as needed
                Function.Call(Hash.CLEAR_PED_PROP, player, 1);
                ShowNotification("Glasses are off");
            }
        }
        else
        {
            ShowNotification("You are not wearing Glasses");
        }
    }

    // Function to show the notification
    private void ShowNotification(string text)
    {
        Function.Call(Hash._SET_NOTIFICATION_TEXT_ENTRY, "STRING");
        Function.Call(Hash._ADD_TEXT_COMPONENT_STRING, text);
        Function.Call(Hash._DRAW_NOTIFICATION, false, false);
    }

    // Function to play an animation
    private void PlayAnimation(Entity entity, string animDict, string animName)
    {
        if (!Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, animDict))
        {
            Function.Call(Hash.REQUEST_ANIM_DICT, animDict);
            while (!Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, animDict))
            {
                Wait(100);
            }
        }

        Function.Call(Hash.TASK_PLAY_ANIM, entity.Handle, animDict, animName, 8.0f, -8.0f, -1, 0, 0, false, false, false);
    }
}
