using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

using Posh;
using Svg;
using Svg.Transforms;
using VVVV.Core;

namespace Timeliner
{
	/// <summary>
	/// Description of RulerView.
	/// </summary>
	public class RulerView: TLViewBase
	{
		protected SvgDefinitionList Definitions = new SvgDefinitionList();
		
		//MainGroup has a clippath set 
		public SvgClipPath RulerClipPath = new SvgClipPath();
		protected SvgRectangle ClipRect = new SvgRectangle();
		
		//MainGroup holds
		protected SvgRectangle Background = new SvgRectangle();
		private SvgText Label = new SvgText();
		private SvgRectangle LabelBackground = new SvgRectangle();
		
		public SvgGroup TickNumGroup = new SvgGroup();
		public SvgGroup PanZoomGroup = new SvgGroup();
		private SvgLine TickDefinition = new SvgLine();
		private SvgLine SubTickDefinition = new SvgLine();
		
		public SvgPolygon LoopStart = new SvgPolygon();
		public SvgPolygon LoopEnd = new SvgPolygon();
		
		private float FOffset = 0;
		
		private Matrix FView = new Matrix(Timer.PPS, 0, 0, 1, 100, 0);
		private bool FViewChanged = true;
		
		public SvgMatrix PanZoomMatrix;
		
		public new TLRuler Model
        {
            get
            {
                return (TLRuler)base.Model;
            }
            protected set
            {
                base.Model = value;
            }
        }
        
        public new TimelineView Parent
        {
        	get
            {
                return (TimelineView)base.Parent;
            }
            protected set
            {
                base.Parent = value;
            }
        }
		
		public RulerView(TLRuler ruler, TimelineView tv)
			: base(ruler, tv)
		{
			Model = ruler;
			Parent = tv;
			
			PanZoomMatrix = new SvgMatrix(FView.Elements.ToList());
			
			MainGroup.Transforms = new SvgTransformCollection();
			MainGroup.Transforms.Add(new SvgTranslate(0, 0));
			
			var width = new SvgUnit(SvgUnitType.Percentage, 100);
			
			Background.Width = width;
			Background.Height = 20; 
			Background.Fill = TimelinerColors.LightGray;
			Background.ID = "bg";
			
			//register event handlers
			Background.MouseDown += Background_MouseDown;
			Background.MouseUp += Background_MouseUp;
			Background.MouseMove += Background_MouseMove;
			
			LabelBackground.Width = 95;
			LabelBackground.Height = 20;
			LabelBackground.Fill = TimelinerColors.LightGray;
						
			Label.FontSize = 12;
			Label.FontFamily = "Lucida Console";
			Label.X = 2;
			Label.Y = Label.FontSize + 2;
			Label.Fill = TimelinerColors.Black;
			Label.ID = Model.GetID() + "/label";
			Label.Text = "\u00A00:00:00:000";
			
			TickDefinition.ID = Model.GetID() + "/tick";
			TickDefinition.Stroke = TimelinerColors.DarkGray;
			TickDefinition.StartX = 0;
			TickDefinition.StartY = 0;
			TickDefinition.EndX = 0;
			TickDefinition.EndY = 20;
			TickDefinition.Transforms = new SvgTransformCollection();
			TickDefinition.Transforms.Add(new SvgScale(1, 1));
			TickDefinition.CustomAttributes["class"] = "hair";
			
			SubTickDefinition.ID = Model.GetID() + "/subtick";
			SubTickDefinition.Stroke = TimelinerColors.DarkGray;
			SubTickDefinition.StartX = 0;
			SubTickDefinition.StartY = 14;
			SubTickDefinition.EndX = 0;
			SubTickDefinition.EndY = 20;
			SubTickDefinition.Transforms = new SvgTransformCollection();
			SubTickDefinition.Transforms.Add(new SvgScale(1, 1));
			SubTickDefinition.CustomAttributes["class"] = "hair";
	
			ClipRect.Width = width;
			ClipRect.Height = Background.Height;
			ClipRect.ID = "ClipRect";
			
			//document roots id is "svg". this is where the trackclips are added to
			RulerClipPath.ID = "svg/clip" + IDGenerator.NewID;
			RulerClipPath.Children.Add(ClipRect);
			
			var uri = new Uri("url(#" + RulerClipPath.ID + ")", UriKind.Relative);
			MainGroup.ClipPath = uri;
			
			TickNumGroup.ID = "Ticks";
			TickNumGroup.Transforms = new SvgTransformCollection();
			TickNumGroup.Transforms.Add(PanZoomMatrix);
			
			PanZoomGroup.ID = "PanZoom";
			PanZoomGroup.Transforms = new SvgTransformCollection();
			PanZoomGroup.Transforms.Add(PanZoomMatrix);
			
			for (int i=0; i<70; i++)
			{
				var tick = new SvgUse();
				tick.ReferencedElement = new Uri("#" + TickDefinition.ID, UriKind.Relative);
				tick.X = i;
				TickNumGroup.Children.Add(tick);
				
				var subrange = 1 / 10f;
				for (int j=1; j<10; j++)
				{
					var subtick = new SvgUse();
					subtick.ReferencedElement = new Uri("#" + SubTickDefinition.ID, UriKind.Relative);
					subtick.X = tick.X + j * subrange;
					TickNumGroup.Children.Add(subtick);
				}
				
				var num = new SvgText(i.ToString());
				num.FontSize = 8;
				num.FontFamily = "Lucida Sans Unicode";
				num.Fill = TimelinerColors.DarkGray;
				num.CustomAttributes["pointer-events"] = "none";
				//num.X = tick.X * 10 + 1f; 
				num.Y = 10;
				num.Transforms = new SvgTransformCollection();
				num.Transforms.Add(new SvgTranslate(tick.X + 0.1f));
				num.Transforms.Add(new SvgScale(1/Timer.PPS, 1));
				
				num.CustomAttributes["class"] = "hair";
				TickNumGroup.Children.Add(num);
			}
			
			LoopStart.ID = "LoopStart";
			LoopStart.Transforms = new SvgTransformCollection();
			LoopStart.Transforms.Add(new SvgTranslate(0, 0)); 
			var points1 = new SvgUnitCollection();
			points1.Add(0);
			points1.Add(0);
			points1.Add(0.3f);
			points1.Add(10);
			points1.Add(0);
			points1.Add(20);
			LoopStart.Points = points1;
			PanZoomGroup.Children.Add(LoopStart);
			LoopStart.MouseDown += (s, e) => Parent.Default_MouseDown(LoopStart, e);
			LoopStart.MouseUp += (s, e) => Parent.Default_MouseUp(LoopStart, e);
			LoopStart.MouseMove += (s, e) => Parent.Default_MouseMove(LoopStart, e);
			Model.LoopStart.ValueChanged += Model_LoopStart_ValueChanged;
			
			LoopEnd.ID = "LoopEnd";
			LoopEnd.Transforms = new SvgTransformCollection();
			LoopEnd.Transforms.Add(new SvgTranslate(10, 0)); 
			var points2 = new SvgUnitCollection();
			points2.Add(0);
			points2.Add(0);
			points2.Add(-0.3f);
			points2.Add(10);
			points2.Add(0);
			points2.Add(20);
			LoopEnd.Points = points2;
			PanZoomGroup.Children.Add(LoopEnd);
			LoopEnd.MouseDown += (s, e) => Parent.Default_MouseDown(LoopEnd, e);
			LoopEnd.MouseUp += (s, e) => Parent.Default_MouseUp(LoopEnd, e);
			LoopEnd.MouseMove += (s, e) => Parent.Default_MouseMove(LoopEnd, e);
			Model.LoopEnd.ValueChanged += Model_LoopEnd_ValueChanged;
			
			//init scalings
			PanZoom(0, 0, 0);
			ApplyInverseScaling();
		}

