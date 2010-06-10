using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml;
using System.Runtime.Serialization;
using System.ComponentModel;
using EasyInstall.Interfaces.Entities;
using System.IO;
using EasyInstall.Interfaces;

namespace EasyInstall
{
   [DataContract]
   public partial class EntitySchema : IMutableEntitySchema//: ISerializable
   {
      public event EventHandler<SchemaChangedEventArgs> OnAttributeAdded;
      public event EventHandler<SchemaChangedEventArgs> OnAttributeRemoved;

      #region SERIALIZABLE ITEMS
      //[Serializable]
   
      [DataMember(Name="Definition")]
      private IDictionary<string, SchemaItemInfo> _schema = new Dictionary<string, SchemaItemInfo>();

      [DataMember(Name = "SupportedSubEntities")]
      private IList<string> _supportedSubEntities = new List<string>();

      [DataMember(Name = "EntityType")]
      private string _EntityType;

      [DataMember(Name = "EntityCategory")]
      private string _EntityCategory;

      #endregion
      
      public string EntityType
      {
         get { return _EntityType; }
      }

      public string Category
      {
         get { return _EntityCategory; }
      }

      public IEnumerable<string> SupportedEntityTypes
      {
         get { return _supportedSubEntities; }
         private set
         {
            _supportedSubEntities = _supportedSubEntities.Concat(value).ToList();
         }
      }

      public IEnumerable<KeyValuePair<string, SchemaItemInfo>> Attributes
      {
         get
         {
            return _schema;
         }
         private set
         {
            foreach (var val in value)
            {
               _schema.Add(val);
            }
         }
      }

      public EntitySchema()
      {
      }

      public EntitySchema(string entityType, string category, IEnumerable<string> supportedSubEntities)
      {
         if ( string.IsNullOrEmpty(entityType) )
            throw new ArgumentNullException("entityType");

         this._EntityType = entityType;
         this._EntityCategory = category;

         if (supportedSubEntities != null)
            _supportedSubEntities.Concat(supportedSubEntities);
      }

      public EntitySchema(string entityType, string category)
         : this(entityType, category, null)
      {
         
      }

      public IMutableEntitySchema Copy()
      {
         NetDataContractSerializer sr = new NetDataContractSerializer();
         using ( MemoryStream ms = new MemoryStream() )
         {
            sr.WriteObject(ms, this);
            ms.Seek(0, SeekOrigin.Begin);

            return (EntitySchema)sr.ReadObject(ms);
         }
      }

      public void AddAttribute(string attributeName, SchemaItemInfo SchemaItemInfo)
      {
         CheckTypeCompatibility(SchemaItemInfo);

         if (_schema.ContainsKey(attributeName))
         {
            throw new DuplicateSchemaItemException(attributeName);
         }

         _schema.Add(new KeyValuePair<string, SchemaItemInfo>(attributeName, SchemaItemInfo));

         FireAttributeAddedEvent(attributeName, SchemaItemInfo);
      }

      public void RemoveAttribute(string attributeName)
      {
         if ( !_schema.ContainsKey(attributeName) )
         {
            throw new Exception("Attribute Not Found!");
         }

         SchemaItemInfo ii = _schema[attributeName];

         _schema.Remove(attributeName);

         FireAttributeRemovedEvent(attributeName, ii); 
      }

      void FireAttributeAddedEvent(string attributeName, SchemaItemInfo schemaItemInfo)
      {
         if (OnAttributeAdded != null)
         {
            OnAttributeAdded(this, new SchemaChangedEventArgs(this, attributeName, schemaItemInfo));
         }
      }

      void FireAttributeRemovedEvent(string attributeName, SchemaItemInfo schemaItemInfo)
      {
         if ( OnAttributeRemoved != null )
         {
            OnAttributeRemoved(this, new SchemaChangedEventArgs(this, attributeName, schemaItemInfo));
         }
      }

      // Check if the defaultvalue is compatible with the type
      public static void CheckTypeCompatibility(object o, SchemaItemInfo SchemaItemInfo)
      {
         if (!SchemaItemInfo.AllowNull && o == null)
         {
            throw new Exception("Value cannot be null");
         }
         
         if (o == null)
            return;
         try
         {
            if ( SchemaItemInfo.ItemType == typeof(object) )
               return;
         }
         catch ( Exception e )
         {
            System.Diagnostics.Trace.WriteLine("Error while loading type {0}", SchemaItemInfo.TypeName);
            System.Diagnostics.Trace.WriteLine(e.ToString());
            throw;
         }

         if (o.GetType() != SchemaItemInfo.ItemType)
         {
            object convertedObj = Convert.ChangeType(o, SchemaItemInfo.ItemType);

            if (convertedObj == null)
               throw new Exception("Incompatible types");
         }
      }

      public static void CheckTypeCompatibility(SchemaItemInfo SchemaItemInfo)
      {
         CheckTypeCompatibility(SchemaItemInfo.DefaultValue, SchemaItemInfo);
      }

      #region ISerializable Members - NOT USED AT PRESENT

      public EntitySchema(SerializationInfo info, StreamingContext context)
      {
         object o = info.GetValue("Attributes", typeof(List<SerializableKVP<string, SchemaItemInfo>>));

         List<SerializableKVP<string, SchemaItemInfo>> lkvp = o as List<SerializableKVP<string, SchemaItemInfo>>;

         if (lkvp != null)
         {
            foreach (var item in lkvp)
            {
               _schema.Add(new KeyValuePair<string, SchemaItemInfo>(item.Key, item.Value));
            }
         }
      }

      public void GetObjectData(SerializationInfo info, StreamingContext context)
      {
         List<SerializableKVP<string, SchemaItemInfo>> list = new List<SerializableKVP<string, SchemaItemInfo>>();

         foreach (var kvp in Attributes)
         {
            list.Add(new SerializableKVP<string, SchemaItemInfo>(kvp.Key, kvp.Value));
         }

         info.AddValue("Attributes", list);
      }

      #endregion

      public override string ToString()
      {
         return string.Format("{0} Schema", this.EntityType);
      }
   }

   class SerializableKVP<K, V>
   {
      [XmlElement]
      public K Key;

      [XmlElement]
      public V Value;

      public SerializableKVP(K key, V value)
      {
         this.Key = key;
         this.Value = value;
      }
   }
}
