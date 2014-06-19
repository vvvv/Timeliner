using System;
using System.Linq;

using VVVV.Core.Collections;
using VVVV.Core.Model;

namespace Timeliner
{
    public class TLRuler : TLModelBase
    {
    	public EditableIDList<TLValueKeyframe> Marker
        {
            get;
            private set;
        }
    	
    	[TrackMenuEntry(Order=0)]
    	public EditableProperty<int> FPS
    	{
            get;
            private set;
        }
    	
    	[TrackMenuEntry(Order=1)]
    	public EditableProperty<float> Speed
    	{
            get;
            private set;
        }
    	
    	[TrackMenuEntry(Order=2)]
    	public EditableProperty<bool> Loop
    	{
            get;
            private set;
        }
    	
    	[TrackMenuEntry(Order=3)]
    	public EditableProperty<float> LoopStart
    	{
            get;
            private set;
        }
    	
    	[TrackMenuEntry(Order=4)]
    	public EditableProperty<float> LoopEnd
    	{
            get;
            private set;
        }
    	
        private bool FLoading;
        public bool Loading
        {
        	get {return FLoading;}
        	set
        	{
        		FLoading = value;
        		if (!FLoading)
        			LoadingFinished();
        	}
        }
        
        public TLRuler()
        	: base(IDGenerator.NewID)
        {
        	Marker = new EditableIDList<TLValueKeyframe>("Marker");
        	
        	FPS = new EditableProperty<int>("FPS");
        	FPS.Value = 30;
        	Speed = new EditableProperty<float>("Speed");
        	Speed.Value = 1;
        	
        	Loop = new EditableProperty<bool>("Loop");
        	Loop.Value = true;
        	LoopStart = new EditableProperty<float>("LoopStart");
        	LoopEnd = new EditableProperty<float>("LoopEnd");
        	LoopEnd.Value = 10;
        	//panzoom matrix
        	
        	Add(Marker);
        	
        	Add(FPS);
        	Add(Speed);
        	Add(Loop);
        	Add(LoopStart);
        	Add(LoopEnd);        		
        }

        public virtual void LoadingFinished()
        {
        
        }
    }
}
