using System;
using System.Drawing;

using Svg;

namespace Timeliner
{
	/// <summary>
	/// Parses a svg document and provides events.
	/// </summary>
	public class SvgStringWidget: SvgWidget
	{
        private SvgText Label = new SvgText();
		public Action<string> OnValueChanged;	
		
		public string Caption
		{
			get; set;
		}
		
		public SvgStringWidget(string label): base()
		{
			Background.CustomAttributes["class"] = "menu";
			
            Label.Text = label;
            Label.ID = "wer";
			Label.FontSize = 20;
			Label.X = 2;
			Label.Y = Label.FontSize + 2;
            Label.FontFamily = "Lucida Console";
            Label.CustomAttributes["class"] = "";
            Label.Change += Change;
            
            this.Children.Add(Label);
		}

		void Change(object sender, StringArg e)
		{
			Label.Text = e.s;
            OnValueChanged(e.s);
		}
	}
}
