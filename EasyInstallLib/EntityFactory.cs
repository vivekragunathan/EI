using System;
using System.Collections.Generic;
using EasyInstall.Interfaces;
using System.IO;

namespace EasyInstall
{
   public  static class EntityFactory
   {
      internal static IPackage CreatePackage(string name)
      {
         return new Package(name);
      }

      internal static IEntity CreateEntity(string entityType, string name, IPackage package, Guid parentID, object createParams)
      {
         if (entityType != null)
            switch (entityType)
            {
               case EntityTypes.FILEENTITY:
                  {
                     return new FileEntity(name, package, parentID, createParams);
                  }
               case EntityTypes.FOLDERENTITY:
                  {
                     return new FolderEntity(name, package, parentID, createParams);
                  }
            }

         throw new Exception("Entity type not supported!");
      }

      public static IList<string> GetEntities(object o)
      {
         // TODO: Create the entities by asking the Entity Type objects
         // This following is only a temporary solution.
         // It MUST be changed to a better approach

         // Right now we will support only File and Folder entities in this method

         var entities = new List<string>();

         if ( o == null )
            return entities;

         var path = o.ToString();

         if ( Path.GetFileName(path) != string.Empty )
         {
            if (File.Exists(path))
            {
               entities.Add(EntityTypes.FILEENTITY);
            }
         }
         else
         // possibly a directory

         if ( Path.GetDirectoryName(path) != string.Empty )
         {
            // Check if the directory exists
            if (Directory.Exists(path))
            {
               entities.Add(EntityTypes.FOLDERENTITY);
            }
         }

         return entities;
      }
   }
}
