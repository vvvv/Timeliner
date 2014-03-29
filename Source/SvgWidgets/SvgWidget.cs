using System;

using Svg;

namespace Timeliner
{
	public class SvgWidget: SvgGroup
	{
		protected SvgRectangle Background;
		
		public float Width
		{
			get {return Background.Width;}
			set {Background.Width = value;}
		}
		
		public float Height
		{
			get {return Background.Height;}
			set {Background.Height = value;}
		}
		
		public SvgWidget()
		{
			Background = new SvgRectangle();
			Background.CustomAttributes["class"] = "menu";
			
			this.Children.Add(Background);
            this.Visible = true;
		}
		
		public SvgWidget(float width, float height): this()
		{
			Background.Width = width;
			Background.Height = height;
		}
	}
}
