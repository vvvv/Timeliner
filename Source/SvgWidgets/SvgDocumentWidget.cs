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
		
		public void SetViewBox(int view)
		{
            ViewBox = new SvgViewBox(0, Height * view, Width, Height);
		}

		public void ParseElements(ISvgEventCaller caller, SvgElement element)
		{
			//register events
			if(element.HasNonEmptyCustomAttribute("onclick"))
			{
				element.RegisterEvents(caller);
				element.Click += child_Click;
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
		
		public void UnregisterEvents(ISvgEventCaller caller)
		{
			UnregisterElementEvents(caller, this);
		}
		
		void UnregisterElementEvents(ISvgEventCaller caller, SvgElement element)
		{
			element.UnregisterEvents(caller);
			
			foreach (var child in element.Children)
			{
				UnregisterElementEvents(caller, child);
			}
		}

		void child_Click(object sender, MouseArg e)
		{
			RaiseMouseClick(this, e);
		}
		
		public static SvgDocumentWidget Load(string filePath, ISvgEventCaller caller, int viewCount)
		{
			var newWidget = SvgDocument.Open<SvgDocumentWidget>(filePath);
			
			newWidget.AutoPublishEvents = false;
			newWidget.ParseElements(caller, newWidget);
            
            newWidget.Height = newWidget.Height / viewCount;
            newWidget.SetViewBox(0);
            newWidget.Overflow = SvgOverflow.hidden;
            
			return newWidget;
		}
		
	}

}
