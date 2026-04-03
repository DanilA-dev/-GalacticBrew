namespace D_Dev.TimerSystem
{
    public class IncrementTimer : BaseTimer
    {
        #region Fields

        private float _targetTime;

        #endregion

        #region Construct

        public IncrementTimer(float initialTime, float targetTime) : base(0)
        {
            _targetTime = targetTime;
        }

        #endregion

        #region Override

        public override void Tick(float deltaTime)
        {
            if (IsRunning && Time < _targetTime)
            {
                Time += deltaTime;
                OnProgressUpdate();
            }
        
            if(IsRunning && Time >= _targetTime)
                Stop();
        }

        public override void Reset()
        {
            base.Reset();
            Time = 0;
        }

        #endregion
    }
}
