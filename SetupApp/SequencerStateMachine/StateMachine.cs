using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyInstall
{
   public class StateMachine<TSignal, TContext>
   {
      public delegate void StateChange(State oldState, State newState);
      public event StateChange OnStateChanging;
      public event StateChange OnStateChanged;

      public TContext Context { get; set; }

      public abstract class State
      {
         StateMachine<TSignal, TContext> sm;
         public string Name { get; private set; }

         protected TContext Context
         {
            get { return sm.Context; }
         }

         public State(StateMachine<TSignal, TContext> sm, string name)
         {
            this.sm = sm;
            this.Name = name;
         }

         public virtual void OnInit()
         {

         }

         public virtual void OnEntry()
         {
         }

         public virtual void OnExit()
         {

         }

         public abstract State HandleSignal(TSignal signal);
         
         public virtual bool IsInitState
         {
            get
            {
               return false;
            }
         }
      }

      IList<State> states = new List<State>();
      public State CurrentState { get; protected set; }

      static StateMachine()
      {
         if (!typeof(TSignal).IsEnum)
            throw new Exception("Enum type expected for TSignal");
      }

      public StateMachine()
      {
      }

      public StateMachine(IList<State> states)
      {
         this.states = states;

         // Check for duplicate names
         HashSet<string> stateNames = new HashSet<string>();

         foreach (var state in states)
         {
            if (stateNames.Contains(state.Name))
            {
               throw new Exception("Duplicate state name");
            }

            stateNames.Add(state.Name);
         }
      }

      void ValidateState(State state)
      {
         if (states.Any(e => e.Name == state.Name))
         {
            throw new Exception(string.Format("Duplicate state name '{0}'", state.Name));
         }

         if (state.IsInitState)
         {
            if (states.Any(e => e.IsInitState))
               throw new Exception("Duplicate init state");
         }
      }

      public void Start()
      {
         CurrentState = states.Where((e) => e.IsInitState).Single();

         CurrentState.OnInit();
         CurrentState.OnEntry();
      }

      public void Reset()
      {
         CurrentState = null;
      }

      protected void AddState(State state)
      {
         ValidateState(state);

         states.Add(state);
      }

      public State GetState(string name)
      {
         return (from state in states
                 where state.Name == name
                 select state).Single();
      }

      public void DispatchSignal(TSignal signal)
      {
         State newState = CurrentState.HandleSignal(signal);

         if (newState != CurrentState)
         {
            newState.OnInit();

            if (OnStateChanging != null)
               OnStateChanging(CurrentState, newState);

            State tempState = CurrentState;
            CurrentState = newState;

            if (OnStateChanged != null)
            {
               OnStateChanged(tempState, newState);
            }
            tempState.OnExit();
            newState.OnEntry();
         }
      }
   }
}