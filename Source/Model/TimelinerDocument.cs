using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

using VVVV.Core;
using VVVV.Core.Collections;
using VVVV.Core.Commands;
using VVVV.Core.Model;
using VVVV.Core.Serialization;

namespace Timeliner
{
	public static class IDGenerator
	{
		static int ID = 0;
		public static string NewID
		{
			get
			{
				return (++ID).ToString();
			}
		}
	}

    public class TLDocument : Document
    {
    	public TLRuler Ruler
    	{
    		get;
    		private set;
    	}
    	
        public IEditableIDList<TLTrack> Tracks
        {
            get;
            private set;
        }
        
        //imitate base constructor
        public TLDocument(string name, string location)
            : base(name, location)
        {
            //create tracks list
            Tracks = new EditableIDList<TLTrack>("Tracks");

            //add to self
            Add(Tracks);
        }
        
        public void Initialize()
        {
        	Ruler = new TLRuler();
        	Add(Ruler);
        }

        //set mapper
        public void CreateMapper(MappingRegistry registry)
        {
            Mapper = new ModelMapper(this, registry);
        }

        //save data of this document to disk
        public override void SaveTo(string path)
        {
            var serializer = this.GetSerializer();
            var xml = serializer.Serialize(Tracks);
            xml.Save(path);
        }

        public void LoadFromXML(XElement data, Serializer serializer)
        {
        	data.DeserializeAndAddToList(Tracks, serializer);
        }
        
        public void Evaluate(float time)
        {
        	foreach (var track in Tracks)
        		track.Evaluate(time);
        }

        #region factory method
        public static TLDocument FromFile(string path, Serializer serializer)
        {
            //load data of this document from disk
            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                // We can handle empty files too
                var document = new TLDocument(Path.GetFileName(path), path);
                if (fileStream.Length > 0)
                {
                    var xml = XElement.Load(fileStream);
                    
                    if(xml.IsEmpty)
                    {
                    	return document;
                    }
                    
                    document = FromXML(xml, serializer);
                }
                
                return document;
            }
        }
        
        public static TLDocument FromXML(XElement xml, Serializer serializer)
        {
        	return serializer.Deserialize<TLDocument>(xml);
        }
        
        #endregion
    }

    public class TLHistory : CommandHistory
    {
    	private List<Command> InsertedCommands = new List<Command>();
    	private int FCommandPointer = 0;
    	
        public TLHistory(Serializer s)
            : base(s)
        {
        }
        
        public override void Insert(Command command)
        {
        	if(command is CompoundCommand)
        	{
        		var compound = (command as CompoundCommand);
        		if(compound.CommandCount == 0)
        			throw new Exception("empty compound command");
        	}
        	
            base.Insert(command);
            InsertedCommands.Add(command);
            FCommandPointer++;
            //TODO: send JSON to GUI
        }
        
		public override void Undo()
		{
			if(this.PreviousCommand != null)
				FCommandPointer--;
			
			base.Undo();
			
		}
        
		public override void Redo()
		{
			if(this.NextCommand != null)
				FCommandPointer++;
			
			base.Redo();
			
		}
        
    }

    public class TLContext
    {
        public Mapper Mapper { get; private set; }
        public MappingRegistry MappingRegistry { get; private set; }
        public ICommandHistory History { get; private set; }
        public Serializer Serializer { get; private set; }

        public void Initialize()
        {
            MappingRegistry = new MappingRegistry();
            Mapper = new Mapper(MappingRegistry);

            Serializer = new Serializer();
            TLSerializerRegistration.Register(Serializer);
            MappingRegistry.RegisterDefaultInstance<Serializer>(Serializer);

            History = new TLHistory(Serializer);
            MappingRegistry.RegisterDefaultInstance<ICommandHistory>(History);
            
        }
    }


}