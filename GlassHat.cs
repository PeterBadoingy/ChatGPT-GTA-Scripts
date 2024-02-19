using System;
using GTA;
using GTA.Native;
using GTA.Math;
using System.Windows.Forms;
using System.Threading.Tasks;

public class HatGlassesScript : Script
{
    private const Keys HatToggleKey = Keys.H;
    private const Keys GlassesToggleKey = Keys.G;
    private const int HatPropIndex = 0;
    private const int GlassesPropIndex = 1;

    private bool hatsOn = false;
    private int currentHats = -1;
    private int myHats = -1;
    private int sgTexture = -1;
    private bool hatsSet = false;
    private bool noHats = false;

    private bool glassesOn = false;
    private int currentGlasses = -1;
    private int myGlasses = -1;
    private int sgTextureGlasses = -1;
    private bool glassesSet = false;
    private bool noGlasses = false;

    public HatGlassesScript()
    {
        Tick += OnTick;
        KeyDown += OnKeyUp;
        LoadAnimations();
    }

    private void OnTick(object sender, EventArgs e)
    {
        // Your main logic can go here if needed.
    }

    private void OnKeyUp(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == HatToggleKey)
        {
            HatsEventHandler();
        }
        else if (e.KeyCode == GlassesToggleKey)
        {
            SunglassesEventHandler();
        }
    }

    private void HatsEventHandler()
    {
        Ped player = Game.Player.Character;
        currentHats = Function.Call<int>(Hash.GET_PED_PROP_INDEX, player, HatPropIndex);

        if (currentHats == -1 && !hatsSet)
        {
            noHats = true;
            hatsSet = false;
        }
        else if (currentHats != -1 && !hatsSet)
        {
            myHats = Function.Call<int>(Hash.GET_PED_PROP_INDEX, player, HatPropIndex);
            sgTexture = Function.Call<int>(Hash.GET_PED_PROP_TEXTURE_INDEX, player, HatPropIndex);
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
            myHats = Function.Call<int>(Hash.GET_PED_PROP_INDEX, player, HatPropIndex);
            sgTexture = Function.Call<int>(Hash.GET_PED_PROP_TEXTURE_INDEX, player, HatPropIndex);
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
                Function.Call(Hash.SET_PED_PROP_INDEX, player, HatPropIndex, myHats, sgTexture, 2);
                //ShowNotification("Hat is on");
            }
            else
            {
                Wait(500); // Adjust the time as needed
                Function.Call(Hash.CLEAR_PED_PROP, player, HatPropIndex);
                //ShowNotification("Hat is off");
            }
        }
        else
        {
            //ShowNotification("You are not wearing a Hat");
			hatsOn = false;
        }
    }

    private void SunglassesEventHandler()
    {
        Ped player = Game.Player.Character;
        currentGlasses = Function.Call<int>(Hash.GET_PED_PROP_INDEX, player, GlassesPropIndex);

        if (currentGlasses == -1 && !glassesSet)
        {
            noGlasses = true;
            glassesSet = false;
        }
        else if (currentGlasses != -1 && !glassesSet)
        {
            myGlasses = Function.Call<int>(Hash.GET_PED_PROP_INDEX, player, GlassesPropIndex);
            sgTextureGlasses = Function.Call<int>(Hash.GET_PED_PROP_TEXTURE_INDEX, player, GlassesPropIndex);
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
            myGlasses = Function.Call<int>(Hash.GET_PED_PROP_INDEX, player, GlassesPropIndex);
            sgTextureGlasses = Function.Call<int>(Hash.GET_PED_PROP_TEXTURE_INDEX, player, GlassesPropIndex);
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
                Wait(3200); // Adjust the time as needed
                Function.Call(Hash.SET_PED_PROP_INDEX, player, GlassesPropIndex, myGlasses, sgTextureGlasses, 2);
                //ShowNotification("Glasses are on");
            }
            else
            {
                Wait(1000); // Adjust the time as needed
                Function.Call(Hash.CLEAR_PED_PROP, player, GlassesPropIndex);
                //ShowNotification("Glasses are off");
            }
        }
        else
        {
            //ShowNotification("You are not wearing Glasses");
			glassesOn = false;
        }
    }

    private void LoadAnimations()
    {
        LoadAnimationDictionary("missheistdockssetup1hardhat@");
        LoadAnimationDictionary("missheist_agency2ahelmet");
        LoadAnimationDictionary("clothingspecs");
    }

private async void LoadAnimationDictionary(string dictionary)
{
    if (!Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, dictionary))
    {
        Function.Call(Hash.REQUEST_ANIM_DICT, dictionary);
        await Task.Delay(100); // Asynchronously wait for 100 milliseconds
        while (!Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, dictionary))
        {
            await Task.Delay(100); // Asynchronously wait for 100 milliseconds
        }
    }
}

private void ShowNotification(string text)
{
    GTA.UI.Notification.Show(text);
}

    private void PlayAnimation(Entity entity, string animDict, string animName)
    {
        if (!Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, animDict))
        {
            // Log an error here if the animation dictionary is not loaded.
            return;
        }

        Function.Call(Hash.TASK_PLAY_ANIM, entity.Handle, animDict, animName, 8.0f, -8.0f, -1, 48, 0, false, false, false);
    }
}
