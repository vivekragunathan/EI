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
using System.Windows.Media;

namespace EasyInstallIDE
{
   /// <summary>
   /// Interaction logic for Window1.xaml
   /// </summary>
   public partial class Window1 : Window
   {
      const string TITLE = "EasyInstall";

      public static ObservableCollection<PackageViewModel> CurrentPackage
      {
         get;
         set;
      }

      static Window1()
      {
         CurrentPackage = new ObservableCollection<PackageViewModel>();

         CurrentPackage.Add(PackageManager.CreateNewPackage("TestPackage"));
      }

      string _currentFileName;
      string CurrentFileName
      {
         get { return _currentFileName; }
         set 
         {
            _currentFileName = value;

            // Update the Title
            this.Title = string.Format("{0} - {1}", TITLE, value);
         }
      }
      public Window1()
      {
         InitializeComponent();
      }

      private void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         CurrentPackage[0] = PackageManager.CreateNewPackage("TestPackage");

         this.DataContext = null;
         this.DataContext = CurrentPackage;
      }

      private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         try
         {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Package Files(*.xml)|*.xml";

            ofd.DefaultExt = ".xml";

            if ( ofd.ShowDialog() == true )
            {
               CurrentPackage[0] = PackageManager.LoadPackage(ofd.SafeFileName);
               CurrentFileName = ofd.SafeFileName;
               this.DataContext = CurrentPackage;
            }
         }
         catch ( Exception )
         {
            MessageBox.Show("Unable load package. Package may be corrupt!");
         }
      }

      private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         if ( string.IsNullOrEmpty(CurrentFileName) )
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

            if ( sfd.ShowDialog() == true )
            {
               using ( FileStream fs = new FileStream(sfd.SafeFileName, FileMode.Create, FileAccess.ReadWrite) )
               {
                  CurrentPackage[0].Package.Save(fs);
                  CurrentFileName = sfd.SafeFileName;
               }
            }
         }
         catch ( Exception ex )
         {
            MessageBox.Show("Unable to Save package.");
         }
      }

      private void CloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         CurrentPackage[0] = null;
         this.DataContext = CurrentPackage;
      }

      private void Themes_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         Settings.Default.Theme = e.AddedItems[0].ToString();
      }

      private void Window_Loaded(object sender, RoutedEventArgs e)
      {
         Themes.ItemsSource = ThemeManager.GetThemes();

         Themes.SelectedItem = Settings.Default.Theme;
      }

      private void TreeView_ContextMenuOpening(object sender, ContextMenuEventArgs e)
      {
         EntityViewModel evm = (sender as ContentPresenter).Tag as EntityViewModel;

         if ( evm == null )
            return;

         TreeWorkArea.ContextMenu = GetContextMenu(evm);
      }

      ContextMenu GetContextMenu(EntityViewModel evm)
      {
         ContextMenu menu = new ContextMenu();

         IEnumerable<string> entities;

         if ( evm is PackageViewModel )
         {
            entities = from e in EntitySchema.SchemaNames
                       where e != EasyInstall.Constants.PACKAGEENTITY
                       select e;
         }
         else
         {
            entities = evm.Entity.Schema.SupportedEntityTypes;
         }

         // Add Entity Creation menu Items
         if ( entities.Count() > 0 )
         {
            MenuItem mi = new MenuItem();
            mi.Header = "Add";

            foreach ( var entity in entities )
            {
               MenuItem submi = new MenuItem();
               submi.Header = entity;
               submi.Command = EasyInstall.ViewModels.Commands.GetRoutedCommandForEntity(entity);
               submi.CommandParameter = evm;
               mi.Items.Add(submi);
            }

            menu.Items.Add(mi);
            menu.Items.Add(new Separator());
         }
         // Rename
         //{
         //   MenuItem mi = new MenuItem();
         //   mi.Header = "Rename";
         //   //mi.Command = EasyInstall.ViewModels.Commands.RenameEntity;
         //   mi.CommandParameter = evm;
         //   mi.CommandParameter = mi;
         //   menu.Items.Add(mi);
         //}

         // Add Delete current
         if (! (evm is PackageViewModel) )
         {
            
            MenuItem mi = new MenuItem();
            mi.Header = "Remove";
            mi.Command = EasyInstall.ViewModels.Commands.RemoveEntity;
            mi.CommandParameter = evm;
            menu.Items.Add(mi);
         }

         if ( evm.Entity.SubEntities.Count() > 0 )
         {
            MenuItem mi = new MenuItem();
            mi.Header = "Remove Children";
            mi.Command = EasyInstall.ViewModels.Commands.RemoveAllEntities;
            mi.CommandParameter = evm;
            menu.Items.Add(mi);
         }

         return menu;
      }

      private void TreeWorkArea_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
      {

      }

   }
}
