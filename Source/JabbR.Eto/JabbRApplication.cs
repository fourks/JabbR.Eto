using System;
using Eto.Forms;
using Eto;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using JabbR.Eto.Model;
using System.Net;
using System.Diagnostics;
using System.Linq;
using System.Xml;

namespace JabbR.Eto
{
    public interface IJabbRApplication : IApplication
    {
        void SendNotification(string text);
        
        string EncryptString(string serverName, string accountName, string password);

        string DecryptString(string serverName, string accountName, string value);
    }
    
    public class JabbRApplication : Application, IXmlReadable
    {
        new IJabbRApplication Handler { get { return (IJabbRApplication)base.Handler; } }

        XmlElement interfaceElement;

        public Configuration Configuration { get; private set; }

        public static new JabbRApplication Instance
        {
            get { return Application.Instance as JabbRApplication; }
        }

        static JabbRApplication()
        {
            AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;
            ServicePointManager.DefaultConnectionLimit = 100; // needed for windows
        }

        static void HandleUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (Generator.HasCurrent)
                MessageBox.Show(string.Format("An unexpected error has occurred. Please report this to support@picoe.ca\n\nDetails: {0}", e.ExceptionObject));
        }

        public JabbRApplication()
            : base(Generator.Detect, typeof(IJabbRApplication))
        {
            this.Style = "application";
            this.Name = "JabbReto";
            this.Configuration = new JabbR.Eto.Model.Configuration();
            HandleEvent(TerminatingEvent);
        }

        string SettingsFileName
        {
            get
            { 
                var path = EtoEnvironment.GetFolderPath(EtoSpecialFolder.ApplicationSettings);
                return Path.Combine(path, "settings.xml");
            }
        }

        void LoadSettings()
        {
            if (File.Exists(SettingsFileName))
            {
                //JsonConvert.PopulateObject (File.ReadAllText(SettingsFileName), form);
                try
                {
                    this.LoadXml(SettingsFileName);
                }
                catch (Exception ex)
                {
                    // don't worry about not loading
                    Debug.WriteLine("Error loading settings: {0}", ex);
                }
            }
        }
        
        public override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            LoadSettings();
            var form = new MainForm(Configuration);
            this.MainForm = form;
            if (interfaceElement != null)
            {
                form.ReadXml(interfaceElement);
                interfaceElement = null;
            }
            
            form.Initialize();
            this.BadgeLabel = null;
            this.MainForm.Show();
            
            if (!Configuration.Servers.Any())
            {
                Application.Instance.AsyncInvoke(delegate
                {
                    var action = new Actions.AddServer {
                        AutoConnect = true
                    };
                    action.Activate();
                });
            }
            else
            {
                foreach (var server in Configuration.Servers)
                {
                    if (server.ConnectOnStartup)
                        server.Connect();
                }
            }
        }

        bool disconnecting;
        
        public override void OnTerminating(System.ComponentModel.CancelEventArgs e)
        {
            if (!disconnecting)
            {
                disconnecting = true;
                SaveConfiguration();
                Configuration.DisconnectAll(() => {
                    Application.Instance.AsyncInvoke(delegate
                    {
                        this.Quit();
                    });
                });
                e.Cancel = true;
            }
            base.OnTerminating(e);
        }
        
        public void SaveConfiguration()
        {
            Debug.Print("Saving settings to {0}", SettingsFileName);
            this.SaveXml(SettingsFileName, "jabbreto");
        }
        
        public string EncryptString(string serverName, string accountName, string password)
        {
            return Handler.EncryptString(serverName, accountName, password);
        }
        
        public string DecryptString(string serverName, string accountName, string value)
        {
            return Handler.DecryptString(serverName, accountName, value);
        }
        
        public void SendNotification(string text)
        {
            Handler.SendNotification(text);
        }
        

        #region IXmlReadable implementation
        public void ReadXml(System.Xml.XmlElement element)
        {
            if (this.MainForm != null)
                element.ReadChildXml("interface", this.MainForm as IXmlReadable);
            else
                interfaceElement = element.SelectSingleNode("interface") as XmlElement;
            element.ReadChildXml("config", Configuration);
        }

        public void WriteXml(System.Xml.XmlElement element)
        {
            element.WriteChildXml("interface", this.MainForm as IXmlReadable);
            element.WriteChildXml("config", Configuration);
        }
        #endregion
    }
}

