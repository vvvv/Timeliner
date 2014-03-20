using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using Svg;
using Svg.Transforms;
using Svg.DataTypes;

using VVVV.Core.Commands;
using VVVV.Core.Model;

namespace Timeliner
{
	internal class MainMenuHandler : MouseHandlerBase<TimelineView>
	{
		public MainMenuHandler(TimelineView view, string sessionID)
			: base(view, sessionID)
		{
		}
		
		public override void MouseClick(object sender, MouseArg arg)
		{
			if (arg.Button == 2)
			{
				Instance.MainMenu.Transforms[0] = new SvgTranslate(arg.x, arg.y - 50);
				Instance.MainMenu.Visible = true;
			}
		}
	}
	
	internal class TimeBarHandler : MouseHandlerBase<TimelineView>
	{
		public TimeBarHandler(TimelineView view, string sessionID)
			: base(view, sessionID)
		{
		}
		
		public override void MouseDrag(object sender, PointF arg, PointF delta, int callNr)
		{
			Instance.Timer.Time += Instance.Ruler.XDeltaToTime(delta.X);
		}
	}
	
	internal class TrackPanZoomHandler : MouseHandlerBase<TimelineView>
	{
		public TrackPanZoomHandler(TimelineView view, string sessionID)
			: base(view, sessionID)
		{
		}
		
		public override void MouseDrag(object sender, PointF arg, PointF delta, int callNr)
		{
			var zoom = Math.Abs(delta.X) < Math.Abs(delta.Y);
			
			if (zoom)
				Instance.Ruler.PanZoom(0, delta.Y, arg.X);
			else
				Instance.Ruler.PanZoom(delta.X, 0, arg.X);
			
			foreach (var tv in Instance.Tracks)
				tv.View = Instance.Ruler.PanZoomMatrix;
			
			Instance.UpdateScene();
		}
	}
	
	internal class TrackResizeMouseHandler : MouseHandlerBase<TrackView>
	{
		private CompoundCommand FMoveCommands;
		
		public TrackResizeMouseHandler(TrackView tv, string sessionID)
			: base(tv, sessionID)
		{
		}
		
		public override IMouseEventHandler MouseDown(object sender, MouseArg arg)
		{
			if(arg.Button == 1)
			{
				var ret = base.MouseDown(sender, arg);
				
				//start collecting movecommands in drag
				FMoveCommands = new CompoundCommand();
				
				return ret;
			}
			else
				return null;
		}
		
		public override void MouseDrag(object sender, PointF arg, PointF delta, int dragCall)
		{
			var cmd = new CompoundCommand();
			
			if(delta.Y != 0)
			{
				var h = Instance.Model.Height.Value;
				if ((delta.Y < 0) || (arg.Y > Instance.Top + Instance.Height))
				{
					h += delta.Y;
					
					//scale this track
					cmd.Append(Command.Set(Instance.Model.Height, Instance.Model.Height.Value + delta.Y));
					
					//execute changes immediately
					cmd.Execute();
					//collect changes for history
					FMoveCommands.Append(cmd);
				}
			}
			
			Instance.Parent.UpdateScene();
		}
		
		public override IMouseEventHandler MouseUp(object sender, MouseArg arg)
		{
			if(arg.Button == 1)
				//add collected commands to history
				Instance.History.InsertOnly(FMoveCommands);
			
			return base.MouseUp(sender, arg);
		}
	}
	
	internal class LabelDragMouseHandler : MouseHandlerBase<TrackView>
	{
		public LabelDragMouseHandler(TrackView tv, string sessionID)
			: base(tv, sessionID)
		{
		}
		
		//delete on right click
		public override IMouseEventHandler MouseDown(object sender, MouseArg arg)
		{
			if(arg.Button == 1)
			{
				var ret = base.MouseDown(sender, arg);
				
				Instance.SizeBarDragRect.Width = new SvgUnit(SvgUnitType.Percentage, 100);
				Instance.SizeBarDragRect.Height = 20;
				Instance.SizeBarDragRect.X = 0;
				Instance.SizeBarDragRect.Y = Instance.Top - Instance.SizeBarDragRect.Height / 2 - (Instance.MainGroup.Parent.Transforms[0] as SvgTranslate).Y - Instance.SizeBar.Height / 2;
				Instance.SizeBarDragRect.Visible = true;
				Instance.Label.FontWeight = SvgFontWeight.bold;
				Instance.Label.CustomAttributes["style"] = "font-weight: bold;";
				return ret;
			}
			else return null;
		}
		
