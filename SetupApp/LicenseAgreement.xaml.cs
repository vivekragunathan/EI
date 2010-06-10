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
   /// Interaction logic for LicenseAgreement.xaml
   /// </summary>
   public partial class LicenseAgreement : UserControl
   {
      public static readonly DependencyProperty AgreementTextProperty;

      static LicenseAgreement()
      {
         AgreementTextProperty = DependencyProperty.Register("AgreementTextProperty", 
            typeof(string), 
            typeof(WelcomeScreen),
            new FrameworkPropertyMetadata("Test Agreement"));
      }

      public string WelcomeText
      {
         get
         {
            return GetValue(AgreementTextProperty) as string;
         }

         set
         {
            SetValue(AgreementTextProperty, value);
         }
      }

      public LicenseAgreement()
      {
         InitializeComponent();
      }
   }
}