		void Model_LoopStart_ValueChanged(IViewableProperty<float> property, float newValue, float oldValue)
		{
			LoopStart.Transforms[0] = new SvgTranslate(newValue);
		}
		
		void Model_LoopEnd_ValueChanged(IViewableProperty<float> property, float newValue, float oldValue)
		{
			LoopEnd.Transforms[0] = new SvgTranslate(newValue);
		}
		
		public override void Dispose()
		{
			UnbuildSVG();
			
			Background.MouseDown -= Background_MouseDown;
			Background.MouseMove -= Background_MouseMove;
			Background.MouseUp -= Background_MouseUp;

			base.Dispose();
		}
		
		/// <summary>
		/// Updates the visual elements
		/// </summary>
		protected override void BuildSVG()
		{
			Definitions.Children.Clear();
			Definitions.Children.Add(TickDefinition);
			Definitions.Children.Add(SubTickDefinition);
			
			Parent.SvgRoot.Children.Add(RulerClipPath);
			
			MainGroup.Children.Add(Definitions);
			MainGroup.Children.Add(Background);
			MainGroup.Children.Add(TickNumGroup);
			MainGroup.Children.Add(PanZoomGroup);
			MainGroup.Children.Add(LabelBackground);
			MainGroup.Children.Add(Label);
		}
		
		protected override void UnbuildSVG()
		{
			Parent.SvgRoot.Children.Remove(RulerClipPath);
			Parent.FTrackGroup.Children.Remove(MainGroup);
		}
	
