using System;
using System.Collections.Generic;
using System.Drawing;
using Svg;
using Svg.Transforms;

namespace Timeliner
{
	/// <summary>
	/// Parses a svg document and provides events.
	/// </summary>
	public class SvgMenuWidget: SvgWidget
	{
		private const float CLineHeight = 20;
		private float FWidth;
        private float FHeight = 0;
		private List<SvgGroup> MenuItems;
		
		public SvgMenuWidget(float width): base()
		{
			FWidth = width;
			MenuItems = new List<SvgGroup>();
			
			Transforms = new SvgTransformCollection();
			Transforms.Add(new SvgTranslate(0, 0));
			Visible = false;
		}
		
		public void AddItem(SvgWidget item)
		{
			if (item is SvgButtonWidget)
				(item as SvgButtonWidget).OnButtonPressed += item_OnButtonPressed;
			item.Transforms = new SvgTransformCollection();
			item.Transforms.Add(new SvgTranslate(0, MenuItems.Count * CLineHeight));
			item.Width = FWidth;
			item.Height = CLineHeight;
            
            FHeight += CLineHeight;
			                    
			MenuItems.Add(item);
			this.Children.Add(item);
		}

		void item_OnButtonPressed()
		{
			Hide();
		}
        
        public void Show(PointF point)
        {
            Transforms[0] = new SvgTranslate(point.X - FWidth/2f, point.Y - FHeight/2f);
            Visible = true;
        }
		
		public void Hide()
		{
			Visible = false;
		}
	}
}
