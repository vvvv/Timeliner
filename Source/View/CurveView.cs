using System;
using System.Collections.Generic;
using System.Drawing;

using Svg;
using VVVV.Core;

namespace Timeliner
{
	/// <summary>
	/// Draws a curve between two keyframes
	/// </summary>
	public class CurveView : TLViewBaseTyped<TLCurve, ValueTrackView>, IDisposable
	{
	    public SvgPath Path = new SvgPath();
	
	    private bool FIsDirty;
	    public bool IsDirty
	    {
	    	get
	    	{
	    		return FIsDirty;
	    	}
	    }
	    
	    public CurveView(TLCurve curve, ValueTrackView tv)
	        : base(curve, tv)
	    {
	        Path.ID = "path";
	        Path.Stroke = new SvgColourServer(Color.Gray);
	        Path.StrokeWidth = 1.0f;
	        Path.CustomAttributes["vector-effect"] = "non-scaling-stroke";
	        Path.CustomAttributes["pointer-events"] = "none";
	
	        if (Model.Name.StartsWith("Start"))
	        {
	            RegisterAtKeyframeListeners(Model.End);
	        }
	        else if (Model.Name.StartsWith("End"))
	        {
	            RegisterAtKeyframeListeners(Model.Start);
	        }
	        else
	        {
	            RegisterAtKeyframeListeners(Model.Start);
	            RegisterAtKeyframeListeners(Model.End);
	        }
	    }
	    
	    private void RegisterAtKeyframeListeners(TLKeyframe kf)
	    {
	        kf.Time.ValueChanged += kf_ValueChanged;
	        kf.Value.ValueChanged += kf_ValueChanged;
	    }
	    
	    public override void Dispose()
	    {
	        if (Model.Start != null)
	        {
	            Model.Start.Time.ValueChanged -= kf_ValueChanged;
	            Model.Start.Value.ValueChanged -= kf_ValueChanged;
	        }
	
	        if (Model.End != null)
	        {
	            Model.End.Time.ValueChanged -= kf_ValueChanged;
	            Model.End.Value.ValueChanged -= kf_ValueChanged;
	        }
	        
	        base.Dispose();
	    }
	
	    #region build scenegraph
	    protected override void BuildSVG()
	    {
	    	MainGroup.Children.Clear();
	    	
	    	UpdatePathData();
	
	        MainGroup.Children.Add(Path);
	    }
	    
		protected override void UnbuildSVG()
		{
			Parent.CurveGroup.Children.Remove(MainGroup);
		}
		#endregion
		
		#region update scenegraph
		private void UpdatePathData()
	    {
	    	Path.PathData.Clear();
	
	        if (Model.Name.StartsWith("Start"))
	        {
	            CreatePath(-99999.9f, Model.End.Value.Value, Model.End.Time.Value, Model.End.Value.Value);
	        }
	        else if (Model.Name.StartsWith("End"))
	        {
	            CreatePath(Model.Start.Time.Value, Model.Start.Value.Value, 99999.9f, Model.Start.Value.Value);
	        }
	        else
	        {
	            CreatePath(Model.Start.Time.Value, Model.Start.Value.Value, Model.End.Time.Value, Model.End.Value.Value);
	        }
	    }
		
		private void CreatePath(float x1, float y1, float x2, float y2)
	    {
	        var coords = new List<float>();
	        var curveMove = 'M';
	        var curveType = 'L';
	
	        coords.Add(x1);
	        coords.Add(-y1);
	
	        SvgPathBuilder.CreatePathSegment(curveMove, Path.PathData, coords, char.IsLower(curveMove));
	        coords.Clear();
	
	        coords.Add(x2);
	        coords.Add(-y2);
	        SvgPathBuilder.CreatePathSegment(curveType, Path.PathData, coords, char.IsLower(curveType));
	    }
		#endregion
		
	    #region model eventhandler
	    void kf_ValueChanged(IViewableProperty<float> property, float newValue, float oldValue)
	    {
	    	//FIsDirty = true;
	    	UpdatePathData();
	    }
	    #endregion
	    
	    public void ResetDirty()
	    {
	    	FIsDirty = false;
	    }
	}
}
