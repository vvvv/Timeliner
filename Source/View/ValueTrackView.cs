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
using VVVV.Utils.VMath;

namespace Timeliner
{
	public class ValueTrackView: TrackView
	{
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
		
		public EditableList<ValueKeyframeView> Keyframes
		{
			get;
			protected set;
		}
		
		public override IEnumerable<KeyframeView> KeyframeViews
		{
			get
			{
				return Keyframes;
			}
		}
		
		public EditableList<CurveView> Curves = new EditableList<CurveView>();
		
		Synchronizer<ValueKeyframeView, TLValueKeyframe> KFSyncer;
		Synchronizer<CurveView, TLCurve> CurveSyncer;
		
		public SvgCircle KeyframeDefinition = new SvgCircle();
		public SvgLine CollapsedKeyframeDefinition = new SvgLine();
		public SvgGroup CurveGroup = new SvgGroup();
		public SvgGroup KeyframeGroup = new SvgGroup();
		SvgText CurrentValue = new SvgText();
		
		public ValueTrackView(TLValueTrack track, TimelineView tv, RulerView rv)
			: base(track, tv, rv)
		{
			
			Keyframes = new EditableList<ValueKeyframeView>();
			
			KFSyncer = Keyframes.SyncWith(Model.Keyframes,
			                              kf =>
			                              {
			                              	var kv = new ValueKeyframeView(kf, this);
			                              	kf.NeighbourChanged += NeedsRebuild;
			                              	var kfs = Model.Keyframes.ToList();
			                              	var prev = kfs.FindLastIndex(x => x.Time.Value < kf.Time.Value);
			                              	kv.AddToSceneGraphAt(KeyframeGroup, Keyframes.Count - 1 - prev);
			                              	return kv;
			                              },
			                              kv =>
			                              {
			                              	kv.Model.NeighbourChanged -= NeedsRebuild;
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
			
			CurrentValue.FontSize = 12;
			CurrentValue.X = 5;
			CurrentValue.CustomAttributes["class"] = "trackfont";
			CurrentValue.CustomAttributes["pointer-events"] = "none";
			CurrentValue.Y = 24;
			
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
		
		void UpdateMinMaxView()
		{
			//zoom to min/max
			var oldScale = PanZoomGroup.Transforms[1].Matrix.Elements[3];
			var oldOffset = PanZoomGroup.Transforms[1].Matrix.Elements[5];
			
			var scaleY = Model.Maximum.Value - Model.Minimum.Value;
			
			var m = new Matrix();
			m.Scale(1, 1/scaleY);
			m.Translate(0, Model.Maximum.Value);
			
			PanZoomGroup.Transforms[1] = new SvgMatrix(m.Elements.ToList());
			
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
			
			KeyframeDefinition.Transforms[0] = new SvgMatrix(m.Elements.ToList());
			CollapsedKeyframeDefinition.Transforms[0] = KeyframeDefinition.Transforms[0];
		}
		
		
		public override void Nudge(ref CompoundCommand cmds, NudgeDirection direction, float timeDelta, float valueDelta)
		{
			base.Nudge(ref cmds, direction, timeDelta, valueDelta);
			
			foreach(var kf in Keyframes.Where(x => x.Model.Selected.Value))
			{
				switch (direction)
				{
					case NudgeDirection.Up:
						var newValue = (float) VMath.Clamp(kf.Model.Value.Value + valueDelta, Model.Minimum.Value, Model.Maximum.Value);
						cmds.Append(Command.Set(kf.Model.Value, newValue));
						break;
					case NudgeDirection.Down:
						newValue = (float) VMath.Clamp(kf.Model.Value.Value - valueDelta, Model.Minimum.Value, Model.Maximum.Value);
						cmds.Append(Command.Set(kf.Model.Value, newValue));
						break;
				}
			}
		}
		
		public override void UpdateKeyframeMenu(KeyframeView kf)
		{
			base.UpdateKeyframeMenu(kf);
			
			//also update the value of the keyframe menu
			var item = (SvgValueWidget) KeyframeMenu.GetItem("Value");
			item.Value = (kf as ValueKeyframeView).Model.Value.Value;
		}
		#endregion
		
		#region scenegraph eventhandler
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
		
		public override void Evaluate()
		{
			CurrentValue.Text = Model.GetCurrentValueAsString();
		}
	}
}
