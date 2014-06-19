using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using Svg;
using Svg.Transforms;

namespace Timeliner
{
	/// <summary>
	/// Parses a svg document and provides events.
	/// </summary>
	public class SvgMenuWidget: SvgWidget
	{
		float FWidth;
		float FHeight = 0;
		Dictionary<string, SvgWidget> MenuEntries = new Dictionary<string, SvgWidget>();
		
		public SvgMenuWidget(float width): base("")
		{
			FWidth = width;
			
			Transforms = new SvgTransformCollection();
			Transforms.Add(new SvgTranslate(0, 0));
			Visible = false;
		}
		
		public void AddItem(SvgWidget item, int order)
		{
			if (item is SvgButtonWidget)
				(item as SvgButtonWidget).ValueChanged += item_OnButtonPressed;
			item.Transforms = new SvgTransformCollection();
			item.Transforms.Add(new SvgTranslate(0, 0));
			item.Width = FWidth;
			
			MenuEntries.Add(item.Name, item);

			order = Math.Min(Children.Count-1, order);
			this.Children.Insert(order, item);

			//update entry positions
			FHeight = 0;
			foreach (var entry in Children.Where(x => x is SvgWidget))
			{
				entry.Transforms[0] = new SvgTranslate(0, FHeight);
				FHeight += (entry as SvgWidget).Height;
			}
		}
		
		public SvgWidget GetItem(string name)
		{
			return MenuEntries[name];
		}

		void item_OnButtonPressed(SvgWidget widget, object newValue, object delta)
		{
			Hide();
		}
		
		public void Show(PointF point)
		{
			Transforms[0] = new SvgTranslate(point.X - FWidth/2f, Math.Max(0, point.Y - FHeight/2f));
			Visible = true;
		}
		
		public void Hide()
		{
			Visible = false;
		}
		
		public override void Dispose()
		{
		}
	}
}
