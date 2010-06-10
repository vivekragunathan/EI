using System;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace EasyInstall.Interfaces.Entities
{
   public class SchemaChangedEventArgs : EventArgs
   {
      public IEntitySchema Schema { get; internal set; }
      public string AttributeName { get; internal set; }
      public SchemaItemInfo Info { get; internal set; }

      public SchemaChangedEventArgs(IEntitySchema schema, string attrName, SchemaItemInfo itemInfo)
      {
         this.Schema = schema;
         this.AttributeName = attrName;
         this.Info = itemInfo;
      }
   }

   public class EntityEventArgs : EventArgs
   {
      public IEntity Entity { get; internal set; }

      public EntityEventArgs(IEntity entity)
      {
         this.Entity = entity;
      }
   }

   public class ProgressEventArgs : EntityEventArgs
   {
      public double ProgressPercent { get; private set; }
      public bool Cancel { get; set; }
      public ProgressEventArgs(IEntity entity, double progress)
         : base(entity)
      {
         this.Entity = entity;
         this.ProgressPercent = progress;
         Cancel = false;
      }
   }

   public class SetupEventArgs : EventArgs
   {
      public IPackage Package { get; private set; }
      public ISetupInfo SetupInfo { get; private set; }

      public SetupEventArgs(IPackage package, ISetupInfo setupInfo)
      {
         this.Package = package;
         this.SetupInfo = setupInfo;
      }
   }

   public class EntityAttributeEventArgs : EntityEventArgs
   {
      public EntityAttributeEventArgs(IEntity entity, string attributeName, object oldValue, object newValue)
         : base(entity) 
      {
         this.AttributeName = attributeName;
         this.OldValue = oldValue;
         this.NewValue = newValue;
      }

      public string AttributeName { get; internal set; }
      public object OldValue { get; internal set; }
      public object NewValue { get; internal set; }
   }

   [DataContract]
   public class SchemaItemInfo
   {
      [DataMember]
      public string TypeName { get; set; }
      [DataMember]
      public object DefaultValue { get; set; }
      [DataMember]
      public bool IsReadonly { get; set; }

      [DataMember]
      bool _allowNull = false;

      public bool AllowNull
      {
         get { return _allowNull; }
         set
         {
            _allowNull = value;
         }
      }

      public Type ItemType
      {
         get
         {
            return Type.GetType(this.TypeName);
         }
      }

      public SchemaItemInfo()
      {
      }

      public SchemaItemInfo(string typeName, object defaultValue, bool isReadOnly, bool allowNull)
      {
         this._allowNull = allowNull;
         this.DefaultValue = defaultValue; // How to copy this?
         this.IsReadonly = isReadOnly;
         this.TypeName = typeName;
      }

      public SchemaItemInfo(SchemaItemInfo itemInfo)
      {
         this._allowNull = itemInfo.AllowNull;
         this.DefaultValue = itemInfo.DefaultValue;
         this.IsReadonly = itemInfo.IsReadonly;
         this.TypeName = itemInfo.TypeName;
      }
   }

   public enum ScriptLanguage
   { 
      CSharp,
      JScript,
      FSharp
   }
}