using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

using Svg;
using Svg.Transforms;
using VVVV.Core;

namespace Timeliner
{
	public class ValueKeyframeView : KeyframeView, IDisposable
	{
		public new TLValueKeyframe Model
        {
            get
            {
                return (TLValueKeyframe)base.Model;
            }
            protected set
            {
                base.Model = value;
            }
        }
        
        public new ValueTrackView Parent
        {
        	get
            {
                return (ValueTrackView)base.Parent;
            }
            protected set
            {
                base.Parent = value;
            }
        }
        
        bool FHovered;
		
		public SvgUse Background = new SvgUse();
        public SvgUse CollapsedView = new SvgUse();
		SvgText Label = new SvgText();
		
		public ValueKeyframeView(TLValueKeyframe kf, ValueTrackView trackview)
			: base(kf, trackview)
		{
			//configure svg
			Background.ReferencedElement = new Uri("#" + Parent.Model.GetID() + "_KF", UriKind.Relative);
			Background.ID = "bg";
			Background.CustomAttributes["class"] = "kf";
			Background.MouseDown += Background_MouseDown;
			Background.MouseUp += Background_MouseUp;
			Background.MouseOver += Background_MouseOver;
			Background.MouseOut += Background_MouseOut;
			Background.MouseMove += Background_MouseMove;
			
			Label.FontSize = 12;
			Label.ID = "label";
			Label.CustomAttributes["class"] = "kffont";
			Label.Visible = false;
			Label.Transforms = new SvgTransformCollection();
			Label.Transforms.Add(new SvgScale(1, 1));
            
            CollapsedView.ReferencedElement = new Uri("#" + Parent.Model.GetID() + "_CKF", UriKind.Relative);
            CollapsedView.ID = "fg";
            CollapsedView.CustomAttributes["class"] = "ckf";
            CollapsedView.MouseDown += Background_MouseDown;
			CollapsedView.MouseUp += Background_MouseUp;
			CollapsedView.MouseOver += Background_MouseOver;
			CollapsedView.MouseOut += Background_MouseOut;
			CollapsedView.MouseMove += Background_MouseMove;
		}
        
        public override void Dispose()
        {
            Background.MouseDown -= Background_MouseDown;
			Background.MouseUp -= Background_MouseUp;
			Background.MouseOver -= Background_MouseOver;
			Background.MouseOut -= Background_MouseOut;
			Background.MouseMove -= Background_MouseMove;
            CollapsedView.MouseDown -= Background_MouseDown;
			CollapsedView.MouseUp -= Background_MouseUp;
			CollapsedView.MouseOver -= Background_MouseOver;
			CollapsedView.MouseOut -= Background_MouseOut;
			CollapsedView.MouseMove -= Background_MouseMove;
            
            base.Dispose();
        }
		
		#region build scenegraph
		protected override void BuildSVG()
		{
    		MainGroup.Children.Add(Background);
            MainGroup.Children.Add(CollapsedView);
			MainGroup.Children.Add(Label);
		}
		
		protected override void UnbuildSVG()
		{
			Parent.KeyframeGroup.Children.Remove(MainGroup);
		}
		#endregion
		
		#region update scenegraph
		public override void UpdateScene()
		{
			base.UpdateScene();
			
			Background.X = Model.Time.Value;
			Background.Y = -Model.Value.Value;
            
			CollapsedView.X = Background.X;
            
			var isSelected = Model.Selected.Value;
			Label.Visible = isSelected || FHovered;			
			if (Label.Visible)
			{
				var m = new Matrix();
				var h = Parent.KeyframeDefinition.Radius * 3 * Parent.KeyframeDefinition.Transforms[0].Matrix.Elements[3];
				var y = Math.Max(Background.Y, -Parent.Model.Maximum.Value + h);
				m.Translate(Background.X + 0.15f, y);
				m.Multiply(Parent.KeyframeDefinition.Transforms[0].Matrix);

				Label.Transforms[0] = new SvgMatrix(m.Elements.ToList());
				Label.Text = string.Format("{0:0.0000}", Model.Value.Value) + " " + string.Format("{0:0.00}", Model.Time.Value);
			}
            
			Background.CustomAttributes["class"] = isSelected ? "kf selected" : "kf";
            CollapsedView.CustomAttributes["class"] = isSelected ? "ckf selected" : "ckf";
            
            Background.Visible = !Parent.Collapsed;
            CollapsedView.Visible = Parent.Collapsed;
		}
		#endregion

		#region scenegraph eventhandler
		void Background_MouseOver(object sender, MouseArg e)
		{
			FHovered = true;
			UpdateScene();
		}
		
		void Background_MouseOut(object sender, MouseArg e)
		{
			FHovered = false;
			UpdateScene();
		}
		#endregion
        
        public override Boolean IsSelectedBy(RectangleF rect)
        {
            if (Parent.Collapsed)
            {
                return rect.IntersectsWith(new RectangleF(Model.Time.Value, Parent.Model.Minimum.Value, 0.1f, Parent.Model.Maximum.Value - Parent.Model.Minimum.Value));
            }
            else
            {
                return rect.Contains(Model.Time.Value, Model.Value.Value);
            }
        }
	}
}
