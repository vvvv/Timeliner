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
	
	internal class PanZoomMenuHandler : MouseHandlerBase<TLViewBase>
	{
		bool FWasMoved;
		RulerView FRulerView;
		TimelineView FTimelineView;
		TrackView FTrackView;
		
		public PanZoomMenuHandler(TLViewBase view, string sessionID)
			: base(view, sessionID)
		{
			if (Instance is RulerView)
			{
				FRulerView = Instance as RulerView;
				FTimelineView = FRulerView.Parent;
			}
			else if (Instance is TimelineView)
			{
				FTimelineView = Instance as TimelineView;
				FRulerView = FTimelineView.Ruler;
			}
			else if (Instance is TrackView)
			{
				FTrackView = Instance as TrackView;
				FTimelineView = FTrackView.Parent;
				FRulerView = FTimelineView.Ruler;
			}
		}
		
		public override void MouseDrag(object sender, PointF arg, PointF delta, int callNr)
		{
			FWasMoved = true;
			var zoom = Math.Abs(delta.X) < Math.Abs(delta.Y);
			
			if (zoom)
				FRulerView.PanZoom(0, delta.Y, arg.X);
			else
				FRulerView.PanZoom(delta.X, 0, arg.X);
			
			foreach (var tv in FTimelineView.Tracks)
				tv.View = FRulerView.PanZoomMatrix;
			
			FTimelineView.UpdateScene();
		}
		
		public override void MouseClick(object sender, MouseArg arg)
		{
			if (!FWasMoved)
				if (Instance == FRulerView)
			{
				var rulerHandler = new RulerMouseHandler(FRulerView, null, null, SessionID);
				rulerHandler.MouseClick(sender, arg);
			}
			else if (Instance == FTimelineView)
			{
				FTimelineView.ShowMenu(arg);
			}
			else if (Instance == FTrackView)
			{
				FTrackView.ShowTrackMenu(arg);
			}

			base.MouseClick(sender, arg);
		}
	}
	
	internal class TrackResizeMouseHandler : MouseHandlerBase<TrackView>
	{
		CompoundCommand FMoveCommands;
		
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
			if(delta.Y != 0)
			{
				var h = Math.Max(Instance.Model.CollapsedHeight, Instance.Model.Height.Value + delta.Y);
				if ((delta.Y < 0) || (arg.Y > Instance.Top + Instance.Height))
				{
					//scale this track
					var cmds = Command.Set(Instance.Model.Height, h);
					
					//execute changes immediately
					cmds.Execute();
					//collect changes for history
					FMoveCommands.Append(cmds);
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
		int FNewOrder;
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
					FNewOrder = target.Model.Order.Value;
					
					var oldOrder = Instance.Model.Order.Value;
					if (FNewOrder > oldOrder)
						FNewOrder -= 1;
				}
				else
				{
					Instance.SizeBarDragRect.Y = Instance.Parent.Tracks.Max(x => x.Top + x.Height) - Instance.SizeBarDragRect.Height / 2 - (Instance.MainGroup.Parent.Transforms[0] as SvgTranslate).Y  - Instance.SizeBar.Height / 2;
					FNewOrder = Instance.Parent.Tracks.Count-1;
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
				
				//send a commmand to set the order of each trackview
				var cmds = new CompoundCommand();
				if (FNewOrder > oldOrder)
				{
					foreach (var track in Instance.Parent.Tracks.Where(x => x != Instance))
						if ((track.Model.Order.Value > oldOrder) && (track.Model.Order.Value <= FNewOrder))
							cmds.Append(Command.Set(track.Model.Order, track.Model.Order.Value - 1));
				}
				else
				{
					foreach (var track in Instance.Parent.Tracks.Where(x => x != Instance))
						if ((track.Model.Order.Value >= FNewOrder) && (track.Model.Order.Value < oldOrder))
							cmds.Append(Command.Set(track.Model.Order, track.Model.Order.Value + 1));
				}
				
				cmds.Append(Command.Set(Instance.Model.Order, FNewOrder));
				
				Instance.History.Insert(cmds);
				//resort tracks after order
				//Instance.Parent.Tracks.Sort((t1, t2) => t1.Model.Order.Value.CompareTo(t2.Model.Order.Value));
			}
			
			return base.MouseUp(sender, arg);
		}
	}
	
	internal class SelectionMouseHandler : MouseHandlerBase<TrackView>
	{
		CompoundCommand FSelectionCommands;
		List<KeyframeView> FPreviouslySelected = new List<KeyframeView>();
		
		public SelectionMouseHandler(TrackView view, string sessionID)
			
			: base(view, sessionID)
		{
		}
		
		public override IMouseEventHandler MouseDown(object sender, MouseArg arg)
		{
			if (arg.CtrlKey || arg.AltKey)
			{
				foreach (var track in Instance.Parent.Tracks)
					foreach (var kf in track.KeyframeViews.Where(x => x.Model.Selected.Value))
						FPreviouslySelected.Add(kf);
			}
			else //deselect keyframes
			{
				var cmds = new CompoundCommand();
				foreach (var track in Instance.Parent.Tracks)
					foreach (var kf in track.KeyframeViews.Where(x => x.Model.Selected.Value))
						cmds.Append(Command.Set(kf.Model.Selected, false));

				if (cmds.CommandCount > 0)
					Instance.History.Insert(cmds);
			}
			
			//start collecting movecommands in drag
			FSelectionCommands = new CompoundCommand();
			
			return base.MouseDown(sender, arg);
		}
		
		public override void MouseSelection(object sender, MouseArg arg, RectangleF rect)
		{
			var cmds = new CompoundCommand();
			
			foreach (var track in Instance.Parent.Tracks)
			{
				var trackRect = track.ToTrackRect(rect);
				foreach (var kf in track.KeyframeViews)
				{
					var wasSelected = kf.Model.Selected.Value;
					var isSelected = kf.IsSelectedBy(trackRect);
					
					if (arg.AltKey)
					{
						if (FPreviouslySelected.Contains(kf)) //only remove from previously selected
							cmds.Append(Command.Set(kf.Model.Selected, !isSelected));
					}
					else if (arg.CtrlKey)
					{
						if (!FPreviouslySelected.Contains(kf)) //only add to previously selected
							cmds.Append(Command.Set(kf.Model.Selected, isSelected));
					}
					else if (isSelected != wasSelected) //only send selection change
						cmds.Append(Command.Set(kf.Model.Selected, isSelected));
				}
			}
			
			if(cmds.CommandCount > 0)
			{
				//execute changes immediately
				cmds.Execute();
				
				//collect changes for history
				FSelectionCommands.Append(cmds);
			}
			
			rect = new RectangleF(rect.X, rect.Y - Instance.Parent.FTrackGroup.Transforms[0].Matrix.Elements[5], rect.Width, rect.Height);
			Instance.Parent.SetSelectionRect(rect);
			Instance.Parent.UpdateScene();
		}
		
		public override IMouseEventHandler MouseUp(object sender, MouseArg arg)
		{
			FPreviouslySelected.Clear();
			
			Instance.Parent.SetSelectionRect(new RectangleF(0, 0, 0, 0));
			
			//add collected commands to history
			Instance.History.InsertOnly(FSelectionCommands);
			
			return base.MouseUp(sender, arg);
		}
	}
	
	internal class KeyframeMouseHandler : MouseHandlerBase<KeyframeView>
	{
		public KeyframeMouseHandler(KeyframeView view, string sessionID)
			: base(view, sessionID)
		{
		}
		
		bool FWasSelected;
		List<TrackView> FAffectedTracks = new List<TrackView>();
		List<KeyframeView> FSelectedKeyframes = new List<KeyframeView>();
		List<float> FActualValues = new List<float>();
		CompoundCommand FMoveCommands;
		
		//delete on right click
		public override IMouseEventHandler MouseDown(object sender, MouseArg arg)
		{
			var ret = base.MouseDown(sender, arg);
			if ((arg.Button == 1) || (arg.Button == 3))
			{
				FWasSelected = Instance.Model.Selected.Value;
				var cmd = new CompoundCommand();
				if ((!FWasSelected) && (!arg.CtrlKey))
				{
					//unselect all keyframes
					foreach (var track in Instance.Parent.Parent.Tracks)
						foreach (var kf in track.KeyframeViews.Where(x => x.Model.Selected.Value))
							cmd.Append(Command.Set(kf.Model.Selected, false));
				}
				//set keyframe selected
				cmd.Append(Command.Set(Instance.Model.Selected, true));
				Instance.History.Insert(cmd);
				
				//start collecting movecommands in drag
				FMoveCommands = new CompoundCommand();
				
				//store initial values to operate on for being able to drag beyond min/max
				foreach (var track in Instance.Parent.Parent.Tracks)
					foreach (var kf in track.KeyframeViews.Where(x => x.Model.Selected.Value))
				{
					if (!FAffectedTracks.Contains(track))
						FAffectedTracks.Add(track);
					
					FSelectedKeyframes.Add(kf);
					
					if(kf is ValueKeyframeView)
					{
						FActualValues.Add((kf as ValueKeyframeView).Model.Value.Value);
					}
				}
			}
			return ret;
		}
		
		public override void MouseClick(object sender, MouseArg arg)
		{
			if (arg.Button == 3)
			{
				Instance.Parent.UpdateKeyframeMenu(Instance);
				Instance.Parent.ShowKeyframeMenu(arg);
			}
		}

		public override void MouseDrag(object sender, PointF arg, PointF delta, int callNr)
		{
			if(FMoveCommands != null)
			{
				var cmds = new CompoundCommand();
				
				var dx = Instance.Parent.Parent.Ruler.XDeltaToTime(delta.X);
				var i = 0;
				
				foreach (var kf in FSelectedKeyframes)
				{
					//set time
					cmds.Append(Command.Set(kf.Model.Time, kf.Model.Time.Value + dx));
					
					//set value if value keyframe
					if(kf is ValueKeyframeView)
					{
						var vkf = kf as ValueKeyframeView;
						
						var dy = vkf.Parent.YDeltaToValue(delta.Y);
						var min = vkf.Parent.Model.Maximum.Value;
						var max = vkf.Parent.Model.Minimum.Value;
						
						FActualValues[i] += dy;

						if (!FAffectedTracks.Any(x => x.Collapsed))
							cmds.Append(Command.Set(vkf.Model.Value, Math.Min(min, Math.Max(max, FActualValues[i]))));
						
						i++;
					}
				}
				
				//execute changes immediately
				cmds.Execute();
				//collect changes for history
				FMoveCommands.Append(cmds);
				
				Instance.Parent.Parent.UpdateScene();
			}
		}

		public override IMouseEventHandler MouseUp(object sender, MouseArg arg)
		{
			if (arg.Button == 1)
			{
				//unselect?
				if(FWasSelected && !(FMoveCommands.CommandCount > 0))
				{
					Instance.History.Insert(Command.Set(Instance.Model.Selected, false));
				}
				else
				{
					//add collected commands to history
					if(FMoveCommands.CommandCount > 0)
						Instance.History.InsertOnly(FMoveCommands);
				}
			}
			
			return base.MouseUp(sender, arg);
		}
	}
	
	internal class RulerMouseHandler : MouseHandlerBase<RulerView>
	{
		EditableProperty<float> FStart;
		EditableProperty<float> FEnd;
		CompoundCommand FMoveCommands;
		
		public RulerMouseHandler(RulerView rv, EditableProperty<float> start, EditableProperty<float> end, string sessionID)
			: base(rv, sessionID)
		{
			FStart = start;
			FEnd = end;
			
			//start collecting movecommands in drag
			FMoveCommands = new CompoundCommand();
		}
		
		public override void MouseDrag(object sender, PointF arg, PointF delta, int dragCall)
		{
			if (delta.X != 0)
			{
				var cmds = new CompoundCommand();
				var x = Instance.XDeltaToTime(delta.X);
				var mouseX = Instance.XPosToTime(arg.X);
				
				//move start/end
				if ((FStart == null) || (FEnd == null))
				{
					if ((FStart != null) && (mouseX < Instance.Model.LoopEnd.Value))
						cmds.Append(Command.Set(FStart, mouseX));
					if ((FEnd != null) && (mouseX > Instance.Model.LoopStart.Value))
						cmds.Append(Command.Set(FEnd, mouseX));
				}
				else //move region
				{
					cmds.Append(Command.Set(FStart, FStart.Value + x));
					cmds.Append(Command.Set(FEnd, FEnd.Value + x));
				}
				
				//execute changes immediately
				cmds.Execute();
				//collect changes for history
				FMoveCommands.Append(cmds);
				
				Instance.Parent.UpdateScene();
			}
		}
		
		public override IMouseEventHandler MouseUp(object sender, MouseArg arg)
		{
			//add collected commands to history
			Instance.History.InsertOnly(FMoveCommands);
			
			return base.MouseUp(sender, arg);
		}
		
		public override void MouseClick(object sender, MouseArg arg)
		{
			if (FMoveCommands.CommandCount == 0)
				switch (arg.Button)
			{
				case 3:
					//show rulermenu
					Instance.ShowMenu(arg);
					break;
			}
		}
	}
	
	internal class SeekHandler : MouseHandlerBase<RulerView>
	{
		public SeekHandler(RulerView view, string sessionID)
			: base(view, sessionID)
		{
		}
		
		public override IMouseEventHandler MouseDown(object sender, MouseArg arg)
		{
		    Instance.Parent.Timer.Time = Instance.XPosToTime(arg.x);
		    return base.MouseDown(sender, arg);
		}
		
		public override void MouseDrag(object sender, PointF arg, PointF delta, int callNr)
		{
			Instance.Parent.Timer.Time = Instance.XPosToTime(arg.X);
		}
	}
}
