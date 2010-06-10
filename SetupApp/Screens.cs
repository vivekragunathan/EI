using System;
using System.Windows;
using System.Windows.Data;

namespace EasyInstall.SequencerStateMachine
{
   class Screen<T> : IScreen where T : EasyInstallScreen, new()
   {
      public event NavigationContextChanged OnNavigationInfoChanged;

      public DependencyObject ScreenContent { get; private set; }

      public Screen(DependencyObject screen)
      {
         T eiscreen = new T();
         eiscreen.ScreenContent = screen;

         InitScreen(eiscreen);
      }

      private void InitScreen(T content)
      {
         this.ScreenContent = content;

         FrameworkElement fwe = content as FrameworkElement;

         CanFinish = true;
         CanMoveNext = true;
         CanMovePrevious = true;
         CanCancel = true;

         if (fwe != null)
         {
            object canMoveNext = fwe.TryFindResource("CanMoveNext");
            if (null != canMoveNext)
            {
               CanMoveNext = (bool)canMoveNext;
            }

            object canMovePrevious = fwe.TryFindResource("CanMovePrevious");
            if (null != canMovePrevious)
            {
               CanMovePrevious = (bool)canMovePrevious;
            }

            object canCancel = fwe.TryFindResource("CanCancel");
            if (null != canCancel)
            {
               CanCancel = (bool)canCancel;
            }

            object canFinish = fwe.TryFindResource("CanFinish");
            if (null != canFinish)
            {
               CanFinish = (bool)canFinish;
            }
         }
      }

      public bool CanMoveNext
      {
         get;
         private set;
      }

      public bool CanMovePrevious
      {
         get;
         private set;
      }

      public bool CanCancel
      {
         get;
         private set;
      }

      public bool CanFinish
      {
         get;
         private set;
      }
   }

   class ScreenWrapper : Screen<EasyInstallScreen>
   {
      public ScreenWrapper(DependencyObject content)
         : base(content)
      {
         
      }
   }

   class DeployScreenWrapper : Screen<EasyInstallDeployScreen>
   {
      EasyInstallDeployScreen fwe { get { return ScreenContent as EasyInstallDeployScreen; } }
      public DeployScreenWrapper(DependencyObject content)
         : base(content)
      {
         // Create bindings

         Binding b = new Binding();
         b.Source = fwe;
         b.
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

            if (fwe != null)
            {
               fwe.Resources["ProgressPercent"] = value;
            }
         }
      }

      string _progressText;
      public string ProgressText
      {
         get
         {
            return _progressText;
         }
         set
         {
            _progressText = value;

            if (fwe != null)
            {
               fwe.Resources["ProgressText"] = value;
            }
         }
      }

      string _progressHeading;
      public string Progressheading
      {
         get
         {
            return _progressHeading;
         }
         set
         {
            _progressHeading = value;

            if (fwe != null)
            {
               fwe.Resources["ProgressHeading"] = value;
            }
         }
      }
   }
}