		public void PanZoom(float delta, float scale, float xPos)
		{
			var scaleX = 1 + scale*0.003f;
			
			//PanZoomGroup
			var m = new Matrix(scaleX, 0, 0, 1, xPos - xPos*scaleX, 0);
			m.Multiply(FView);
			m.Translate(delta / m.Elements[0], 0);
			FView = m;
			PanZoomMatrix = new SvgMatrix(FView.Elements.ToList());
			PanZoomGroup.Transforms[0] = PanZoomMatrix;
			
			//TickNumGroup
			var unit = FView.Elements[0];
			//make sure ticks are only shifted within one unit range
			var off = m.Elements[4] % unit;
			//make a copy of the above elements and just replace translation
			var el = new List<float>(FView.Elements);
			el[4] = off;
			TickNumGroup.Transforms[0] = new SvgMatrix(el);
			
			//update number labels
			var p = new PointF[]{new PointF(0, 0)};
			FView.TransformPoints(p);
			var start = -(int) Math.Truncate(p[0].X / unit);
//			System.Diagnostics.Debug.WriteLine(start + " + " + p[0].X + " + " + off + " + " + m.Elements[4]);
			
			var nums = TickNumGroup.Children.Where(x => x is SvgText);
			
			foreach (SvgText num in nums)
			{
				var showMinus = start < 0 ? true : false;
				var time = Math.Abs(start);
			
				var ss = (int) (time % 60);
				var mm = (int) (time / 60 % 60);
				var h = (int) (time / 60 / 60 % 60);
				DateTime dt = new DateTime(2008, 1, 1, h + 1, mm, ss, 0);
				
				if (showMinus)
					num.Text = "-" + dt.ToString("mm:ss"); 
				else
					num.Text = " " + dt.ToString("mm:ss");
				
				start++;
			}
			
			if (scaleX != 1)
				ApplyInverseScaling();
			
			FViewChanged = true;
		}
		
		protected virtual void ApplyInverseScaling()
		{
			//apply inverse scaling to ticks

			var m = TickNumGroup.Transforms[0].Matrix;
			var z = 1.0f / m.Elements[0];
			
			//zoom only
			TickDefinition.Transforms[0] = new SvgMatrix(new List<float>(new float[]{z, 0, 0, 1, 0, 0}));
			SubTickDefinition.Transforms[0] = new SvgMatrix(new List<float>(new float[]{z, 0, 0, 1, 0, 0}));
			
//			var nums = PanZoomGroup.Children.Where(x => x is SvgText);
//			foreach (var num in nums)
//				num.Transforms[0] = mat;
			
			//pan/zoom
//			var s1 = new SvgScale(m.Elements[0], m.Elements[3]);
//			m.Multiply(s1.Matrix);
//			m.Invert();
//			
//			var mat = new SvgMatrix(new List<float>(m.Elements));
//				
//			var nums = PanZoomGroup.Children.Where(x => x is SvgText);
//			foreach (var num in nums)
//				num.Transforms[0] = mat;
			
			
			//pan/zoom
//			m = PanZoomMatrix.Matrix;
//			var s1 = new SvgScale(m.Elements[0], m.Elements[3]);
//			
//			//min/max
//			m = PanZoomGroup.Transforms[1].Matrix;
//			var s2 = new SvgScale(m.Elements[0], m.Elements[3]);
//			
//			//trackheight
//			m = TrackGroup.Transforms[0].Matrix;
//			
//			m.Multiply(s2.Matrix);
//			m.Multiply(s1.Matrix);
//			m.Invert();
			
//			LoopStart.Transforms[0] = new SvgMatrix(new List<float>(m.Elements));
		}
		
		//dispatch events to parent
		void Background_MouseDown(object sender, MouseArg e)
		{
			Parent.Default_MouseDown(this, e);
		}
		
		void Background_MouseUp(object sender, MouseArg e)
		{
			Parent.Default_MouseUp(this, e);
		}

		void Background_MouseMove(object sender, PointArg e)
		{
			Parent.Default_MouseMove(this, e);
		}
		
		public float XPosToTime(float x)
		{
			return (x - PanZoomMatrix.Matrix.Elements[4]) / PanZoomMatrix.Matrix.Elements[0];
		}
		
		public float TimeToXPos(float time)
		{
			return time * PanZoomMatrix.Matrix.Elements[0] + PanZoomMatrix.Matrix.Elements[4];
		}
		
		public float XDeltaToTime(float x)
		{
			return x * 1 / PanZoomMatrix.Matrix.Elements[0];
		}
		
		public RectangleF ToTrackRect(RectangleF rect)
		{
//			var x1 = XPosToTime(rect.X);
//			var y1 = YPosToValue(rect.Y);
//			var width = XDeltaToTime(rect.Width);
//			var height = YDeltaToValue(-rect.Height);
//			
			return new RectangleF(); //x1, y1 - height, width, height);
		}
		
		public void Evaluate(RemoteContext mainloopUpdate)
		{
			if (Parent.Timer.TimeDelta != 0 || FViewChanged)
			{
				Parent.TimeBar.Transforms[0] = new SvgTranslate(TimeToXPos(Parent.Timer.Time));
				mainloopUpdate.AddAttribute(Parent.TimeBar.ID, "transform", Parent.TimeBar.Transforms);
			}
			
			if (Parent.Timer.TimeDelta != 0)
			{
				Label.Text = Parent.Timer.ToString();
				mainloopUpdate.AddAttribute(Label.ID, "", Label.Text);				
			}
			
			FViewChanged = false;
		}
	}
}
