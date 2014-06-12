using System;
using System.CodeDom;
using System.Linq;
using System.Xml.Linq;
using VVVV.Core.Serialization;
using VVVV.Core.Collections;
using VVVV.Core;

namespace Timeliner
{
    public static class TLSerializerRegistration
    {
        public static void Register(Serializer serializer)
        {
            //register serializers
            serializer.RegisterGeneric<TLDocument, TLDocumentSerializer>();
            serializer.RegisterGeneric<EditableIDList<TLTrackBase>, TLTrackListSerializer>();
            
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
            var x = serializer.Serialize(value.Tracks);
            x.Add(new XAttribute("Name", value.Name));
            x.Add(new XAttribute("Path", value.LocalPath));
            return x;
        }

        public TLDocument Deserialize(XElement data, Type type, Serializer serializer)
        {
            var doc = new TLDocument(data.Attribute("Name").Value, data.Attribute("Path").Value);
            doc.LoadFromXML(data, serializer);
            return doc;
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
            track.Order.Value = int.Parse(data.Attribute("Order").Value);
            track.Height.Value = float.Parse(data.Attribute("Height").Value);
            
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
            x.Add(new XAttribute("Order", value.Order.Value));
            x.Add(new XAttribute("Height", value.Height.Value));
            
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
            x.Add(new XAttribute("Time", value.Time.Value));
            x.Add(new XAttribute("Value", value.Value.Value));
            return x;
        }

        public TLValueKeyframe Deserialize(XElement data, Type type, Serializer serializer)
        {
            var kf = new TLValueKeyframe(data.Attribute("Name").Value);
            kf.Time.Value = float.Parse(data.Attribute("Time").Value);
            kf.Value.Value = float.Parse(data.Attribute("Value").Value);
            System.Diagnostics.Debug.WriteLine("deserialized value keyframe: " + kf.Name);
            return kf;
        }
    }
    
    public class TLStringKeyframeSerializer : ISerializer<TLStringKeyframe>
    {
        public XElement Serialize(TLStringKeyframe value, Serializer serializer)
        {
            var x = value.GetXML("Keyframe");
            x.Add(new XAttribute("Time", value.Time.Value));
            x.Add(new XAttribute("Text", value.Text.Value));
            return x;
        }

        public TLStringKeyframe Deserialize(XElement data, Type type, Serializer serializer)
        {
            var kf = new TLStringKeyframe(data.Attribute("Name").Value);
            kf.Time.Value = float.Parse(data.Attribute("Time").Value);
            kf.Text.Value = data.Attribute("Text").Value;
            System.Diagnostics.Debug.WriteLine("deserialized string keyframe: " + kf.Name);
            return kf;
        }
    }

}
