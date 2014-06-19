using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;

using Posh;
using Svg;
using Svg.Transforms;
using VVVV.Core;
using VVVV.Core.Collections;
using VVVV.Core.Commands;
using VVVV.Core.Model;

namespace Timeliner
{
	/// <summary>
	/// Description of TrackView.
	/// </summary>
	public abstract class TrackView: TLViewBase
	{
		public abstract IEnumerable<KeyframeView> KeyframeViews
		{
			get;
		}
		
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
		public SvgMenuWidget KeyframeMenu;
		
		//trackmenu
		protected SvgButtonWidget CollapseButton;
		protected SvgButtonWidget RemoveButton;
		
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
		
		public new TLTrackBase Model
		{
			get
			{
				return (TLTrackBase)base.Model;
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
		
		protected bool FScalingChanged;
		protected RulerView FRuler;
		public SvgMatrix View
		{
			set
			{
				FScalingChanged |= PanZoomGroup.Transforms[0].Matrix.Elements[0] != value.Matrix.Elements[0];
				PanZoomGroup.Transforms[0] = value;
			}
		}
		
		public bool Collapsed
		{
			get {return Model.Height.Value == Model.CollapsedHeight;}
		}
		
		Dictionary<SvgWidget, TLModelBase> TrackMenuDict = new Dictionary<SvgWidget, TLModelBase>();
		
		public TrackView(TLTrackBase track, TimelineView tv, RulerView rv)
			: base(track, tv)
		{
			Model = track;
			Parent = tv;
			FRuler = rv;
			
			MainGroup.Transforms = new SvgTransformCollection();
			MainGroup.Transforms.Add(new SvgTranslate(0, 0));
			
			var width = new SvgUnit(SvgUnitType.Percentage, 100);
			
			Label.FontSize = 20;
			Label.X = 5;
			Label.Y = Label.FontSize;
			Label.Text = Model.Label.Value;
			Label.ID = "label";
			Label.MouseDown += Background_MouseDown;
			Label.MouseUp += Background_MouseUp;
			Label.CustomAttributes["class"] = "trackfont";
			
			SizeBarDragRect.FillOpacity = 0.3f;
			SizeBarDragRect.Visible = false;
			SizeBarDragRect.CustomAttributes["pointer-events"] = "none";
			
			Background.Width = width;
			Background.Height = 1; // this is the value range, not the actual track size
			Background.ID ="bg";
			
			ClipRect.Width = width;
			ClipRect.Height = Background.Height;
			ClipRect.ID = "ClipRect";
			
			//document roots id is "svg". this is where the trackclips are added to
			TrackClipPath.ID = "svg_clip" + IDGenerator.NewID;
			TrackClipPath.Children.Add(ClipRect);
			
			TrackGroup.ID = "Clip";
			TrackGroup.Transforms = new SvgTransformCollection();
			TrackGroup.Transforms.Add(new SvgScale(1, 1));
			var uri = new Uri("url(#" + TrackClipPath.ID + ")", UriKind.Relative);
			TrackGroup.ClipPath = uri;
			
			PanZoomGroup.ID = "PanZoom";
			PanZoomGroup.Transforms = new SvgTransformCollection();
			PanZoomGroup.Transforms.Add(FRuler.PanZoomMatrix); //pan/zoom
			PanZoomGroup.Transforms.Add(new SvgTranslate(0)); //min/max
			
			SizeBar.Width = width;
			SizeBar.Height = 5;
			SizeBar.ID = "SizeBar";
			SizeBar.CustomAttributes["class"] = "sizebar";
			SizeBar.Y = Background.Height.Value;
			
			//register event handlers
			Background.MouseDown += Background_MouseDown;
			Background.MouseUp += Background_MouseUp;
			Background.MouseMove += Background_MouseMove;
			
			SizeBar.MouseDown += Background_MouseDown;
			SizeBar.MouseMove += Background_MouseMove;
			SizeBar.MouseUp += Background_MouseUp;
			
			CreateTrackMenu();
			CreateKeyframeMenu();
		}
		
		public override void Dispose()
		{
			TrackMenuDict.Clear();
			
			Background.MouseDown -= Background_MouseDown;
			Background.MouseMove -= Background_MouseMove;
			Background.MouseUp -= Background_MouseUp;
			
			SizeBar.MouseDown -= Background_MouseDown;
			SizeBar.MouseMove -= Background_MouseMove;
			SizeBar.MouseUp -= Background_MouseUp;
			
			Label.MouseDown -= Background_MouseDown;
			Label.MouseUp -= Background_MouseUp;
			
			CollapseButton.ValueChanged -= CollapseTrack;
			RemoveButton.ValueChanged -= RemoveTrack;
			
			base.Dispose();
		}
		
		#region build scenegraph
		protected override void BuildSVG()
		{
			Definitions.Children.Clear();
			TrackGroup.Children.Clear();
			PanZoomGroup.Children.Clear();
			
			TrackGroup.Children.Add(Background);
			TrackGroup.Children.Add(PanZoomGroup);
			
			MainGroup.Children.Add(Definitions);
			MainGroup.Children.Add(TrackGroup);
			MainGroup.Children.Add(Label);
			MainGroup.Children.Add(SizeBar);
			
			//stuff added to parents
			//needs to be removed extra!
			TrackMenu.ID = "TrackMenu" + IDGenerator.NewID;
			Parent.FOverlaysGroup.Children.Add(TrackMenu);
			KeyframeMenu.ID = "KeyframeMenu" + IDGenerator.NewID;
			Parent.FOverlaysGroup.Children.Add(KeyframeMenu);
			
			SizeBarDragRect.ID = "DragRect" + IDGenerator.NewID;
			Parent.FOverlaysGroup.Children.Add(SizeBarDragRect);
			
			Parent.SvgRoot.Children.Add(TrackClipPath);
		}
		
		protected override void UnbuildSVG()
		{
			Parent.FOverlaysGroup.Children.Remove(SizeBarDragRect);
			Parent.FOverlaysGroup.Children.Remove(TrackMenu);
			Parent.FOverlaysGroup.Children.Remove(KeyframeMenu);

			Parent.SvgRoot.Children.Remove(TrackClipPath);
			
			Parent.FTrackGroup.Children.Remove(MainGroup);
		}
		
		void CreateTrackMenu()
		{
			TrackMenu = new SvgMenuWidget(110);

			CollapseButton = new SvgButtonWidget(0, 20, "Collapse");
			CollapseButton.ValueChanged += CollapseTrack;
			TrackMenu.AddItem(CollapseButton, 1);
			
			//get all [MenuEntry] properties of Model
			var props = Model.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(TrackMenuEntryAttribute)));
			foreach (var p in props)
			{
				var attributes = p.GetCustomAttributes(true);
				var menuEntry = (attributes[0] as TrackMenuEntryAttribute);
				
				//get an editor for the property
				var editor = WidgetFactory.GetWidget(Model, p, 0, menuEntry.Height);
				editor.ValueChanged += ChangeTrackMenuEntry;
				
				TrackMenuDict.Add(editor, Model);
				
				//add it to the TrackMenu
				TrackMenu.AddItem(editor, menuEntry.Order);
			}
			
			RemoveButton = new SvgButtonWidget(0, 20, "Remove");
			RemoveButton.ValueChanged += RemoveTrack;
			TrackMenu.AddItem(RemoveButton, 4);
		}
		
