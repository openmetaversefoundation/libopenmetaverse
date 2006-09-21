
using System;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Reflection;

namespace PrimsTest
{
	public class PrimsLoader
	{
		
		public PrimsLoader()
		{
		}
		
		public Schemas.primitives Load(System.IO.Stream stream)
		{
		
			System.Xml.XmlTextReader r =
				new XmlTextReader( stream );

			System.Xml.XmlValidatingReader vr =
				new XmlValidatingReader(r);
			
			System.IO.Stream schemaStream =
				System.Reflection.Assembly.GetExecutingAssembly().
					GetManifestResourceStream("prims01.xsd");
			
			System.Xml.XmlTextReader schemaReader =
				new System.Xml.XmlTextReader( schemaStream );
				
			System.Xml.Schema.XmlSchema schema =
				System.Xml.Schema.XmlSchema.Read(schemaReader, MyValidationEventHandler);
		
			// Rewind the stream
			//schemaStream.Seek(0,System.IO.SeekOrigin.Begin);
			
			schemaStream = System.Reflection.Assembly.GetExecutingAssembly().
					GetManifestResourceStream("prims01.xsd");
			
			vr.Schemas.Add(schema.TargetNamespace,
						   new System.Xml.XmlTextReader( schemaStream )
						   );
				
			vr.ValidationType = ValidationType.Schema;
			
			vr.ValidationEventHandler +=
				new ValidationEventHandler(MyValidationEventHandler);
			
			System.Xml.Serialization.XmlSerializer s =
				new System.Xml.Serialization.XmlSerializer(typeof(Schemas.primitives));
			
			Schemas.primitives ps = (Schemas.primitives) s.Deserialize(vr);

			return ps;	
		}
		
		public Schemas.primitives Load(System.String fname)
		{
					
			System.IO.FileStream stream =
				new System.IO.FileStream(fname, System.IO.FileMode.Open);
			
			return this.Load(stream);

		}
		
		public static void MyValidationEventHandler(object sender, 
                                            		ValidationEventArgs args) 
		{
			Console.WriteLine("Validation event: " + args.Message);
		}		
	}
}
