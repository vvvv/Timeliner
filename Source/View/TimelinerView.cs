using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;

using Posh;
using Svg;
using Svg.Transforms;
using VVVV.Core;
using VVVV.Core.Collections;
using VVVV.Core.Collections.Sync;
using VVVV.Core.Commands;
using VVVV.Utils;

namespace Timeliner
{
	/// <summary>
	/// View class of the timeliner root object.
	/// </summary>
	public class TimelineView : TLViewBase
	{
		private static string SResourcePath;
		public static string ResourcePath
		{
			get
			{
				if(string.IsNullOrEmpty(SResourcePath))
				{
					SResourcePath = (new System.Uri(Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath;
					SResourcePath = Path.Combine(Path.GetDirectoryName(SResourcePath), "Resources");
				}
				
				return SResourcePath;
			}
		}
		
		public TLDocument Document;
		public SvgDocument SvgRoot = new SvgDocument();
        
		public RulerView Ruler;
		public EditableList<TrackView> Tracks = new EditableList<TrackView>();
		private Synchronizer<TrackView, TLTrackBase> Syncer;

		public SvgDefinitionList Definitions = new SvgDefinitionList();
		private SvgDocumentWidget PlayButton;
        private SvgDocumentWidget StopButton;
        
        public SvgGroup FRulerGroup = new SvgGroup();
        
		public SvgGroup FTrackGroup = new SvgGroup();
		private SvgRectangle Background = new SvgRectangle();
        private SvgRectangle SizeBar = new SvgRectangle();
		
		public SvgGroup FOverlaysGroup = new SvgGroup();
		public SvgRectangle Selection = new SvgRectangle();
        public SvgRectangle TimeBar = new SvgRectangle();
        public SvgLine MouseTimeLine = new SvgLine();
        public SvgText MouseTimeLabel = new SvgText();
        public SvgMenuWidget MainMenu;
        
        public SvgMenuWidget NodeBrowser;
        
        public Timer Timer;
        
        public TrackView ActiveTrack;
		
		public TimelineView(TLDocument tl, ICommandHistory history, Timer timer)
		{
			History = history;
			History.CommandInserted += History_Changed;
			History.Undone += History_Changed;
			History.Redone += History_Changed;
			
            Document = tl;
            Timer = timer;
             	
            //replace id manager before any svg element was added
            var caller = Document.Mapper.Map<ISvgEventCaller>();
            var manager = new SvgIdManager(SvgRoot, caller, Document.Mapper.Map<RemoteContext>());
            SvgRoot.ID = "svg";
            SvgRoot.OverwriteIdManager(manager);
            
            Background.Width = new SvgUnit(SvgUnitType.Percentage, 100);
            Background.Height = 500;
            Background.ID = Document.GetID() + "_Background";
            Background.Opacity = 0;
            
            Background.MouseDown += Default_MouseDown;
            Background.MouseMove += Default_MouseMove;
            Background.MouseUp += Default_MouseUp;
            Background.Click += Background_Click;
            
            Selection.ID = "Selection";
            Selection.CustomAttributes["pointer-events"] = "none";
            Selection.CustomAttributes["class"] = "selection";
            
            Ruler = new RulerView(Document.Ruler, this);
            
            MouseTimeLine.ID = "MouseTime";
            MouseTimeLine.StartX = 0;
            MouseTimeLine.StartY = 0;
            MouseTimeLine.EndX = 0;
            
            MouseTimeLabel.ID = "MouseTimeLabel";
            MouseTimeLabel.FontSize = 14;
            
            SizeBar.Width = Background.Width;
			SizeBar.Height = 10;
			SizeBar.ID = "SizeBar";
			SizeBar.Y = Ruler.Height;
            SizeBar.MouseDown += Default_MouseDown;
            SizeBar.MouseMove += Default_MouseMove;
            SizeBar.MouseUp += Default_MouseUp;
            
            TimeBar.ID = "Timebar";
            TimeBar.Y = -Ruler.Height - SizeBar.Height;
            TimeBar.X = -1;
            TimeBar.Width = 2;
            TimeBar.MouseDown += Default_MouseDown;
            TimeBar.MouseMove += Default_MouseMove;
            TimeBar.MouseUp += Default_MouseUp;
            
            PlayButton = SvgDocumentWidget.Load(Path.Combine(TimelineView.ResourcePath, "PlayButton.svg"), caller, 2);
            StopButton = SvgDocumentWidget.Load(Path.Combine(TimelineView.ResourcePath, "StopButton.svg"), caller, 1);
            StopButton.X = 25;
            
            PlayButton.Click += PlayButton_Click;
            StopButton.Click += StopButton_Click;
            
            MainMenu = new SvgMenuWidget(120);
            MainMenu.ID = "MainMenu";
            
            var addValueTrack = new SvgButtonWidget(0, 20, "Add Value Track");
            addValueTrack.ValueChanged += AddValueTrack;
            
            var addStringTrack = new SvgButtonWidget(0, 20, "Add String Track");
            addStringTrack.ValueChanged += AddStringTrack;
            
            MainMenu.AddItem(addValueTrack, 0);
            MainMenu.AddItem(addStringTrack, 1);
            
            FRulerGroup.ID = "Ruler";
            FRulerGroup.Transforms = new SvgTransformCollection();
            
        	FTrackGroup.ID = "Tracks";
        	FTrackGroup.Transforms = new SvgTransformCollection();
        	FOverlaysGroup.ID = "Overlays";
        	FOverlaysGroup.Transforms = new SvgTransformCollection();

        	//initialize svg tree
        	BuildSVGRoot();
            
            Syncer = Tracks.SyncWith(Document.Tracks, 
                                        tm => 
                                        {
                                        	TrackView tv;
                                        	if (tm is TLValueTrack)
	                                     		tv = new ValueTrackView(tm as TLValueTrack, this, Ruler);
                                        	else if(tm is TLStringTrack)
                                        		tv = new StringTrackView(tm as TLStringTrack, this, Ruler);
											else 
												tv = new AudioTrackView(tm as TLAudioTrack, this, Ruler);
											
											if (ActiveTrack == null)
        										ActiveTrack = tv;
	                                     	tv.AddToSceneGraphAt(FTrackGroup);
	                                     	return tv;
                                        },
                                     	tv => 
                                     	{
	                                     	var order = tv.Model.Order.Value;
	                                     	tv.Dispose();
	                                     	
	                                     	//update Order on all tracks below the one removed
	                                     	foreach (var track in Tracks.Where(x => x.Model.Order.Value > order))
	                                     			track.Model.Order.Value -= 1;
                                     	});
		}

		public override void Dispose()
		{
			History.CommandInserted -= History_Changed;
			History.Undone -= History_Changed;
			History.Redone -= History_Changed;
            
            SizeBar.MouseDown -= Default_MouseDown;
            SizeBar.MouseMove -= Default_MouseMove;
            SizeBar.MouseUp -= Default_MouseUp;
            
            TimeBar.MouseDown -= Default_MouseDown;
            TimeBar.MouseMove -= Default_MouseMove;
            TimeBar.MouseUp -= Default_MouseUp;
            
            Background.MouseDown -= Default_MouseDown;
            Background.MouseMove -= Default_MouseMove;
            Background.MouseUp -= Default_MouseUp;
            Background.Click -= Background_Click;
            
            PlayButton.Click -= PlayButton_Click;
            StopButton.Click -= StopButton_Click;
            
            UnbuildSVG();
			
			base.Dispose();
		}

		void History_Changed(object sender, EventArgs<Command> e)
		{
			UpdateScene();
			
			if (!Tracks.Contains(ActiveTrack))
				if (Tracks.Count > 0)
					ActiveTrack = Tracks[0];
				else 
					ActiveTrack = null;
		}
		
		public void RebuildAfterUpdate()
		{
			foreach(var track in Tracks)
			{
				track.RebuildAfterUpdate();
			}
		}
		
		#region build scenegraph
		public SvgDocument BuildSVGRoot()
		{
			//draw self
            
            //clear
            SvgRoot.Children.Clear();
            FRulerGroup.Children.Clear();
            FTrackGroup.Children.Clear();
            FTrackGroup.Transforms.Clear();
            FOverlaysGroup.Children.Clear();
            FOverlaysGroup.Transforms.Clear();
            
            SvgRoot.Children.Add(Definitions);
            SvgRoot.Children.Add(FRulerGroup);
            Ruler.AddToSceneGraphAt(FRulerGroup);
            SvgRoot.Children.Add(PlayButton);
            SvgRoot.Children.Add(StopButton);
            
            SvgRoot.Children.Add(SizeBar);
            
            var menuOffset = new SvgTranslate(0, Ruler.Height+SizeBar.Height);
            FTrackGroup.Transforms.Add(menuOffset);
            FTrackGroup.Children.Add(Background);
			
			SvgRoot.Children.Add(FTrackGroup);
			
			FOverlaysGroup.Transforms.Add(menuOffset);
			FOverlaysGroup.Children.Add(Selection);
			FOverlaysGroup.Children.Add(TimeBar);
            FOverlaysGroup.Children.Add(MouseTimeLine);
            FOverlaysGroup.Children.Add(MouseTimeLabel);
			FOverlaysGroup.Children.Add(MainMenu);
			FOverlaysGroup.Children.Add(Ruler.Menu);
			SvgRoot.Children.Add(FOverlaysGroup);			
			
			return SvgRoot;
		}
		
		protected override void BuildSVG()
		{
			throw new NotImplementedException("should not call this method of the timeline root");
		}
		
		protected override void UnbuildSVG()
		{
			if(SvgRoot != null)
			{
				var caller = Document.Mapper.Map<ISvgEventCaller>();
				foreach (var element in SvgRoot.Children)
				{
					if(element is SvgDocumentWidget)
					{
						(element as SvgDocumentWidget).UnregisterEvents(caller);
					}
				}
				
				SvgRoot.Children.Clear();
				SvgRoot = null;
			}
		}
		
		#endregion

		#region update scenegraph
		public override void UpdateScene()
		{
			base.UpdateScene();
			
            PlayButton.SetViewBox(Convert.ToInt32(Timer.IsRunning));
			Ruler.UpdateScene();
            			
			foreach (var track in Tracks)
				track.UpdateScene();
			
			var totalHeight = Tracks.Sum(x => x.Height);
			TimeBar.Height = totalHeight + Math.Abs(TimeBar.Y);
			MouseTimeLine.EndY = totalHeight;
			Background.Height = Math.Max(500, totalHeight + 250);
			SvgRoot.CustomAttributes["style"] = "height: "+ Background.Height + "px";
		}
		
		public void SetSelectionRect(RectangleF rect)
		{
			Selection.SetRectangle(rect);
		}
		#endregion
		
		#region scenegraph eventhandler
		void Background_Click(object sender, MouseArg e)
		{
			if (e.ClickCount == 2)
				MainMenu.Show(new PointF(e.x, e.y - FTrackGroup.Transforms[0].Matrix.Elements[5]));
		}
		
		int FTrackCount = 0;
		void AddValueTrack(SvgWidget widget, object newValue, object delta)
		{
			var track = new TLValueTrack(FTrackCount++.ToString());
			track.Order.Value = Document.Tracks.Count;
        	History.Insert(Command.Add(Document.Tracks, track));
		}
		
		void AddStringTrack(SvgWidget widget, object newValue, object delta)
		{
			var track = new TLStringTrack(FTrackCount++.ToString());
			track.Order.Value = Document.Tracks.Count;
        	History.Insert(Command.Add(Document.Tracks, track));
		}
		
		void PlayButton_Click(object sender, MouseArg e)
		{
            Timer.Play(!Timer.IsRunning);
            UpdateScene();
		}

		void StopButton_Click(object sender, MouseArg e)
		{
            Timer.Stop();
            UpdateScene();
		}
		#endregion
		
		public void Evaluate()
		{
			Ruler.Evaluate();
            
			foreach (var track in Tracks)
				track.Evaluate();
		}
		
		//gets the right mouse handler
		protected override IMouseEventHandler GetMouseHandler(object sender, MouseArg e)
		{
			if(sender is IMouseEventHandler)
			{
				return sender as IMouseEventHandler;
			}
			else if(sender is TrackView)
			{
				ActiveTrack = sender as TrackView;
				HideMenus();
				if ((e.Button == 1) && (sender is TrackView))
					return new SelectionMouseHandler(sender as TrackView, e.SessionID);
				else if (e.Button == 2)
					return new TrackMenuHandler(sender as TrackView, e.SessionID);
				else if (e.Button == 3)
					return new TrackPanZoomHandler(this, e.SessionID);
				else 
					return null;
			}
			else if (sender == Ruler.Background)
			{
				HideMenus();
				if (e.Button == 1)
					return new SeekHandler(Ruler, e.SessionID);					
				else if (e.Button == 3)
					return new TrackPanZoomHandler(this, e.SessionID);
				else
					return null;
			}
			else if (sender == Ruler.LoopStart)
			{
				HideMenus();
				if (e.Button == 1)
					return new LoopRegionMouseHandler(Ruler, Ruler.Model.LoopStart, null, e.SessionID);
				else
					return null;					
			}
			else if (sender == Ruler.LoopEnd)
			{
				HideMenus();
				if (e.Button == 1)
					return new LoopRegionMouseHandler(Ruler, null, Ruler.Model.LoopEnd, e.SessionID);
				else
					return null;					
			}
			else if (sender == Ruler.LoopRegion)
			{
				HideMenus();
				if (e.Button == 1)
					return new LoopRegionMouseHandler(Ruler, Ruler.Model.LoopStart, Ruler.Model.LoopEnd, e.SessionID);
				else if (e.Button == 3)
					return new TrackPanZoomHandler(this, e.SessionID);
				else
					return null;					
			}
			else if(sender is KeyframeView)
			{
				HideMenus();
				return new KeyframeMouseHandler(sender as KeyframeView, e.SessionID);
			}
			else if(sender == TimeBar)
			{
				HideMenus();
				return new TimeBarHandler(this, e.SessionID);
			}
			else if (sender == Background) 
			{
				HideMenus();
				if ((e.Button == 1) && (ActiveTrack != null))
					return new SelectionMouseHandler(ActiveTrack, e.SessionID);					
				else if (e.Button == 3)
					return new TrackPanZoomHandler(this, e.SessionID);
				else 
					return null;
			}
			else
			{
				HideMenus();
				return null;
			}
		}
        
        public override void Default_MouseMove(object sender, MouseArg e)
		{
            base.Default_MouseMove(sender, e);
            
            MouseTimeLine.StartX = e.x;
            MouseTimeLine.EndX = e.x;
            
            MouseTimeLabel.X = Math.Max(0, e.x - 110);
            MouseTimeLabel.Text = Timer.TimeToString(Ruler.XPosToTime(e.x));
		}
		
		private void HideMenus()
		{
			foreach (var track in Tracks)
			{
				track.TrackMenu.Hide();
				track.KeyframeMenu.Hide();
			}
			Ruler.Menu.Hide();
			MainMenu.Hide();
		}
	}
}
