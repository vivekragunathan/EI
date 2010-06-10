using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Collections;
using EasyInstall.Interfaces.Entities;

namespace EasyInstall.ViewModels
{
   public delegate void ItemEndEditEventHandler(IEditableObject sender);

   public class ItemInfoKVP : ViewModelBase, IEditableObject
   {
      public ItemInfoKVP()
      {
         _key = string.Empty;

         _value = new SchemaItemInfo();
      }
      
      ItemInfoKVP(ItemInfoKVP kvp)
      {
         Key = this.Key;
         Value = new SchemaItemInfo()
                 {
                    AllowNull = kvp.Value.AllowNull,
                    DefaultValue = kvp.Value.DefaultValue,
                    IsReadonly = kvp.Value.IsReadonly,
                    TypeName = kvp.Value.TypeName
                 };
      }

      string _key;
      public string Key
      {
         get { return _key; }
         set
         {
            if ( _key != value )
            {
               if ( value.Contains(" ") )
                  throw new Exception("Attribute Name cannot contain spaces");

               NotifyPropertyChanging("Key", _key, value);

               OldKey = _key;
               _key = value;
               NotifyCurrentPropertyChanged();
            }
         }
      }

      public string OldKey { get; private set; }

      SchemaItemInfo _value;
      public SchemaItemInfo Value
      {
         get { return _value; }
         set
         {
            NotifyPropertyChanging("Value", _value, value);

            _value = value;
            NotifyCurrentPropertyChanged();
         }
      }

      #region IEditableObject Members

      public void BeginEdit()
      {
      }

      public void CancelEdit()
      {
      }

      public void EndEdit()
      {
         if ( OnItemEndEdit != null )
            OnItemEndEdit(this);
      }

      public event ItemEndEditEventHandler OnItemEndEdit;

      #endregion
   }

   public class EntitySchemaViewModel : ViewModelBase
   {
      public EntitySchema Schema { get; private set; }

      public string EntityType
      {
         get;
         set;
      }

      public string Category
      {
         get;
         set;
      }

      public ObservableCollection<string> SupportedEntities
      {
         get;
         private set;
      }

      public EntitySchemaViewModel(EntitySchemaCollection srcCollection, EntitySchema schema)
      {
         Attributes = new ObservableCollection<ItemInfoKVP>();
         this.Schema = schema;
         this.EntityType = schema.EntityType;
         this.Category = schema.Category;
         this.SupportedEntities = new ObservableCollection<string>();

         foreach ( var se in schema.SupportedEntityTypes )
         {
            SupportedEntities.Add(se);
         }

         foreach ( var kvp in schema.Attributes )
         {
            ItemInfoKVP item = new ItemInfoKVP() { Key = kvp.Key, Value = kvp.Value};
            Attributes.Add(item);
            item.OnItemEndEdit += item_OnItemEndEdit;
            item.PropertyChanged += item_PropertyChanged;
            item.PropertyChanging += item_PropertyChanging;
         }

         Attributes.CollectionChanged += Attributes_CollectionChanged;
      }

      void item_PropertyChanging(object sender, PropertyChangingEventArgs e)
      {
         PropertyChangingEventArgsEx args = e as PropertyChangingEventArgsEx;
         if ( e.PropertyName == "Key" )
         {
            ItemInfoKVP item = sender as ItemInfoKVP;

            if ( Attributes.Any((i) => ((i != item) && (i.Key.Equals(args.NewValue)))) )
            {
               throw new Exception("Duplicate Attribute Name");
            }
         }
      }

      void item_PropertyChanged(object sender, PropertyChangedEventArgs e)
      {
         
      }

      void item_OnItemEndEdit(IEditableObject sender)
      {
         /*// Check if there is an duplicate
         ItemInfoKVP item = sender as ItemInfoKVP;

         if ( Attributes.Any((i) => ( (i != item) &&  (i.Key == item.Key)) ) )
         {
            throw new Exception("Duplicate Attribute Name");
         }*/
      }

      void Attributes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
      {
         if ( e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
         {
            IEnumerable<ItemInfoKVP> items = e.NewItems.OfType<ItemInfoKVP>();

            foreach ( var item in items )
            {
               item.OnItemEndEdit += item_OnItemEndEdit;
               item.PropertyChanged += item_PropertyChanged;
               item.PropertyChanging += item_PropertyChanging;
            }
         }

         if ( e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove )
         {
            System.Diagnostics.Trace.WriteLine("Item Removed");
         }
      }

      public void Update()
      {
         EntitySchema schema = new EntitySchema(EntityType, Category, SupportedEntities);
         EntitySchema.AddSchema(schema);
         foreach ( var item in Attributes )
         {
            schema.AddAttribute(item.Key, item.Value);
         }
      }

      public ObservableCollection<ItemInfoKVP> Attributes
      {
         get;
         private set;
      }

   }
}
