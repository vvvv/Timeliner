using System;
using Svg;

namespace Timeliner
{
	/// <summary>
	/// Parses a svg document and provides events.
	/// </summary>
	public class SvgButtonWidget: SvgWidget
	{
		private SvgText Label;
		public Action OnButtonPressed;		
		
		public SvgButtonWidget(string label): base()
		{
            Background.MouseOver += Background_MouseOver;
			Background.MouseOut += Background_MouseOut;
			Background.MouseDown += Background_MouseDown;
			
			Label = new SvgText(label);
			Label.FontSize = 12;
			Label.X = 2;
			Label.Y = Label.FontSize + 2;
            Label.FontFamily = "Lucida Console";
            //Label.ID ="/label";
            Label.CustomAttributes["pointer-events"] = "none";
            Label.CustomAttributes["class"] = "front";
            
            this.Children.Add(Label);
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
			OnButtonPressed.Invoke();
		}
	}
}
