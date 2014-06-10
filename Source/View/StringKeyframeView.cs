using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

using Svg;
using Svg.Transforms;
using VVVV.Core;

namespace Timeliner
{
	public class StringKeyframeView : KeyframeView, IDisposable
	{
		public new TLStringKeyframe Model
        {
            get
            {
                return (TLStringKeyframe)base.Model;
            }
            protected set
            {
                base.Model = value;
            }
        }
        
        public new StringTrackView Parent
        {
        	get
            {
                return (StringTrackView)base.Parent;
            }
            protected set
            {
                base.Parent = value;
            }
        }
		
		public SvgUse Background = new SvgUse();
        public SvgUse CollapsedView = new SvgUse();
		private SvgText Label = new SvgText();
		
		public StringKeyframeView(TLStringKeyframe kf, StringTrackView trackview)
			: base(kf, trackview)
		{
			//configure svg
			Background.ReferencedElement = new Uri("#" + Parent.Model.GetID() + "_KF", UriKind.Relative);
			Background.ID = "bg";
			Background.CustomAttributes["class"] = "kf";
			Background.MouseDown += Background_MouseDown;
			Background.MouseUp += Background_MouseUp;
			Background.MouseMove += Background_MouseMove;
			
			Label.FontSize = 12;
			Label.ID = "label";
			Label.CustomAttributes["class"] = "kffont";
			Label.Text = "text";
			Label.Transforms = new SvgTransformCollection();
			Label.Transforms.Add(new SvgScale(1, 1));
            
            CollapsedView.ReferencedElement = new Uri("#" + Parent.Model.GetID() + "_CKF", UriKind.Relative);
            CollapsedView.ID = "fg";
            CollapsedView.CustomAttributes["class"] = "ckf";
            CollapsedView.MouseDown += Background_MouseDown;
			CollapsedView.MouseUp += Background_MouseUp;
			CollapsedView.MouseMove += Background_MouseMove;
		}
        
        public override void Dispose()
        {
            Background.MouseDown -= Background_MouseDown;
			Background.MouseUp -= Background_MouseUp;
			Background.MouseMove -= Background_MouseMove;
            CollapsedView.MouseDown -= Background_MouseDown;
			CollapsedView.MouseUp -= Background_MouseUp;
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
            CollapsedView.X = Background.X;
            
			var isSelected = Model.Selected.Value;
			Label.Visible = true;			
			
			if (true)
			{
				var m = new Matrix();
				var y = Math.Max(Background.Y, -Parent.Model.Height.Value + 10);
				m.Translate(Background.X + 0.1f, y);
				
				Label.Transforms[0] = (SvgMatrix)Parent.CollapsedKeyframeDefinition.Transforms[0].Clone();
				Label.X = CollapsedView.X + 50;
				Label.Y = 20;
				Label.Text = Model.Text.Value;
			}
            
			Background.CustomAttributes["class"] = isSelected ? "kf selected" : "kf";
            CollapsedView.CustomAttributes["class"] = isSelected ? "ckf selected" : "ckf";
            
            Background.Visible = !Parent.Collapsed;
            CollapsedView.Visible = true;
		}
		#endregion

		#region scenegraph eventhandler
		//dipatch events to parent
		void Background_MouseMove(object sender, MouseArg e)
		{
			Parent.MouseMove(this, e);
		}
		
		void Background_MouseUp(object sender, MouseArg e)
		{
			Parent.MouseUp(this, e);
		}
		
		void Background_MouseDown(object sender, MouseArg e)
		{
			Parent.MouseDown(this, e);
		}
		#endregion
        
		public override Boolean IsSelectedBy(RectangleF rect)
		{
			return rect.IntersectsWith(new RectangleF(Model.Time.Value, 0, 0.1f, Parent.Height));
		}
	}
}
