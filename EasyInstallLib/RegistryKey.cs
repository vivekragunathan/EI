using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.Win32;

namespace EasyInstall
{
   [DataContract]
   class RegistryKey : DeployableEntity
   {
      public override bool CanBeDeployed()
      {
         throw new NotImplementedException();
      }

      protected override void OnDeploy()
      {
         throw new NotImplementedException();
      }

      protected override void OnUndeploy()
      {
         throw new NotImplementedException();
      }
   }
}
