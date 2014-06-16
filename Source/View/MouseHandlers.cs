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
			if(delta.Y != 0)
			{
				var h = Math.Max(Instance.Model.CollapsedHeight, Instance.Model.Height.Value + delta.Y);
				if ((delta.Y < 0) || (arg.Y > Instance.Top + Instance.Height))
				{
					//scale this track
					var cmd = Command.Set(Instance.Model.Height, h);
					
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
				Instance.TrackMenu.Show(new PointF(arg.x, arg.y - Instance.Parent.FTrackGroup.Transforms[0].Matrix.Elements[5]));
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
					foreach (var kf in track.KeyframeViews)
						if (kf.Model.Selected.Value)
							FPreviouslySelected.Add(kf);
			}
			else //deselect keyframes
			{
				var cmds = new CompoundCommand();
				foreach (var track in Instance.Parent.Tracks)
					foreach (var kf in track.KeyframeViews)
				{
					if (kf.Model.Selected.Value)
						cmds.Append(Command.Set(kf.Model.Selected, false));
				}
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
		
		private bool FWasSelected;
		private bool FWasMoved;
		private List<TrackView> FAffectedTracks = new List<TrackView>();
		private List<KeyframeView> FSelectedKeyframes = new List<KeyframeView>();
		private List<float> FActualValues = new List<float>();
		private CompoundCommand FMoveCommands;
		
		//delete on right click
		public override IMouseEventHandler MouseDown(object sender, MouseArg arg)
		{
			var ret = base.MouseDown(sender, arg);
			if ((arg.Button == 1) || (arg.Button == 2))
			{
				FWasSelected = Instance.Model.Selected.Value;
				var cmd = new CompoundCommand();
				if ((!FWasSelected) && (!arg.CtrlKey))
				{
					//unselect all keyframes
					foreach (var track in Instance.Parent.Parent.Tracks)
						foreach (var kf in track.KeyframeViews)
							if (kf.Model.Selected.Value)
								cmd.Append(Command.Set(kf.Model.Selected, false));
				}
				//set keyframe selected
				cmd.Append(Command.Set(Instance.Model.Selected, true));
				Instance.History.Insert(cmd);
				
				//start collecting movecommands in drag
				FMoveCommands = new CompoundCommand();
				
				//store initial values to operate on for being able to drag beyond min/max
				foreach (var track in Instance.Parent.Parent.Tracks)
					foreach (var kf in track.KeyframeViews)
						if (kf.Model.Selected.Value)
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
//				causes total freeze in IE11
//				so for now only delete via keyboard
//				var cmd = Command.Remove(Instance.Parent.Model.Keyframes, Instance.Model);
//				Instance.History.Insert(cmd);
			}
			else if (arg.Button == 2)
			{
				Instance.Parent.UpdateKeyframeMenu(Instance);
				Instance.Parent.KeyframeMenu.Show(new PointF(arg.x, arg.y - Instance.Parent.Parent.FTrackGroup.Transforms[0].Matrix.Elements[5]));
			}
		}

		public override void MouseDrag(object sender, PointF arg, PointF delta, int callNr)
		{
			if(FMoveCommands != null)
			{
				var cmd = new CompoundCommand();
				FWasMoved = true;
				
				var dx = Instance.Parent.Parent.Ruler.XDeltaToTime(delta.X);
				var i = 0;
				
				foreach (var kf in FSelectedKeyframes)
				{
					//set time
					cmd.Append(Command.Set(kf.Model.Time, kf.Model.Time.Value + dx));
					
					//set value if value keyframe
					if(kf is ValueKeyframeView)
					{
						var vkf = kf as ValueKeyframeView;
						
						var dy = vkf.Parent.YDeltaToValue(delta.Y);
						var min = vkf.Parent.Model.Maximum.Value;
						var max = vkf.Parent.Model.Minimum.Value;
						
						FActualValues[i] += dy;

						if (!FAffectedTracks.Any(x => x.Collapsed))
							cmd.Append(Command.Set(vkf.Model.Value, Math.Min(min, Math.Max(max, FActualValues[i]))));
						
						i++;
					}
				}
				
				//execute changes immediately
				cmd.Execute();
				//collect changes for history
				FMoveCommands.Append(cmd);
				
				Instance.Parent.Parent.UpdateScene();
			}
		}

		public override IMouseEventHandler MouseUp(object sender, MouseArg arg)
		{
			if (arg.Button == 1)
			{
				//unselect?
				if(FWasSelected && !FWasMoved)
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
	
	internal class LoopRegionMouseHandler : MouseHandlerBase<RulerView>
	{
		private EditableProperty<float> FStart;
		private EditableProperty<float> FEnd;
		private CompoundCommand FMoveCommands;
		
		public LoopRegionMouseHandler(RulerView rv, EditableProperty<float> start, EditableProperty<float> end, string sessionID)
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
			//set timebar if loopregion was not moved
			if (FMoveCommands.CommandCount == 0)
				Instance.Parent.Timer.Time = Instance.XPosToTime(arg.x);
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
