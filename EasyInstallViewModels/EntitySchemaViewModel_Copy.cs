using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Collections;

namespace EasyInstall.ViewModels
{
   public class ItemInfoKVP : ViewModelBase, IEditableObject
   {
      ItemInfoKVP Copy;

      public ItemInfoKVP()
      {
         _key = "[New Key]";

         _value = new EntitySchema.ItemInfo();
         _value.AllowNull = true;
         _value.IsReadonly = false;
         _value.TypeName = typeof(System.String).FullName;
      }
      
      ItemInfoKVP(ItemInfoKVP kvp)
      {
         Key = this.Key;
         Value = new EntitySchema.ItemInfo()
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
               OldKey = _key;
               _key = value;
               NotifyCurrentPropertyChanged();
            }
         }
      }

      public bool IsEditing
      {
         get;
         private set;
      }

      public string OldKey { get; private set; }

      EntitySchema.ItemInfo _value;
      public EntitySchema.ItemInfo Value
      {
         get { return _value; }
         set
         {
            _value = value;
            NotifyCurrentPropertyChanged();
         }
      }

      #region IEditableObject Members

      public void BeginEdit()
      {
         IsEditing = true;
         if ( Copy == null )
         {
            Copy = new ItemInfoKVP(this);
         }
      }

      public void CancelEdit()
      {
         this.Key = Copy.Key;
         this.Value = Copy.Value;

         IsEditing = false;
      }

      public void EndEdit()
      {
         Copy = null;

         IsEditing = false;
      }

      #endregion
   }

   public class EntitySchemaViewModel : ViewModelBase
   {
      public EntitySchema Schema { get; private set; }

      bool suspendUpdate = false;
      EntitySchemaCollection SrcCollection;
      public EntitySchemaViewModel(EntitySchemaCollection srcCollection, EntitySchema schema)
      {
         SrcCollection = srcCollection;
         Attributes = new ObservableCollection<ItemInfoKVP>();
         this.Schema = schema;

         foreach ( var kvp in schema.Attributes )
         {
            Attributes.Add(new ItemInfoKVP() { Key = kvp.Key, Value = kvp.Value });
         }

         foreach ( var item in Attributes )
         {
            item.PropertyChanged += new PropertyChangedEventHandler(item_PropertyChanged);
         }

         schema.OnAttributeAdded += schema_OnAttributeAdded;
         schema.OnAttributeRemoved += schema_OnAttributeRemoved;

         Attributes.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Attributes_CollectionChanged);
      }

      void item_PropertyChanged(object sender, PropertyChangedEventArgs e)
      {
         if ( e.PropertyName == "Key" )
         {
            suspendUpdate = true;
            try
            {
               ItemInfoKVP item = sender as ItemInfoKVP;

               if ( Schema.Attributes.Any((i) => i.Key == item.Key) )
               {
                  throw new Exception("Duplicate Attribute");
               }

               // The Key has changed, so update the map
               this.Schema.RemoveSchemaItem(item.OldKey);

               this.Schema.AddSchemaItem(item.Key, item.Value);
            }
            finally
            {
               suspendUpdate = false;
            }
         }
      }

      void Attributes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
      {
         try
         {
            suspendUpdate = true;
            switch ( e.Action )
            {
               case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                  {
                     foreach ( ItemInfoKVP item in e.NewItems )
                     {
                        item.PropertyChanged += item_PropertyChanged;
                        Schema.AddSchemaItem(item.Key, item.Value);
                     }
                     break;
                  }
               case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                  {
                     foreach ( ItemInfoKVP item in e.NewItems )
                     {
                        item.PropertyChanged += item_PropertyChanged;
                        Schema.RemoveSchemaItem(item.Key);
                     }
                     break;
                  }
            }
         }
         finally
         {
            this.suspendUpdate = false;
         }
      }

      void schema_OnAttributeRemoved(object sender, SchemaChangedEventArgs e)
      {
         if ( !suspendUpdate )
            Attributes.Remove(new ItemInfoKVP() { Key = e.AttributeName, Value = e.Info });
      }

      void schema_OnAttributeAdded(object sender, SchemaChangedEventArgs e)
      {
         if ( !suspendUpdate )
         {
            ItemInfoKVP kvp = new ItemInfoKVP() { Key = e.AttributeName, Value = e.Info };

            Attributes.Add(kvp);

            kvp.PropertyChanged += item_PropertyChanged;
         }
      }

      public ObservableCollection<ItemInfoKVP> Attributes
      {
         get;
         private set;
      }

   }
}
