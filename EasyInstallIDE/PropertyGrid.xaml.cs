using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EasyInstall.Interfaces;
using EasyInstall.ViewModels;

namespace EasyInstallIDE
{
   /// <summary>
   /// Interaction logic for PropertyGrid.xaml
   /// </summary>
   public partial class PropertyGrid : UserControl
   {

      class ValueWrapper : ViewModelBase
      {
         public string PropertyName { get; private set; }

         public IEntity Entity { get; private set; }

         public object Value
         {
            get
            {
               return Entity.GetAttribute(PropertyName);
            }

            set
            {
               Entity.SetAttribute(PropertyName, value);
               NotifyCurrentPropertyChanged();
            }
         }

         public ValueWrapper(string propertyName, IEntity entity)
         {
            this.Entity = entity;
            this.PropertyName = propertyName;
         }
      }
      public static readonly DependencyProperty EntityProperty;

      public object Entity
      {
         get
         {
            return GetValue(EntityProperty);
         }

         set
         {
            SetValue(EntityProperty, value);
         }
      }

      static PropertyGrid()
      {
         EntityProperty = DependencyProperty.Register(
            "Entity", typeof(object), typeof(PropertyGrid),
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnEntityPropertyChanged)));
      }

      private static void OnEntityPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
      {
         PropertyGrid pg = sender as PropertyGrid;

         pg.Properties.RowDefinitions.Clear();
         pg.Properties.Children.Clear();

         if ( pg.PopulateIEntityAttributes(e.NewValue) )
            return;
      }

      private bool PopulateIEntityAttributes(object obj)
      {
         EntityViewModel entity = obj as EntityViewModel;

         if (entity == null)
            return false;

         int rc = 0;
         foreach ( var attr in entity.Entity.Attributes )
         {
            RowDefinition rd = new RowDefinition();
            Properties.RowDefinitions.Add(rd);
            rd.MaxHeight = 20;
            ++rc;

            Border borderPname = new Border();
            borderPname.BorderBrush = Brushes.Black;
            borderPname.BorderThickness = new Thickness(0.5);

            // Add Property Name
            TextBlock tb = new TextBlock();
            tb.Margin = new Thickness(2);
            tb.Text = attr.Key;
            tb.TextAlignment = TextAlignment.Left;
            tb.VerticalAlignment = VerticalAlignment.Center;
            borderPname.Child = tb;
            Properties.Children.Add(borderPname);

            borderPname.SetValue(Grid.RowProperty, rc);
            borderPname.SetValue(Grid.ColumnProperty, 0);

            // Add Value
            TextBox edit = new TextBox();
            Binding valueBinding = new Binding();

            valueBinding.Path = new PropertyPath("Value");
            valueBinding.Source = new ValueWrapper(attr.Key, entity.Entity);
            edit.SetBinding(TextBox.TextProperty, valueBinding);

            Properties.Children.Add(edit);

            edit.SetValue(Grid.RowProperty, rc);
            edit.SetValue(Grid.ColumnProperty, 2);
         }

         GridSplitter gs = new GridSplitter();
         gs.ResizeDirection = GridResizeDirection.Columns;
         gs.ShowsPreview = true;
         gs.SnapsToDevicePixels = true;
         Properties.Children.Add(gs);
         gs.SetValue(Grid.ColumnProperty, 1);
         
         return true;
      }

      public PropertyGrid()
      {
         InitializeComponent();
      }
   }
}
