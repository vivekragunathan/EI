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
using System.Threading;

namespace EasyInstallIDE
{
   /// <summary>
   /// Interaction logic for Window1.xaml
   /// </summary>
   public partial class Window1 : Window
   {
      #region APPLICATION COMMAND HANDLERS

      private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         try
         {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Package Files(*.xml)|*.xml";

            ofd.DefaultExt = ".xml";

            if (ofd.ShowDialog() == true)
            {
               CurrentPackage[0] = PackageManager.LoadPackage(ofd.SafeFileName);
               CurrentFileName = ofd.SafeFileName;
               this.DataContext = CurrentPackage;
            }
         }
         catch (Exception)
         {
            MessageBox.Show("Unable load package. Package may be corrupt!");
         }
      }

      private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         if (string.IsNullOrEmpty(CurrentFileName))
         {
            SaveAsCommand.Command.Execute(this);
         }
      }

      private void SaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         e.CanExecute = (this.DataContext != null);
      }

      private void SaveAsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         try
         {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Package Files(*.xml)|*.xml";

            sfd.DefaultExt = ".xml";

            sfd.AddExtension = true;

            if (sfd.ShowDialog() == true)
            {
               using (FileStream fs = new FileStream(sfd.SafeFileName, FileMode.Create, FileAccess.ReadWrite))
               {
                  CurrentPackage[0].Package.Save(fs);
                  CurrentFileName = sfd.SafeFileName;
               }
            }
         }
         catch (Exception ex)
         {
            MessageBox.Show("Unable to Save package." + ex.ToString());
         }
      }

      private void CloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         CurrentPackage[0] = null;
         this.DataContext = CurrentPackage;
      }

      #endregion
      private void AddFileEntityCommand_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         EntityViewModel evm = GetCurrentEntity(e.Parameter);

         System.Diagnostics.Debug.Assert(null != evm);

         if ( evm == null )
            return;

         evm.Entity.CreateChild(EntityTypes.FILEENTITY, "File");
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

         evm.Entity.CreateChild(EntityTypes.FOLDERENTITY, "Folder");
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

      private void BuildPackageCommand_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         PackageViewModel pvm = GetCurrentEntity(e.Parameter) as PackageViewModel;

         if ( pvm == null )
            return;

         ThreadPool.QueueUserWorkItem((obj) => { pvm.Build(); });
      }

      private void BuildPackageCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         PackageViewModel pvm =  GetCurrentEntity(e.Parameter) as PackageViewModel;

         e.CanExecute = (null != pvm) && (!pvm.IsBuildInProgress);
      }
   }
}