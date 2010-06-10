using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using EasyInstall.ViewModels;
using System.Windows.Data;
using EasyInstall.Interfaces;
using System.Windows.Threading;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;

namespace EasyInstall.SequencerStateMachine
{
   enum Signal
   {
      Next,
      Previous,
      Finish,
      Cancel
   }

   public delegate void NavigationContextChanged(IScreen sender);

   public interface IScreen
   {
      event NavigationContextChanged OnNavigationInfoChanged;

      bool CanMoveNext { get; }
      bool CanMovePrevious { get; }
      bool CanCancel { get; }
      bool CanFinish { get; }

      DependencyObject ScreenContent { get; }
   }

   public interface IDeployScreen : IScreen
   {
      string ProgressText { get; set; }
      string ProgressHeader { get; set; }
      double ProgressPercent { get; set; }
   }

   class NavigationContext : ViewModelBase
   {
      public NavigationContext(PackageViewModel package)
      {
         if (package == null)
            throw new ArgumentNullException("package");

         PackageVM = package;
      }

      bool _canMoveNext;
      public bool CanMoveNext
      {
         get
         {
            return _canMoveNext;
         }
         set
         {
            _canMoveNext = value;
            NotifyCurrentPropertyChanged();
         }
      }

      bool _canMovePrevious;
      public bool CanMovePrevious
      {
         get
         {
            return _canMovePrevious;
         }
         set
         {
            _canMovePrevious = value;
            NotifyCurrentPropertyChanged();
         }
      }

      bool _canFinish;
      public bool CanFinish
      {
         get
         {
            return _canFinish;
         }
         set
         {
            _canFinish = value;
            NotifyCurrentPropertyChanged();
         }
      }

      bool _canCancel;
      public bool CanCancel
      {
         get
         {
            return _canCancel;
         }
         set
         {
            _canCancel = value;
            NotifyCurrentPropertyChanged();
         }
      }

      object _Screen;
      public object Screen
      {
         get
         {
            return _Screen;
         }
         set
         {
            _Screen = value;
            NotifyCurrentPropertyChanged();
         }
      }

      public PackageViewModel PackageVM
      {
         get;
         private set;
      }

   }

   class SetupSequencerState : StateMachine<Signal, NavigationContext>.State
   {
      IList<IScreen> screens;

      public StateMachine<Signal, NavigationContext>.State PrevState { get; set; }
      public StateMachine<Signal, NavigationContext>.State NextState { get; set; }

      int currentScreenPos;

      protected virtual IScreen CurrentScreen
      {
         get
         {
            return screens[currentScreenPos];
         }
      }

      bool HasNextScreen
      {
         get
         {
            return currentScreenPos < screens.Count() - 1;
         }
      }

      bool HasPrevScreen
      {
         get
         {
            return currentScreenPos > 0;
         }
      }

      public SetupSequencerState(SetupSequencerSM sm,
         IList<IScreen> screens,
         string name)
         : base(sm, name)
      {
         if ((screens == null) || (screens.Count() == 0))
         {
            throw new Exception("");
         }
         currentScreenPos = 0;
         this.screens = screens;
         this.PrevState = null;
         this.NextState = null;
      }

      public override void OnInit()
      {
         Context.CanCancel = true;
         Context.CanMoveNext = HasNextScreen || NextState != null;
         Context.CanMovePrevious = HasPrevScreen || PrevState != null;
         Context.CanFinish = HasNextScreen;
      }

      public override void OnEntry()
      {
         Context.Screen = CurrentScreen.ScreenContent;
      }

      void UpdateContext(IScreen screen)
      {
         Context.CanMoveNext = screen.CanMoveNext && (HasNextScreen || NextState != null);
         Context.CanMovePrevious = screen.CanMovePrevious && (HasPrevScreen || PrevState != null);
         Context.CanFinish = screen.CanFinish;
         Context.CanCancel = screen.CanCancel;
      }

      void UpdateCurrentScreen(IScreen oldScreen, IScreen newScreen)
      {
         oldScreen.OnNavigationInfoChanged -= screen_OnNavigationInfoChanged;

         if (newScreen != null)
         {
            Context.Screen = newScreen.ScreenContent;

            newScreen.OnNavigationInfoChanged += screen_OnNavigationInfoChanged;
         }
      }

      void screen_OnNavigationInfoChanged(IScreen sender)
      {
         UpdateContext(sender);
      }

      public override StateMachine<Signal, NavigationContext>.State HandleSignal(Signal signal)
      {
         if (null == Context)
         {
            throw new NullReferenceException("Context is null");
         }
         IScreen screen = screens[currentScreenPos];
         UpdateContext(screen);

         switch (signal)
         {
            case Signal.Next:

               break;
            case Signal.Previous:

               break;
            case Signal.Finish:
               if (!Context.CanFinish)
               {
                  throw new InvalidOperationException("Cannot Finish");
               }
               break;
            case Signal.Cancel:
               if (!Context.CanCancel)
               {
                  throw new InvalidOperationException("Cannot Cancel");
               }
               break;
         }

         switch (signal)
         {
            case Signal.Next:
               {
                  if (!Context.CanMoveNext)
                  {
                     throw new InvalidOperationException("Cannot move next");
                  }

                  if (currentScreenPos == screens.Count() - 1)
                  {
                     UpdateCurrentScreen(screen, null);
                     return NextState;
                  }

                  ++currentScreenPos;
                  UpdateCurrentScreen(screen, screens[currentScreenPos]);


                  return this;
               }

            case Signal.Previous:
               {
                  if (!Context.CanMovePrevious)
                  {
                     throw new InvalidOperationException("Cannot move to previous");
                  }
                  if (currentScreenPos == 0)
                  {
                     UpdateCurrentScreen(screen, null);
                     return PrevState;
                  }

                  --currentScreenPos;
                  UpdateCurrentScreen(screen, screens[currentScreenPos]);

                  return this;
               }
         }
         throw new Exception("Invalid signal");
      }
   }

