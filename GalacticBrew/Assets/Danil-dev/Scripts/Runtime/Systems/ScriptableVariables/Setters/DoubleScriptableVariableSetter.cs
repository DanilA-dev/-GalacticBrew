namespace D_Dev.ScriptableVaiables.Setters
{
    public class DoubleScriptableVariableSetter : BaseScriptableVariableSetter<double, DoubleScriptableVariable>
    {
        #region Public
        public void AddValue(double value) => _variable.Value += value;

        public void Subtract(double value) => _variable.Value -= value;
        #endregion
    }
}
