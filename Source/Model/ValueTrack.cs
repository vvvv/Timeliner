using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using VVVV.Core;
using VVVV.Core.Collections;
using VVVV.Core.Model;
using VVVV.Utils.VMath;

namespace Timeliner
{
    public class TLValueTrack : TLTrackBase
    {
    	public override IEnumerable<TLKeyframeBase> KeyframeModels
    	{
    		get
    		{
    			return Keyframes;
    		}
    	}
    	
        public EditableIDList<TLValueKeyframe> Keyframes
        {
            get;
            private set;
        }

        public IEditableIDList<TLCurve> Curves
        {
            get;
            private set;
        }
        
        [TrackMenuEntry(Order=2)]
        public EditableProperty<float> Minimum
        {
            get;
            private set;
        }
        
        [TrackMenuEntry(Order=3)]
        public EditableProperty<float> Maximum
        {
            get;
            private set;
        }
        
        public float CurrentValue
        {
        	get;
        	protected set;
        }
        
        public TLValueTrack()
        	: this(IDGenerator.NewID)
        {
        }

        public TLValueTrack(string name)
            : base(name)
        {
        	Keyframes = new EditableIDList<TLValueKeyframe>("Keyframes");
            Curves = new EditableIDList<TLCurve>("Curves");
            Minimum = new EditableProperty<float>("Minimum");
            Minimum.Value = -1f;
            Maximum = new EditableProperty<float>("Maximum");
            Maximum.Value = 1f;
            Add(Keyframes);
            Add(Curves);
            Add(Minimum);
            Add(Maximum);
            Keyframes.Added += Keyframes_Added;
            Keyframes.Removed += Keyframes_Removed;
            
            
            Label.Value = "Value " + name;
        }

        void Keyframes_Removed(IViewableCollection<TLValueKeyframe> collection, TLValueKeyframe item)
        {
            SortAndAssignNeighbours();
        }
        
        void Keyframes_Added(IViewableCollection<TLValueKeyframe> collection, TLValueKeyframe item)
        {
        	if (!Loading)
            	SortAndAssignNeighbours();
        }

        public event EventHandler BeforeBuildingCurves;
        public event EventHandler AfterBuildingCurves;
        
        public override void LoadingFinished()
        {
        	SortAndAssignNeighbours();
        }
        
		protected override void SortKeyframeList()
		{
			//sort the keyframes
        	Keyframes.Sort((a, b) => a.Time.Value.CompareTo(b.Time.Value));
		}

        public override void SortAndAssignNeighbours()
        {
        	if(BeforeBuildingCurves != null)
        		BeforeBuildingCurves(this, null);
        	
        	base.SortAndAssignNeighbours();
        	
        	Curves.Clear();

        	if (Keyframes.Count > 0)
        	{
        		
        		//first curve
        		Curves.Add(new TLCurve("Start" + IDGenerator.NewID, null, Keyframes[0]));

        		//between
        		for (int i = 1; i < Keyframes.Count; i++)
        		{
        			Curves.Add(new TLCurve(IDGenerator.NewID, Keyframes[i - 1], Keyframes[i]));
                }

                //last
                Curves.Add(new TLCurve("End" + IDGenerator.NewID, Keyframes[Keyframes.Count - 1], null));
            }
            
            if(AfterBuildingCurves != null)
            	AfterBuildingCurves(this, null);
        }
        
        public override void Evaluate(float time)
        {   
        	var kfs = Keyframes.ToList(); 
        	var kf = kfs.FindLast(k => k.Time.Value <= time);
        	var kf1 = kfs.Find(k => k.Time.Value >= time);
			
			if (kf == null && kf1 == null)
				CurrentValue = 0;
			else if (kf == null)
				CurrentValue =  kf1.Value.Value;
			else if (kf1 == null)
				CurrentValue =  kf.Value.Value;
			else
			{
				var t = VMath.Map(time, kf.Time.Value, kf1.Time.Value, 0, 1, TMapMode.Float);
				CurrentValue = (float) VMath.Lerp(kf.Value.Value, kf1.Value.Value, t); 
			}
        }
    	
		public override string GetCurrentValueAsString()
		{
			return CurrentValue.ToString("f4");
		}
    	
		public override object GetCurrentValueAsObject()
		{
			return CurrentValue;
		}
    }

    public class TLCurve : TLModelBase
    {
        public TLValueKeyframe Start;
        public TLValueKeyframe End;

        public TLCurve(TLValueKeyframe start, TLValueKeyframe end)
            : this(IDGenerator.NewID, start, end)
        {
        }

        public TLCurve(string name, TLValueKeyframe start, TLValueKeyframe end)
            : base(name)
        {
            Start = start;
            End = end;
        }
    }

    public class TLValueKeyframe : TLKeyframeBase
    {
    	[KeyframeMenuEntry]
        public EditableProperty<float> Value { get; private set; }

        public PointF Position
        {
        	get
        	{
        		return new PointF(Time.Value, Value.Value);
        	}
        }
        
        public TLValueKeyframe()
            : this(IDGenerator.NewID)
        {
        }
        
        public TLValueKeyframe(string name)
            : this(name, 0, 0)
        {
        }
        
        public TLValueKeyframe(float time, float value)
            : this(IDGenerator.NewID, time, value)
        {
        }

        public TLValueKeyframe(string name, float time, float value)
            : base(name, time)
        {
            Value = new EditableProperty<float>("Value", value);
            Add(Value);
        }
    }
}
