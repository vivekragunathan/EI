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

namespace EasyInstall
{
   /// <summary>
   /// Interaction logic for WelcomeScreen.xaml
   /// </summary>
   public partial class WelcomeScreen : UserControl
   {
      public static readonly DependencyProperty WelcomeTextProperty;

      static WelcomeScreen()
      {
         WelcomeTextProperty = DependencyProperty.Register("WelcomeTextProperty", 
            typeof(string), 
            typeof(WelcomeScreen),
            new FrameworkPropertyMetadata("Test message"));
      }

      public string WelcomeText
      {
         get
         {
            return GetValue(WelcomeTextProperty) as string;
         }

         set
         {
            SetValue(WelcomeTextProperty, value);
         }
      }

      public WelcomeScreen()
      {
         InitializeComponent();
         WelcomeText = "Test Message";
      }
   }
}
