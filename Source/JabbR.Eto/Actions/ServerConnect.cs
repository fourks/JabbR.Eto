using System;
using Eto.Forms;
using JabbR.Eto.Interface;
using JabbR.Eto.Model;

namespace JabbR.Eto.Actions
{
    public class ServerConnect : ButtonAction
    {
        public const string ActionID = "ServerConnect";
        Channels channels;

        public ServerConnect(Channels channels)
        {
            this.channels = channels;
            this.ID = ActionID;
            this.MenuText = "Connect";
        }
        
        public override bool Enabled
        {
            get
            {
                return base.Enabled && channels.SelectedServer != null && !channels.SelectedServer.IsConnected;
            }
            set
            {
                base.Enabled = value;
            }
        }
        
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            var server = channels.SelectedServer;
            if (server != null && !server.IsConnected)
            {
                server.Connect();
            }
        }
    }
}

