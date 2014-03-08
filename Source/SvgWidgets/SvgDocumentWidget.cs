using System;
using System.Collections.Generic;
using Svg;

namespace Timeliner
{
	/// <summary>
	/// Parses a svg document and provides events.
	/// </summary>
	public class SvgDocumentWidget : SvgDocument
	{
		protected List<SvgVisualElement> BackgroundElements = new List<SvgVisualElement>();
		protected List<SvgVisualElement> ForegroundElements = new List<SvgVisualElement>();
		
		public void SetBackColor(SvgColourServer col)
		{
			foreach (var element in BackgroundElements)
			{
				element.Fill = col;
			}
		}

		public void SetForeColor(SvgColourServer col)
		{
			foreach (var element in ForegroundElements)
			{
				element.Fill = col;
			}
		}
		
		public void ParseElements(ISvgEventCaller caller, SvgElement element)
		{
			//register events
			if(element.HasNonEmptyCustomAttribute("onmousedown"))
			{
				element.RegisterEvents(caller);
				element.MouseDown += child_MouseDown;
			}
			
			//gather color relevant elements
			if(element.HasNonEmptyCustomAttribute("class"))
			{
				if(element.CustomAttributes["class"] == "widgetback")
					if(element is SvgVisualElement)
						BackgroundElements.Add(element as SvgVisualElement);
				else if(element.CustomAttributes["class"] == "widgetfore")
					if(element is SvgVisualElement)
						ForegroundElements.Add(element as SvgVisualElement);
			}
			
			foreach (var child in element.Children)
			{
				ParseElements(caller, child);
			}
		}

		void child_MouseDown(object sender, MouseArg e)
		{
			RaiseMouseDown(this, e);
		}
		
		public static SvgDocumentWidget Load(string filePath, ISvgEventCaller caller)
		{
			var newWidget = SvgDocument.Open<SvgDocumentWidget>(filePath);
			newWidget.AutoPublishEvents = false;
			newWidget.ParseElements(caller, newWidget);
			return newWidget;
		}
		
	}

}
