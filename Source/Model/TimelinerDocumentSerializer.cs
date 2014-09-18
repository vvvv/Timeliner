using System;
using System.CodeDom;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

using VVVV.Core;
using VVVV.Core.Collections;
using VVVV.Core.Model;
using VVVV.Core.Serialization;
using VVVV.Utils.VMath;

namespace Timeliner
{
    public static class TLSerializerRegistration
    {
        public static void Register(Serializer serializer)
        {
            //register serializers
            serializer.RegisterGeneric<TLDocument, TLDocumentSerializer>();
            serializer.RegisterGeneric<EditableIDList<TLTrackBase>, TLTrackListSerializer>();
            serializer.RegisterGeneric<TLRuler, TLRulerSerializer>();
            
            //track
            serializer.RegisterGeneric<TLTrackBase, TLTrackBaseSerializer>();
            serializer.RegisterGeneric<TLValueTrack, TLValueTrackSerializer>();
            serializer.RegisterGeneric<TLStringTrack, TLStringTrackSerializer>();
            
            //keyframes
            serializer.RegisterGeneric<TLValueKeyframe, TLValueKeyframeSerializer>();
            serializer.RegisterGeneric<TLStringKeyframe, TLStringKeyframeSerializer>();
        }
    }
   

    //document 
    public class TLDocumentSerializer : ISerializer<TLDocument>
    {
        public XElement Serialize(TLDocument value, Serializer serializer)
        {
        	var x = value.GetXML("Timeliner");
            x.Add(new XAttribute("Path", value.LocalPath));
            //ruler
            x.Add(serializer.Serialize(value.Ruler));
            
            var tracks = new XElement("Tracks");
            tracks.SerializeAndAddList(value.Tracks, serializer);
            x.Add(tracks);
            return x;
        }

        public TLDocument Deserialize(XElement data, Type type, Serializer serializer)
        {
            var doc = new TLDocument(data.Attribute("Name").Value, data.Attribute("Path").Value);
            doc.LoadFromXML(data, serializer);
            return doc;
        }
    }
    
    //ruler
    public class TLRulerSerializer : ISerializer<TLRuler>
    {
    	
		public XElement Serialize(TLRuler value, Serializer serializer)
		{
			var x = value.GetXML("Ruler");
			value.SerializeProperties(x);
			return x;
		}
    	
		public TLRuler Deserialize(XElement data, Type type, Serializer serializer)
		{
			var ruler = new TLRuler();
			ruler.DeserializeProperties(data);
			return ruler;
		}
    }

    //document tracks
    public class TLTrackListSerializer : ISerializer<EditableIDList<TLTrackBase>>
    {
        public XElement Serialize(EditableIDList<TLTrackBase> value, Serializer serializer)
        {
            var x = new XElement("Timeliner");

            x.SerializeAndAddList(value, serializer);

            return x;
        }

        public EditableIDList<TLTrackBase> Deserialize(XElement data, Type type, Serializer serializer)
        {
            var list = new EditableIDList<TLTrackBase>("Tracks");

            data.DeserializeAndAddToList(list, serializer);

            return list;
        }
    }

    //track serializer base
    public class TLTrackBaseSerializer : ISerializer<TLTrackBase>
    {
        public XElement Serialize(TLTrackBase value, Serializer serializer)
        {
        	throw new NotImplementedException("Should not be called");
        }

        public TLTrackBase Deserialize(XElement data, Type type, Serializer serializer)
        {
            //create track
            var track = CreateTrack(data.Name.LocalName, data.Attribute("Name").Value);
            
            track.DeserializeProperties(data);
            
            track.Loading = true;
            
            if(track is TLValueTrack)
            	data.DeserializeAndAddToList<TLValueKeyframe>((track as TLValueTrack).Keyframes, serializer);
            else if (track is TLStringTrack)
            	data.DeserializeAndAddToList<TLStringKeyframe>((track as TLStringTrack).Keyframes, serializer);
            
            track.Loading = false;

            return track;
        }
        
        protected TLTrackBase CreateTrack(string tag, string name)
        {
        	TLTrackBase result = null;
        	
        	if(tag == "ValueTrack")
        		result = new TLValueTrack(name);
        	else if(tag == "StringTrack")
        		result = new TLStringTrack(name);
        	
        	return result;
        }
    }
    
