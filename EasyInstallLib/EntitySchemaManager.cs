using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Xml;
using System.IO;
using System.Reflection;
using System.Resources;
using EasyInstall.Properties;
using EasyInstall.Interfaces.Entities;

namespace EasyInstall
{
   public partial class EntitySchema
   {
      public class SchemaCollectionChangedEventArgs : EventArgs
      {
         public EntitySchema Schema { get; internal set; }
      }

      public static event EventHandler<SchemaCollectionChangedEventArgs> SchemaAdded;
      public static event EventHandler<SchemaCollectionChangedEventArgs> SchemaRemoved;

      static IDictionary<string, EntitySchema> schemas = new Dictionary<string, EntitySchema>();

      static HashSet<string> categories;

      static object GetIntelliValue(string value, Type type)
      {
         if (type.IsEnum)
         {
            return Enum.Parse(type, value);
         }

         //MethodInfo mi = type.GetMethod("Parse",
         //   BindingFlags.Public | BindingFlags.Static,
         //   Type.DefaultBinder,
         //   new Type[] { value.GetType() },
         //   null);

         //object valueObject = null;

         //if (mi != null && mi.ReturnParameter.ParameterType == type)
         //{
         //   valueObject = mi.Invoke(null, new object[] { value });
         //   return valueObject;
         //}

         return Convert.ChangeType(value, type);
      }

      static EntitySchema()
      {
         XDocument doc = XDocument.Load(new StringReader(Resources.SchemaInfo));

         var schemaNodes = from node in doc.Root.Elements() select node;

         var schemaList = from node in doc.Root.Elements()
                          let categoryNode = node.Attribute("Category")
                          select new EntitySchema(node.Name.ToString(), categoryNode == null? "Miscellaneous" : categoryNode.Value)
                          {
                             SupportedEntityTypes = from e in
                                                       (from se in node.Elements("SupportedSubEntities")
                                                        select se).Elements()
                                                    select e.Name.ToString(),
                             Attributes = from n in node.Elements()
                                          where (n.Name != "SupportedSubEntities")
                                          let typeName = n.Attribute("Type").Value
                                          select new KeyValuePair<string, SchemaItemInfo>(
                                             n.Name.ToString(),
                                             new SchemaItemInfo(
                                                typeName, 
                                                GetIntelliValue(n.Attribute("DefaultValue").Value, Type.GetType(typeName)),
                                                bool.Parse(n.Attribute("IsReadOnly").Value),
                                                bool.Parse(n.Attribute("AllowNull").Value)
                                                )
                                             )
                          };

         foreach (var schema in schemaList)
         {
            AddSchema(schema);
         }
      }

      public static void AddSchema(EntitySchema schema)
      {
         if ( schemas.ContainsKey(schema.EntityType) )
         {
            throw new DuplicateSchemaException(schema.EntityType);
         }

         schemas[schema.EntityType] = schema;

         if ( SchemaAdded != null )
         {
            SchemaAdded(null, new SchemaCollectionChangedEventArgs { Schema = schema });
         }
      }

      public static void RemoveSchema(EntitySchema schema)
      {
         if ( !schemas.ContainsKey(schema.EntityType) )
         {
            throw new SchemaNotFoundException(schema.EntityType);
         }

         schemas.Remove(schema.EntityType);

         if ( SchemaRemoved != null )
         {
            SchemaRemoved(null, new SchemaCollectionChangedEventArgs { Schema = schema });
         }
      }

      public static void AddSchemas(params EntitySchema[] entitySchemas)
      {
         foreach ( var schema in entitySchemas )
         {
            AddSchema(schema);
         }
      }

      static public void LoadSchemas(string schemaXMLFile)
      {
         XDocument schemaDoc = XDocument.Load(schemaXMLFile);

         RemoveAllSchemas();

         var nodes = from n in schemaDoc.Root.Elements()
                     select n;
         
         foreach ( var node in  nodes)
         {
            NetDataContractSerializer sr = new NetDataContractSerializer();
            StringReader strReader = new StringReader(node.ToString());
            XmlReader xr = XmlReader.Create(strReader);
            EntitySchema schema = (EntitySchema)sr.ReadObject(xr);

            AddSchema(schema);
         }
      }

      static public void SaveSchemas(string filename)
      {
         using ( FileStream fs = new FileStream(filename, FileMode.Create) )
         {
            XmlWriter xw = XmlWriter.Create(fs);
            xw.WriteStartDocument();
            
            xw.WriteStartElement("SchemaDefinitions");
            foreach ( var schema in schemas )
            {
               NetDataContractSerializer sr = new NetDataContractSerializer();
               sr.WriteObject(xw, schema.Value);
            }
            xw.WriteEndElement(); // SchemaDefinitions

            xw.WriteEndDocument();
            xw.Flush();
         }
      }

      static public void RemoveAllSchemas()
      {
         schemas.Clear();
      }

      public static EntitySchema FindSchema(string entityType)
      {
         if ( !schemas.ContainsKey(entityType) )
         {
            return null;
         }

         return schemas[entityType];
      }

      public static EntitySchema GetSchema(string entityType)
      {
         EntitySchema schema = FindSchema(entityType);

         if ( schema == null )
         {
            throw new SchemaNotFoundException(entityType);
         }

         return schema;
      }

      public static IList<string> SchemaNames
      {
         get
         {
            
            return schemas.Keys.ToList();
         }
      }

      public static IEnumerable<string> Categories
      {
         get
         {
            if (categories == null)
            {
               categories = new HashSet<string>(
                  from item in schemas
                  select item.Value.Category);
            }

            return categories;
         }
      }

      public static IEnumerable<KeyValuePair<string, EntitySchema>> Schemas
      {
         get
         {
            return schemas;
         }
      }
   }
}
