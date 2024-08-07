using System;
using GTA;
using GTA.Native;
using GTA.Math;
using System.Windows.Forms;
using System.Threading.Tasks;

public class HatGlassesMaskScript : Script
{
    private const Keys HatToggleKey = Keys.H;
    private const Keys GlassesToggleKey = Keys.G;
    private const Keys MaskToggleKey = Keys.M;
    private const int HatPropIndex = 0;
    private const int GlassesPropIndex = 1;
    private const int MaskCompIndex = 1;

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

    private bool maskOn = false;
    private int currentMask = -1;
    private int myMask = -1;
    private bool maskSet = false;
    private bool noMask = false;
    private int maskTextureID = -1; // Variable to store mask texture ID
    private int drawMaskID = -1; // Variable to store mask drawable ID

    public HatGlassesMaskScript()
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
        else if (e.KeyCode == MaskToggleKey)
        {
            MaskEventHandler();
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
            PlayAnimation(player, hatsOn ? "missheistdockssetup1hardhat@" : "missheist_agency2ahelmet",
                          hatsOn ? "put_on_hat" : "take_off_helmet_stand", hatsOn ? 0.15f : 0.7f, hatsOn ? 1000 : 1000); // Adjust animTime and duration as needed
            if (hatsOn)
            {
                Wait(100); // Adjust the time as needed
                Function.Call(Hash.SET_PED_PROP_INDEX, player, HatPropIndex, myHats, sgTexture, 2);
                //ShowNotification("Hat is on");
            }
            else
            {
                Wait(100); // Adjust the time as needed
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
            PlayAnimation(player, glassesOn ? "clothingspecs" : "clothingspecs",
                          glassesOn ? "put_on" : "take_off", glassesOn ? 0.5f : 0.0f, glassesOn ? 1000 : 1000); // Adjust animTime and duration as needed
            if (glassesOn)
            {
                Wait(100); // Adjust the time as needed
                Function.Call(Hash.SET_PED_PROP_INDEX, player, GlassesPropIndex, myGlasses, sgTextureGlasses, 2);
                //ShowNotification("Glasses are on");
            }
            else
            {
                Wait(200); // Adjust the time as needed
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

     // Mask Requires an additional function to Shrink players head features to fit the mask.
     // DoesShopPedApparelHaveRestrictionTag , `SHRINK_HEAD` , HeadBlendData

private void MaskEventHandler()
{
    Ped player = Game.Player.Character;
    currentMask = Function.Call<int>(Hash.GET_NUMBER_OF_PED_DRAWABLE_VARIATIONS, player, MaskCompIndex);

    if (currentMask == -1)
    {
        ShowNotification("You are not wearing a mask");
        return; // Exit the method early if the player doesn't have a mask equipped
    }

    if (currentMask != -1 && !maskSet)
    {
        myMask = currentMask;
        maskTextureID = Function.Call<int>(Hash.GET_PED_TEXTURE_VARIATION, player, MaskCompIndex);
        drawMaskID = Function.Call<int>(Hash.GET_PED_DRAWABLE_VARIATION, player, MaskCompIndex);
        maskSet = true;
        maskOn = true;
        //ShowNotification("Mask Texture ID (Initial): " + maskTextureID);
    }
    else if (currentMask == -1 && maskSet)
    {
        maskOn = false;
        maskSet = false; // Reset mask state if it's removed
    }
    else if (maskSet && currentMask != -1 && myMask != currentMask)
    {
        myMask = currentMask;
        maskTextureID = Function.Call<int>(Hash.GET_PED_TEXTURE_VARIATION, player, MaskCompIndex);
        drawMaskID = Function.Call<int>(Hash.GET_PED_DRAWABLE_VARIATION, player, MaskCompIndex);
        maskSet = false;
        maskOn = true;
        //ShowNotification("Mask Texture ID (Changed): " + maskTextureID);
    }

    // Toggle mask on/off
    maskOn = !maskOn;

    if (!noMask)
    {
        if (maskOn)
        {
            // Play animation for putting on the mask
            PlayAnimation(player, "mp_masks@on_foot", "put_on_mask", 0.3f, 1000); // Adjust animTime and duration as needed

            // Set the mask on (assuming DrawableID: 51)
            Function.Call(Hash.SET_PED_COMPONENT_VARIATION, player, MaskCompIndex, drawMaskID, maskTextureID, 0);
            //ShowNotification("Mask is on. Texture ID: " + maskTextureID);
        }
        else
        {
            // Play animation for taking off the mask
            PlayAnimation(player, "missfbi4", "takeoff_mask", 0.7f, 1000); // Adjust animTime and duration as needed

            // Remove the mask
            Function.Call(Hash.SET_PED_COMPONENT_VARIATION, player, MaskCompIndex, -1, 0, 0);
            //ShowNotification("Mask is off");
        }
    }
    else
    {
        //ShowNotification("You are not wearing a mask");
        maskOn = false;
    }
}


    private void ShowNotification(string text)
    {
        GTA.UI.Notification.Show(text);
    }

    private void LoadAnimations()
    {
        LoadAnimationDictionary("missheistdockssetup1hardhat@");
        LoadAnimationDictionary("missheist_agency2ahelmet");
        LoadAnimationDictionary("clothingspecs");
        LoadAnimationDictionary("mp_masks@on_foot");
        LoadAnimationDictionary("missfbi4");
    }

    public void LoadAnimationDictionary(string dictionary)
    {
        if (!Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, dictionary))
        {
            Function.Call(Hash.REQUEST_ANIM_DICT, dictionary);
            //Wait(100); // Asynchronously wait for 100 milliseconds
            //while (!Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, dictionary))
            {
                //Wait(100); // Asynchronously wait for 100 milliseconds
            }
        }
    }

    private void PlayAnimation(Entity entity, string animDict, string animName, float animTime, int duration)
    {
        if (!Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, animDict))
        {
            // Log an error here if the animation dictionary is not loaded.
            return;
        }

        Function.Call(Hash.TASK_PLAY_ANIM_ADVANCED, entity.Handle, animDict, animName,
                      entity.Position.X, entity.Position.Y, entity.Position.Z,
                      0.0f, 0.0f, entity.Heading, 1.0f, 1.0f, duration, 49, animTime, 0, 0);
        Wait(duration); // Wait for the specified duration before continuing
    }
}

