using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using EasyInstall.Interfaces;
using EasyInstall.Interfaces.Entities;

namespace EasyInstall
{
   [DataContract]
   public class FileEntity : DeployableEntity
   {
      public FileEntity()
      {
      }

      public FileEntity(string name, IPackage package, Guid parentID, object createParams)
         : base(name, EntityTypes.FILEENTITY, package, parentID)
      {
         if (parentID == Package.ID)
         {
            var newSchema = (EntitySchema) ((IMutableEntitySchema) Schema).Copy();
            SchemaItemInfo itemInfo = newSchema.Attributes.First((e) => e.Key == "DestinaltionPath").Value;
            itemInfo.IsReadonly = false;

            OverrideSchema(newSchema);
         }

         if (createParams != null)
         {
            SetAttribute("SourcePath", createParams);

            Trace.WriteLine("Entity created. source Path: " + SourcePath);
         }

         Parent.OnAttributeChanged += Parent_OnAttributeChanged;

         if (Parent.Attributes.Any((e) => e.Key == "DestinationFolder"))
         {
            UpdateDestination(Parent.GetAttribute<string>("DestinationFolder"));
         }
      }

      private string SourcePath
      {
         get
         {
            var path = GetAttribute<string>("SourcePath");

            if (!File.Exists(path))
               throw new FileNotFoundException("Invalid Source path", path);

            return path;
         }
      }

      private string DestinationPath
      {
         get { return GetAttribute<string>("DestinationPath"); }
      }

      private bool Register
      {
         get { return GetAttribute<bool>("Register"); }
      }

      private bool IsService
      {
         get { return GetAttribute<bool>("IsService"); }
      }

      protected override void OnParentDeserialized(IEntity entity)
      {
         RegisterForEvents();
      }

      private void RegisterForEvents()
      {
         // Register for attibute changed event of parent

         IEntity parent = Parent;

         parent.OnAttributeChanged += Parent_OnAttributeChanged;

         if (parent.Attributes.Any(e => e.Key == "DestinationFolder"))
         {
            UpdateDestination(parent.GetAttribute<string>("DestinationFolder"));
         }
      }

      private void UpdateDestination(string destFolder)
      {
         string destPath = Path.Combine(destFolder, GetAttribute<string>("FileName"));

         SetAttribute("DestinationPath", destPath);
      }

      private void Parent_OnAttributeChanged(object sender, EntityAttributeEventArgs e)
      {
         if (e.AttributeName == "DestinationFolder")
         {
            // Modify the Destination folder of this folder according to the
            // parent entity's attribute

            UpdateDestination(e.NewValue as string);
         }
      }

      protected override void AttributeChanged(EntityAttributeEventArgs e)
      {
         if (e.AttributeName == "FileName")
         {
            UpdateDestination(Path.GetDirectoryName(GetAttribute<string>("DestinationPath")));
         }
      }

      public override string ToString()
      {
         return String.Format("Entity Name: {0}, Description: {1}", Name, GetAttribute("SourcePath"));
      }

      #region Actions

      private static void RegisterCOM(FileEntity e)
      {
         // TODO: 
         Trace.WriteLine(string.Format("Registering COM Component '{0}' '{1}' - '{2}'",
                                       e.Name,
                                       e.ID, e.DestinationPath));
      }

      private static void InstallService(FileEntity e)
      {
         // TODO: 
         Trace.WriteLine(string.Format("Installing Service'{0}' '{1}' - '{2}'",
                                       e.Name,
                                       e.ID, e.DestinationPath));
      }

      #endregion

      #region OVERRIDES

      public override bool CanBeDeployed()
      {
         try
         {
            var fp = new FileIOPermission(FileIOPermissionAccess.Read
                                          | FileIOPermissionAccess.Write, DestinationPath);

            fp.Demand();
         }
         catch (SecurityException e)
         {
            Trace.WriteLine(e.ToString());
            return false;
         }

         return true;
      }

      protected override void OnDeploy()
      {
         try
         {
            CanBeDeployed();

            var folderName = Path.GetDirectoryName(DestinationPath);

            if (!Directory.Exists(folderName))
            {
               Directory.CreateDirectory(folderName);
            }

            // Copy the data in stream as the destination file
            using (var s = Package.GetData(ID))
            using (var fs = File.Create(DestinationPath))
            {
               s.CopyTo(fs);

               Trace.WriteLine(string.Format("File {0} deployed successfully!", DestinationPath));
            }
         }
         catch (Exception)
         {
            throw;
         }
      }

      private void Package_OnDeployed(object sender, EntityEventArgs e)
      {
         try
         {
            if (Register)
            {
               RegisterCOM(this);
            }

            if (IsService)
            {
               InstallService(this);
            }
         }
         finally
         {
            Package.OnDeployed -= Package_OnDeployed;
         }
      }

      protected override void OnUndeploy()
      {
         try
         {
            if (File.Exists(DestinationPath))
            {
               File.Delete(DestinationPath);
            }
         }
         catch (Exception e)
         {
            Trace.Write(string.Format("Error deleting entity: {0} - {1}", ToString(), e));
            throw;
         }
      }

      public override Stream GetContent()
      {
         // Create a file stream from the source
         return new FileStream(SourcePath, FileMode.Open);
      }

      #endregion
   }
}