   class SetupState : SetupSequencerState
   {
      public SetupState(SetupSequencerSM sm)
         : base(sm, new List<IScreen>() { new EasyInstallScreen(new WelcomeScreen()), 
            new EasyInstallScreen(new LicenseAgreement()) },
            "SetupState")
      {

      }

      public override bool IsInitState
      {
         get
         {
            return true;
         }
      }
   }

   class DeployState : SetupSequencerState
   {
      public DeployState(SetupSequencerSM sm)
         : base(sm, new List<IScreen> { new EasyInstallScreen(new DeployScreen()) },
            "DeployState")
      {

      }

      protected new IDeployScreen CurrentScreen
      {
         get
         {
            return base.CurrentScreen as IDeployScreen;
         }
      }

      public override StateMachine<Signal, NavigationContext>.State HandleSignal(Signal signal)
      {
         switch (signal)
         {
            case Signal.Cancel:
               ct.Cancel();
               Context.CanCancel = false;
               return NextState;
         }

         return base.HandleSignal(signal);
      }

      CancellationTokenSource ct = new CancellationTokenSource();
      Task deployTask;
      public override void OnEntry()
      {
         base.OnEntry();

         Context.CanCancel = true;
         Context.CanMoveNext = false;
         Context.CanMovePrevious = false;
         Context.CanFinish = false;

         // Start the deploy process
         Context.PackageVM.Package.OnDeploying += Package_OnDeploying;
         Context.PackageVM.Package.OnDeployed += Package_OnDeployed;
         Context.PackageVM.Package.OnDeployProgress += Package_OnDeployProgress;
         Context.PackageVM.Package.OnUndeployProgress += Package_OnUndeployProgress;

         deployTask = new Task(() => Context.PackageVM.Package.Deploy(), ct.Token);
         deployTask.Start();
      }

      void Package_OnUndeployProgress(object sender, Interfaces.Entities.ProgressEventArgs e)
      {
         
      }

      void Package_OnDeployProgress(object sender, Interfaces.Entities.ProgressEventArgs e)
      {
         Dispatcher dispatcher = CurrentScreen.ScreenContent.Dispatcher;

         dispatcher.Invoke(new MethodInvoker(() =>
         {
            Thread.Sleep(1000);
            CurrentScreen.ProgressPercent = e.ProgressPercent;
            CurrentScreen.ProgressText = e.Entity.ToString();
            CurrentScreen.ProgressHeader = string.Format("Deploying {0}", e.Entity.Schema.EntityType);

            
         }), DispatcherPriority.DataBind);
      }

      public override void OnExit()
      {
         Context.PackageVM.Package.OnDeploying -= Package_OnDeploying;
         Context.PackageVM.Package.OnDeployed -= Package_OnDeployed;

         Context.PackageVM.Package.OnDeployProgress -= Package_OnDeployProgress;
         Context.PackageVM.Package.OnUndeployProgress -= Package_OnUndeployProgress;
      }

      // This method will be called from a different thread context
      void Package_OnDeployed(object sender, Interfaces.Entities.EntityEventArgs e)
      {
         Dispatcher dispatcher = CurrentScreen.ScreenContent.Dispatcher;

         dispatcher.Invoke(new MethodInvoker(()=>
         {
            CurrentScreen.ProgressHeader = "Package successfully deployed";
            Context.CanMoveNext = true;
         }), DispatcherPriority.DataBind);
      }

      // This method will be called from a different thread context
      void Package_OnDeploying(object sender, Interfaces.Entities.EntityEventArgs e)
      {
         Dispatcher dispatcher = CurrentScreen.ScreenContent.Dispatcher;

         dispatcher.Invoke(new MethodInvoker(() =>
         {
            CurrentScreen.ProgressHeader = "Preparing to Deploy Package";
         }), DispatcherPriority.DataBind);
         
      }
   }

   class ErrorState : SetupSequencerState
   {
      public ErrorState(SetupSequencerSM sm)
         : base(sm, new List<IScreen>(),
            "ErrorState")
      {

      }
   }

   class CompleteState : SetupSequencerState
   {
      public CompleteState(SetupSequencerSM sm)
         : base(sm, new List<IScreen>() { new EasyInstallScreen(new WelcomeScreen()) },
            "CompleteState")
      {

      }
   }

   class SetupSequencerSM : StateMachine<Signal, NavigationContext>
   {
      public SetupSequencerSM(NavigationContext context)
      {
         this.Context = context;
         System.Diagnostics.Debug.Assert(context != null);
         LoadStates();
      }

      void LoadStates()
      {
         SetupState setup = new SetupState(this);
         DeployState deploy = new DeployState(this);
         //ErrorState error = new ErrorState(this);
         CompleteState complete = new CompleteState(this);

         AddState(setup); // Initial State
         AddState(deploy);
         //AddState(error);
         AddState(complete);

         setup.NextState = deploy;
         setup.PrevState = null;

         deploy.NextState = complete;
         deploy.PrevState = setup;

         complete.PrevState = deploy;
         complete.NextState = null;
      }

      public void Next()
      {
         DispatchSignal(Signal.Next);
      }

      public void Previous()
      {
         DispatchSignal(Signal.Previous);
      }

      public void Cancel()
      {
         DispatchSignal(Signal.Cancel);
      }

      public void Finish()
      {
         DispatchSignal(Signal.Finish);
      }
   }
}
