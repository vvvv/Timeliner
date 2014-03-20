using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;

using Svg;
using Svg.Transforms;
using VVVV.Core;
using VVVV.Core.Collections;
using VVVV.Core.Collections.Sync;
using VVVV.Core.Commands;

using Posh;

namespace Timeliner
{
	public class ValueTrackView: TrackView
	{
		public EditableList<ValueKeyframeView> Keyframes = new EditableList<ValueKeyframeView>();
		public EditableList<CurveView> Curves = new EditableList<CurveView>();
		
		public SvgCircle KeyframeDefinition = new SvgCircle();
		public SvgGroup CurveGroup = new SvgGroup();
		public SvgGroup KeyframeGroup = new SvgGroup();
		
		private Synchronizer<ValueKeyframeView, TLKeyframe> KFSyncer;
		private Synchronizer<CurveView, TLCurve> CurveSyncer;
		
		private SvgValueWidget MinValue, MaxValue;
		private SvgText CurrentValue = new SvgText();
		
		public new TLValueTrack Model
        {
            get
            {
                return (TLValueTrack)base.Model;
            }
            protected set
            {
                base.Model = value;
            }
        }
		
		public ValueTrackView(TLValueTrack track, TimelineView tv, RulerView rv)
			: base(track, tv, rv)
		{
			KFSyncer = Keyframes.SyncWith(Model.Keyframes,
			                              kf =>
			                              {
			                              	var kv = new ValueKeyframeView(kf, this);
			                              	kv.BuildSVGTo(KeyframeGroup);
			                              	return kv;
			                              },
			                              kv =>
			                              {
			                              	kv.Dispose();
			                              });
			
			CurveSyncer = Curves.SyncWith(Model.Curves,
			                              cu =>
			                              {
			                              	var cv = new CurveView(cu, this);
			                              	cv.BuildSVGTo(CurveGroup);
			                              	return cv;
			                              },
			                              cv =>
			                              {
			                              	cv.Dispose();
		                              	  }
			                             );
			
			Background.Click += Background_MouseClick;
			
			KeyframeDefinition.CenterX = 0;
			KeyframeDefinition.CenterY = 0;
			KeyframeDefinition.Radius = 2;
			KeyframeDefinition.ID = Model.GetID() + "/Keyframe";
			KeyframeDefinition.Transforms = new SvgTransformCollection();
			KeyframeDefinition.Transforms.Add(new SvgScale(1, 1));
			
			CurveGroup.ID = "Curves";
			KeyframeGroup.ID = "Keyframes";
			
			MaxValue = new SvgValueWidget("Maximum", 1);
			MaxValue.OnValueChanged += ChangeMaximum;
			TrackMenu.AddItem(MaxValue);
			
			MinValue = new SvgValueWidget("Minimum", -1);
			MinValue.OnValueChanged += ChangeMinimum;
			TrackMenu.AddItem(MinValue);
			
			CurrentValue.FontSize = 10;
			CurrentValue.FontFamily = "Lucida Sans Unicode";
			CurrentValue.Fill = TimelinerColors.Black;
			CurrentValue.Y = 25;
						
			UpdateScene();
		}
		
		public override void Dispose()
		{
			Background.Click -= Background_MouseClick;
			
			base.Dispose();
		}
		
		#region build scenegraph		
		protected override void BuildSVG()
		{
			base.BuildSVG();
				
			CurveGroup.Children.Clear();
			KeyframeGroup.Children.Clear();
			
			Definitions.Children.Add(KeyframeDefinition);
			PanZoomGroup.Children.Add(CurveGroup);
			PanZoomGroup.Children.Add(KeyframeGroup);
			
			MainGroup.Children.Add(CurrentValue);
			
			//draw curves
			foreach (var curve in Curves)
				curve.BuildSVGTo(CurveGroup);
			
			//draw keyframes
			foreach (var keyframe in Keyframes)
				keyframe.BuildSVGTo(KeyframeGroup);
		}
		#endregion
		
		#region update scenegraph
		public override void UpdateScene()
		{
			base.UpdateScene();
			
			UpdateMinMaxView();
			
			foreach (var kf in Keyframes)
				kf.UpdateScene();
			
			foreach (var curve in Curves)
				curve.UpdateScene();
		}
		
		private void UpdateMinMaxView()
		{
			//zoom to min/max
			var oldScale = PanZoomGroup.Transforms[1].Matrix.Elements[4];
			var oldOffset = PanZoomGroup.Transforms[1].Matrix.Elements[5];
			
			var scaleY = Model.Maximum.Value - Model.Minimum.Value;
			
			var m = new Matrix();
			m.Scale(1, 1/scaleY);
			m.Translate(0, Model.Maximum.Value);
			
			PanZoomGroup.Transforms[1] = new SvgMatrix(new List<float>(m.Elements));
			
			var newScale = PanZoomGroup.Transforms[1].Matrix.Elements[4];
			var newOffset = PanZoomGroup.Transforms[1].Matrix.Elements[5];
			
			FScalingChanged |= (oldScale != newScale) || (oldOffset != newOffset);
		}
		
		protected override void ApplyInverseScaling()
		{
			//apply inverse scaling to keyframes
			
			//pan/zoom
			var m = PanZoomGroup.Transforms[0].Matrix;
			var s1 = new SvgScale(m.Elements[0], m.Elements[3]);
			
			//min/max
			m = PanZoomGroup.Transforms[1].Matrix;
			var s2 = new SvgScale(m.Elements[0], m.Elements[3]);
			
			//trackheight
			m = TrackGroup.Transforms[0].Matrix;
			
			m.Multiply(s2.Matrix);
			m.Multiply(s1.Matrix);
			m.Invert();
			
			KeyframeDefinition.Transforms[0] = new SvgMatrix(new List<float>(m.Elements));
		}
		#endregion
		
		#region scenegraph eventhandler
		void ChangeMinimum()
		{
			History.Insert(Command.Set(Model.Minimum, MinValue.Value));
		}
		
		void ChangeMaximum()
		{
			History.Insert(Command.Set(Model.Maximum, MaxValue.Value));
		}
		
		void Background_MouseClick(object sender, MouseArg e)
		{
			if(e.ClickCount >= 2)
			{
				var x = FRuler.XPosToTime(e.x);
				var y = YPosToValue(e.y);
				
				var kf = new TLKeyframe(x, y);
				var cmd = Command.Add(this.Model.Keyframes, kf);
				History.Insert(cmd);
			}
		}
		#endregion
		
		public override void Evaluate()
		{
			CurrentValue.Text = Model.CurrentValue.ToString("f4");
		}
	}
}
