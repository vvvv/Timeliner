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
        			Curves.Add(new TLCurve(IDGenerator.NewID, Keyframes[i - 1], Keyframes[i]));

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
            var kf1 = kfs.Find(k => k.Time.Value > time);
            
            if (kf == null && kf1 == null)
                CurrentValue = 0;
            else if (kf == null)
                CurrentValue =  kf1.Value.Value;
            else if (kf1 == null)
                CurrentValue =  kf.Value.Value;
            else
            {
                var curve = Curves.FirstOrDefault(c => (c.Start == kf) && (c.End == kf1));
                if (curve != null)
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
        public static double APPROXIMATION_EPSILON = 1.0e-09;
        public static double VERYSMALL = 1.0e-20;
        public static int MAXIMUM_ITERATIONS = 100;
        
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
                    case 3: FC1 = FP1 + (d * Start.EaseOut.Value); break;
            }

            switch (End.Ease.Value)
            {
                case 1: 
                    case 3: FC2 = FP2 + (d * End.EaseIn.Value); break;
            }
        }
        
        public float GetValue(float time)
        {
            var x = ApproximateCubicBezierParameter(time, (float) FP1.x, (float) FC1.x, (float) FC2.x, (float) FP2.x);
            return (float) BezierInterpolate(x, FP1, FC1, FC2, FP2).y;
        }
        
        //simply clamps a value between 0 .. 1
        float ClampToZeroOne(float value) {
            if (value < .0f)
                return .0f;
            else if (value > 1.0f)
                return 1.0f;
            else
                return value;
        }
        
        /**
         * Returns the approximated parameter of a parametric curve for the value X
         * @param atX At which value should the parameter be evaluated
         * @param P0_X The first interpolation point of a curve segment
         * @param C0_X The first control point of a curve segment
         * @param C1_X The second control point of a curve segment
         * @param P1_x The second interpolation point of a curve segment
         * @return The parametric argument that is used to retrieve atX using the parametric function representation of this curve
         */
        float ApproximateCubicBezierParameter (
            float atX, float P0_X, float C0_X, float C1_X, float P1_X ) {
            
            if (atX - P0_X < VERYSMALL)
                return 0.0f;
            
            if (P1_X - atX < VERYSMALL)
                return 1.0f;
            
            long iterationStep = 0;
            
            float u = 0.0f; float v = 1.0f;
            
            //iteratively apply subdivision to approach value atX
            while (iterationStep < MAXIMUM_ITERATIONS) {
                
                // de Casteljau Subdivision.
                float a = (P0_X + C0_X)*0.5f;
                float b = (C0_X + C1_X)*0.5f;
                float c = (C1_X + P1_X)*0.5f;
                float d = (a + b)*0.5f;
                float e = (b + c)*0.5f;
                float f = (d + e)*0.5f; //this one is on the curve!
                
                //The curve point is close enough to our wanted atX
                if (Math.Abs(f - atX) < APPROXIMATION_EPSILON) {
                    return ClampToZeroOne((u + v)*0.5f);
                }
                
                //dichotomy
                if (f < atX) {
                    P0_X = f;
                    C0_X = e;
                    C1_X = c;
                    u = (u + v)*0.5f;
                } else {
                    C0_X = a;
                    C1_X = d;
                    P1_X = f;
                    v = (u + v)*0.5f;
                }
                
                iterationStep++;
            }
            
            return ClampToZeroOne((u + v)*0.5f);
        }
        
        Vector2D BezierInterpolate(float s, Vector2D p0, Vector2D c0, Vector2D c1, Vector2D p1)
        {
            return Math.Pow(1 - s, 3) * p0 + 3 * Math.Pow(1 - s, 2) * s * c0 + 3 * (1 - s) * Math.Pow(s, 2) * c1 + Math.Pow(s, 3) * p1;
        }
    }

    public class TLValueKeyframe : TLKeyframeBase
    {
        [KeyframeMenuEntry]
        public EditableProperty<float> Value { get; private set; }
        
        [KeyframeMenuEntry]
        public EditableProperty<int> Ease { get; private set; }
        
        public EditableProperty<Vector2D> EaseIn { get; private set; }
        public EditableProperty<Vector2D> EaseOut { get; private set; }

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
            
            //handles should probably be clamped to 0.5
            EaseIn = new EditableProperty<Vector2D>("EaseIn", new Vector2D(-1, 0));
            Add(EaseIn);
            
            EaseOut = new EditableProperty<Vector2D>("EaseOut", new Vector2D(1, 0));
            Add(EaseOut);
        }
    }
}
