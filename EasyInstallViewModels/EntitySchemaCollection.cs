using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using EasyInstall;
using System.Data;

namespace EasyInstall.ViewModels
{
   public class EntitySchemaCollection : ObservableCollection<EntitySchemaViewModel>
   {
      public EntitySchemaCollection()
      {
         foreach ( var kvp in EntitySchema.Schemas )
         {
            this.Add(new EntitySchemaViewModel(this, kvp.Value));
         }

         this.OrderBy((s) => s.Schema.EntityType);

         EntitySchema.SchemaAdded += EntitySchema_SchemaAdded;
         EntitySchema.SchemaRemoved += EntitySchema_SchemaRemoved;
      }

      void EntitySchema_SchemaRemoved(object sender, EntitySchema.SchemaCollectionChangedEventArgs e)
      {
         if ( suspendUpdate )
            return;

         var viewModel = from evm in this
                 where evm.Schema.EntityType == e.Schema.EntityType
                 select evm;

         this.Remove(viewModel as EntitySchemaViewModel);
      }

      void EntitySchema_SchemaAdded(object sender, EntitySchema.SchemaCollectionChangedEventArgs e)
      {
         if ( suspendUpdate )
            return;

         this.Add(new EntitySchemaViewModel(this, e.Schema));
      }

      public void AddSchema(EntitySchema schema)
      {
         EntitySchema.AddSchema(schema);
      }

      public void RemoveSchema(EntitySchema schema)
      {
         EntitySchema.RemoveSchema(schema);
      }

      bool suspendUpdate = false;
      public void SaveChanges(string filename)
      {
         suspendUpdate = true;
         try
         {
            EntitySchema.RemoveAllSchemas();

            List<EntitySchemaViewModel> copy = this.ToList();

            foreach ( var schemaViewModel in copy )
            {
               schemaViewModel.Update();
            }

            EntitySchema.SaveSchemas(filename);
         }
         finally
         {
            suspendUpdate = false;
         }
      }

      public void LoadSchemas(string filename)
      {
         this.ClearItems();

         EntitySchema.LoadSchemas(filename);
      }
      
   }
}
