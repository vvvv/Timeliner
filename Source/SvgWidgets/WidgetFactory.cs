using System;
using System.Reflection;
using VVVV.Core.Model;

namespace Timeliner
{
	/// <summary>
	/// Description of WidgetFactory.
	/// </summary>
	public static class WidgetFactory
	{
		public static SvgWidget GetWidget(object model, PropertyInfo property, float width, float height)
		{
			if (property.PropertyType.GenericTypeArguments[0] == typeof(string))
			{
				if (model == null)
					return new SvgStringWidget(property.Name, width, height, "");
				else
				{
					var value = (EditableProperty<string>) property.GetValue(model);
					return new SvgStringWidget(property.Name, width, height, (string) value.Value);
				}
			}
			else if (property.PropertyType.GenericTypeArguments[0] == typeof(float))
			{
				if (model == null)
					return new SvgValueWidget(property.Name, width, height, property.Name, 0);
				else
				{
					var value = (EditableProperty<float>) property.GetValue(model);
					return new SvgValueWidget(property.Name, width, height, property.Name, (float) value.Value);
				}
			}
			else if (property.PropertyType.GenericTypeArguments[0] == typeof(int))
			{
				if (model == null)
					return new SvgValueWidget(property.Name, width, height, property.Name, 0);
				else
				{
					var value = (EditableProperty<int>) property.GetValue(model);
					return new SvgValueWidget(property.Name, width, height, property.Name, (int) value.Value);
				}
			}
			else if (property.PropertyType.GenericTypeArguments[0] == typeof(bool))
			{
				if (model == null)
					return new SvgValueWidget(property.Name, width, height, property.Name, 0);
				else
				{
					var value = (EditableProperty<bool>) property.GetValue(model);
					return new SvgValueWidget(property.Name, width, height, property.Name, value.Value ? 1 : 0);
				}
			}
			else
				return null;
		}
	}
}
