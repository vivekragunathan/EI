using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace EasyInstall
{
   [DataContract]
   class DeployAction
   {
      public string Name
      {
         get;
         private set;
      }

      public DeployAction(string name)
      {
         if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Action name should not be null or empty");

         this.Name = Name;
      }

      public void Execute()
      {

      }

      public void Unexecute()
      {

      }
   }
}
