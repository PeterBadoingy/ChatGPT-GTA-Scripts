using System;
using GTA;
using GTA.Native;
using System.Windows.Forms;

public class HatGlassesScript : Script
{
    int currentHatIndex = -1;
    int currentGlassesIndex = -1;
    int currentHatTexture = -1;
    int currentGlassesTexture = -1;

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

        if (currentHatIndex == -1)
        {
            // Player is removing the hat
            currentHatIndex = Function.Call<int>(Hash.GET_PED_PROP_INDEX, player, 0);
            currentHatTexture = Function.Call<int>(Hash.GET_PED_PROP_TEXTURE_INDEX, player, 0);

            PlayAnimation(player, "missheist_agency2ahelmet", "take_off_helmet_stand");
            Wait(500); // Adjust the time as needed

            // Add logic to take off the hat (if needed)
            Function.Call(Hash.CLEAR_PED_PROP, player, 0);
            ShowNotification("Hat removed");
        }
        else
        {
            // Player is putting the hat back on
            PlayAnimation(player, "missheistdockssetup1hardhat@", "put_on_hat");
            Wait(1200); // Adjust the time as needed

            Function.Call(Hash.SET_PED_PROP_INDEX, player, 0, currentHatIndex, currentHatTexture, 2);
            ShowNotification("Hat put back on");
            currentHatIndex = -1;
        }
    }

    private void SunglassesEventHandler()
    {
        Ped player = Game.Player.Character;

        if (currentGlassesIndex == -1)
        {
            // Player is removing the glasses
            currentGlassesIndex = Function.Call<int>(Hash.GET_PED_PROP_INDEX, player, 1);
            currentGlassesTexture = Function.Call<int>(Hash.GET_PED_PROP_TEXTURE_INDEX, player, 1);

            PlayAnimation(player, "clothingspecs", "take_off");
            Wait(1000); // Adjust the time as needed

            // Add logic to take off the glasses (if needed)
            Function.Call(Hash.CLEAR_PED_PROP, player, 1);
            ShowNotification("Glasses removed");
        }
        else
        {
            // Player is putting the glasses back on
            PlayAnimation(player, "clothingspecs", "put_on");
            Wait(3500); // Adjust the time as needed

            Function.Call(Hash.SET_PED_PROP_INDEX, player, 1, currentGlassesIndex, currentGlassesTexture, 2);
            ShowNotification("Glasses put back on");
            currentGlassesIndex = -1;
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
