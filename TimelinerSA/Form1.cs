using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net;
using System.Threading;

using Timeliner;
using Posh;
using VVVV.Utils.OSC;

namespace TimeLinerSA
{
    public partial class Form1 : Form, IDisposable
    {
        List<PoshTimeliner> FPoshTimeliners = new List<PoshTimeliner>();
        WebBrowser FWebBrowser;
        Stopwatch Clock = new Stopwatch();
        OSCTransmitter FOSCTransmitter;
        OSCReceiver FOSCReceiver;
        bool FListening = false;
        Thread FOSCListener;
		
        public Form1()
        {
            InitializeComponent();
			
            //register url on http server
            WebServer.TerminalPath = Path.Combine(Path.GetDirectoryName(Assembly.GetCallingAssembly().Location), @"web");
            WebServer.UnknownURL += AddUrl;
            WebServer.OpenURL += OpenUrl;
							
            webBrowser1.Navigate("about:blank");
            webBrowser1.Navigate(new Uri("http://localhost:4444/timeliner"));
            
            OpenTransmitter();
			StartOSCReceiver();
			
            timer1.Interval = 1000 / 30;
            timer1.Tick += timer1_Tick;
            timer1.Start();
            //FWAMPTimeliner.SaveData = xml => FData[0] = xml.ToString();
			
            Clock.Start();
			
            //dispose web- and wampserver
            this.Disposed += (s, e) => 
            {
                WebServer.Stop();
                CloseTransmitter();
                
                foreach (var tl in FPoshTimeliners) 
                {
                    tl.Dispose();
                }
            };
			
        }
        		
        void timer1_Tick(object sender, EventArgs e)
        {
            var hosttime = Clock.ElapsedMilliseconds / 1000f;
            var bundle = new OSCBundle();
            var prefix = "/" + PrefixTextBox.Text.Trim('/');
            
            lock(FPoshTimeliners)
                foreach (var timeliner in FPoshTimeliners)
                {
                    timeliner.Evaluate(hosttime);
                    foreach (var tl in timeliner.Timeliner.TimelineView.Tracks)
                    {
                        var label = (tl as ValueTrackView).Model.Label.Value;
                        var val = (tl as ValueTrackView).Model.CurrentValue;
                        
                        var message = new OSCMessage(prefix + "/" + label);
                        message.Append((float) val);
                        bundle.Append(message);                    
                    }
                }
            
            if (FOSCTransmitter != null)
                FOSCTransmitter.Send(bundle);
        }
		
        private PoshTimeliner AddTimeliner(string url)
        {
            var _url = WebServer.AddURL(url);
            var port = WebServer.URLPort[_url];
            PoshTimeliner timeliner;
            lock(FPoshTimeliners)
            {
                FPoshTimeliners.Add(new PoshTimeliner(_url, port));
                timeliner = FPoshTimeliners.Last(); 
            }
			
            timeliner.Log = x => Console.WriteLine(x);
            return timeliner;
        }
        
        private void AddUrl(string url)
        {
            if (!url.Contains("."))
                AddTimeliner(url);
        }
        
        private void OpenUrl(string url)
        {
            if (!WebServer.URLPort.ContainsKey(url))
            {
                var timeliner = AddTimeliner(url);
                var path = Path.Combine(WebServer.TerminalPath, url) + ".xml";
				var element = XElement.Load(path);
				timeliner.LoadData(element);
            }
        }
        
        void NumericUpDown1ValueChanged(object sender, System.EventArgs e)
        {
            UpdateTransmitter();
        }
        
        void TextBox1TextChanged(object sender, System.EventArgs e)
        {
            UpdateTransmitter();
        }
        
        void ReceivePortNumberBoxValueChanged(object sender, System.EventArgs e)
		{
            UpdateReceiver();
		}
        
        #region OSC
        void UpdateTransmitter()
        {
            CloseTransmitter();
            OpenTransmitter();
        }
        
        void CloseTransmitter()
        {
            if (FOSCTransmitter != null)
            {
                FOSCTransmitter.Close();
                FOSCTransmitter = null;
            }
        }
        
        void OpenTransmitter()
        {
            var ip = TargetIPTextBox.Text.Trim();
            try
            {
                var ipAddress = IPAddress.Parse(TargetIPTextBox.Text.Trim()); 
                
                FOSCTransmitter = new OSCTransmitter(ip, (int) TargetPortNumberBox.Value);
                FOSCTransmitter.Connect();
            }
            catch
            {}
        }
        
        void UpdateReceiver()
        {
            StopOSCReceiver();
            StartOSCReceiver();
        }
        
		void StartOSCReceiver()
		{
            FOSCReceiver = new OSCReceiver((int) ReceivePortNumberBox.Value);
			FOSCListener = new Thread(new ThreadStart(ListenToOSC));
			
            FListening = true;
            FOSCListener.Start();
		}
		
		void StopOSCReceiver()
		{
			FListening = false;
            FOSCListener.Abort();
			if (FOSCReceiver != null)
				FOSCReceiver.Close();

			FOSCReceiver = null;
		}

		void ListenToOSC()
		{
			while(FListening)
			{
				try
				{
					var packet = FOSCReceiver.Receive();
					if (packet!=null)
					{
						if (packet.IsBundle())
						{
							var messages = packet.Values;
							for (int i=0; i<messages.Count; i++)
								ProcessOSCMessage((OSCMessage)messages[i]);
						}
						else
							ProcessOSCMessage((OSCMessage)packet);
					}
				}
				catch (Exception e)
				{
				}
			}
		}
		
		void ProcessOSCMessage(OSCMessage message)
		{
			var address = message.Address;
			var args = message.Values;
			
			char[] s = {'/'};
			string[] path = address.Split(s);
			
            if (path[1] == PrefixTextBox.Text.Trim('/'))
                switch(path[2])
            {
                case "play":
                    {
                        var arg = (int) args[0];
                        FPoshTimeliners[0].Timeliner.Timer.Play(arg == 1);
                        break;
                    }
                case "stop":
                    {
                        FPoshTimeliners[0].Timeliner.Timer.Stop();
                        break;
                    }
                case "seek":
                    {
                        FPoshTimeliners[0].Timeliner.Timer.Time = (float) args[0];
                        break;
                    }
                case  "loop":
                    {
                        FPoshTimeliners[0].Timeliner.Timeline.Ruler.LoopStart.Value = (float) args[0];
                        FPoshTimeliners[0].Timeliner.Timeline.Ruler.LoopEnd.Value = (float) args[1];
                        FPoshTimeliners[0].Timeliner.TimelineView.UpdateScene();
                        break;
                    }
            }
		}
		#endregion OSC
            }
		}
    }
}
