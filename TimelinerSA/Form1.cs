using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Diagnostics;

using Timeliner;
using Posh;

namespace TimeLinerSA
{
    public partial class Form1 : Form, IDisposable
    {
    	private List<WAMPTimeliner> FWAMPTimeliners = new List<WAMPTimeliner>();
		private WebBrowser FWebBrowser;
		private Stopwatch Clock = new Stopwatch();
		
        public Form1()
        {
            InitializeComponent();

            //register url on http server
            WebServer.TerminalPath = Path.Combine(Path.GetDirectoryName(Assembly.GetCallingAssembly().Location), @"web");
			WebServer.UnknownURL += AddUrl;
			WebServer.OpenURL += OpenUrl;
							
			webBrowser1.Navigate("about:blank");
			webBrowser1.Navigate(new Uri("http://localhost:4444/timeliner"));
			
			timer1.Interval = 1000 / 30;
			timer1.Tick += timer1_Tick;
			timer1.Start();
			//FWAMPTimeliner.SaveData = xml => FData[0] = xml.ToString();
			
			Clock.Start();
			
			//dispose web- and wampserver
			this.Disposed += (s, e) => 
				{
					WebServer.Stop();
					foreach (var tl in FWAMPTimeliners) 
					{
						tl.Dispose();
					}
				};
			
        }

        void timer1_Tick(object sender, EventArgs e)
        {
        	var hosttime = Clock.ElapsedMilliseconds / 1000f;
        	
        	foreach (var timeliner in FWAMPTimeliners)
        		timeliner.Evaluate(hosttime);
        }
        
        private void AddUrl(string url)
        {
        	var _url = WebServer.AddURL(url);
			var port = WebServer.URLPort[_url];
			FWAMPTimeliners.Add(new WAMPTimeliner(_url, port));
			FWAMPTimeliners.Last().Log = x => Console.WriteLine(x); //FLogger.Log(LogType.Debug, x);
        }
        
        private void OpenUrl(string url)
        {
        	if (! WebServer.URLPort.ContainsKey(url))
        	{
        		var _url = WebServer.AddURL(url);
				var port = WebServer.URLPort[_url];
				FWAMPTimeliners.Add(new WAMPTimeliner(_url, port));
				
				FWAMPTimeliners.Last().Log = x => Console.WriteLine(x); //FLogger.Log(LogType.Debug, x);
				var path = Path.Combine(WebServer.TerminalPath, url) + ".xml";
				var element = XElement.Load(path);
				FWAMPTimeliners.Last().LoadData(element);
        	}
        }
    }
}
