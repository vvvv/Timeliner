/*
 * Created by SharpDevelop.
 * User: Tebjan Halm
 * Date: 02.06.2014
 * Time: 20:41
 * 
 * 
 */
using System;
using VVVV.Core.Collections;
using VVVV.Core.Model;

namespace Timeliner
{
	/// <summary>
	/// Timeliner track so sequence strings
	/// </summary>
	public class TLStringTrack : TLTrackBase
	{
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
			base.Evaluate(time);
			CurrentText = "";
		}
		
		public string CurrentText
		{
			get;
			protected set;
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
            : this(name, 0, "text")
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
