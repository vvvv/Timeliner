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
		private List<PoshTimeliner> FPoshTimeliners = new List<PoshTimeliner>();
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
				foreach (var tl in FPoshTimeliners) 
				{
					tl.Dispose();
				}
			};
			
		}
		
		void timer1_Tick(object sender, EventArgs e)
		{
			var hosttime = Clock.ElapsedMilliseconds / 1000f;
        	
			lock(FPoshTimeliners)
				foreach (var timeliner in FPoshTimeliners)
					timeliner.Evaluate(hosttime);
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
	}
}
