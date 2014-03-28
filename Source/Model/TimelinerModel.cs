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
        
        public TLTrack()
        	: this(IDGenerator.NewID)
        {
        }

        public TLTrack(string name)
            : base(name)
        {
            Order = new EditableProperty<int>("Order");
            Height = new EditableProperty<float>("Height");
            Height.Value = 100;
            Height.AllowChange = (p, v) => v >= 30;
            
            UncollapsedHeight = new EditableProperty<float>("Uncollapsed Height");
            UncollapsedHeight.Value = Height.Value;
            
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
