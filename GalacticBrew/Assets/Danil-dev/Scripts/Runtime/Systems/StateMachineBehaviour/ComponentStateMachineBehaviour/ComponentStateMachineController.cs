using System.Linq;
using D_Dev.StateMachine;
using Sirenix.OdinInspector;
using UnityEngine;

namespace  D_Dev.StateMachineBehaviour
{
    public class ComponentStateMachineController  : StateMachineController
    {
        #region Fields

        [FoldoutGroup("Module Settings", 99)]
        [SerializeField] protected bool _findOnObject;
        [FoldoutGroup("Module Settings", 99)]
        [HideIf(nameof(_findOnObject))]
        [SerializeField] protected BaseComponentState[] _states;

        #endregion

        #region Virtual/Abstract

        protected override void InitStates()
        {
            if (_findOnObject)
                _states = GetComponents<BaseComponentState>();
            
            if (_states == null || _states.Length == 0)
                return;
            
            foreach (var state in _states)
                AddState(state.StateName, state);
            
            InitTransitions();
            OnStatesInitialized();
        }

        protected virtual void InitTransitions()
        {
            if (_states == null || _states.Length == 0)
                return;
            
            foreach (var state in _states)
            {
                if (state.Transitions == null || state.Transitions.Length == 0)
                    continue;
                
                foreach (var transition in state.Transitions)
                {
                    if (transition.Conditions != null && transition.Conditions.Length > 0)
                    {
                        AddTransition(transition.FromStates, state.StateName, new FuncCondition(() => 
                            state.CanBeTransitioned.Value &&
                            transition.Conditions.All(c => c.IsConditionMet())));
                    }
                    
                    if (transition.FixedConditions != null && transition.FixedConditions.Length > 0)
                    {
                        AddFixedTransition(transition.FromStates, state.StateName, new FuncFixedCondition(() => 
                            state.CanBeTransitioned.Value &&
                            transition.FixedConditions.All(c => c.IsConditionMet())));
                    }
                }
            }
        }
        
        protected virtual void OnStatesInitialized(){}
        
        #endregion
    }
}