using System;
using Svg;

namespace Timeliner
{
	/// <summary>
	/// Parses a svg document and provides events.
	/// </summary>
	public class SvgButtonWidget: SvgWidget
	{
		private SvgText FLabel;
        public string Label
        {
            get { return FLabel.Text;}
            set { FLabel.Text = value;}
        }
        
		public SvgButtonWidget(string label): base(label)
		{
            Background.MouseOver += Background_MouseOver;
			Background.MouseOut += Background_MouseOut;
			Background.MouseDown += Background_MouseDown;
			
			FLabel = new SvgText(label);
			FLabel.FontSize = 12;
			FLabel.X = 2;
			FLabel.Y = FLabel.FontSize + 2;
            FLabel.CustomAttributes["class"] = "menufont";
            
            Children.Add(FLabel);
		}
		
		public SvgButtonWidget(float width, float height, string label): this(label)
		{
			Width = width;
			Height = height;
		}
        
        void Background_MouseOver(object sender, EventArgs e)
		{
		}
		
		void Background_MouseOut(object sender, EventArgs e)
		{
		}
	
		void Background_MouseDown(object sender, EventArgs e)
		{
			ValueChanged(this, true, null);
		}
		
		public override void Dispose()
		{
			Background.MouseOver -= Background_MouseOver;
			Background.MouseOut -= Background_MouseOut;
			Background.MouseDown -= Background_MouseDown;
		}
	}
}
