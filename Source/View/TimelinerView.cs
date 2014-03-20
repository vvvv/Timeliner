using System;
using System.Drawing;
using System.IO;
using System.Reflection;

using Svg;
using Svg.Transforms;
using VVVV.Core;
using VVVV.Core.Collections;
using VVVV.Core.Collections.Sync;
using VVVV.Core.Commands;
using VVVV.Utils;

using Posh;

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
		private Synchronizer<TrackView, TLTrack> Syncer;

		public SvgDefinitionList Definitions = new SvgDefinitionList();
		private SvgDocumentWidget PlayButton;
        private SvgDocumentWidget StopButton;
        
        public SvgGroup FRulerGroup = new SvgGroup();
        
		public SvgGroup FTrackGroup = new SvgGroup();
		private SvgRectangle Background = new SvgRectangle();
		
		public SvgGroup FOverlaysGroup = new SvgGroup();
		public SvgRectangle Selection = new SvgRectangle();
        public SvgLine TimeBar = new SvgLine();
        public SvgMenuWidget MainMenu;
        
        public SvgMenuWidget NodeBrowser;
        
        public Timer Timer;
		
		public TimelineView(TLDocument tl, ICommandHistory history, Timer timer)
		{
			History = history;
			History.CommandInserted += History_CommandInserted;
			
            Document = tl;
            Timer = timer;
             	
            Syncer = Tracks.SyncWith(Document.Tracks, 
                                        tm => 
                                        {
                                        	TrackView tv;
                                        	if (tm is TLValueTrack)
	                                     		tv = new ValueTrackView(tm as TLValueTrack, this, Ruler);
											else 
												tv = new AudioTrackView(tm as TLAudioTrack, this, Ruler);
											
	                                     	tv.BuildSVGTo(FTrackGroup);
	                                     	return tv;
                                        },
                                     	tv => 
                                     	{
	                                     	var order = tv.Model.Order.Value;
	                                     	tv.Dispose();
	                                     	
	                                     	//assign new Order value to each track
	                                     	//and update their position
	                                     	for (int i = order; i < Tracks.Count; i++)
	                                     	{
	                                     		Tracks[i].Model.Order.Value = i;
	                                     		Tracks[i].UpdateScene();
	                                     	}
                                     	});
            
            
            //replace id manager before any svg element was added
            var caller = Document.Mapper.Map<ISvgEventCaller>();
            var manager = new SvgIdManager(SvgRoot, caller, Document.Mapper.Map<RemoteContext>());
            SvgRoot.OverwriteIdManager(manager);
            
            Ruler = new RulerView(Document.Ruler, this);
            
            Background.Width = new SvgUnit(SvgUnitType.Percentage, 100);
            Background.Height = new SvgUnit(SvgUnitType.Percentage, 100);
            Background.Opacity = 0.1f;
            Background.ID = Document.GetID() + "/Background";
            
            Background.MouseDown += Default_MouseDown;
            Background.MouseMove += Default_MouseMove;
            Background.MouseUp += Default_MouseUp;
            
            Selection.ID = "Selection";
            Selection.FillOpacity = 0.1f;
            Selection.CustomAttributes["pointer-events"] = "none";
            
            TimeBar.ID = "TimeBar";
            TimeBar.Stroke = TimelinerColors.Black;
            TimeBar.StartX = 0;
            TimeBar.StartY = 0;
            TimeBar.EndX = 0;
            TimeBar.EndY = new SvgUnit(SvgUnitType.Percentage, 100);
            TimeBar.CustomAttributes["style"] = "cursor:col-resize";
            TimeBar.Transforms = new SvgTransformCollection();
            TimeBar.Transforms.Add(new SvgTranslate(0, 0));
            
            TimeBar.MouseDown += Default_MouseDown;
            TimeBar.MouseMove += Default_MouseMove;
            TimeBar.MouseUp += Default_MouseUp;
            
            PlayButton = SvgDocumentWidget.Load(Path.Combine(TimelineView.ResourcePath, "PlayButton.svg"), caller);
            StopButton = SvgDocumentWidget.Load(Path.Combine(TimelineView.ResourcePath, "StopButton.svg"), caller);
            StopButton.CustomAttributes["x"] = "50"; //TODO: fix in svg lib
            
            PlayButton.MouseDown += PlayButton_MouseDown;
            StopButton.MouseDown += StopButton_MouseDown;
            
            MainMenu = new SvgMenuWidget(100);
            MainMenu.ID = "MainMenu";
            var addTrack = new SvgButtonWidget("Add Track");
            addTrack.OnButtonPressed += AddTrack;
            
            MainMenu.AddItem(addTrack);
            
            FRulerGroup.ID = "Ruler";
            FRulerGroup.Transforms = new SvgTransformCollection();
            
        	FTrackGroup.ID = "Tracks";
        	FTrackGroup.Transforms = new SvgTransformCollection();
        	FOverlaysGroup.ID = "Overlays";
        	FOverlaysGroup.Transforms = new SvgTransformCollection();
        	

//        	NodeBrowser = new SvgMenuWidget(400);
//			var browser = new SvgTextListWidget("bla");
//			browser.OnValueChanged += browser_OnValueChanged;
//			NodeBrowser.AddItem(browser);
//			NodeBrowser.Visible = true;
//			
//			NodeBrowser.Transforms = new SvgTransformCollection();
//			NodeBrowser.Transforms.Add(new SvgTranslate(300, 300));

        	//initialize svg tree
        	BuildSVGRoot();
		}
		
		public override void Dispose()
		{
			History.CommandInserted -= History_CommandInserted;
			
			base.Dispose();
		}

		void History_CommandInserted(object sender, EventArgs<Command> e)
		{
			UpdateScene();
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
            //SvgRoot.ViewBox = new SvgViewBox(0, 0, 1000, 200);
            SvgRoot.Children.Add(PlayButton);
            SvgRoot.Children.Add(StopButton);
            
            FRulerGroup.Transforms.Add(new SvgTranslate(0, PlayButton.Height + 9));
            SvgRoot.Children.Add(FRulerGroup);
            Ruler.BuildSVGTo(FRulerGroup);
            
            var menuOffset = new SvgTranslate(0, PlayButton.Height+30);
            FTrackGroup.Transforms.Add(menuOffset);
            FTrackGroup.Children.Add(Background);
			
			//draw tracks
			foreach (var track in Tracks)
                track.BuildSVGTo(FTrackGroup);
			
			SvgRoot.Children.Add(FTrackGroup);
			
			FOverlaysGroup.Transforms.Add(menuOffset);
			FOverlaysGroup.Children.Add(Selection);
			FOverlaysGroup.Children.Add(TimeBar);
			FOverlaysGroup.Children.Add(MainMenu);
			//FOverlaysGroup.Children.Add(NodeBrowser);
			SvgRoot.Children.Add(FOverlaysGroup);			
			
			return SvgRoot;
		}
		
		protected override void BuildSVG()
		{
			throw new NotImplementedException("should not call this draw method of the timeline root");
		}
		
		protected override void UnbuildSVG()
		{
			throw new NotImplementedException("unbuild timeliner document");
		}
		#endregion

		#region update scenegraph
		public override void UpdateScene()
		{
			base.UpdateScene();
			
			Ruler.UpdateScene();
			
			foreach (var track in Tracks)
				track.UpdateScene();
		}
		
		public void SetSelectionRect(RectangleF rect)
		{
			Selection.SetRectangle(rect);
		}
		#endregion
		
		#region scenegraph eventhandler
		void AddTrack()
		{
			var track = new TLValueTrack(Document.Tracks.Count.ToString());
			track.Order.Value = Document.Tracks.Count;
        	var cmd = Command.Add(Document.Tracks, track);
        	History.Insert(cmd);
		}
		
		void PlayButton_MouseDown(object sender, MouseArg e)
		{
			Timer.IsRunning = !Timer.IsRunning;
			UpdateButtonColor();
		}

		void StopButton_MouseDown(object sender, MouseArg e)
		{
			Timer.IsRunning = false;
			Timer.Time = 0;
			UpdateButtonColor();
		}
		
		void UpdateButtonColor()
		{
			PlayButton.SetBackColor(Timer.IsRunning ? TimelinerColors.Red : TimelinerColors.LightGray);
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
				(sender as TrackView).TrackMenu.Hide();
				HideMenus();
				if ((e.Button == 1) && (sender is ValueTrackView))
					return new SelectionMouseHandler(sender as ValueTrackView, e.SessionID);
				else if (e.Button == 2)
					return new TrackMenuHandler(sender as TrackView, e.SessionID);
				else if (e.Button == 3)
					return new TrackPanZoomHandler(this, e.SessionID);
				else 
					return null;
			}
			else if (sender is RulerView)
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
					return new LoopRegionMouseHandler(Ruler, Ruler.Model.LoopStart, e.SessionID);
				else
					return null;					
			}
			else if (sender == Ruler.LoopEnd)
			{
				HideMenus();
				if (e.Button == 1)
					return new LoopRegionMouseHandler(Ruler, Ruler.Model.LoopEnd, e.SessionID);
				else
					return null;					
			}
			else if(sender is ValueKeyframeView)
			{
				HideMenus();
				return new KeyframeMouseHandler(sender as ValueKeyframeView, e.SessionID);
			}
			else if(sender == TimeBar)
			{
				HideMenus();
				return new TimeBarHandler(this, e.SessionID);
			}
			else
			{
				HideMenus();
				return new MainMenuHandler(this, e.SessionID);
			}
		}
		
		private void HideMenus()
		{
			foreach (var track in Tracks)
				track.TrackMenu.Hide();
			MainMenu.Hide();
		}
	}
}
