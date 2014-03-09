using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

using Svg;
using Svg.Transforms;
using VVVV.Core;
using VVVV.Core.Collections;
using VVVV.Core.Collections.Sync;

namespace Timeliner
{
	public class AudioTrackView: TrackView
	{
		public EditableList<SampleView> Samples = new EditableList<SampleView>();
		
		public SvgRectangle SampleDefinition = new SvgRectangle();
		public SvgGroup SampleGroup = new SvgGroup();
		public SvgPath Path = new SvgPath();
		
		private Synchronizer<SampleView, TLSample> SampleSyncer;
		
//		private SvgValueWidget MinValue, MaxValue;
		
		public new TLAudioTrack Model
        {
            get
            {
                return (TLAudioTrack)base.Model;
            }
            protected set
            {
                base.Model = value;
            }
        }
		
		public AudioTrackView(TLAudioTrack track, TimelineView tv, RulerView rv)
			: base(track, tv, rv)
		{
//			SampleSyncer = Samples.SyncWith(Model.Samples,
//			                              s =>
//			                              {
//			                              	var sv = new SampleView(s, this);
//			                              	sv.BuildSVGTo(SampleGroup);
//			                              	Model.Mapper.Map<AddContext>().DrawList.Add(sv);
//			                              	return sv;
//			                              },
//			                              sv =>
//			                              {
//			                              	Model.Mapper.Map<RemoveContext>().IDList.Add(sv.MainGroup.ID);
//			                              	sv.Dispose();
//			                              });
//			
			SampleDefinition.Width = 5;
			SampleDefinition.Height = 1;
			SampleDefinition.ID = "Sample";
//			SampleDefinition.Transforms = new SvgTransformCollection();
//			SampleDefinition.Transforms.Add(new SvgScale(1, 1));
			
			SampleGroup.ID = "Samples";
			
			Path.ID = "path";
	        Path.Stroke = new SvgColourServer(Color.Gray);
	        Path.StrokeWidth = 1.0f;
	        Path.CustomAttributes["vector-effect"] = "non-scaling-stroke";
	        Path.CustomAttributes["pointer-events"] = "none";
	        
	        
			
//			Model.Minimum.ValueChanged += Model_Range_ValueChanged;
//			Model.Maximum.ValueChanged += Model_Range_ValueChanged;
//
//			MaxValue = new SvgValueWidget("Maximum", 1);
//			MaxValue.OnValueChanged += ChangeMaximum;
//			TrackMenu.AddItem(MaxValue);
//			
//			MinValue = new SvgValueWidget("Minimum", -1);
//			MinValue.OnValueChanged += ChangeMinimum;
//			TrackMenu.AddItem(MinValue);
//			
			UpdateMinMaxView();
		}
		
		public override void Dispose()
		{
//			Model.Minimum.ValueChanged -= Model_Range_ValueChanged;
//			Model.Maximum.ValueChanged -= Model_Range_ValueChanged;

			base.Dispose();
		}
		
		protected override void BuildSVG()
		{
			base.BuildSVG();
				
			SampleGroup.Children.Clear();
			
			Definitions.Children.Add(SampleDefinition);
			PanZoomGroup.Children.Add(SampleGroup);
			
			//draw waveform
			Path.PathData.Clear();
	        var coords = new List<float>();
	        var curveMove = 'M';
	        var curveType = 'L';
	
	        foreach (var sample in Model.Samples)
	        {
	        	coords.Add(sample.Time.Value);
	        	coords.Add(sample.Value.Value);
	        }
	        SvgPathBuilder.CreatePathSegment(curveMove, Path.PathData, coords, char.IsLower(curveMove));
	        coords.Clear();
	        
	        SampleGroup.Children.Add(Path);
	        
//			foreach (var sample in Samples)
//				sample.BuildSVGTo(SampleGroup);
		}
		
		protected override void UnbuildSVG()
		{
			SampleGroup.Children.Remove(MainGroup);
		}
		
		private void UpdateMinMaxView()
		{
			//zoom to min/max
			var scaleY = 1f;
			
			var m = new Matrix();
			m.Scale(1, 1/scaleY);
			m.Translate(0, 0.5f);
			
			PanZoomGroup.Transforms[1] = new SvgMatrix(new List<float>(m.Elements));
			
			ApplyInverseScaling();
		}
		
//		void ChangeMinimum()
//		{
//			Model.Minimum.Value = MinValue.Value;
//		}
//		
//		void ChangeMaximum()
//		{
//			Model.Maximum.Value = MaxValue.Value;
//		}
		
//		void Model_Range_ValueChanged(IViewableProperty<float> property, float newValue, float oldValue)
//		{
//			UpdateMinMaxView();
//		}
		
//		private void UpdateMinMaxView()
//		{
//			//zoom to min/max
//			var scaleY = Model.Maximum.Value - Model.Minimum.Value;
//			
//			var m = new Matrix();
//			m.Scale(1, 1/scaleY);
//			m.Translate(0, Model.Maximum.Value);
//			
//			PanZoomGroup.Transforms[1] = new SvgMatrix(new List<float>(m.Elements));
//			
//			ApplyInverseScaling();
//		}
		
		public override void ApplyInverseScaling()
		{
//			if (SampleDefinition.Transforms.Count == 1)
//			{
//				//apply inverse scaling to keyframes
//				
//				//pan/zoom
//				var m = PanZoomGroup.Transforms[0].Matrix;
//				var s1 = new SvgScale(m.Elements[0], m.Elements[3]);
//				
//				//min/max
//				m = PanZoomGroup.Transforms[1].Matrix;
//				var s2 = new SvgScale(m.Elements[0], m.Elements[3]);
//				
//				//trackheight
//				m = ClipGroup.Transforms[0].Matrix;
//				
//				m.Multiply(s2.Matrix);
//				m.Multiply(s1.Matrix);
//				m.Invert();
//				
//				SampleDefinition.Transforms[0] = new SvgMatrix(new List<float>(m.Elements));
//			}
		}
	}
	
	
	public class SampleView : TLViewBaseTyped<TLSample, AudioTrackView>, IDisposable
	{
		public SvgUse Background = new SvgUse();
		
		public SampleView(TLSample s, AudioTrackView trackview)
			: base(s, trackview)
		{
			//configure svg
			Background.ReferencedElement = new Uri("#" + Parent.Model.GetID() + "/Sample", UriKind.Relative);
			Background.ID = "bg";
			Background.CustomAttributes["class"] = "kf";
			Background.Transforms = new SvgTransformCollection();
			Background.Transforms.Add(new SvgScale(1, 1));
		}
		
		protected override void BuildSVG()
		{
			Background.X = Model.Time.Value;
			Background.Y = 0;
			(Background.Transforms[0] as SvgScale).Y = Model.Value.Value;
			
			MainGroup.Children.Add(Background);
		}
		
		protected override void UnbuildSVG()
		{
			Parent.SampleGroup.Children.Remove(MainGroup);
		}
	}
}
