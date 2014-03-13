using System;
using System.Drawing;

using Svg;
using Svg.Transforms;
using VVVV.Core;
using VVVV.Core.Commands;

using Posh;

namespace Timeliner
{
    /// <summary>
    /// Base class for individual view elements in the timeliner
    /// </summary>
    public abstract class TLViewBase : IDisposable
    {
        public virtual TLModelBase Model { get; protected set; }
        public virtual TLViewBase Parent { get; protected set; }
        
        public ICommandHistory History;
        
        protected IMouseEventHandler MouseHandler;

        private SvgGroup FGroup = new SvgGroup();
    
        /// <summary>
        /// The enclosing group for all svg elements in this view
        /// </summary>
        public SvgGroup MainGroup
        {
        	get
        	{
        		return FGroup;
        	}
        }
        
        public TLViewBase()
        {
        }

        public TLViewBase(TLModelBase model, TLViewBase parent)
        {
            FGroup.Transforms = new SvgTransformCollection();
        	Parent = parent;
            Model = model;
            History = Model.Mapper.Map<ICommandHistory>();
            FGroup.ID = GetGroupID();
        }
        
        protected virtual string GetGroupID()
        {
        	return Model.GetID();
        }
        
        /// <summary>
        /// Update graphical elements
        /// </summary>
        protected abstract void BuildSVG();
        
        /// <summary>
        /// Remove the roots of this svg tree from the main svg tree
        /// </summary>
        protected abstract void UnbuildSVG();

        /// <summary>
        /// Put graphical elements into a given group and rebuild the svg tree.
        /// </summary>
        /// <param name="parentGroup"></param>
        public void BuildSVGTo(SvgGroup parentGroup)
        {
            FGroup.Children.Clear();
            parentGroup.Children.Add(FGroup);
            BuildSVG();
        }
        
        /// <summary>
        /// Put svg elements into a given group without rebuilding the svg tree.
        /// This is used for the add context.
        /// </summary>
        /// <param name="parentGroup"></param>
        public virtual void DeepCopySVGTo(SvgGroup parentGroup)
        {
        	parentGroup.Children.Add(FGroup.DeepCopy());
        }

        public virtual void Dispose()
        {
        }

        protected bool CheckMouseHandler(SVGArg arg)
        {
        	return MouseHandler != null && MouseHandler.SessionID == arg.SessionID;
        }
        
        //default event dispatching
		public void Default_MouseMove(object sender, MouseArg e)
		{
			if(CheckMouseHandler(e))
			{
				MouseHandler = MouseHandler.MouseMove(sender, e);
			}
		}

		public void Default_MouseUp(object sender, MouseArg e)
		{
			if(CheckMouseHandler(e))
			{
				MouseHandler = MouseHandler.MouseUp(sender, e);
			}
		}
		
		public void Default_MouseDown(object sender, MouseArg e)
		{	
			//if there is a mouse handler, return
			if (MouseHandler != null) return;
				
			MouseHandler = GetMouseHandler(sender, e);
			if(CheckMouseHandler(e))
			{
				MouseHandler = MouseHandler.MouseDown(sender, e);
			}
		}
		
		protected virtual IMouseEventHandler GetMouseHandler(object sender, MouseArg e)
		{
			return null;
		}
        
    }

    /// <summary>
    /// Base class for individual view elements with typed model
    /// </summary>
    public abstract class TLViewBaseTyped<TModel, TParent> : TLViewBase where TModel : TLModelBase where TParent : TLViewBase
    {
        //when calling model it should be this
        public new TModel Model
        {
            get
            {
                return (TModel)base.Model;
            }
            protected set
            {
                base.Model = value;
            }
        }
        
        public new TParent Parent
        {
        	get
            {
                return (TParent)base.Parent;
            }
            protected set
            {
                base.Parent = value;
            }
        }

        public TLViewBaseTyped(TModel model, TParent parent)
            : base(model, parent)
        {  
        }
    }
    
    	/// <summary>
	/// Mouse event handler interface
	/// </summary>
	public interface IMouseEventHandler
	{
		IMouseEventHandler MouseDown(object sender, MouseArg arg);
		IMouseEventHandler MouseMove(object sender, MouseArg arg);
		IMouseEventHandler MouseUp(object sender, MouseArg arg);
		string SessionID { get; }
	}
	
	/// <summary>
	/// Does basic mouse event hadling
	/// </summary>
	public abstract class MouseHandlerBase<TView> : IMouseEventHandler where TView : TLViewBase
	{
		bool pressed;
		int Button;
		PointF StartPoint;
		PointF LastPoint;
		int DragCallCounter = 0;
		protected TView Instance;
		public string SessionID { get; protected set; }
		
		public MouseHandlerBase(TView view, string sessionID)
		{
			Instance = view;
			SessionID = sessionID;
		}
		
		public virtual IMouseEventHandler MouseDown(object sender, MouseArg arg)
		{
			pressed = true;
			Button = arg.Button;
			StartPoint = new PointF(arg.x, arg.y);
			LastPoint = StartPoint;
			return this;
		}
		
		public virtual IMouseEventHandler MouseMove(object sender, MouseArg arg)
		{
			if(pressed)
			{
				var point = new PointF(arg.x, arg.y);
				MouseDrag(sender, point, new PointF(point.X - LastPoint.X, point.Y - LastPoint.Y), DragCallCounter);
				
				var rect = new RectangleF(StartPoint, new SizeF(point.X - StartPoint.X, point.Y - StartPoint.Y));
				var x = rect.X;
				var y = rect.Y;
				var w = Math.Abs(rect.Width);
				var h = Math.Abs(rect.Height);
					
				if (rect.Width < 0)
					x = x + rect.Width;
				if (rect.Height < 0)
					y = y + rect.Height;
				
				MouseSelection(sender, new RectangleF(x, y, w, h));
				LastPoint = point;
				DragCallCounter++;
			}
			
			return this;
		}
		
		public virtual void MouseDrag(object sender, PointF arg, PointF delta, int dragCall)
		{
			
		}
		
		public virtual void MouseSelection(object sender, RectangleF selection)
		{
			
		}
		
		public virtual void MouseClick(object sender, MouseArg arg)
		{
			
		}
		
		public virtual IMouseEventHandler MouseUp(object sender, MouseArg arg)
		{
			pressed = false;
			MouseClick(sender, arg);

			return null;
		}
	}
 }

