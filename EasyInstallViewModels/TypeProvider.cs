using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Reflection;

namespace EasyInstall.ViewModels
{
   public class TypeProvider
   {
      public class TypeInfo
      {
         public string TypeName { get; private set; }
         public bool BuiltIn { get; private set;  }

         public TypeInfo(string typeName, bool builtIn)
         {
            this.TypeName = typeName;
            this.BuiltIn = builtIn;
         }
      }

      static string[] _builtInTypes = new String[]
      {
         "System.String",
         "System.Int32",
         "System.Int16",
         "System.Int64",
         "System.Object",
         "System.Boolean",
         "System.Byte",
         "System.Char",
         "System.Double",
         "System.Decimal",

         /*"System.String[]",
         "System.Int32[]",
         "System.Int16[]",
         "System.Int64[]",
         "System.Object[]",
         "System.Boolean[]",
         "System.Byte[]",
         "System.Char[]",
         "System.Double[]",
         "System.Decimal[]",*/
      };

      public static ObservableCollection<TypeInfo> _Types = new ObservableCollection<TypeInfo>();

      public static ObservableCollection<TypeInfo> Types
      {
         get { return _Types; }
      }

      static TypeProvider()
      {
         foreach (var type in _builtInTypes)
         {
            Types.Add(new TypeInfo(type, true));
         }

         // Refresh from Schema

         var schemas = from schema in EntitySchema.Schemas
                     select schema.Value;

         foreach ( var schema in schemas )
         {
            foreach ( var attr in schema.Attributes )
            {
               AddNewType(attr.Value.TypeName);
            }
         }
      }

      public static bool IsBuiltIn(string typeName)
      {
         return _builtInTypes.Contains(typeName);
      }

      public static bool AddNewType(string typeFullName)
      {
         Type t = Type.GetType(typeFullName);

         if ( _Types.Any((type) => type.TypeName == t.FullName) )
            return false;

         Types.Add(new TypeInfo(typeFullName, false));

         return true;
      }
   }
}
