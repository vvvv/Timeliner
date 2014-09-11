using System;
using System.Collections.Generic;
using System.Linq;

using VVVV.Core;
using VVVV.Core.Model;

namespace Timeliner
{
    public class TLModelBase : IDContainer
    {
        public TLModelBase(string name)
            : base(name)
        {
            Mapper = Shell.Instance.Root.Mapper.CreateChildMapper(this);
        }
    }
    
    /// <summary>
    /// Base class for keyframes, has only time and selected as property
    /// </summary>
    public abstract class TLKeyframeBase : TLModelBase
    {
    	[KeyframeMenuEntry]
    	public EditableProperty<float> Time { get; private set; }
    	
    	public EditableProperty<bool> Selected { get; private set; }
    	
    	public TLKeyframeBase NeighbourLeft
        {
        	get;
        	set;
        }
        
        public TLKeyframeBase NeighbourRight
        {
        	get;
        	set;
        }
    	
    	public TLKeyframeBase()
    		: this(IDGenerator.NewID)
    	{
    	}
    	
    	public TLKeyframeBase(string name)
    		: this(name, 0)
    	{
    	}
    	
    	public TLKeyframeBase(string name, float time)
    		: base(name)
    	{
    		Time = new EditableProperty<float>("Time", time);
    		Selected = new EditableProperty<bool>("Selected", false);
    		Add(Time);
    		//do not add seleted to container so it will not be serialized
    		//Add(Selected); 
    		Time.ValueChanged += Time_ValueChanged;
    	}
    	
    	void Time_ValueChanged(IViewableProperty<float> property, float newValue, float oldValue)
        {
        	if(NeighbourLeft != null)
        	{
        		if(NeighbourLeft.Time.Value > this.Time.Value)
        		{
        			NeighbourChanged();
        			return;
        		}
        	}
        	
        	if(NeighbourRight != null)
        	{
        		if(NeighbourRight.Time.Value < this.Time.Value)
        		{
        			NeighbourChanged();
        			return;
        		}
        	}
        }
                
        public Action NeighbourChanged = () => {};
    }

    public abstract class TLTrackBase : TLModelBase
    {
        public float CCollapsedHeight = 15;
        
    	public abstract IEnumerable<TLKeyframeBase> KeyframeModels
    	{
    		get;
    	}
    	
        public EditableProperty<int> Order
        {
            get;
            private set;
        }
        
        public EditableProperty<float> Height
        {
        	get;
            private set;
        }
        
        public EditableProperty<float> UncollapsedHeight
        {
        	get;
            private set;
        }

        //the Name property cannot have spaces so we need an extra Label property
        [TrackMenuEntry(Order=0, Height=30)]
        public EditableProperty<string> Label
        {
            get;
            private set;
        }
        
        bool FLoading;
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
        
        public float CollapsedHeight
        {
            get {return CCollapsedHeight;}
        }
        
        public TLTrackBase()
        	: this(IDGenerator.NewID)
        {
        }

        public TLTrackBase(string name)
            : base(name)
        {
        	UncollapsedHeight = new EditableProperty<float>("Uncollapsed Height");
            UncollapsedHeight.Value = 75;
            
            Order = new EditableProperty<int>("Order");
            Height = new EditableProperty<float>("Height");
            Height.Value = UncollapsedHeight.Value;
            Height.AllowChange = (p, v) => v >= CollapsedHeight;
            
            Label = new EditableProperty<string>("Label");
            Label.Value = this.GetID();
            Add(Order);
            Add(Height);
        }

        public virtual void LoadingFinished()
        {
        
        }
        
        public virtual void Evaluate(float time)
        {

        }
        
        protected abstract void SortKeyframeList();
        
        public virtual void SortAndAssignNeighbours()
        {
        	//make sure keyframes are sorted
        	SortKeyframeList();
        	
        	var keyframes = KeyframeModels.ToList();
        	
        	//assign neighbours
        	if(keyframes.Count > 1)
        	{       		
        		//arrange neighbours
        		keyframes[0].NeighbourLeft = null;
        		keyframes[0].NeighbourRight = keyframes[1];
        		for (int i = 1; i < keyframes.Count-1; i++)
        		{
        			keyframes[i].NeighbourLeft = keyframes[i-1];
        			keyframes[i].NeighbourRight = keyframes[i+1];
        		}
        		keyframes[keyframes.Count - 1].NeighbourLeft = keyframes[keyframes.Count - 2];
        		keyframes[keyframes.Count - 1].NeighbourRight = null;
        	}
        	else if (keyframes.Count == 1)
        	{
        		keyframes[0].NeighbourLeft = null;
        		keyframes[0].NeighbourRight = null;
        	}
        }
        
        /// <summary>
        /// For display purposes
        /// </summary>
        public abstract string GetCurrentValueAsString();
    	
        /// <summary>
        /// The current value as object
        /// </summary>
        /// <returns></returns>
		public abstract object GetCurrentValueAsObject();
		
    }
    
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class TrackMenuEntryAttribute: System.Attribute
    {
    	public int Order;
    	public int Height;
    	
        public TrackMenuEntryAttribute()
        {
        	Height = 20;
        }
    }
    
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class KeyframeMenuEntryAttribute: System.Attribute
    {
    	public int Order;
    	public int Height;
    	
        public KeyframeMenuEntryAttribute()
        {
        	Height = 20;
        }
    }
}
