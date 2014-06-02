using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;

using Posh;
using Svg;
using Svg.Transforms;
using VVVV.Core;
using VVVV.Core.Collections;
using VVVV.Core.Collections.Sync;
using VVVV.Core.Commands;

namespace Timeliner
{
	public class ValueTrackView: TrackView
	{
		public EditableList<ValueKeyframeView> Keyframes = new EditableList<ValueKeyframeView>();
		public EditableList<CurveView> Curves = new EditableList<CurveView>();
		
		public SvgCircle KeyframeDefinition = new SvgCircle();
        public SvgLine CollapsedKeyframeDefinition = new SvgLine();
		public SvgGroup CurveGroup = new SvgGroup();
		public SvgGroup KeyframeGroup = new SvgGroup();
		
		private Synchronizer<ValueKeyframeView, TLValueKeyframe> KFSyncer;
		private Synchronizer<CurveView, TLCurve> CurveSyncer;
		
		private SvgValueWidget MinValueEdit, MaxValueEdit;
		public SvgValueWidget ValueEdit;
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
			                              	kv.AddToSceneGraphAt(KeyframeGroup);
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
			                              	cv.AddToSceneGraphAt(CurveGroup);
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
			KeyframeDefinition.Radius = 3;
			KeyframeDefinition.ID = Model.GetID() + "_KF";
			KeyframeDefinition.Transforms = new SvgTransformCollection();
			KeyframeDefinition.Transforms.Add(new SvgScale(1, 1));
            
            CollapsedKeyframeDefinition.ID = Model.GetID() + "_CKF";
            CollapsedKeyframeDefinition.StartX = 0;
            CollapsedKeyframeDefinition.StartY = -25f;
            CollapsedKeyframeDefinition.EndX = 0;
            CollapsedKeyframeDefinition.EndY = 25f;
            CollapsedKeyframeDefinition.Transforms = new SvgTransformCollection();
			CollapsedKeyframeDefinition.Transforms.Add(new SvgScale(1, 1));
			
			CurveGroup.ID = "Curves";
			KeyframeGroup.ID = "Keyframes";
			
			CurrentValue.FontSize = 20;
            CurrentValue.X = 5;
            CurrentValue.CustomAttributes["class"] = "trackfont";
            CurrentValue.CustomAttributes["pointer-events"] = "none";
			CurrentValue.Y = 40;
						
			UpdateScene();
		}
		
		public override void Dispose()
		{
			Background.Click -= Background_MouseClick;
			
			MaxValueEdit.OnValueChanged -= ChangeMaximum;
			MinValueEdit.OnValueChanged -= ChangeMinimum;
			ValueEdit.OnValueChanged -= ChangeKeyframeValue;
			
			base.Dispose();
		}
		
		#region build scenegraph		
		protected override void BuildSVG()
		{
			base.BuildSVG();
				
			CurveGroup.Children.Clear();
			KeyframeGroup.Children.Clear();
			
			Definitions.Children.Add(KeyframeDefinition);
            Definitions.Children.Add(CollapsedKeyframeDefinition);
			PanZoomGroup.Children.Add(CurveGroup);
			PanZoomGroup.Children.Add(KeyframeGroup);
			
			MainGroup.Children.Add(CurrentValue);
			
			//draw curves
			foreach (var curve in Curves)
				curve.AddToSceneGraphAt(CurveGroup);
			
			//draw keyframes
			foreach (var keyframe in Keyframes)
				keyframe.AddToSceneGraphAt(KeyframeGroup);
		}
		#endregion
		
		#region update scenegraph
		public override void UpdateScene()
		{
			base.UpdateScene();
			
			UpdateMinMaxView();
			
			CollapsedKeyframeDefinition.StartY = - Model.CollapsedHeight * PanZoomGroup.Transforms[1].Matrix.Elements[5];
			CollapsedKeyframeDefinition.EndY = CollapsedKeyframeDefinition.StartY + Model.CollapsedHeight;
			
			foreach (var kf in Keyframes)
				kf.UpdateScene();
			
			foreach (var curve in Curves)
				curve.UpdateScene();
		}
		
		private void UpdateMinMaxView()
		{
			//zoom to min/max
			var oldScale = PanZoomGroup.Transforms[1].Matrix.Elements[3];
			var oldOffset = PanZoomGroup.Transforms[1].Matrix.Elements[5];
			
			var scaleY = Model.Maximum.Value - Model.Minimum.Value;
			
			var m = new Matrix();
			m.Scale(1, 1/scaleY);
			m.Translate(0, Model.Maximum.Value);
			
			PanZoomGroup.Transforms[1] = new SvgMatrix(new List<float>(m.Elements));
			
			var newScale = PanZoomGroup.Transforms[1].Matrix.Elements[3];
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
            CollapsedKeyframeDefinition.Transforms[0] = KeyframeDefinition.Transforms[0];
		}
		#endregion
		
		#region scenegraph eventhandler
		void ChangeMinimum()
		{
			History.Insert(Command.Set(Model.Minimum, MinValueEdit.Value));
		}
		
		void ChangeMaximum()
		{
			History.Insert(Command.Set(Model.Maximum, MaxValueEdit.Value));
		}
		
		protected override void ChangeKeyframeTime()
		{
			History.Insert(Command.Set(Keyframes.ToList().First(x => x.Model.Selected.Value).Model.Time, TimeEdit.Value));
		}
		
		void ChangeKeyframeValue()
		{
			var cmd = new CompoundCommand();
			
			var min = Model.Maximum.Value;
			var max = Model.Minimum.Value;
			var value = Math.Min(min, Math.Max(max, ValueEdit.Value));
			
			foreach(var kf in Keyframes)
			{
				if (kf.Model.Selected.Value)
					cmd.Append(Command.Set(kf.Model.Value, value));
			}
					
			History.Insert(cmd);
		}
		
		void Background_MouseClick(object sender, MouseArg e)
		{
			if(e.ClickCount >= 2)
			{
				var x = FRuler.XPosToTime(e.x);
				var y = YPosToValue(e.y);
				
				var kf = new TLValueKeyframe(x, y);
				var cmd = Command.Add(this.Model.Keyframes, kf);
				History.Insert(cmd);
			}
		}
		#endregion
        
        protected override void FillTrackMenu()
        {
            MaxValueEdit = new SvgValueWidget(0, 20, "Maximum", 1);
			MaxValueEdit.OnValueChanged += ChangeMaximum;
			TrackMenu.AddItem(MaxValueEdit);
			
			MinValueEdit = new SvgValueWidget(0, 20, "Minimum", -1);
			MinValueEdit.OnValueChanged += ChangeMinimum;
			TrackMenu.AddItem(MinValueEdit);
        }
        
        protected override void FillKeyframeMenu()
        {
            ValueEdit = new SvgValueWidget(0, 20, "Value", 0);
			ValueEdit.OnValueChanged += ChangeKeyframeValue;
			KeyframeMenu.AddItem(ValueEdit);
        }
        
		public override void Evaluate()
		{
			CurrentValue.Text = Model.CurrentValue.ToString("f4");
		}
	}
}
