/*
 * Created by SharpDevelop.
 * User: Tebjan Halm
 * Date: 04.06.2014
 * Time: 19:28
 * 
 * 
 */
using System;
using System.Drawing;

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
		
		protected override void UnbuildSVG()
		{
			
		}
		
		protected override void BuildSVG()
		{
			
		}
		
		public abstract Boolean IsSelectedBy(RectangleF rect);
	}
}
