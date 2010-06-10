using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EasyInstall.ViewModels;
using EasyInstall.Controls;
using System.Collections.Generic;
using System.Linq;
using EasyInstall.Interfaces;
using System.Windows.Media;
using EasyInstall;
using System.IO;
using System.Collections.ObjectModel;
using System;

namespace EasyInstallIDE
{
   /// <summary>
   /// Interaction logic for Window1.xaml
   /// </summary>
   public partial class Window1 : Window
   {
      #region Context menu

      void GetEntityAtCursor(Point p, out TreeViewItem item, out EntityViewModel evm)
      {
         evm = null;

         HitTestResult htr = VisualTreeHelper.HitTest(TreeWorkArea, p);

         item = htr.VisualHit.FindVisualParent<TreeViewItem>();
         
         if (item != null)
         {
            evm = item.Header as EntityViewModel;
         }
      }

      private void TreeView_ContextMenuOpening(object sender, ContextMenuEventArgs e)
      {
         TreeWorkArea.ContextMenu = null;

         TreeViewItem tvItem;
         EntityViewModel evm;

         GetEntityAtCursor(Mouse.GetPosition(TreeWorkArea), out tvItem, out evm);

         if (tvItem == null)
         {
            evm = CurrentPackage[0];
         }
         else
         {
            evm = tvItem.Header as EntityViewModel;
         }

         if (evm == null)
         {
            Nullable<KeyValuePair<string, ObservableCollection<EntityViewModel>>> catInfo;
            if (tvItem.Header is KeyValuePair<string, ObservableCollection<EntityViewModel>>)
            {
               catInfo = (KeyValuePair<string, ObservableCollection<EntityViewModel>>)tvItem.Header;
            }

            // Check if the Item is a category

            return;
         }

         TreeWorkArea.ContextMenu = GetContextMenu(evm);
      }


      private void ContentPresenter_ContextMenuClosing(object sender, ContextMenuEventArgs e)
      {
      }

      ContextMenu GetContextMenu(EntityViewModel evm)
      {
         ContextMenu menu = new ContextMenu();

         IEnumerable<string> entities;

         if (evm is PackageViewModel)
         {
            entities = from e in EntitySchema.SchemaNames
                       where e != EasyInstall.EntityTypes.PACKAGEENTITY
                       select e;
         }
         else
         {
            entities = evm.Entity.Schema.SupportedEntityTypes;
         }

         // Add Entity Creation menu Items
         if (entities.Count() > 0)
         {
            MenuItem mi = new MenuItem();
            mi.Header = "Add";

            foreach (var entity in entities)
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
         
         // Add Delete current
         if (!(evm is PackageViewModel))
         {
            MenuItem mi = new MenuItem();
            mi.Header = "Remove";
            mi.Command = EasyInstall.ViewModels.Commands.RemoveEntity;
            mi.CommandParameter = evm;
            menu.Items.Add(mi);
         }

         if ( evm is PackageViewModel )
         {
            MenuItem mi = new MenuItem();
            mi.Header = "Build";
            mi.Command = EasyInstall.ViewModels.Commands.BuildPackage;
            mi.CommandParameter = evm;
            menu.Items.Add(mi);
         }

         if (evm.Entity.SubEntities.Count() > 0)
         {
            MenuItem mi = new MenuItem();
            mi.Header = "Remove Children";
            mi.Command = EasyInstall.ViewModels.Commands.RemoveAllEntities;
            mi.CommandParameter = evm;
            menu.Items.Add(mi);
         }

         return menu;
      }
      #endregion

      #region Navigation
      private void TreeWorkArea_KeyDown(object sender, KeyEventArgs e)
      {
         if ( e.Key == Key.F2 )
         {
            SetCurrentItemInEditMode(true);
         }
      }

      private void SetCurrentItemInEditMode(bool editMode)
      {
         TreeViewItem tvItem = TreeWorkArea.Tag as TreeViewItem;

         if (tvItem.Header is EntityViewModel)
         {
            EditableTextBlock etb = tvItem.FindVisualChild<EditableTextBlock>();
            etb.IsInEditMode = editMode;
         }
      }

      EntityViewModel GetCurrentEntity(object parameter)
      {
         if ( parameter == null )
         {
            return CurrentEntity;
         }
         return parameter as EntityViewModel;
      }

      public EntityViewModel CurrentEntity { get; private set; } // Keeps track of the currently selected entity
      private void TreeWorkArea_Selected(object sender, RoutedEventArgs e)
      {
         TreeWorkArea.Tag = e.OriginalSource;
         CurrentEntity = (e.OriginalSource as TreeViewItem).Header as EntityViewModel;

         Resources["SelectedEntity"] = CurrentEntity;
      }

      #endregion

      private void TreeWorkArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
      {

      }

      private void TreeWorkArea_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
      {

      }

      #region DRAG N DROP

      IList<string> draggedEntites;

      private void TreeWorkArea_GiveFeedback(object sender, GiveFeedbackEventArgs e)
      {
         System.Diagnostics.Debug.WriteLine(e.Source.ToString());

      }

      private void TreeWorkArea_DragOver(object sender, DragEventArgs e)
      {
         e.Effects = DragDropEffects.None;

         EntityViewModel evm;
         if ( CheckDropTarget(e, out evm) )
         {
            e.Effects = DragDropEffects.Link;
         }

         e.Handled = true;
      }

      private void TreeWorkArea_DragEnter(object sender, DragEventArgs e)
      {
         draggedEntites = new List<string>();

         string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];

         foreach ( var item in files )
         {
            if ( Directory.Exists(item) )
            {
               draggedEntites.Add(EntityTypes.FOLDERENTITY);
               continue;
            }

            if ( File.Exists(item) )
            {
               draggedEntites.Add(EntityTypes.FILEENTITY);
            }
         }
      }

      private void TreeWorkArea_Drop(object sender, DragEventArgs e)
      {
         try
         {
            EntityViewModel evm;

            if ( CheckDropTarget(e, out evm) )
            {
               if ( evm != null )
               {
                  string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];

                  foreach ( var item in files )
                  {
                     if ( Directory.Exists(item) )
                     {
                        IEntity subEntity = evm.Entity.CreateChild(EntityTypes.FOLDERENTITY, Path.GetFileName(item));
                        subEntity.SetAttribute("SourceFolder", item);
                        continue;
                     }

                     if ( File.Exists(item) )
                     {
                        IEntity subEntity = evm.Entity.CreateChild(EntityTypes.FILEENTITY, Path.GetFileName(item));
                        subEntity.SetAttribute("SourcePath", item);
                     }
                  }
               }
            }
         }
         finally
         {
            draggedEntites = null;
         }
      }

      // Check what kind of entities can be created using the dragsource data
      private bool CheckDropTarget(DragEventArgs e, out EntityViewModel entityVM)
      {
         EntityViewModel evm = null;
         entityVM = null;

         if (draggedEntites == null)
            return false;

         HitTestResult htr = VisualTreeHelper.HitTest(TreeWorkArea, e.GetPosition(TreeWorkArea));

         TreeViewItem tvItem = htr.VisualHit.FindVisualParent<TreeViewItem>();

         if ( null == tvItem )
            return false;

         evm = tvItem.Header as EntityViewModel;

         if ( null == evm )
            return false;

         entityVM = evm;

         return draggedEntites.All((en) => evm.Entity.Schema.SupportedEntityTypes.Contains(en));
      }

      #endregion
   }
}