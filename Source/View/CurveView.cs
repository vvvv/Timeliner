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
            Path.StrokeWidth = 1.0f;
            Path.CustomAttributes["vector-effect"] = "non-scaling-stroke";
            Path.CustomAttributes["pointer-events"] = "none";
            Path.CustomAttributes["class"] = "sfront";
            
            if (Model.Start != null)
            {
                Model.Start.Value.ValueChanged += CurveChanged;
                Model.Start.Time.ValueChanged += CurveChanged;
            }
            
            if (Model.End != null)
            {
                Model.End.Value.ValueChanged += CurveChanged;
                Model.End.Time.ValueChanged += CurveChanged;
            }
            
            FIsDirty = true;
        }
        
        public override void Dispose()
        {
            if (Model.Start != null)
            {
                Model.Start.Value.ValueChanged -= CurveChanged;
                Model.Start.Time.ValueChanged -= CurveChanged;
            }    
            if (Model.End != null)
            {
                Model.End.Value.ValueChanged -= CurveChanged;
                Model.End.Time.ValueChanged -= CurveChanged;
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
		
        public override void UpdateScene()
        {
            UpdatePathData();
            base.UpdateScene();
        }
		
        private void UpdatePathData()
        {
            if (FIsDirty)
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
                
                FIsDirty = false;
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
        #endregion
        
        void CurveChanged(IViewableProperty<float> property, float newValue, float oldValue)
        {
            FIsDirty = true;
        }
    }
}
