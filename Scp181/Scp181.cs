using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MEC;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Events;

namespace Scp181
{
    public class Scp181
    {
        public string userid;
        [PluginEntryPoint("Scp181","1.0.0","Scp181","SIlver Wolf")]
        public void OnEnabled()
        {
            Log.Info("Plugin Loaded");
        }
        [PluginEvent(PluginAPI.Enums.ServerEventType.RoundStart)]
        public void OnRoundStart(RoundStartEvent ev)
        {
            Timing.CallDelayed(2, () => { userid = Player.GetPlayers().RandomItem().UserId; Init181(); } );
        }
        public void Init181()
        {
            Player player = Player.Get(userid);
            player.SendBroadcast("<size=24>你是181</size>", 5, shouldClearPrevious: true);

        }
    }
}
