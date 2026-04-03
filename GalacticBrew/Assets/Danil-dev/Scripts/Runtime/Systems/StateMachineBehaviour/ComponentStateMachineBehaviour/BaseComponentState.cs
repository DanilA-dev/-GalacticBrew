using System;
using System.Linq;
using D_Dev.Base;
using D_Dev.PolymorphicValueSystem;
using D_Dev.StateMachine;
using UnityEngine;

namespace D_Dev.StateMachineBehaviour
{
    public abstract class BaseComponentState : MonoBehaviour, IState
    {
        #region Classes

        [Serializable]
        public class TransitionData
        {
            #region Fields

            [SerializeReference] private PolymorphicValue<string>[] _fromStates;
            [SerializeReference] private ICondition[] _conditions;
            [SerializeReference] private IFixedCondition[] _fixedConditions;

            #endregion

            #region Properties

            public string[] FromStates => _fromStates.Select(x => x.Value).ToArray();
            public ICondition[] Conditions => _conditions;
            public IFixedCondition[] FixedConditions => _fixedConditions;

            #endregion
        }

        #endregion

        #region Fields

        [SerializeReference] protected PolymorphicValue<string> _stateName = new StringConstantValue();
        [SerializeReference] protected PolymorphicValue<bool> _canBeTransitioned = new BoolConstantValue() { Value = true };
        [SerializeReference] protected PolymorphicValue<float> _stateExitTime = new FloatConstantValue();
        [SerializeField] protected TransitionData[] _transitions;

        #endregion
        
        #region Properties

        public float ExitTime => _stateExitTime.Value;
        public string StateName => _stateName.Value;
        public TransitionData[] Transitions => _transitions;
        public PolymorphicValue<bool> CanBeTransitioned
        {
            get => _canBeTransitioned;
            set => _canBeTransitioned = value;
        }

        #endregion

        #region IState

        public virtual void OnEnter(){}

        public virtual void OnUpdate(){}

        public virtual void OnFixedUpdate(){}

        public virtual void OnExit(){}


        #endregion
    }
}