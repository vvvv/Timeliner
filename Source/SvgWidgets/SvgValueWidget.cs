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
		private SvgText Label = new SvgText();
		private SvgText ValueLabel = new SvgText();
		public Action OnValueChanged;	
		private bool FMouseDown = false;
		private PointF FMouseDownPos;
		private PointF FLastMousePos;
		
		private float FValue;
		public float Value
		{
			get {return FValue;}
			set
			{
				FValue = value;
				UpdateScene();
			}
		}
		
		private string FCaption;
		public string Caption
		{
			get {return FCaption;}
			set
			{
				FCaption = value;
				UpdateScene();
			}
		}
		
		public SvgValueWidget(string label, float value): base()
		{
			Caption = label;
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
			ValueLabel.X = 65;
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

		void ValueLabel_Change(object sender, StringArg e)
		{
			Value = float.Parse(e.s);
				
			UpdateScene();
			OnValueChanged();
		}
		
		public SvgValueWidget(float width, float height, string label, float value): this(label, value)
		{
			Width = width;
			Height = height;
		}

		void UpdateScene()
		{
			Label.Text = Caption + ": ";
			ValueLabel.Text = string.Format("{0:0.00}", Value);
		}
		
		void Background_MouseScroll(object sender, MouseScrollArg e)
		{
			Value += (e.Scroll) / (120*10f);
				
			UpdateScene();
			OnValueChanged();
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
