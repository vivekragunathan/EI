using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using WPF.Themes;
using EasyInstallIDE.Properties;
using EasyInstall.ViewModels;
using EasyInstall.Interfaces;
using EasyInstall;
using Microsoft.Win32;
using System.IO;
using System.Collections.ObjectModel;

namespace EasyInstallIDE
{
   /// <summary>
   /// Interaction logic for Window1.xaml
   /// </summary>
   public partial class Window1 : Window
   {
      private void AddFileEntityCommand_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         EntityViewModel evm = GetCurrentEntity(e.Parameter);

         System.Diagnostics.Debug.Assert(null != evm);

         if ( evm == null )
            return;

         evm.Entity.CreateChild(Constants.FILEENTITY, "File");
      }

      private void AddFileEntityCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         e.CanExecute = true;
      }

      private void AddFolderEntityCommand_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         EntityViewModel evm = GetCurrentEntity(e.Parameter);

         System.Diagnostics.Debug.Assert(null != evm);

         if ( evm == null )
            return;

         evm.Entity.CreateChild(Constants.FOLDERENTITY, "Folder");
      }

      private void AddFolderEntityCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         e.CanExecute = true;
      }

      private void RemoveEntityCommand_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         EntityViewModel evm = GetCurrentEntity(e.Parameter);

         evm.Entity.Parent.RemoveEntity(evm.Entity);
      }

      private void RemoveEntityCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         e.CanExecute = true;
      }

      private void RemoveAllEntitiesCommand_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         EntityViewModel evm = GetCurrentEntity(e.Parameter);

         List<IEntity> entities = new List<IEntity>(evm.Entity.SubEntities);

         entities.ForEach((en) => evm.Entity.RemoveEntity(en));
      }

      private void RemoveAllEntitiesCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         e.CanExecute = true;
      }
   }
}