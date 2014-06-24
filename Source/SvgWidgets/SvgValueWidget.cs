using System;
using System.Drawing;

using Svg;

namespace Timeliner
{
	/// <summary>
	/// Parses a svg document and provides events.
	/// </summary>
	public class SvgValueWidget: SvgWidget
	{
		SvgText Label = new SvgText();
		SvgText ValueLabel = new SvgText();
		bool FMouseDown = false;
		PointF FMouseDownPos;
		PointF FLastMousePos;
		
		float FValue;
		public float Value
		{
			get {return FValue;}
			set
			{
				FValue = value;
				UpdateScene();
			}
		}
		
		 string FCaption;
		public string Caption
		{
			get {return FCaption;}
			set
			{
				FCaption = value;
				UpdateScene();
			}
		}
		
		public SvgValueWidget(string name, float value): base(name)
		{
			Caption = name;
			Value = value;
			
			Background.MouseScroll += Background_MouseScroll;
            //over/out need to be registered for scrolling to work
			Background.MouseOver += Background_MouseOver;
			Background.MouseOut += Background_MouseOut;
			
			Label.FontSize = 12;
			Label.X = 2;
			Label.Y = Label.FontSize + 2;
            Label.CustomAttributes["class"] = "menufont";
            
            ValueLabel.FontSize = 12;
			ValueLabel.X = 80;
			ValueLabel.Y = ValueLabel.FontSize + 2;
            ValueLabel.CustomAttributes["class"] = "labelmenufont";
            ValueLabel.Change += ValueLabel_Change;
            ValueLabel.MouseScroll += Background_MouseScroll;
            //over/out need to be registered for scrolling to work
			ValueLabel.MouseOver += Background_MouseOver;
			ValueLabel.MouseOut += Background_MouseOut;
            
            Children.Add(Label);
            Children.Add(ValueLabel);
            
            UpdateScene();
		}

		public SvgValueWidget(string name, float width, float height, string label, float value): this(name, value)
		{
			Width = width;
			Height = height;
		}
		
		void ValueLabel_Change(object sender, StringArg e)
		{
			var newValue = float.Parse(e.s);
			var delta = newValue - Value;			
			Value = newValue;
				
			UpdateScene();
			ValueChanged(this, newValue, delta);
		}

		void UpdateScene()
		{
			Label.Text = Caption + ": ";
			ValueLabel.Text = string.Format("{0:0.00}", Value);
		}
		
		void Background_MouseScroll(object sender, MouseScrollArg e)
		{
			var delta = (e.Scroll) / (120*100f);
			
			var step = e.AltKey ? 10f : 0.1f;
			if (e.ShiftKey) 
				delta *= step;
			if (e.CtrlKey) 
				delta *= step;
			
			Value += delta;
				
			UpdateScene();
			ValueChanged(this, Value, delta);
		}
		
		void Background_MouseOver(object sender, EventArgs e)
		{
		}
		
		void Background_MouseOut(object sender, EventArgs e)
		{
		}
		
		void Background_MouseUp(object sender, MouseArg e)
		{
			FMouseDown = false;
		}
		
		public override void Dispose()
		{
			Background.MouseScroll -= Background_MouseScroll;
			Background.MouseOver -= Background_MouseOver;
			Background.MouseOut -= Background_MouseOut;
			ValueLabel.Change -= ValueLabel_Change;
		}
	}
}
