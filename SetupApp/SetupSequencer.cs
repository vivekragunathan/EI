using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using EasyInstall.Interfaces;
using EasyInstall.ViewModels;
using System.Windows.Controls;
using System.Xaml;
using EasyInstall.SequencerStateMachine;

namespace EasyInstall
{
   class SetupSequencer : ViewModelBase
   {
      public NavigationContext Context  { get; private set; }
      SetupSequencerSM stateMachine;

      public SetupSequencer(PackageViewModel package)
      {
         Context = new NavigationContext(package);
         
         stateMachine = new SetupSequencerSM(Context);
         stateMachine.Start();
      }

      public void Next()
      {
         stateMachine.Next();
      }

      public void Previous()
      {
         stateMachine.Previous();
      }

      public void Cancel()
      {
         stateMachine.Cancel();
      }
   }
}
