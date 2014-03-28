using System;

using Svg;

namespace Timeliner
{
	public class SvgWidget: SvgGroup
	{
		protected SvgRectangle Background;
		
		public float Width
		{
			set 
			{ 
				Background.Width = value;
			}
		}
		
		public float Height
		{
			set 
			{ 
				Background.Height = value;
			}
		}
		
		public SvgWidget()
		{
			Background = new SvgRectangle();
			Background.CustomAttributes["class"] = "wback";
			
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
