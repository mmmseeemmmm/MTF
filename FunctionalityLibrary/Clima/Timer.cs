using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace ClimaChamberDriver
{
    public delegate void OnTimerEventHandler(object sender);
    public class Timer
    {
        public Timer()
        {
            //eventGenerator= new Timer();

            this.Interval = -1;
            
        }

        public double Interval { get; set; }
        public event OnTimerEventHandler OnTimer;
        private Thread eventGeneratorThread;
        private bool stop = false;
        private double time = DateTime.Now.ToOADate() * 24 * 3600;

        //private Timer eventGenerator;


        public void Start()
        {
            stop = false;
            time = DateTime.Now.ToOADate() * 24 * 3600;
            eventGeneratorThread = new Thread(Run);
            eventGeneratorThread.Start();

        }

        #region Run()
        public void Run()
        {
            while (!stop)
            {
                System.Threading.Thread.Sleep((int)(Interval * 1000 - 500));

                while ((DateTime.Now.ToOADate() * 24 * 3600 - time) < Interval)
                {
                    //do nothing
                    System.Threading.Thread.Sleep(100);
                }

                time = time + Interval;
#if !DEBUG
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) + ": Event Fired");
#endif
                //generate event
                if (OnTimer != null)
                {
                    OnTimer(this);
                }

            }
        }
        #endregion

        public void Stop()
        {
            stop = true;
        }

        ~Timer()
        {
            stop = true;
        }
    }
}
