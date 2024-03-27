using System;
using System.Collections.Generic;
using GTA;
using GTA.Math;
using GTA.Native;
using GTA.UI;
using System.Windows.Forms;

public class FireStarter : Script
{
    private bool scriptEnabled = false;
    private Dictionary<int, System.Threading.Timer> fireTimers = new Dictionary<int, System.Threading.Timer>();
    private readonly object dictionaryLock = new object();

    public FireStarter()
    {
        Tick += OnTick;
        KeyDown += OnKeyDown;
    }

    private void OnTick(object sender, EventArgs e)
    {
        if (!scriptEnabled)
            return;

        Ped player = Game.Player.Character;

        if (player.IsDead)
            return;

        // Check if the player is being attacked by aggressive NPCs
        List<Ped> aggressiveNPCs = GetAggressiveNPCs();
        foreach (Ped aggressiveNPC in aggressiveNPCs)
        {
            // Player is being attacked by an aggressive NPC, set the NPC on fire!
            TaseNPC(aggressiveNPC);
        }
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.T)
        {
            // Toggle script activation
            scriptEnabled = !scriptEnabled;

            if (scriptEnabled)
            {
                ShowNotification("Wrath of God: Enabled");
            }
            else
            {
                ShowNotification("Wrath of God: Disabled");
            }
        }
    }

    private List<Ped> GetAggressiveNPCs()
    {
        List<Ped> aggressiveNPCs = new List<Ped>();

        // Get the player's position
        Vector3 playerPos = Game.Player.Character.Position;

        // Iterate through nearby peds (NPCs)
        foreach (Ped ped in World.GetNearbyPeds(playerPos, 45.0f))
        {
            // Check if the ped is not the player, is alive, and is aggressive towards the player
            if (ped.Exists() && ped != Game.Player.Character && ped.IsAlive && IsPedAggressive(ped))
            {
                aggressiveNPCs.Add(ped);
            }
        }

        return aggressiveNPCs;
    }

    private bool IsPedAggressive(Ped ped)
    {
        return Function.Call<bool>(Hash.IS_PED_IN_COMBAT, ped.Handle, Game.Player.Character.Handle);
    }

    private void TaseNPC(Ped npc)
    {
        // Apply taser damage to the NPC
        Function.Call(Hash.SET_ENTITY_HEALTH, npc.Handle, 10);
        
        // Apply taser shock effect
        Function.Call(Hash.START_ENTITY_FIRE, npc.Handle);

        // Schedule a task to stop the fire after a duration
        ScheduleFireStop(npc.Handle);
    }

private void ShowNotification(string text)
{
    Notification.PostTicker(text, false);
}

private void ScheduleFireStop(int npcHandle)
{
    lock (dictionaryLock)
    {
        if (!fireTimers.ContainsKey(npcHandle))
        {
            // Create a timer for the NPC to stop the fire after 10 seconds
            System.Threading.Timer timer = null;
            timer = new System.Threading.Timer((state) =>
            {
                try
                {
                    // Check if the NPC is dead
                    if (Function.Call<bool>(Hash.IS_ENTITY_DEAD, npcHandle))
                    {
                        // Stop the fire after the delay if the NPC is dead
                        Function.Call(Hash.STOP_ENTITY_FIRE, npcHandle);

                        lock (dictionaryLock)
                        {
                            if (fireTimers.ContainsKey(npcHandle))
                            {
                                fireTimers[npcHandle].Dispose();
                                fireTimers.Remove(npcHandle);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions
                    ShowNotification("Error in ScheduleFireStop: " + ex.Message);
                }
                finally
                {
                    if (timer != null)
                        timer.Dispose(); // Dispose the timer
                }
            }, null, 10000, System.Threading.Timeout.Infinite); // Set the timer duration to 10 seconds

            // Add the timer to the dictionary with the NPC handle as the key
            fireTimers.Add(npcHandle, timer);
        }
    }
}


}
