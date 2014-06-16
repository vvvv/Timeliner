using System;

using Svg;

namespace Timeliner
{
	public abstract class SvgWidget: SvgGroup, IDisposable
	{
		public Action<SvgWidget, object, object> ValueChanged { get; set; }
		public string Name { get; set; }
		
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
		
		public SvgWidget(string name)
		{
			Name = name;
			Background = new SvgRectangle();
			Background.CustomAttributes["class"] = "menu";
			
			this.Children.Add(Background);
            this.Visible = true;
		}
		
		public SvgWidget(string name, float width, float height): this(name)
		{
			Background.Width = width;
			Background.Height = height;
		}
		
		public abstract void Dispose();
	}
}
