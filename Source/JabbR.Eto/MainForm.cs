using System;
using Eto.Forms;
using Eto.Drawing;
using JabbR.Eto.Interface.Dialogs;
using JabbR.Client;
using Eto;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using JabbR.Eto.Model;
using Newtonsoft.Json;
using JabbR.Eto.Interface;
using System.Collections.ObjectModel;

namespace JabbR.Eto
{
	public class MainForm : Form, IXmlReadable
	{
		TopSection top;
		Configuration config;
		
		public MainForm (Configuration config)
		{
			this.config = config;
			this.Title = "JabbR.Eto";
			this.ClientSize = new Size (800, 600);
			this.MinimumSize = new Size (640, 400);
			this.Style = "mainForm";
			top = new TopSection (config);
			CreateActions ();
			this.AddDockedControl (top);
			HandleEvent (ShownEvent);
		}
		
		void CreateActions ()
		{
			var args = new GenerateActionArgs ();
			
			Application.Instance.GetSystemActions (args, true);
			
			//top.GetActions(args);
			
			args.Actions.Add (new Actions.AddServer ());
			args.Actions.Add (new Actions.EditServer (top.Channels, config));
			args.Actions.Add (new Actions.RemoveServer (top.Channels, config));
			args.Actions.Add (new Actions.ServerConnect (top.Channels, config));
			args.Actions.Add (new Actions.ServerDisconnect (top.Channels, config));
			args.Actions.Add (new Actions.ChannelList (top.Channels));
			args.Actions.Add (new Actions.Quit ());
			args.Actions.Add (new Actions.About ());
			
			var file = args.Menu.FindAddSubMenu ("&File", 100);
			var help = args.Menu.FindAddSubMenu ("&Help", 900);
			var server = args.Menu.FindAddSubMenu ("&Server", 500);
			

			server.Actions.Add (Actions.ServerConnect.ActionID);
			server.Actions.Add (Actions.ServerDisconnect.ActionID);
			server.Actions.AddSeparator ();
			server.Actions.Add (Actions.AddServer.ActionID);
			server.Actions.Add (Actions.EditServer.ActionID);
			server.Actions.Add (Actions.RemoveServer.ActionID);
			server.Actions.AddSeparator ();
			server.Actions.Add (Actions.ChannelList.ActionID);
			
			if (Generator.ID == "mac") {
				var application = args.Menu.FindAddSubMenu (Application.Instance.Name, 100);
				application.Actions.Add (Actions.About.ActionID, 100);
				application.Actions.Add (Actions.Quit.ActionID, 900);
			} else {
				file.Actions.Add (Actions.Quit.ActionID, 900);
				
				help.Actions.Add (Actions.About.ActionID, 100);
			}
			
			top.CreateActions(args);
			
			this.Menu = args.Menu.GenerateMenuBar ();
		}
		
		public void Initialize ()
		{
			top.Initialize();
		}

		#region IXmlReadable implementation
		
		public void ReadXml (XmlElement element)
		{
			this.ClientSize = element.ReadChildSizeXml ("clientSize") ?? new Size (800, 600);
			element.ReadChildXml ("top", top);
		}

		public void WriteXml (XmlElement element)
		{
			element.WriteChildXml ("clientSize", this.ClientSize);
			element.WriteChildXml ("top", top);
			
		}
		
		#endregion
	}
}
