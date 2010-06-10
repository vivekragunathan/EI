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
using EasyInstall.SequencerStateMachine;
using System.ComponentModel;

namespace EasyInstall
{
   /// <summary>
   /// Interaction logic for EasyInstallSetupScreen.xaml
   /// </summary>
   public partial class EasyInstallScreen : UserControl, IDeployScreen, INotifyPropertyChanged
   {
      //public static readonly DependencyProperty CanMoveNextProperty;
      //public static readonly DependencyProperty CanMovePreviousProperty;
      //public static readonly DependencyProperty CanCancelProperty;
      //public static readonly DependencyProperty CanFinishProperty;

      //public static readonly DependencyProperty ProgressHeaderProperty;
      //public static readonly DependencyProperty ProgressTextProperty;
      //public static readonly DependencyProperty ProgressPercentProperty;

      static EasyInstallScreen()
      {
         //CanMoveNextProperty = DependencyProperty.Register("CanMoveNextProperty", typeof(bool), typeof(EasyInstallScreen),
         //   new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.None));

         //CanMovePreviousProperty = DependencyProperty.Register("CanMovePreviousProperty", typeof(bool), typeof(EasyInstallScreen),
         //   new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.None));

         //CanCancelProperty = DependencyProperty.Register("CanCancelProperty", typeof(bool), typeof(EasyInstallScreen),
         //   new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.None));

         //CanFinishProperty = DependencyProperty.Register("CanFinishProperty", typeof(bool), typeof(EasyInstallScreen),
         //   new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.None));

         //ProgressHeaderProperty = DependencyProperty.Register("ProgressHeaderProperty", typeof(string), typeof(EasyInstallScreen),
         //   new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.None));

         //ProgressTextProperty = DependencyProperty.Register("ProgressTextProperty", typeof(string), typeof(EasyInstallScreen),
         //   new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.None));

         //ProgressPercentProperty = DependencyProperty.Register("ProgressPercentProperty", typeof(double), typeof(EasyInstallScreen),
         //   new FrameworkPropertyMetadata(50.0, FrameworkPropertyMetadataOptions.None));
      }

      bool _canMoveNext = true;
      public bool CanMoveNext
      {
         get
         {
            return _canMoveNext;
         }

         set
         {
            _canMoveNext = value;
            FirePropertyChanged("CanMoveNext");

            if (OnNavigationInfoChanged != null)
            {
               OnNavigationInfoChanged(this);
            }
         }
      }

      bool _canMovePrevious = true;
      public bool CanMovePrevious
      {
         get
         {
            return _canMovePrevious;
         }

         set
         {
            _canMovePrevious = value;
            FirePropertyChanged("CanMovePrevious");
            if (OnNavigationInfoChanged != null)
            {
               OnNavigationInfoChanged(this);
            }
         }
      }

      bool _canCancel = true;
      public bool CanCancel
      {
         get
         {
            return _canCancel;
         }

         set
         {
            _canCancel = value;

            FirePropertyChanged("CanCancel");

            if (OnNavigationInfoChanged != null)
            {
               OnNavigationInfoChanged(this);
            }
         }
      }

      bool _canFinish = false;
      public bool CanFinish
      {
         get
         {
            return _canFinish;
         }

         set
         {
            _canFinish = value;
            FirePropertyChanged("CanFinish");
            if (OnNavigationInfoChanged != null)
            {
               OnNavigationInfoChanged(this);
            }
         }
      }

      string _progressHeader = string.Empty;
      public string ProgressHeader
      {
         get
         {
            return _progressHeader;
         }
         set
         {
            _progressHeader = value;
            FirePropertyChanged("ProgressHeader");
         }
      }

      string _progressText = string.Empty;
      public string ProgressText
      {
         get
         {
            return _progressText;
         }
         set
         {
            _progressText = value;
            FirePropertyChanged("Progresstext");
         }
      }

      double _progressPercent;
      public double ProgressPercent
      {
         get
         {
            return _progressPercent;
         }
         set
         {
            _progressPercent = value;
            FirePropertyChanged("ProgressPercent");

            if (value == 100.0)
            {
               CanMoveNext = true;
            }
         }
      }

      public EasyInstallScreen()
      {
         InitializeComponent();
         this.DataContext = this;
      }

      public EasyInstallScreen(DependencyObject content)
         : this()
      {
         this.Content = content;
         //(content as FrameworkElement).DataContext = this;
      }

      public event NavigationContextChanged OnNavigationInfoChanged;

      public DependencyObject ScreenContent
      {
         get { return this; }
      }

      void FirePropertyChanged(string propName)
      {
         if (PropertyChanged != null)
         {
            PropertyChanged(this, new PropertyChangedEventArgs(propName));
         }
      }

      public event PropertyChangedEventHandler PropertyChanged;
   }
}
