/*
 * Created by SharpDevelop.
 * User: Tebjan Halm
 * Date: 02.06.2014
 * Time: 20:41
 * 
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;

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
            Add(Keyframes);
            
            //set the name of this track
            Label.Value = "String " + name;
		}
		
		public override void Evaluate(float time)
		{
			var kfs = Keyframes.ToList(); 
        	kfs.Sort((k1, k2) => k1.Time.Value.CompareTo(k2.Time.Value));
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
