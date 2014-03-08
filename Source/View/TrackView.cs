using System;
using System.Drawing;
using System.Drawing.Drawing2D;

using Posh;
using Svg;
using Svg.Transforms;
using VVVV.Core;
using VVVV.Core.Commands;

namespace Timeliner
{
	/// <summary>
	/// Description of TrackView.
	/// </summary>
	public abstract class TrackView: TLViewBase
	{
		protected SvgDefinitionList Definitions = new SvgDefinitionList();
		
		//time-range zooming (x) is done on the PanZoomGroup
		//value-range zooming (y) is done on the TrackGroup
				
		//the TrackGroup has a clippath set 
		protected SvgGroup TrackGroup = new SvgGroup();
		public SvgClipPath TrackClipPath = new SvgClipPath();
		protected SvgRectangle ClipRect = new SvgRectangle();

		//MainGroup holds: 
		//TrackGroup
		public SvgText Label = new SvgText();
		public SvgRectangle SizeBar = new SvgRectangle();

		//TrackGroup holds:  
		protected SvgRectangle Background = new SvgRectangle();
		protected SvgGroup PanZoomGroup = new SvgGroup();
		
		//parent.OverlayGroup holds:				
		public SvgRectangle SizeBarDragRect = new SvgRectangle();
		public SvgMenuWidget TrackMenu;

		public float Top
		{
			get
			{
				return (MainGroup.Transforms[0] as SvgTranslate).Y + (MainGroup.Parent.Transforms[0] as SvgTranslate).Y;
			}
		}
		
		public float Height
		{
			get
			{
				return (TrackGroup.Transforms[0] as SvgScale).Y + SizeBar.Height;
			}
		}
		
