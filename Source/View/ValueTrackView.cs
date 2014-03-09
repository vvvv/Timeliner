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
		public EditableList<KeyframeView> Keyframes = new EditableList<KeyframeView>();
		public EditableList<CurveView> Curves = new EditableList<CurveView>();
		
		public SvgCircle KeyframeDefinition = new SvgCircle();
		public SvgGroup CurveGroup = new SvgGroup();
		public SvgGroup KeyframeGroup = new SvgGroup();
		
		private Synchronizer<KeyframeView, TLKeyframe> KFSyncer;
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
			                              	var kv = new KeyframeView(kf, this);
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
			
			Model.BeforeBuildingCurves += Model_BeforeBuildingCurves;
			Model.AfterBuildingCurves += Model_AfterBuildingCurves;
			Model.Minimum.ValueChanged += Model_Range_ValueChanged;
			Model.Maximum.ValueChanged += Model_Range_ValueChanged;

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
			CurrentValue.CustomAttributes["pointer-events"] = "none";
			
			UpdateMinMaxView();
		}
		
		public override void Dispose()
		{
			Background.Click -= Background_MouseClick;
			
			Model.BeforeBuildingCurves -= Model_BeforeBuildingCurves;
			Model.AfterBuildingCurves -= Model_AfterBuildingCurves;
			Model.Minimum.ValueChanged -= Model_Range_ValueChanged;
			Model.Maximum.ValueChanged -= Model_Range_ValueChanged;

			base.Dispose();
		}
		
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
		
		void ChangeMinimum()
		{
			Model.Minimum.Value = MinValue.Value;
		}
		
		void ChangeMaximum()
		{
			Model.Maximum.Value = MaxValue.Value;
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
		
		void Model_Range_ValueChanged(IViewableProperty<float> property, float newValue, float oldValue)
		{
			UpdateMinMaxView();
		}
		
		private void UpdateMinMaxView()
		{
			//zoom to min/max
			var scaleY = Model.Maximum.Value - Model.Minimum.Value;
			
			var m = new Matrix();
			m.Scale(1, 1/scaleY);
			m.Translate(0, Model.Maximum.Value);
			
			PanZoomGroup.Transforms[1] = new SvgMatrix(new List<float>(m.Elements));
			
			ApplyInverseScaling();
		}
		
		public override void ApplyInverseScaling()
		{
			if (KeyframeDefinition.Transforms.Count == 1)
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
		}
		
		//remove all curves
		void Model_BeforeBuildingCurves(object sender, EventArgs e)
		{

		}
		
		//add all curves
		void Model_AfterBuildingCurves(object sender, EventArgs e)
		{

		}
		
		public override void Evaluate(RemoteContext mainloopUpdate)
		{
			CurrentValue.Text = Model.CurrentValue.ToString("f4");
			mainloopUpdate.AddAttribute(CurrentValue.ID, "", CurrentValue.Text);
		}
	}
}
