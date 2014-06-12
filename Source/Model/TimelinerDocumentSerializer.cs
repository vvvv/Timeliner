using System;
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
            
            //tracks
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
        
        private void DeserializeAndAddToTrackList<T>(XElement x, IEditableCollection<T> list, Serializer serializer)
        {
        	if (x != null)
        	{
        		foreach (XElement current in x.Elements())
        		{
        			if (current.Name == "ValueTrack")
        				list.Add(serializer.Deserialize<TLValueTrack>(current));
        			else if (current.Name == "StringTrack")
        				list.Add(serializer.Deserialize<TLStringTrack>(current));
        		}
        	}
        }

        public EditableIDList<TLTrackBase> Deserialize(XElement data, Type type, Serializer serializer)
        {
            var list = new EditableIDList<TLTrackBase>("Tracks");

            DeserializeAndAddToTrackList(data, list, serializer);

            return list;
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
            //create track
            var track = CreateTrack(data.Attribute("Name").Value);
            track.Order.Value = int.Parse(data.Attribute("Order").Value);
            track.Height.Value = float.Parse(data.Attribute("Height").Value);
            
            track.Loading = true;
            
            DeserializeKeyframes(data, track, serializer);
            
            track.Loading = false;

            return track;
        }
        
        protected abstract TTrack CreateTrack(string name);
        
        protected abstract void DeserializeKeyframes(XElement data, TTrack track, Serializer serializer);
    }
    
    //value track
    public class TLValueTrackSerializer : TLTrackSerializer<TLValueTrack>
    {
    	
    	protected override void SerializeKeyframes(XElement x, TLValueTrack track, Serializer serializer)
    	{
    		x.SerializeAndAddList(track.Keyframes, serializer);
    	}
    	
    	protected override void DeserializeKeyframes(XElement data, TLValueTrack track, Serializer serializer)
    	{
    		data.DeserializeAndAddToList<TLValueKeyframe>(track.Keyframes, serializer);
    	}
    	
    	protected override TLValueTrack CreateTrack(string name)
    	{
    		return new TLValueTrack(name);
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
    	
		protected override void DeserializeKeyframes(XElement data, TLStringTrack track, Serializer serializer)
		{
			data.DeserializeAndAddToList<TLStringKeyframe>(track.Keyframes, serializer);
		}
    	
		protected override TLStringTrack CreateTrack(string name)
		{
			return new TLStringTrack(name);
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