		public new TLTrack Model
        {
            get
            {
                return (TLTrack)base.Model;
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
        
        protected RulerView FRuler;
        public SvgMatrix View
        {
        	set
        	{
        		PanZoomGroup.Transforms[0] = value;
        	}
        }
		
		public TrackView(TLTrack track, TimelineView tv, RulerView rv)
			: base(track, tv)
		{
			Model = track;
			Parent = tv;
			FRuler = rv;
			
			MainGroup.Transforms = new SvgTransformCollection();
			MainGroup.Transforms.Add(new SvgTranslate(0, 0));
			
			var width = new SvgUnit(SvgUnitType.Percentage, 100);
			
			Label.FontSize = 12;
			Label.FontFamily = "Lucida Sans Unicode";
			Label.X = 2;
			Label.Y = Label.FontSize + 2;
			Label.Fill = TimelinerColors.Black;
			Label.Text = Model.Label.Value;
			Label.ID = Model.GetID() + "/label";
			Label.Change += Label_Change;
			Label.MouseDown += Background_MouseDown;
			Label.MouseUp += Background_MouseUp;
			
			SizeBarDragRect.FillOpacity = 0.3f;
			SizeBarDragRect.Visible = false;
			SizeBarDragRect.CustomAttributes["pointer-events"] = "none";
			
			Background.Width = width;
			Background.Height = 1; // this is the value range, not the actual track size
			Background.Fill = TimelinerColors.LightGray;
			Background.ID = MainGroup.ID + "/bg";
			
			ClipRect.Width = width;
			ClipRect.Height = Background.Height;
			ClipRect.ID = Background.ID + "/ClipRect";
			
			//document roots id is "svg". this is where the trackclips are added to
			TrackClipPath.ID = "svg/clip" + IDGenerator.NewID;
			TrackClipPath.Children.Add(ClipRect);
			
			TrackGroup.ID = MainGroup.ID + "/Clip";
			TrackGroup.Transforms = new SvgTransformCollection();
			TrackGroup.Transforms.Add(new SvgScale(1, 1));
			var uri = new Uri("url(#" + TrackClipPath.ID + ")", UriKind.Relative);
			TrackGroup.ClipPath = uri;
			
			PanZoomGroup.ID = MainGroup.ID + "/PanZoom";
			PanZoomGroup.Transforms = new SvgTransformCollection();
			PanZoomGroup.Transforms.Add(FRuler.PanZoomMatrix); //pan/zoom
			PanZoomGroup.Transforms.Add(new SvgTranslate(0)); //min/max
			
			SizeBar.Width = width;
			SizeBar.Height = 5;
			SizeBar.Fill = TimelinerColors.DarkGray;
			SizeBar.ID = MainGroup.ID + "/" + "SizeBar";
			SizeBar.CustomAttributes["style"] = "cursor:row-resize";
			SizeBar.Y = Background.Height.Value;
			
			//register event handlers
			Background.MouseDown += Background_MouseDown;
			Background.MouseUp += Background_MouseUp;
			Background.MouseMove += Background_MouseMove;
			
			SizeBar.MouseDown += Background_MouseDown;
			SizeBar.MouseMove += Background_MouseMove;
			SizeBar.MouseUp += Background_MouseUp;
			
			Model.Height.ValueChanged += Model_Height_ValueChanged;

			//track menu
			TrackMenu = new SvgMenuWidget(100);
			
			var collapseTrack = new SvgButtonWidget("Collapse");
			collapseTrack.OnButtonPressed += CollapseTrack;
			TrackMenu.AddItem(collapseTrack);
			
			var removeTrack = new SvgButtonWidget("Remove");
			removeTrack.OnButtonPressed += RemoveTrack;
			TrackMenu.AddItem(removeTrack);
			
			UpdateTrackHeightAndPos();
		}
		
		void CollapseTrack()
		{
			if (Model.Height.Value > 20)
			{
				Model.UncollapsedHeight.Value = Model.Height.Value; 
				Model.Height.Value = 20;
			}
			else
				Model.Height.Value = Model.UncollapsedHeight.Value;
		}
		
		void RemoveTrack()
		{
			var cmd = Command.Remove(Parent.Document.Tracks, Model);
			History.Insert(cmd);
		}
		
		public override void Dispose()
		{
			UnbuildSVG();
			
			Background.MouseDown -= Background_MouseDown;
			Background.MouseMove -= Background_MouseMove;
			Background.MouseUp -= Background_MouseUp;
			
			Label.MouseDown -= Background_MouseDown;
			Label.MouseUp -= Background_MouseUp;
			Label.Change -= Label_Change;
			
			Model.Height.ValueChanged -= Model_Height_ValueChanged;
			
			base.Dispose();
		}
		
		/// <summary>
		/// Updates the visual elements
		/// </summary>
		protected override void BuildSVG()
		{
			Definitions.Children.Clear();
			TrackGroup.Children.Clear();
			PanZoomGroup.Children.Clear();

			Parent.SvgRoot.Children.Add(TrackClipPath);
			
			TrackGroup.Children.Add(Background);
			TrackGroup.Children.Add(PanZoomGroup);
			
			MainGroup.Children.Add(Definitions);
			MainGroup.Children.Add(TrackGroup);
			MainGroup.Children.Add(Label);
			MainGroup.Children.Add(SizeBar);
			
			//stuff added to parents
			//needs to be removed extra!
			TrackMenu.ID = Parent.FOverlaysGroup.ID + "/TrackMenu" + IDGenerator.NewID;
			Parent.FOverlaysGroup.Children.Add(TrackMenu);
			SizeBarDragRect.ID = Parent.FOverlaysGroup.ID + "/DragRect" + IDGenerator.NewID;
			Parent.FOverlaysGroup.Children.Add(SizeBarDragRect);
		}
		
		protected override void UnbuildSVG()
		{
			Parent.FOverlaysGroup.Children.Remove(SizeBarDragRect);
			Parent.FOverlaysGroup.Children.Remove(TrackMenu);

			Parent.FTrackGroup.Children.Remove(MainGroup);
			
			Parent.SvgRoot.Children.Remove(TrackClipPath);
		}
		
		/// <summary>
		/// Update self and all sibling tracks
		/// </summary>
		void Model_Height_ValueChanged(IViewableProperty<float> property, float newValue, float oldValue)
		{
			//first set new value
			UpdateTrackHeightAndPos();
			
			//recalc others
			foreach (var tv in Parent.Tracks)
			{
				if(tv.Model.Order.Value > Model.Order.Value)
					tv.UpdateTrackHeightAndPos();
			}
		}
		
		public void UpdateTrackHeightAndPos()
		{
			//calc y position
			var y = 0.0f;
			foreach (var track in Parent.Document.Tracks)
			{
				if(track.Order.Value < Model.Order.Value)
					y += track.Height.Value + SizeBar.Height;
			}
			
			MainGroup.Transforms[0] = new SvgTranslate(0, y);
			
			var yScale = Model.Height.Value;
			var oldScale = TrackGroup.Transforms[0] as SvgScale;
			TrackGroup.Transforms[0] = new SvgScale(1, yScale);
			SizeBar.Y = yScale;
			
			if (yScale != oldScale.Y)
				ApplyInverseScaling();
		}
	
		public virtual void ApplyInverseScaling()
		{}
		
		void Label_Change(object sender, StringArg e)
		{
			Model.Label.Value = e.s;
			Label.Text = e.s;
		}
		
		//dispatch events to parent
		void Background_MouseDown(object sender, MouseArg e)
		{
			if(sender == SizeBar)
				Parent.Default_MouseDown(new TrackResizeMouseHandler(this, e.SessionID), e);
			else if (sender == Label)
				Parent.Default_MouseDown(new LabelDragMouseHandler(this, e.SessionID), e);
			else
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
		
		//called from child
		public void MouseMove(object sender, PointArg e)
		{
			Parent.Default_MouseMove(sender, e);
		}
		
		public void MouseUp(object sender, MouseArg e)
		{
			Parent.Default_MouseUp(sender, e);
		}
		
		public void MouseDown(object sender, MouseArg e)
		{
			Parent.Default_MouseDown(sender, e);
		}
		
		public float YPosToValue(float y)
		{
			//min/max
			var m1 = PanZoomGroup.Transforms[1].Matrix;
			
			//trackheight
			var m2 = TrackGroup.Transforms[0].Matrix;
			
			m2.Multiply(m1);
			m2.Invert();
			
			var p = new PointF[1];
			p[0] = new PointF(0, y - Top);

			m2.TransformPoints(p);
			
			return -p[0].Y;
		}
		
		public float YDeltaToValue(float y)
		{
			var relY = y / Model.Height.Value;
			
			//min/max
			var s = 1 / PanZoomGroup.Transforms[1].Matrix.Elements[3];
			return -relY * s;
		}
		
		public RectangleF ToTrackRect(RectangleF rect)
		{
			var x1 = FRuler.XPosToTime(rect.X);
			var y1 = YPosToValue(rect.Y);
			var width = FRuler.XDeltaToTime(rect.Width);
			var height = YDeltaToValue(-rect.Height);
			
			return new RectangleF(x1, y1 - height, width, height);
		}
		
		public virtual void Evaluate(RemoteContext mainloopUpdate)
		{}
	}
}
