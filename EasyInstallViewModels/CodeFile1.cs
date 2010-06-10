using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EasyInstall.Interfaces;
using EasyInstall.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace EasyInstall.ViewModels
{
   public class EntityPropertDescriptor : PropertyDescriptor
   {
      IEntity _entity;
      string _propertyName;

      EntitySchema.ItemInfo _itemInfo;

      public EntityPropertDescriptor(IEntity entity, string propertyName)
         : base(propertyName, null)
      {
         this._entity = entity;
         this._propertyName = propertyName;

         _itemInfo = entity.Schema.Attributes.First((e) => e.Key == propertyName).Value;
      }

      public override bool CanResetValue(object component)
      {
         return true;
      }

      public override Type ComponentType
      {
         get { return _entity.GetType(); }
      }

      public override object GetValue(object component)
      {
         return _entity.GetAttribute(_propertyName);
      }

      public override bool IsReadOnly
      {
         get { return _itemInfo.IsReadonly; }
      }

      public override Type PropertyType
      {
         get { return _itemInfo.ItemType; }
      }

      public override void ResetValue(object component)
      {
         _entity.SetAttribute(_propertyName, _itemInfo.DefaultValue);
      }

      public override void SetValue(object component, object value)
      {
         _entity.SetAttribute(_propertyName, value);
      }

      public override bool ShouldSerializeValue(object component)
      {
         return false;
      }
   }
}