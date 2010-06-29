using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.Win32;

namespace EasyInstall
{
   [DataContract]
   class RegKey : DeployableEntity
   {
      public RegistryHive Hive
      {
         get
         {
            return GetAttribute<RegistryHive>("Hive");
         }
      }

      public string Path
      {
         get
         {
            return GetAttribute<string>("Path");
         }
      }

      public override bool CanBeDeployed()
      {
         
      }

      protected override void OnDeploy()
      {
         RegistryKey rk = RegistryKey.OpenBaseKey(Hive, RegistryView.Registry32);

         if (null != rk)
         {
            rk.CreateSubKey(Path);
         }
      }

      protected override void OnUndeploy()
      {
         throw new NotImplementedException();
      }
   }
}
