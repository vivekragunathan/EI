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

namespace Borgstrup.EditableTextBlock
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
        }

        private void treeView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2)
                SetCurrentItemInEditMode(true);
        }

        private void SetCurrentItemInEditMode(bool EditMode)
        {
            // Make sure that the SelectedItem is actually a TreeViewItem
            // and not null or something else
            if (treeView1.SelectedItem is TreeViewItem)
            {
                TreeViewItem tvi = treeView1.SelectedItem as TreeViewItem;

                // Also make sure that the TreeViewItem
                // uses an EditableTextBlock as its header
                if (tvi.Header is EditableTextBlock)
                {
                    EditableTextBlock etb = tvi.Header as EditableTextBlock;

                    // Finally make sure that we are
                    // allowed to edit the TextBlock
                    if (etb.IsEditable)
                        etb.IsInEditMode = EditMode;
                }
            }
        }
    }
}