		public override void MouseDrag(object sender, PointF arg, PointF delta, int dragCall)
		{
			if (delta.Y != 0)
			{
				if (sender is TrackView)
				{
					var target = sender as TrackView;
					Instance.SizeBarDragRect.Y = target.Top - Instance.SizeBarDragRect.Height / 2 - (Instance.MainGroup.Parent.Transforms[0] as SvgTranslate).Y - Instance.SizeBar.Height / 2;
				}
				else
				{
					Instance.SizeBarDragRect.Y = Instance.Parent.Tracks.Last().Top + Instance.Parent.Tracks.Last().Height - Instance.SizeBarDragRect.Height / 2 - (Instance.MainGroup.Parent.Transforms[0] as SvgTranslate).Y  - Instance.SizeBar.Height / 2;
				}
			}
		}
		
		public override IMouseEventHandler MouseUp(object sender, MouseArg arg)
		{
			Instance.SizeBarDragRect.Visible = false;
			Instance.Label.CustomAttributes["style"] = "";
			
			if (arg.Button == 1)
			{
				var oldOrder = Instance.Model.Order.Value;
				var newOrder = -1;
				if (sender is TrackView)
					newOrder = (sender as TrackView).Model.Order.Value;
				else
					newOrder = Instance.Parent.Tracks.Count-1;
				
				//send a commmand to set the order of each trackview
				var cmds = new CompoundCommand();
				
				if (newOrder > oldOrder)
				{
					foreach (var track in Instance.Parent.Tracks)
						if (track != Instance)
							if ((track.Model.Order.Value > oldOrder) && (track.Model.Order.Value <= newOrder))
								cmds.Append(Command.Set(track.Model.Order, track.Model.Order.Value - 1));
				}
				else
				{
					foreach (var track in Instance.Parent.Tracks)
						if (track != Instance)
							if ((track.Model.Order.Value >= newOrder) && (track.Model.Order.Value < oldOrder))
								cmds.Append(Command.Set(track.Model.Order, track.Model.Order.Value + 1));
				}
				
				cmds.Append(Command.Set(Instance.Model.Order, newOrder));
				
				Instance.History.Insert(cmds);
				//resort tracks after order
				//Instance.Parent.Tracks.Sort((t1, t2) => t1.Model.Order.Value.CompareTo(t2.Model.Order.Value));
			}
			
			return base.MouseUp(sender, arg);
		}
	}
	
	internal class TrackMenuHandler : MouseHandlerBase<TrackView>
	{
		public TrackMenuHandler(TrackView view, string sessionID)
			: base(view, sessionID)
		{
		}
		
		public override void MouseClick(object sender, MouseArg arg)
		{
			if (arg.Button == 2)
			{
				Instance.TrackMenu.Transforms[0] = new SvgTranslate(arg.x, arg.y - 50);
				Instance.TrackMenu.Visible = true;
			}
		}
	}
	
	internal class SelectionMouseHandler : MouseHandlerBase<ValueTrackView>
	{
		private CompoundCommand FMoveCommands;
		
		public SelectionMouseHandler(ValueTrackView view, string sessionID)
			
			: base(view, sessionID)
		{
		}
		
		public override IMouseEventHandler MouseDown(object sender, MouseArg arg)
		{
			//deselect keyframes
			foreach (var kf in (Instance as ValueTrackView).Keyframes)
			{
				if (kf.Model.Selected.Value)
				{
					Instance.History.Insert(Command.Set(kf.Model.Selected, false));
				}
			}
			
			//start collecting movecommands in drag
			FMoveCommands = new CompoundCommand();
			
			return base.MouseDown(sender, arg);
		}
		
		public override void MouseSelection(object sender, RectangleF rect)
		{
			var cmd = new CompoundCommand();
			foreach (var track in Instance.Parent.Tracks.OfType<ValueTrackView>())
			{
				var trackRect = track.ToTrackRect(rect);
				foreach (var kf in track.Keyframes)
				{
					var wasSelected = kf.Model.Selected.Value;
					var isSelected = trackRect.Contains(kf.Model.Time.Value, kf.Model.Value.Value);
					if (isSelected != wasSelected)
					{
						cmd.Append(Command.Set(kf.Model.Selected, isSelected));
					}
				}
			}
			
			//execute changes immediately
			cmd.Execute();
			//collect changes for history
			FMoveCommands.Append(cmd);
			
			rect = new RectangleF(rect.X, rect.Y - Instance.Parent.FTrackGroup.Transforms[0].Matrix.Elements[5], rect.Width, rect.Height);
			Instance.Parent.SetSelectionRect(rect);
			Instance.Parent.UpdateScene();
		}
		
		public override IMouseEventHandler MouseUp(object sender, MouseArg arg)
		{
			Instance.Parent.SetSelectionRect(new RectangleF(0, 0, 0, 0));
			
			//add collected commands to history
			Instance.History.InsertOnly(FMoveCommands);
			
			return base.MouseUp(sender, arg);
		}
	}
	
