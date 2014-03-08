using System;
using System.Collections.Generic;
using System.Xml;
using System.Linq;

using Svg;

namespace Timeliner
{
	/// <summary>
	/// Parses a svg document and provides events.
	/// </summary>
	public class SvgTextListWidget: SvgWidget
	{
		private SvgText Edit;
		private List<SvgText> Labels = new List<SvgText>();
		public Action OnValueChanged;
		private List<string> NodeList = new List<string>();
		
		public SvgTextListWidget(string caption): base()
		{
			Edit = new SvgText();
			Edit.FontSize = 12;
			Edit.X = 2;
			Edit.Y = Edit.FontSize + 2;
			Edit.FontFamily = "Lucida Sans Unicode";
			Edit.Change += Edit_Change;
			Edit.Text = caption;
			
			var xml = new XmlDocument();
			xml.Load("nodelist.xml");
			
			var nodes = xml.SelectNodes("/NODELIST/NODE/@systemname");
			foreach (XmlAttribute n in nodes)
				NodeList.Add(n.Value);
			
			this.Children.Add(Edit);
			
			for (int i = 0; i < 10; i++)
			{
				var label = new SvgText();
				Labels.Add(label);
				label.FontSize = 12;
				label.X = 2;
				label.Y = (Edit.FontSize + 2) * (i+2);
				label.FontFamily = "Lucida Sans Unicode";
				label.CustomAttributes["pointer-events"] = "none";

				var s = new SvgTextSpan();
				s.ID = "span1" + i.ToString();
				s.Text = "a";
				label.Children.Add(s);
				
				s = new SvgTextSpan();
				s.ID = "span2" + i.ToString();
				s.Text = "b";
				s.CustomAttributes["class"] = "bold";
				label.Children.Add(s);
				
				s = new SvgTextSpan();
				s.ID = "span3" + i.ToString();
				s.Text = "c";
				s.DX = -4;
				label.Children.Add(s);
				//label.Text = "foo";
				
				this.Children.Add(label);
			}
			//Label.ID ="/label";
		}

		void Edit_Change(object sender, StringArg e)
		{
			var selected = NodeList.Where(s => s.ToLower().Contains(e.s));
			var sorted = selected.ToList();
			
			sorted.Sort((e1, e2) => {
			            	var a = e1.ToLower().IndexOf(e.s);
			            	var b = e2.ToLower().IndexOf(e.s);
			            	if (a==b)
			            		return 0;
			            	else if (a>b)
			            		return 1;
			            	else
			            		return -1;
			            });
			
			for (int i = 0; i < Labels.Count; i++)
			{
				if ((i < sorted.Count()) && (e.s.Length > 0))
				{
					var node = sorted.ElementAt(i);
					var s = (Labels[i].Children[0] as SvgTextSpan);
					s.Text = node.Substring(0, node.ToLower().IndexOf(e.s));
					
					(Labels[i].Children[1] as SvgTextSpan).Text = node.Substring(node.ToLower().IndexOf(e.s), e.s.Length);
					if (s.Text.Length > 0)
						s.DX = -4;
					
					s = (Labels[i].Children[2] as SvgTextSpan);
					s.Text = node.Substring(node.ToLower().IndexOf(e.s) + e.s.Length);
					
				}
				else
				{
					(Labels[i].Children[0] as SvgTextSpan).Text = "";
					(Labels[i].Children[1] as SvgTextSpan).Text = "";
					(Labels[i].Children[2] as SvgTextSpan).Text = "";
				}
			}
			
			//OnValueChanged();
		}
	}
}
