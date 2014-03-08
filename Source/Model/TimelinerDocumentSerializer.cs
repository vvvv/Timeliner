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
            serializer.RegisterGeneric<EditableIDList<TLTrack>, TLTrackListSerializer>();
            serializer.RegisterGeneric<TLValueTrack, TLValueTrackSerializer>();
            serializer.RegisterGeneric<TLTrack, TLTrackSerializer>();
            serializer.RegisterGeneric<TLKeyframe, TLKeyframeSerializer>();
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
    public class TLTrackListSerializer : ISerializer<EditableIDList<TLTrack>>
    {
        public XElement Serialize(EditableIDList<TLTrack> value, Serializer serializer)
        {
            var x = new XElement("Timeliner");

            x.SerializeAndAddList(value, serializer);

            return x;
        }

        public EditableIDList<TLTrack> Deserialize(XElement data, Type type, Serializer serializer)
        {
            var list = new EditableIDList<TLTrack>("Tracks");

            data.DeserializeAndAddToList<TLTrack>(list, serializer);

            return list;
        }
    }

    //track
    public class TLTrackSerializer : ISerializer<TLTrack>
    {
        public XElement Serialize(TLTrack value, Serializer serializer)
        {
            var x = value.GetXML("Track");
            x.Add(new XAttribute("Order", value.Order.Value));
            x.Add(new XAttribute("Height", value.Height.Value));
            x.SerializeAndAddList((value as TLValueTrack).Keyframes, serializer);

            return x;
        }

        public TLTrack Deserialize(XElement data, Type type, Serializer serializer)
        {
            //create track
            var track = new TLValueTrack(data.Attribute("Name").Value);
            track.Order.Value = int.Parse(data.Attribute("Order").Value);
            track.Height.Value = float.Parse(data.Attribute("Height").Value);
            
            track.Loading = true;
            
            data.DeserializeAndAddToList<TLKeyframe>(track.Keyframes, serializer);
            
            track.Loading = false;

            return track;
        }
    }
    
    //value track
    public class TLValueTrackSerializer : ISerializer<TLValueTrack>
    {
        public XElement Serialize(TLValueTrack value, Serializer serializer)
        {
            var x = value.GetXML("Track");
            x.Add(new XAttribute("Order", value.Order.Value));
            x.Add(new XAttribute("Height", value.Height.Value));
            x.SerializeAndAddList(value.Keyframes, serializer);

            return x;
        }

        public TLValueTrack Deserialize(XElement data, Type type, Serializer serializer)
        {
            //create track
            var track = new TLValueTrack(data.Attribute("Name").Value);
            track.Order.Value = int.Parse(data.Attribute("Order").Value);
            track.Height.Value = float.Parse(data.Attribute("Height").Value);
            
            track.Loading = true;
            
            data.DeserializeAndAddToList<TLKeyframe>(track.Keyframes, serializer);
            
            track.Loading = false;

            return track;
        }
    }

    public class TLKeyframeSerializer : ISerializer<TLKeyframe>
    {
        public XElement Serialize(TLKeyframe value, Serializer serializer)
        {
            var x = value.GetXML("Keyframe");
            x.Add(new XAttribute("Time", value.Time.Value));
            x.Add(new XAttribute("Value", value.Value.Value));
            return x;
        }

        public TLKeyframe Deserialize(XElement data, Type type, Serializer serializer)
        {
            var kf = new TLKeyframe(data.Attribute("Name").Value);
            kf.Time.Value = float.Parse(data.Attribute("Time").Value);
            kf.Value.Value = float.Parse(data.Attribute("Value").Value);
            System.Diagnostics.Debug.WriteLine(kf.Name);
            return kf;
        }
    }

}
