using System;
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
    	public EditableProperty<float> Time { get; private set; }
    	public EditableProperty<bool> Selected { get; private set; }
    	
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
    		Add(Selected);
    	}
    }

    public abstract class TLTrack : TLModelBase
    {
    	
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
        public EditableProperty<string> Label
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
        
        public float CollapsedHeight
        {
            get {return 50;}
        }
        
        public TLTrack()
        	: this(IDGenerator.NewID)
        {
        }

        public TLTrack(string name)
            : base(name)
        {
            Order = new EditableProperty<int>("Order");
            Height = new EditableProperty<float>("Height");
            Height.Value = CollapsedHeight;
            Height.AllowChange = (p, v) => v >= CollapsedHeight;
            
            UncollapsedHeight = new EditableProperty<float>("Uncollapsed Height");
            UncollapsedHeight.Value = 150;
            
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
    }
}
