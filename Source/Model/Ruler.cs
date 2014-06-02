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
    	
    	public EditableProperty<float> LoopStart
    	{
            get;
            private set;
        }
    	
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
        	LoopStart = new EditableProperty<float>("LoopStart");
        	LoopEnd = new EditableProperty<float>("LoopEnd");
        	LoopEnd.Value = 10;
        	//panzoom matrix
        	
        	Add(Marker);
        	Add(LoopStart);
        	Add(LoopEnd);        		
        }

        public virtual void LoadingFinished()
        {
        
        }
    }
}
