using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;

using Svg;
using Svg.Transforms;
using VVVV.Core;

namespace Timeliner
{
	/// <summary>
	/// Description of Keyframe.
	/// </summary>
	public class KeyframeView : TLViewBaseTyped<TLKeyframe, ValueTrackView>, IDisposable
	{
		public SvgUse Background = new SvgUse();
		private SvgText Label = new SvgText();
		
		public KeyframeView(TLKeyframe kf, ValueTrackView trackview)
			: base(kf, trackview)
		{
			//listen to model
			Model.Selected.ValueChanged += Model_Selected_ValueChanged;
			Model.Time.ValueChanged += Model_Time_ValueChanged;
			Model.Value.ValueChanged += Model_Value_ValueChanged;
			
			//configure svg
			Background.ReferencedElement = new Uri("#" + Parent.Model.GetID() + "/Keyframe", UriKind.Relative);
			Background.ID = "bg";
			Background.CustomAttributes["class"] = "kf";
			
			Background.MouseDown += Background_MouseDown;
			Background.MouseUp += Background_MouseUp;
			Background.MouseMove += Background_MouseMove;
			
			Label.FontSize = 8;
			Label.FontFamily = "Lucida Sans Unicode";
			Label.ID = "label";
			Label.CustomAttributes["pointer-events"] = "none";
			Label.Visible = false;
			Label.Transforms = new SvgTransformCollection();
			Label.Transforms.Add(new SvgScale(1, 1));
		}
		
		//auto update attributes
		void Model_Selected_ValueChanged(IViewableProperty<bool> property, bool newValue, bool oldValue)
		{
			Background.CustomAttributes["class"] = newValue ? "kf selected" : "kf";
			if (newValue)
			{
				Label.Visible = true;
				Label.Text = string.Format("{0:0.0000}", Model.Value.Value);
				PositionLabel();
			}
			else
				Label.Visible = false;
		}
		
		void Model_Time_ValueChanged(IViewableProperty<float> property, float newValue, float oldValue)
		{
			Background.X = newValue;
			if (Model.Selected.Value)
				PositionLabel();
		}
		
		void Model_Value_ValueChanged(IViewableProperty<float> property, float newValue, float oldValue)
		{
			Background.Y = -newValue;
			if (Model.Selected.Value)
			{
				Label.Text = string.Format("{0:0.0000}", newValue);
				PositionLabel();
			}
		}
		
		private void PositionLabel()
		{
			var m = new Matrix();
			m.Translate(LabelX(), LabelY());
			m.Multiply(Parent.KeyframeDefinition.Transforms[0].Matrix);
			Label.Transforms[0] = new SvgMatrix(new List<float>(m.Elements));
		}
		
		private float LabelX()
		{
			return Background.X + 0.5f;
		}
		
		private float LabelY()
		{
			return Background.Y; // + Label.FontSize / 2 - 1;
		}
		
		//dipatch events to parent
		void Background_MouseMove(object sender, MouseArg e)
		{
			Parent.MouseMove(this, e);
		}
		
		void Background_MouseUp(object sender, MouseArg e)
		{
			Parent.MouseUp(this, e);
		}
		
		void Background_MouseDown(object sender, MouseArg e)
		{
			Parent.MouseDown(this, e);
		}
		
		protected override void BuildSVG()
		{
			Background.X = Model.Time.Value;
			Background.Y = -Model.Value.Value;
			
			MainGroup.Children.Add(Background);
			MainGroup.Children.Add(Label);
		}
		
		protected override void UnbuildSVG()
		{
			Parent.KeyframeGroup.Children.Remove(MainGroup);
		}
		
		public override void Dispose()
		{
			Model.Time.ValueChanged -= Model_Value_ValueChanged;
			Model.Value.ValueChanged -= Model_Time_ValueChanged;
			Model.Selected.ValueChanged -= Model_Selected_ValueChanged;
			
			UnbuildSVG();
			
			base.Dispose();
		}
	}
}
