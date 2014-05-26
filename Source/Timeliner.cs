#region usings
using System;
using System.Xml.Linq;
using Posh;
using VVVV.Core;
using VVVV.Core.Commands;
using VVVV.Core.Model;

#endregion usings
namespace Timeliner
{
    public class Timeliner: IDisposable
	{
		//public bool Play;
		public float SeekTime;
		public bool DoSeek;
		
        public TLDocument TimelineModel;
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
			
            TimelineModel = new TLDocument("", @"timeline.xml");
            Shell.Instance.Root = TimelineModel;
            
            TimelineModel.CreateMapper(context.MappingRegistry);
            //only after mapper and root are set
            TimelineModel.Initialize();

            var commandHistory = TimelineModel.Mapper.Map<ICommandHistory>();
            TimelineView = new TimelineView(TimelineModel, commandHistory, Timer);
            
            TimelineModel.Tracks.Added += Timeline_Tracks_Added;
			TimelineModel.Tracks.Removed += Timeline_Tracks_Removed;
			TimelineModel.Tracks.OrderChanged += Timeline_Tracks_OrderChanged;
			
			TimelineView.Tracks.OrderChanged += TimelineView_Tracks_OrderChanged;
		}

		public void Dispose()
		{
            TimelineView.Dispose();
            TimelineModel.Dispose();			
		}
		#endregion constructor/destructor
        
        void TimelineView_Tracks_OrderChanged(IViewableList<TrackView> list)
		{
			if (TrackOrderChanged != null)
				TrackOrderChanged();
		}
		
		public void Evaluate(float hosttime)
		{
			Timer.HostTime = hosttime;
			if (DoSeek)
				Timer.Time = SeekTime;

			Timer.LoopStart = TimelineModel.Ruler.LoopStart.Value;
			Timer.LoopEnd = TimelineModel.Ruler.LoopEnd.Value;
			
			Timer.Evaluate();
			TimelineModel.Evaluate(Timer.Time);
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
			TimelineModel.LoadFromXML(data, TimelineModel.GetSerializer());
		}
		
		public void Save(string path)
		{
			TimelineModel.SaveTo(path);
		}
	}
}