using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;

using Svg;
using Svg.Transforms;
using VVVV.Core;

namespace Timeliner
{
	public class ValueKeyframeView : TLViewBaseTyped<TLKeyframe, ValueTrackView>, IDisposable
	{
		public SvgUse Background = new SvgUse();
		private SvgText Label = new SvgText();
		
		public ValueKeyframeView(TLKeyframe kf, ValueTrackView trackview)
			: base(kf, trackview)
		{
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
		
		#region build scenegraph
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
		#endregion
		
		#region update scenegraph
		public override void UpdateScene()
		{
			base.UpdateScene();
			
			Background.X = Model.Time.Value;
			Background.Y = -Model.Value.Value;
			
			var isSelected = Model.Selected.Value;
			Label.Visible = isSelected;			
			if (isSelected)
			{
				var m = new Matrix();
				m.Translate(Background.X + 0.5f, Background.Y);
				m.Multiply(Parent.KeyframeDefinition.Transforms[0].Matrix);

				Label.Transforms[0] = new SvgMatrix(new List<float>(m.Elements));
				Label.Text = string.Format("{0:0.0000}", Model.Value.Value);
			}
			Background.CustomAttributes["class"] = isSelected ? "kf selected" : "kf";
		}
		#endregion

		#region scenegraph eventhandler
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
		#endregion
	}
}
