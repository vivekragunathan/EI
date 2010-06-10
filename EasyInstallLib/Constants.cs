using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyInstall
{
   public class EntityTypes
   {
      public const string PACKAGEENTITY = "Package";
      public const string FILEENTITY = "File";
      public const string FOLDERENTITY = "Folder";
      public const string REGISTRYKEYENTITY = "RegistryKey";
      public const string REGISTRYVALUEENTITY = "RegistryValue";
   }

   public class EntityCategories
   {
      public const string MISCELLANEOUS = "Miscellaneous";
      public const string FILESANDFOLDERS = "Folders and Files";
      public const string REGISTRY = "Registry";
   }
}
