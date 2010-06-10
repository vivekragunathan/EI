using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Permissions;
using EasyInstall.Interfaces;
using System.Security;
using System.Runtime.Serialization;
using System.Security.AccessControl;
using EasyInstall.Interfaces.Entities;

namespace EasyInstall
{
   [DataContract]
   class FolderEntity : DeployableEntity
   {
      public FolderEntity()
      {
         // Ctor for serialization/deserialization
      }

      protected override void OnParentDeserialized(IEntity entity)
      {
         RegisterEvents();
      }

      void RegisterEvents()
      {
         // Register for attibute changed event of parent

         IEntity parent = Parent;

         parent.OnAttributeChanged += Parent_OnAttributeChanged;

         if ( parent.Attributes.Any((e) => e.Key == "DestinationFolder") )
         {
            UpdateDestination(parent.GetAttribute<string>("DestinationFolder"));
         }
      }

      void UnregisterEvents()
      {
         // Register for attibute changed event of parent

         Parent.OnAttributeChanged -= Parent_OnAttributeChanged;
      }

      public string DestinationFolder
      {
         get
         {
            return GetAttribute<string>("DestinationFolder");
         }
      }

      public string SourceFolder
      {
         get
         {
            return GetAttribute<string>("SourceFolder");
         }
      }

      public FolderEntity(string name, IPackage package, Guid parentID, object createParams)
         :base(name, EntityTypes.FOLDERENTITY, package, parentID)
      {
         // Override the schema first if required

         // Overriding is required to have instance specific schemas
         // The DestinationFolder attribute of the folder entity is
         // either readonly or not based on whether it is a direct child of a package entity
         // or not. If it is a child of a Package the attribute is readonly and
         // if it not it is editable
         //
         if ( parentID == Package.ID )
         {
            EntitySchema newSchema = (EntitySchema)(Schema as IMutableEntitySchema).Copy();
            SchemaItemInfo itemInfo= newSchema.Attributes.First((e) => e.Key == "DestinationFolder").Value;
            itemInfo.IsReadonly = false;

            OverrideSchema(newSchema);
         }

         SetAttribute("FolderName", Name);

         if ( createParams != null )
         {
            SetAttribute("SourceFolder", createParams);

            System.Diagnostics.Trace.WriteLine("Entity created. source Path: " + SourceFolder);
         }

         RegisterEvents();
      }

      void UpdateDestination(string destPath)
      {
         string destFolder = Path.Combine(destPath, GetAttribute<string>("FolderName"));

         SetAttribute("DestinationFolder", destFolder);
      }

      public override bool CanBeDeployed()
      {
         // Check for Write Permissions on the destination directory
         try
         {
            FileIOPermission fp = new FileIOPermission(FileIOPermissionAccess.Read | FileIOPermissionAccess.Write, DestinationFolder);

            fp.Demand();
         }
         catch ( SecurityException e )
         {
            System.Diagnostics.Trace.WriteLine(e.ToString());
            return false;
         }
         
         return !SubEntities.Any((e) => !(e as IDeployableEntity).CanBeDeployed());
      }

      protected override void OnDeploy()
      {
         // Create the folder and call deploy on child entites
         Directory.CreateDirectory(DestinationFolder);

         System.Diagnostics.Trace.WriteLine(string.Format("Folder {0} created successfully!", DestinationFolder));

         foreach ( IDeployableEntity entity in SubEntities )
         {
            if ( entity != null )
               entity.Deploy();
         }
      }

      protected override void OnUndeploy()
      {
         try
         {
            foreach ( var entity in from de in SubEntities
                                    where de is IDeployableEntity
                                    select de as IDeployableEntity )
            {
               entity.Undeploy();
            }

            if ( Directory.Exists(DestinationFolder) )
            {
               Directory.Delete(DestinationFolder);
            }
         }
         catch ( Exception e)
         {
            System.Diagnostics.Trace.Write(string.Format("Error deleting entity: {0} - {1}", ToString(), e.ToString()));
            throw;
         }
      }

      protected override void AttributeChanging(EntityAttributeEventArgs e)
      {
         if ( e.AttributeName == "SourceFolder" )
         {
            if ( !Directory.Exists(e.NewValue.ToString()) )
            {
               throw new DirectoryNotFoundException(e.NewValue.ToString());
            }
         }
      }

      protected override void AttributeChanged(EntityAttributeEventArgs e)
      {
         if ( e.AttributeName == "SourceFolder" )
         {
            _subEntities.Clear();
            // Remove all the sub entities and read the entire sub folder contents and create new
            // sub entities

            RecursiveReadFolderAndCreateEntities(this, SourceFolder);
         }

         if ( e.AttributeName == "FolderName" )
         {
            UpdateDestination(GetAttribute<string>("DestinationFolder"));
         }
      }

      void Parent_OnAttributeChanged(object sender, EntityAttributeEventArgs e)
      {
         if (e.AttributeName == "DestinationFolder")
         {
            // Modify the Destination folder of this folder according to the
            // parent entity's attribute

            UpdateDestination(e.NewValue as string);
         }
      }

      private void RecursiveReadFolderAndCreateEntities(IEntity entity, string folder)
      {
         foreach ( var file in Directory.GetFiles(folder) )
         {
            FileEntity fe = entity.CreateChild(EntityTypes.FILEENTITY, Path.GetFileName(file)) as FileEntity;

            if ( fe != null )
            {
               fe.SetAttribute("SourcePath", file);
               fe.SetAttribute("FileName", Path.GetFileName(file));
            }
         }

         foreach ( var subFolder in Directory.GetDirectories(folder) )
         {
            FolderEntity fle = this.CreateChild(this.Schema.EntityType, Path.GetFileName(subFolder)) as FolderEntity;
            fle.SetAttribute("SourceFolder", subFolder);
            fle.SetAttribute("FolderName", Path.GetFileName(subFolder));
         }
      }

      protected override void Dispose(bool disposing)
      {
         if ( !disposed )
         {
            if ( disposing )
            {
               Parent.OnAttributeChanged -= Parent_OnAttributeChanged;               
            }
         }

         base.Dispose(disposing);
      }
   }
}
