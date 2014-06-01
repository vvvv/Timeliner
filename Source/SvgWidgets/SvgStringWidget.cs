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
            Label.Text = label;
			Label.FontSize = 20;
			Label.X = 2;
			Label.Y = Label.FontSize + 2;
            Label.CustomAttributes["class"] = "labelmenufont";
            Label.Change += Change;
            
            Children.Add(Label);
		}
		
		public SvgStringWidget(float width, float height, string label): this(label)
		{
			Width = width;
			Height = height;
		}

		void Change(object sender, StringArg e)
		{
			Label.Text = e.s;
            OnValueChanged(e.s);
		}
		
		public override void Dispose()
		{
			Label.Change -= Change;
		}
	}
}
