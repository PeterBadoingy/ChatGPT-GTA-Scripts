using System;
using GTA;
using GTA.Math;
using GTA.Native;
using System.Windows.Forms;

public class GodsWrath : Script
{
    private bool scriptEnabled = true;

    public GodsWrath()
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

        // Check if the player is being attacked by an aggressive NPC
        Ped aggressiveNPC = GetAggressiveNPC();
        if (aggressiveNPC != null)
        {
            // Player is being attacked by an aggressive NPC, tase the NPC!
            Judgement(aggressiveNPC);
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
                UI.ShowSubtitle("Wrath of God: Enabled");
            }
            else
            {
                UI.ShowSubtitle("Wrath of God: Disabled");
            }
        }
    }

    private Ped GetAggressiveNPC()
    {
        // Get the player's position
        Vector3 playerPos = Game.Player.Character.Position;

        // Iterate through nearby peds (NPCs)
        foreach (Ped ped in World.GetNearbyPeds(playerPos, 65.0f))
        {
            // Check if the ped is not the player, is alive, and is aggressive towards the player
            if (ped.Exists() && ped != Game.Player.Character && ped.IsAlive && IsPedAggressive(ped))
            {
                return ped;
            }
        }

        return null;
    }

    private bool IsPedAggressive(Ped ped)
    {
        return Function.Call<bool>(Hash.IS_PED_IN_COMBAT, ped.Handle, Game.Player.Character.Handle);
    }

    private void Judgement(Ped npc)
    {
        // Apply damage to the NPC
        Function.Call(Hash.SET_ENTITY_HEALTH, npc.Handle, 0);

        // Apply fire effect
        Function.Call(Hash.START_ENTITY_FIRE, npc.Handle);

        // Play animation
        Function.Call(Hash.TASK_PLAY_ANIM, npc.Handle, "reaction@shove", "shoved_back", 8.0f, -8.0f, -1, 0, 0, false, false, false);

        Wait(2000);
    }
}