    //track serializer base
    public abstract class TLTrackSerializer<TTrack> : ISerializer<TTrack> where TTrack : TLTrackBase
    {
        public XElement Serialize(TTrack value, Serializer serializer)
        {
            var x = value.GetXML(GetTagName());
            
			value.SerializeProperties(x);
            
            SerializeKeyframes(x, value, serializer);

            return x;
        }
        
        protected abstract string GetTagName();
        protected abstract void SerializeKeyframes(XElement x, TTrack track, Serializer serializer);

        public TTrack Deserialize(XElement data, Type type, Serializer serializer)
        {
        	throw new NotImplementedException("Should not be called");
        }

    }
    
    //value track
    public class TLValueTrackSerializer : TLTrackSerializer<TLValueTrack>
    {
    	
    	protected override void SerializeKeyframes(XElement x, TLValueTrack track, Serializer serializer)
    	{
    		x.SerializeAndAddList(track.Keyframes, serializer);
    	}
    	
		protected override string GetTagName()
		{
			return "ValueTrack";
		}
    }
    
    //string track
    public class TLStringTrackSerializer : TLTrackSerializer<TLStringTrack>
    {
        
		protected override void SerializeKeyframes(XElement x, TLStringTrack track, Serializer serializer)
		{
			x.SerializeAndAddList(track.Keyframes, serializer);
		}
    	
		protected override string GetTagName()
		{
			return "StringTrack";
		}
    }


    public class TLValueKeyframeSerializer : ISerializer<TLValueKeyframe>
    {
        public XElement Serialize(TLValueKeyframe value, Serializer serializer)
        {
            var x = value.GetXML("Keyframe");
            value.SerializeProperties(x);
            return x;
        }

        public TLValueKeyframe Deserialize(XElement data, Type type, Serializer serializer)
        {
            var kf = new TLValueKeyframe(data.Attribute("Name").Value);
            kf.DeserializeProperties(data);
            System.Diagnostics.Debug.WriteLine("deserialized value keyframe: " + kf.Name);
            return kf;
        }
    }
    
    public class TLStringKeyframeSerializer : ISerializer<TLStringKeyframe>
    {
        public XElement Serialize(TLStringKeyframe value, Serializer serializer)
        {
            var x = value.GetXML("Keyframe");
            value.SerializeProperties(x);
            return x;
        }

        public TLStringKeyframe Deserialize(XElement data, Type type, Serializer serializer)
        {
            var kf = new TLStringKeyframe(data.Attribute("Name").Value);
            kf.DeserializeProperties(data);
            System.Diagnostics.Debug.WriteLine("deserialized string keyframe: " + kf.Name);
            return kf;
        }
    }
    
    public static class IDContainerExtentions
    {
    	public static void SerializeProperties(this IDContainer container, XElement x)
    	{
    		foreach (var element in container)
    		{
    			if(element.GetType().IsGenericType && (element.GetType().GetGenericTypeDefinition() == typeof(EditableProperty<>)))
    			{
    			    dynamic prop = element;
    			    
    			    //HACK 
                    if (element.GetType() == typeof(EditableProperty<Vector2D>))
    			    {
                        var v = element as EditableProperty<Vector2D>;
                        var s = v.Value.x.ToString() + "," + v.Value.y.ToString();;
    			        x.Add(new XAttribute(prop.Name, s));
    			    }
    			    else
    				    x.Add(new XAttribute(prop.Name, prop.Value));
    			}
    		}
    	}
    	
    	public static void DeserializeProperties(this IDContainer container, XElement data)
    	{
    		foreach (var element in container)
    		{
    			if(element.GetType().IsGenericType && (element.GetType().GetGenericTypeDefinition() == typeof(EditableProperty<>)))
    			{
    				var type = element.GetType().GetGenericArguments()[0];
    				dynamic prop = element;
    				
    				var attribute = data.Attribute(prop.Name);
    				
    				//HACK
    				if (element.GetType() == typeof(EditableProperty<Vector2D>))
    			    {
    				    var v = attribute.Value.Split(',');
    				    prop.Value = new Vector2D(double.Parse(v[0]), double.Parse(v[1]));
    			    }
    				else
    				    prop.Value = TypeDescriptor.GetConverter(type).ConvertFromString(null, CultureInfo.InvariantCulture, attribute.Value);
    			}
    		}
    	}
    }

}
