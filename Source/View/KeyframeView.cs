/*
 * Created by SharpDevelop.
 * User: Tebjan Halm
 * Date: 04.06.2014
 * Time: 19:28
 * 
 * 
 */
using System;

namespace Timeliner
{
	public interface IKeyframeView
	{
		
	}
	
	/// <summary>
	/// Description of KeyframeView.
	/// </summary>
	public class KeyframeView : TLViewBaseTyped<TLKeyframeBase, TrackView> 
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
	}
}
