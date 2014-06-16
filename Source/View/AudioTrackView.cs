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
		
		public EditableList<SampleView> Samples = new EditableList<SampleView>();
		
		public override IEnumerable<KeyframeView> KeyframeViews
		{
			get 
			{
				yield return null;
			}
		}
		
		Synchronizer<SampleView, TLSample> SampleSyncer;
		
		public SvgRectangle SampleDefinition = new SvgRectangle();
		public SvgGroup SampleGroup = new SvgGroup();
		public SvgPath Path = new SvgPath();
		
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
			UpdateScene();
		}
		
		public override void Dispose()
		{
//			Model.Minimum.ValueChanged -= Model_Range_ValueChanged;
//			Model.Maximum.ValueChanged -= Model_Range_ValueChanged;

			base.Dispose();
		}
		
		#region build scenegraph
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
		#endregion
		
		#region update scenegraph
		public override void UpdateScene()
		{
			base.UpdateScene();
			
			UpdateMinMaxView();
		}
		
		private void UpdateMinMaxView()
		{
			//zoom to min/max
			var scaleY = 1f;
			
			var m = new Matrix();
			m.Scale(1, 1/scaleY);
			m.Translate(0, 0.5f);
			
			PanZoomGroup.Transforms[1] = new SvgMatrix(m.Elements.ToList());
			
			ApplyInverseScaling();
		}
		
		protected override void FillTrackMenu()
		{}
		
        protected override void FillKeyframeMenu()
        {}
		#endregion
		
        public override void Evaluate()
        {}
	}
	
	public class SampleView : TLViewBaseTyped<TLSample, AudioTrackView>, IDisposable
	{
		public SvgUse Background = new SvgUse();
		
		public SampleView(TLSample s, AudioTrackView trackview)
			: base(s, trackview)
		{
			//configure svg
			Background.ReferencedElement = new Uri("#" + Parent.Model.GetID() + "_Sample", UriKind.Relative);
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
