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
	
        bool FIsDirty;
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
            
            if (Model.Start != null)
            {
                Model.Start.Value.ValueChanged += CurveChanged;
                Model.Start.Time.ValueChanged += CurveChanged;
                Model.Start.Ease.ValueChanged += EaseChanged;
            }
            
            if (Model.End != null)
            {
                Model.End.Value.ValueChanged += CurveChanged;
                Model.End.Time.ValueChanged += CurveChanged;
                Model.End.Ease.ValueChanged += EaseChanged;
            }
            
            FIsDirty = true;
        }
        
        public override void Dispose()
        {
            if (Model.Start != null)
            {
                Model.Start.Value.ValueChanged -= CurveChanged;
                Model.Start.Time.ValueChanged -= CurveChanged;
                Model.Start.Ease.ValueChanged -= EaseChanged;
            }
            if (Model.End != null)
            {
                Model.End.Value.ValueChanged -= CurveChanged;
                Model.End.Time.ValueChanged -= CurveChanged;
                Model.End.Ease.ValueChanged -= EaseChanged;
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
        	var cls = Parent.Collapsed ? "collapsed " : "";
            var selected = false;
            if (Model.Start == null) 
            	selected = Model.End.Selected.Value;
            else if (Model.End == null)
            	selected = Model.Start.Selected.Value;
            else 
            	selected = Model.Start.Selected.Value || Model.End.Selected.Value;
            cls += selected ? "pathsel" : "";
            
             Path.CustomAttributes["class"] = cls.Trim();
             
            UpdatePathData();
            base.UpdateScene();
        }
		
        void UpdatePathData()
        {
            if (FIsDirty)
            {
            	
                Path.PathData.Clear();
	
                if (Model.Name.StartsWith("Start"))
                {
                    CreatePath(-99999.9f, Model.End.Value.Value, Model.End.Time.Value, Model.End.Value.Value, true, false, false);
                }
                else if (Model.Name.StartsWith("End"))
                {
                    CreatePath(Model.Start.Time.Value, Model.Start.Value.Value, 99999.9f, Model.Start.Value.Value, true, false, false);
                }
                else
                {
                    CreatePath(Model.Start.Time.Value, Model.Start.Value.Value, Model.End.Time.Value, Model.End.Value.Value, (Model.Start.Ease.Value == 0) && (Model.End.Ease.Value == 0), (Model.End.Ease.Value == 1) || (Model.End.Ease.Value == 3), (Model.Start.Ease.Value == 2) || (Model.Start.Ease.Value == 3));
                }
                
                FIsDirty = false;
            }
        }
        
        void CreatePath(float x1, float y1, float x2, float y2, bool isLinear, bool easeIn, bool easeOut)
        {
            var coords = new List<float>();
            var curveMove = 'M';
            
            var curveType = 'C';
            if (isLinear)
                curveType = 'L';
            
            coords.Add(x1);
            coords.Add(-y1);
            
            SvgPathBuilder.CreatePathSegment(curveMove, Path.PathData, coords, char.IsLower(curveMove));
            coords.Clear();
            
            if (!isLinear)
            {
                if (easeOut)
                {
                    coords.Add(x1 + (x2-x1) * 0.5f);
                    coords.Add(-y1);
                }
                else
                {
                    coords.Add(x1 + (x2-x1) * (1/3f));
                    coords.Add(-(y1 + (y2-y1) * (1/3f)));
                }
                
                if (easeIn)
                {
                    coords.Add(x2 - (x2-x1) * 0.5f);
                    coords.Add(-y2);
                }
                else
                {
                    coords.Add(x2 - (x2-x1) * (1/3f));
                    coords.Add(-(y2 - (y2-y1) * (1/3f)));
                }
            }
            
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
        
        void EaseChanged(IViewableProperty<int> property, int newValue, int oldValue)
        {
            FIsDirty = true;
        }
    }
}
