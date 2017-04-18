using System;
using System.Collections.Generic;
using System.Linq;

using VVVV.Core;
using VVVV.Core.Collections;
using VVVV.Core.Model;

namespace Timeliner
{
	/// <summary>
	/// Timeliner track so sequence strings
	/// </summary>
	public class TLStringTrack : TLTrackBase
	{
		public override IEnumerable<TLKeyframeBase> KeyframeModels
    	{
    		get
    		{
    			return Keyframes;
    		}
    	}
		
		//property which holds the keyframes
		public EditableIDList<TLStringKeyframe> Keyframes
		{
			get;
			private set;
		}
		
		public TLStringTrack(string name)
            : base(name)
		{
			//create the keyframe list and add it to self
			Keyframes = new EditableIDList<TLStringKeyframe>("Keyframes");
			Keyframes.Added += Keyframes_Added;
            Add(Keyframes);

            //set the name of this track
            string label;
            int x;
            if (Int32.TryParse(name, out x))
            {
                label = "String " + x.ToString();
            }
            else
            {
                label = name;
            }
            Label.Value = label;
		}

		void Keyframes_Added(IViewableCollection<TLStringKeyframe> collection, TLStringKeyframe item)
		{
			//sort the keyframes and assign neighbours
			SortAndAssignNeighbours();
		}
		
		protected override void SortKeyframeList()
		{
			Keyframes.Sort((a, b) => a.Time.Value.CompareTo(b.Time.Value));
		}
		
		public override void Evaluate(float time)
		{
			var kfs = Keyframes.ToList();
			var kf = kfs.FindLast(k => k.Time.Value <= time);
			
			if (kf == null)
				CurrentText = "";
			else
			{
				CurrentText = kf.Text.Value; 
			}
		}
		
		public string CurrentText
		{
			get;
			protected set;
		}
		
		public override string GetCurrentValueAsString()
		{
			return CurrentText;
		}
		
		public override object GetCurrentValueAsObject()
		{
			return CurrentText;
		}
	}
	
	/// <summary>
	/// String keyframe
	/// </summary>
	public class TLStringKeyframe : TLKeyframeBase
    {
		//holds the actual string of this keyframe
		[TrackMenuEntry]
        public EditableProperty<string> Text 
        { 
        	get; 
        	private set;
        }
        
        public TLStringKeyframe()
            : this(IDGenerator.NewID)
        {
        }
        
        public TLStringKeyframe(string name)
            : this(name, 0, "text " + name)
        {
        }
        
        public TLStringKeyframe(float time, string text)
            : this(IDGenerator.NewID, time, text)
        {
        }

        public TLStringKeyframe(string name, float time, string text)
            : base(name, time)
        {
            Text = new EditableProperty<string>("Text", text);
            Add(Text);
        }
    }
}
