using System;
using System.Drawing;
using Svg;

namespace Timeliner
{
	public interface IKeyframeView
	{
		
	}
	
	/// <summary>
	/// Description of KeyframeView.
	/// </summary>
	public abstract class KeyframeView : TLViewBaseTyped<TLKeyframeBase, TrackView> 
	{
		public KeyframeView(TLKeyframeBase kf, TrackView trackView)
			: base(kf, trackView)
		{
		}
		
		#region build scenegraph
		protected override void UnbuildSVG()
		{
			
		}
		
		protected override void BuildSVG()
		{
			
		}
		#endregion
		
		#region scenegraph eventhandler
		//dipatch events to parent
		protected void Background_MouseMove(object sender, MouseArg e)
		{
			Parent.MouseMove(this, e);
		}
		
		protected void Background_MouseUp(object sender, MouseArg e)
		{
			Parent.MouseUp(this, e);
		}
		
		protected void Background_MouseDown(object sender, MouseArg e)
		{
			Parent.MouseDown(this, e);
		}
		#endregion
		
		public abstract Boolean IsSelectedBy(RectangleF rect);
	}
}
