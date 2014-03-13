#region usings
using System;
using System.Xml.Linq;

using VVVV.Core;
using VVVV.Core.Commands;
using Posh;

#endregion usings
namespace Timeliner
{
	public class Timeliner
	{
		//public bool Play;
		public float SeekTime;
		public bool DoSeek;
		
        public TLDocument Timeline;
        public TimelineView TimelineView;
       
        public Timer Timer = new Timer();
        
        public TLContext Context;
        
        public Action<string, int> TrackAdded;
        public Action<string> TrackRemoved;
        public Action<string, string> TrackRenamed;
        public Action TrackOrderChanged;
		
		#region constructor/destructor
		public Timeliner(TLContext context)
		{
			Context = context;
			
            Timeline = new TLDocument("", @"timeline.xml");
            Shell.Instance.Root = Timeline;
            
            Timeline.CreateMapper(context.MappingRegistry);
            //only after mapper and root are set
            Timeline.Initialize();

            var commandHistory = Timeline.Mapper.Map<ICommandHistory>();
            TimelineView = new TimelineView(Timeline, commandHistory, Timer);
            
            Timeline.Tracks.Added += Timeline_Tracks_Added;
			Timeline.Tracks.Removed += Timeline_Tracks_Removed;
			Timeline.Tracks.OrderChanged += Timeline_Tracks_OrderChanged;
			
			TimelineView.Tracks.OrderChanged += TimelineView_Tracks_OrderChanged;
		}

		void TimelineView_Tracks_OrderChanged(IViewableList<TrackView> list)
		{
			if (TrackOrderChanged != null)
				TrackOrderChanged();
		}
		#endregion constructor/destructor
		
		public void Evaluate(float hosttime)
		{
			Timer.HostTime = hosttime;
			if (DoSeek)
				Timer.Time = SeekTime;

			Timer.LoopStart = Timeline.Ruler.LoopStart.Value;
			Timer.LoopEnd = Timeline.Ruler.LoopEnd.Value;
			
			Timer.Evaluate();
			Timeline.Evaluate(Timer.Time);
			TimelineView.Evaluate();	
		}

		void Timeline_Tracks_OrderChanged(IViewableList<TLTrack> list)
		{
			if (TrackOrderChanged != null)
				TrackOrderChanged();
		}

		void Timeline_Tracks_Removed(IViewableCollection<TLTrack> collection, TLTrack item)
		{
			item.Label.ValueChanged -= item_Label_ValueChanged;
			if (TrackRemoved != null)
				TrackRemoved(item.Label.Value);
		}

		void Timeline_Tracks_Added(IViewableCollection<TLTrack> collection, TLTrack item)
		{
			item.Label.ValueChanged += item_Label_ValueChanged;
			if (TrackAdded != null)
				TrackAdded(item.Label.Value, item.Order.Value);
		}

		void item_Label_ValueChanged(IViewableProperty<string> property, string newValue, string oldValue)
		{
			if (TrackRenamed != null)
				TrackRenamed(oldValue, newValue);
		}
		
		public void Load(XElement data)
		{
			Timeline = Timeline.GetSerializer().Deserialize<TLDocument>(data);
			Timeline.CreateMapper(Context.MappingRegistry);
			Timeline.Initialize();
			
			TimelineView = new TimelineView(Timeline, Timeline.Mapper.Map<ICommandHistory>(), Timer);
		}
	}
}