		void ChangeTrackMenuEntry(SvgWidget editor, object newValue, object delta)
		{
			var cmds = new CompoundCommand();
			SendValue(ref cmds, TrackMenuDict[editor], editor.Name, newValue);
			History.Insert(cmds);
		}
		
		void CreateKeyframeMenu()
		{
			KeyframeMenu = new SvgMenuWidget(110);
			
			//get all [MenuEntry] properties of Model
			IEnumerable<PropertyInfo> props = new List<PropertyInfo>();
			if (this is ValueTrackView)
				props = typeof(TLValueKeyframe).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(KeyframeMenuEntryAttribute)));
			else if (this is StringTrackView)
				props = typeof(TLStringKeyframe).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(KeyframeMenuEntryAttribute)));

			foreach (var p in props)
			{
				var attributes = p.GetCustomAttributes(true);
				var menuEntry = (attributes[0] as KeyframeMenuEntryAttribute);
				
				//get an editor for the property
				var editor = WidgetFactory.GetWidget(null, p, 0, menuEntry.Height);
				editor.ValueChanged += ChangeKeyframeMenuEntry;
				
				//add it to the TrackMenu
				KeyframeMenu.AddItem(editor, menuEntry.Order);
			}
		}
		
		void ChangeKeyframeMenuEntry(SvgWidget editor, object newValue, object delta)
		{
			var cmds = new CompoundCommand();
			foreach (var track in Parent.Document.Tracks)
				foreach (var kf in track.KeyframeModels.Where(x => x.Selected.Value))
					SendValue(ref cmds, kf, editor.Name, delta);
			History.Insert(cmds);
		}
		
		void SendValue(ref CompoundCommand cmds, TLModelBase model, string name, object newValue)
		{
			//from editor get editableproperty
			var property = model.GetType().GetProperty(name);
			if (property.PropertyType.GenericTypeArguments[0] == typeof(string))
			{
				var prop = (EditableProperty<string>) property.GetValue(model);
				cmds.Append(Command.Set(prop, newValue));
			}
			else if (property.PropertyType.GenericTypeArguments[0] == typeof(float))
			{
				var prop = (EditableProperty<float>) property.GetValue(model);
				cmds.Append(Command.Set(prop, prop.Value + (float) newValue));
			}
		}
		#endregion
		
		#region update scenegraph
		public override void UpdateScene()
		{
			base.UpdateScene();
			
			UpdateTrackHeightAndPos();
			
			if (FScalingChanged)
			{
				ApplyInverseScaling();
				FScalingChanged = false;
			}
			
			Label.Text = Model.Label.Value;
			CollapseButton.Label = Collapsed ? "Uncollapse" : "Collapse";
		}
		
		void UpdateTrackHeightAndPos()
		{
			//calc y position
			var y = 0.0f;
			foreach (var track in Parent.Document.Tracks.Where(x => x.Order.Value < Model.Order.Value))
				y += track.Height.Value + SizeBar.Height;
			
			MainGroup.Transforms[0] = new SvgTranslate(0, y);
			
			var yScale = Model.Height.Value;
			var oldScale = TrackGroup.Transforms[0] as SvgScale;
			TrackGroup.Transforms[0] = new SvgScale(1, yScale);
			SizeBar.Y = yScale;
			
			FScalingChanged |= yScale != oldScale.Y;
			
			Background.CustomAttributes["class"] = Model.Order.Value % 2 == 0 ? "even": "odd";
		}
		
		protected virtual void ApplyInverseScaling()
		{}
		
		public virtual void RebuildAfterUpdate()
		{}
		
		public virtual void UpdateKeyframeMenu(KeyframeView kf)
		{
			var item = (SvgValueWidget) KeyframeMenu.GetItem("Time");
			item.Value = kf.Model.Time.Value;
		}
		#endregion
		
		#region scenegraph eventhandler
		void RemoveTrack(SvgWidget widget, object newValue, object delta)
		{
			History.Insert(Command.Remove(Parent.Document.Tracks, Model));
		}
		
		public void CollapseTrack(SvgWidget editor, object newValue, object delta)
		{
			var newHeight = 0f;
			if (Model.Height.Value > 50)
			{
				Model.UncollapsedHeight.Value = Model.Height.Value;
				newHeight = 50;
			}
			else
				newHeight = Model.UncollapsedHeight.Value;
			
			History.Insert(Command.Set(Model.Height, newHeight));
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

		void Background_MouseMove(object sender, MouseArg e)
		{
			Parent.Default_MouseMove(this, e);
		}
		
		//called from child
		public void MouseMove(object sender, MouseArg e)
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
		#endregion
		
		#region helper
		public virtual void Nudge(ref CompoundCommand cmds, NudgeDirection direction, float timeDelta, float valueDelta)
		{
			foreach(var kf in KeyframeViews)
			{
				if (kf.Model.Selected.Value)
					switch (direction)
				{
					case NudgeDirection.Back:
						cmds.Append(Command.Set(kf.Model.Time, kf.Model.Time.Value - timeDelta));
						break;
					case NudgeDirection.Forward:
						cmds.Append(Command.Set(kf.Model.Time, kf.Model.Time.Value + timeDelta));
						break;
				}
			}
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
		#endregion
		
		public abstract void Evaluate();
	}
}