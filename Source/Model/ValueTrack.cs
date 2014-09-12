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
                var curve = Curves.Where(c => (c.Start == kf) && (c.End == kf1)).First();
                CurrentValue = curve.GetValue(time);
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
        int FResolution;
        double[] FLut;
        Vector2D FP1, FP2, FC1, FC2;
        
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
        
        public void UpdateCurve()
        {
            if ((Start == null) || (End == null))
                return;
            
            //compute points between kf and kf given the ease modes of both
            FP1 = new Vector2D(Start.Time.Value, Start.Value.Value);
            FP2 = new Vector2D(End.Time.Value, End.Value.Value);
            
            var d = FP2 - FP1;
            FC1 = FP1 + d * (1/3.0);
            FC2 = FP2 - d * (1/3.0);
                                  
            switch (Start.Ease.Value)
            {
                case 2: 
                    case 3: FC1 = new Vector2D(FP1.x + d.x * 0.5, FP1.y); break;
            }

            switch (End.Ease.Value)
            {
                case 1: 
                    case 3: FC2 = new Vector2D(FP2.x - d.x * 0.5, FP2.y); break;
            }
            
            FResolution = (int) (d.x * 100);
            var pts = new Vector2D[FResolution];
            for (int i=0; i<FResolution; i++)
                pts[i] = CalculateBezierPoint(i / (float)(FResolution-1), FP1, FC1, FC2, FP2);
            
            //create a LUT that for each t along the curve saves the length traveled on the path (by adding up the length between consecutive points)
            FLut = new double[FResolution];
            for (int i=0; i<FResolution-1; i++)
            {
                var length = VMath.Dist(pts[i], pts[i+1]);
                FLut[i+1] = FLut[i] + length;
            }
        }
        
        public float GetValue(float time)
        {
            //given x to sample the curve [0..1] consider x as length traveled along path and from LUT find t it takes to get to that point
            var x = VMath.Map(time, Start.Time.Value, End.Time.Value, 0, 1, TMapMode.Clamp);
            x *= FLut.Last();
            
            var t = 0f;
            for (int i=0; i<FResolution; i++)
            {
                if (FLut[i] > x)
                {
                    t = i + (float)VMath.Map(x, FLut[i-1], FLut[i], 0, 1, TMapMode.Clamp);
                    t /= (FResolution-1);
                    break;
                }
            }
            
            //from that t again get the bezierpoint
            return (float)CalculateBezierPoint(t, FP1, FC1, FC2, FP2).y;
        }
        
        Vector2D CalculateBezierPoint(float t, Vector2D p0, Vector2D p1, Vector2D p2, Vector2D p3)
        {
            float u = 1 - t;
            float tt = t*t;
            float uu = u*u;
            float uuu = uu * u;
            float ttt = tt * t;
            
            var p = uuu * p0; //first term
            p += 3 * uu * t * p1; //second term
            p += 3 * u * tt * p2; //third term
            p += ttt * p3; //fourth term
            
            return p;
        }
    }

    public class TLValueKeyframe : TLKeyframeBase
    {
        [KeyframeMenuEntry]
        public EditableProperty<float> Value { get; private set; }
        
        [KeyframeMenuEntry]
        public EditableProperty<int> Ease { get; private set; }

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
            
            Ease = new EditableProperty<int>("Ease", 0);
            Add(Ease);
        }
    }
}
