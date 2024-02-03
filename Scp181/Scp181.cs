using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandSystem;
using HarmonyLib;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using MEC;
using PlayerRoles;
using PlayerStatsSystem;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Events;
using Respawning.NamingRules;

namespace Scp181
{
    public class Scp181
    {
        public static List<string> userids = new List<string>();
        public static Scp181 instance;
        public Harmony Harmony { get; private set; }
        public static List<ItemType> cards = new List<ItemType> 
        {
            ItemType.KeycardResearchCoordinator,
            ItemType.KeycardFacilityManager,
            ItemType.KeycardGuard,
            ItemType.KeycardMTFOperative,
        };
        [PluginEntryPoint("Scp181","1.0.0","Scp181","Silver Wolf")]
        public void OnEnabled()
        {
            Log.Info("Plugin Loaded");
            Harmony = new Harmony("com.SilverWolf.Scp181");
            Harmony.PatchAll();
            EventManager.RegisterEvents<Scp181>(this);
            instance = this;
        }
        [PluginEvent(PluginAPI.Enums.ServerEventType.RoundStart)]
        public void OnRoundStart(RoundStartEvent ev)
        {
            Timing.CallDelayed(2, () => { Init181(Player.GetPlayers().FindAll(p => p.Role == PlayerRoles.RoleTypeId.ClassD).RandomItem().UserId); } );
        }
        [PluginEvent(PluginAPI.Enums.ServerEventType.PlayerInteractDoor)]
        public bool OnDoorInteracting(PlayerInteractDoorEvent ev)
        {
            if (ev.Door.ActiveLocks > 0) return true;
            if (userids.Contains(ev.Player.UserId))
            {
                Random random = new Random();
                int rnd = random.Next(1, 101);
                if (rnd < 31 || ev.CanOpen)
                {
                    DoorVariant door = ev.Door;
                    door.NetworkTargetState = !door.TargetState;
                    if (door.NetworkTargetState)
                    {
                        DoorEvents.TriggerAction(door, DoorAction.Opened, ev.Player.ReferenceHub);
                    }
                    else
                    {
                        DoorEvents.TriggerAction(door, DoorAction.Closed, ev.Player.ReferenceHub);
                    }
                    return false;
                }
            }
            return true;
        }
        [PluginEvent(PluginAPI.Enums.ServerEventType.PlayerDeath)]
        public void OnDeath(PlayerDeathEvent ev)
        {
            if (userids.Contains(ev.Player.UserId))
            {
                userids.Remove(ev.Player.UserId);
                ev.Player.CustomInfo = "";
                ev.Player.ReferenceHub.serverRoles.SetText("");
                string text = "SCP 1 8 1 ";
                DamageHandlerBase db = ev.DamageHandler;
                if (db is WarheadDamageHandler)
                {
                    text += " SUCCESSFULLY TERMINATED BY ALPHA WARHEAD";
                }
                else if (db is UniversalDamageHandler handler)
                {
                    text += $" {handler.CassieDeathAnnouncement.Announcement}";
                }
                else
                {
                    if (ev.Attacker == null)
                    {
                        text += " SUCCESSFULLY TERMINATED . TERMINATION CAUSE UNSPECIFIED";
                    }
                    else
                    {
                        string unitName;
                        if (ev.Attacker.RoleBase is HumanRole humanRole)
                        {
                            unitName = UnitNameMessageHandler.GetReceived(humanRole.AssignedSpawnableTeam, humanRole.UnitNameId);
                            text = text + " CONTAINEDSUCCESSFULLY " + Cassie.ConvertTeam(ev.Attacker.Team, unitName);
                        }
                        text += " SUCCESSFULLY TERMINATED . TERMINATION CAUSE UNSPECIFIED";
                    }
                }
                float num = ((AlphaWarheadController.TimeUntilDetonation <= 0f) ? 3.5f : 1f);
                Cassie.GlitchyMessage(text, UnityEngine.Random.Range(0.1f, 0.14f) * num, UnityEngine.Random.Range(0.07f, 0.08f) * num);
            }
        }
        public void Init181(string userid)
        {
            Player player = Player.Get(userid);
            userids.Add(userid);
            player.SendBroadcast("<size=24>你是181</size>", 5, shouldClearPrevious: true);
            player.AddItem(cards.RandomItem());
            player.Health = 120;
            player.CustomInfo = "SCP181";
            player.ReferenceHub.serverRoles.SetText("SCP181");
            player.ReferenceHub.serverRoles.SetColor("yellow");
        }
        
    }
    [HarmonyPatch]
    public class Patch
    {
        [HarmonyPatch(typeof(HealthStat), nameof(HealthStat.MaxValue), MethodType.Getter)]
        public static void Postfix(HealthStat __instance, ref float __result)
        {
            if (Scp181.userids.Contains(__instance.Hub.authManager.UserId))
            {
                __result += 20;
            }
        }
    }
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class Init181 : ICommand
    {
        public string Command => "init181";

        public string[] Aliases => Array.Empty<string>();

        public string Description => "";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Scp181.instance.Init181(Player.Get((sender as CommandSender).SenderId).UserId);
            response = $"Done!";
            return true;
        }
    }
}
