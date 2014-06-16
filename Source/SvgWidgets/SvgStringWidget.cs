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
		
		public string Caption
		{
			get {return Label.Text;}
			set {Label.Text = value;}
		}
		
		public SvgStringWidget(string name): base(name)
		{
			Label.FontSize = 20;
			Label.X = 2;
			Label.Y = Label.FontSize + 2;
            Label.CustomAttributes["class"] = "labelmenufont";
            Label.Change += Change;
            
            Children.Add(Label);
		}
		
		public SvgStringWidget(string name, float width, float height, string value): this(name)
		{
			Label.Text = value;
			Width = width;
			Height = height;
		}

		void Change(object sender, StringArg e)
		{
			Label.Text = e.s;
			ValueChanged(this, e.s, null);
		}
		
		public override void Dispose()
		{
			Label.Change -= Change;
		}
	}
}
