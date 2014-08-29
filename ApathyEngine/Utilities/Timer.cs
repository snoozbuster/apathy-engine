using System;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace ApathyEngine.Utilities
{
    public class Timer
    {
        /// <summary>
        /// The timer itself.
        /// </summary>
        private TimeSpan time;
        /// <summary>
        /// The original number of milliseconds the timer was initialized to.
        /// </summary>
        private readonly int ms;
        /// <summary>
        /// Determines if the timer is going or not.
        /// </summary>
        public bool Ticking { get; protected set; }
        /// <summary>
        /// The function to call upon the event firing.
        /// </summary>
        public Action EventFunction;
        /// <summary>
        /// Returns true if the function is a continuous timer.
        /// </summary>
        public bool Continuous { get; protected set; }

        /// <summary>
        /// Creates a new Timer. When the timer fires, all associated events are released and will have to be re-added when 
        /// Start() is called.
        /// </summary>
        /// <param name="timeInMilliseconds">The time to tick for in milliseconds.</param>
        /// <param name="function">The EventHandler delegate to call when the timer fires.</param>
        /// <param name="continuous">If true, the timer will automatically reset itself and continue ticking after the event fires.
        /// Start() will throw an error if called more than once, and Stop() will always throw an error.</param>
        public Timer(int timeInMilliseconds, Action function, bool continuous)
        {
            ms = timeInMilliseconds;
            time = new TimeSpan(0, 0, 0, 0, timeInMilliseconds);
            Ticking = false;
            Continuous = continuous;
            EventFunction = function;
        }

        /// <summary>
        /// Called when the timer is 
        /// </summary>
        protected void fire()
        {
            Ticking = false;
            EventFunction();
            if(Continuous)
            {
                Reset();
                Ticking = true;
            }
        }

        /// <summary>
        /// Updates the timer.
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            if(Ticking)
            {
                time -= gameTime.ElapsedGameTime;
                if(time <= TimeSpan.Zero)
                    fire();
            }
        }

        /// <summary>
        /// Updates the timer with a given number of seconds.
        /// </summary>
        /// <param name="seconds"></param>
        public void Update(float seconds)
        {
            if(Ticking)
            {
                time -= new TimeSpan(0, 0, 0, 0, (int)(seconds * 1000));
                if(time <= TimeSpan.Zero)
                    fire();
            }
        }

        /// <summary>
        /// Resets the timer to its original time. The timer will retain its previous state of ticking or not.
        /// </summary>
        public void Reset()
        {
            time = new TimeSpan(0, 0, 0, 0, ms);
        }

        /// <summary>
        /// Starts the timer.
        /// </summary>
        public void Start()
        {
            if(Continuous || Ticking)
                throw new InvalidOperationException("The timer is continuous or is still ticking and cannot be restarted.");
            Ticking = true;
        }

        /// <summary>
        /// Pauses the timer at the current location.
        /// </summary>
        public void Pause()
        {
            Ticking = false;
        }

        /// <summary>
        /// Continues the timer from a paused state.
        /// </summary>
        public void Play()
        {
            Ticking = true;
        }

        /// <summary>
        /// Stops the timer and resets it.
        /// </summary>
        public void Stop()
        {
            if(Continuous)
                throw new NotSupportedException("The timer is continuous and cannot be permanently stopped. Call Pause() instead.");
            Ticking = false;
            Reset();
        }
    }
}
