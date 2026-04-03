using System;

namespace D_Dev.TimerSystem
{
    public abstract class BaseTimer
    {
       #region Fields

       protected float _initialTime;
       
       public event Action OnTimerStart;

       public event Action<float> OnTimerProgressUpdate;
       public event Action OnTimerEnd;

       #endregion

       #region Properties

       protected float Time { get; set; }
       public bool IsRunning { get; protected set; }
       public float Progress => Time / _initialTime;

       #endregion

       #region Construct

       public BaseTimer(float initialTime)
       {
          _initialTime = initialTime;
          IsRunning = false;
       }

       #endregion

       #region Public

       public void Start()
       {
           Time = _initialTime;
           if (!IsRunning)
           {
               IsRunning = true;
               OnTimerStart?.Invoke();
           }
       }
    
       public void Stop()
       {
           if (IsRunning)
           {
               IsRunning = false;
               OnTimerEnd?.Invoke();
           }
       }
 
       public void Resume() => IsRunning = true;
       public void Pause() => IsRunning = false;

       #endregion

       #region Abstract

       public abstract void Tick(float deltaTime);

       public virtual void Reset()
       {
           OnTimerStart?.Invoke();
       }

       #endregion

       #region Protected

       protected void OnProgressUpdate() => OnTimerProgressUpdate?.Invoke(Progress);

       #endregion

    }
}
