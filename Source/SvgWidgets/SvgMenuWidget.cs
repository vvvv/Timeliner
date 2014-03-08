using System;
using System.Collections.Generic;
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
		private List<SvgGroup> MenuItems;
		
		public SvgMenuWidget(float width): base()
		{
			FWidth = width;
			MenuItems = new List<SvgGroup>();
			
			this.Transforms = new SvgTransformCollection();
			this.Transforms.Add(new SvgTranslate(0, 0));
			this.Visible = false;
		}
		
		public void AddItem(SvgWidget item)
		{
			if (item is SvgButtonWidget)
				(item as SvgButtonWidget).OnButtonPressed += item_OnButtonPressed;
			item.Transforms = new SvgTransformCollection();
			item.Transforms.Add(new SvgTranslate(0, MenuItems.Count * CLineHeight));
			item.Width = FWidth;
			item.Height = CLineHeight;
			                    
			MenuItems.Add(item);
			this.Children.Add(item);
		}

		void item_OnButtonPressed()
		{
			Hide();
		}
		
		public void Hide()
		{
			this.Visible = false;
		}
	}
}