	internal class KeyframeMouseHandler : MouseHandlerBase<ValueKeyframeView>
	{
		public KeyframeMouseHandler(ValueKeyframeView view, string sessionID)
			: base(view, sessionID)
		{
		}
		
		private bool FWasSelected;
		private bool FWasMoved;
		private List<ValueTrackView> FAlteredTracks = new List<ValueTrackView>();
		private CompoundCommand FMoveCommands;
		
		//delete on right click
		public override IMouseEventHandler MouseDown(object sender, MouseArg arg)
		{
			if (arg.Button == 3)
			{
				var cmd = Command.Remove(Instance.Parent.Model.Keyframes, Instance.Model);
				Instance.History.Insert(cmd);
				return null;
			}
			else if(arg.Button == 1)
			{
				var ret = base.MouseDown(sender, arg);
				
				FWasSelected = Instance.Model.Selected.Value;
				var cmd = new CompoundCommand();
				if (!FWasSelected)
				{
					//unselect all keyframes
					foreach (var track in Instance.Parent.Parent.Tracks.OfType<ValueTrackView>())
						foreach (var kf in track.Keyframes)
							if (kf.Model.Selected.Value)
								cmd.Append(Command.Set(kf.Model.Selected, false));
				}
				//set keyframe selected
				cmd.Append(Command.Set(Instance.Model.Selected, true));
				
				Instance.History.Insert(cmd);
				
				//start collecting movecommands in drag
				FMoveCommands = new CompoundCommand();
				
				return ret;
			}
			else
				return null;
		}
		
		public override void MouseDrag(object sender, PointF arg, PointF delta, int callNr)
		{
			//used to store the tracks which need to build the curves after keyframe drag
			FAlteredTracks = new List<ValueTrackView>();
			
			var cmd = new CompoundCommand();
			foreach (var track in Instance.Parent.Parent.Tracks.OfType<ValueTrackView>())
			{
				var addTrack = false;
				foreach (var kf in track.Keyframes)
				{
					if (kf.Model.Selected.Value)
					{
						var dx = Instance.Parent.Parent.Ruler.XDeltaToTime(delta.X);
						cmd.Append(Command.Set(kf.Model.Time, kf.Model.Time.Value + dx));
						var dy = track.YDeltaToValue(delta.Y);
						cmd.Append(Command.Set(kf.Model.Value, Math.Min(track.Model.Maximum.Value, Math.Max(track.Model.Minimum.Value, kf.Model.Value.Value + dy))));
						FWasMoved = true;
						addTrack = true;
					}
				}
				
				if (addTrack)
					FAlteredTracks.Add(track);
			}
			
			//execute changes immediately
			cmd.Execute();
			//collect changes for history
			FMoveCommands.Append(cmd);
			
			Instance.Parent.Parent.UpdateScene();
			
		}

		public override IMouseEventHandler MouseUp(object sender, MouseArg arg)
		{
			//unselect?
			if(FWasSelected && !FWasMoved)
			{
				Instance.History.Insert(Command.Set(Instance.Model.Selected, false));
			}
			else
			{
				foreach (var track in FAlteredTracks)
				{
					track.Model.BuildCurves();
				}
				
				//add collected commands to history
				Instance.History.InsertOnly(FMoveCommands);
				
			}
			
			return base.MouseUp(sender, arg);
		}
	}
	
	internal class LoopRegionMouseHandler : MouseHandlerBase<RulerView>
	{
		private EditableProperty<float> FMarker;
		private CompoundCommand FMoveCommands;
		public LoopRegionMouseHandler(RulerView rv, EditableProperty<float> marker, string sessionID)
			: base(rv, sessionID)
		{
			FMarker = marker;
			
			//start collecting movecommands in drag
			FMoveCommands = new CompoundCommand();
		}
		
		public override void MouseDrag(object sender, PointF arg, PointF delta, int dragCall)
		{
			if (delta.X != 0)
			{
				var x = Instance.XDeltaToTime(delta.X);
				var cmd = Command.Set(FMarker, FMarker.Value + x);
				//execute changes immediately
				cmd.Execute();
				//collect changes for history
				FMoveCommands.Append(cmd);
				
				Instance.Parent.UpdateScene();
			}
		}
		
		public override IMouseEventHandler MouseUp(object sender, MouseArg arg)
		{
			//add collected commands to history
			Instance.History.InsertOnly(FMoveCommands);
			
			return base.MouseUp(sender, arg);
		}
	}
	
	internal class SeekHandler : MouseHandlerBase<RulerView>
	{
		public SeekHandler(RulerView view, string sessionID)
			: base(view, sessionID)
		{
		}
		
		public override void MouseClick(object sender, MouseArg arg)
		{
			Instance.Parent.Timer.Time = Instance.XPosToTime(arg.x);
		}
	}
}
