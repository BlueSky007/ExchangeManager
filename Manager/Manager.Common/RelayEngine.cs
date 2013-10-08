using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace Manager.Common
{
    public class RelayEngine<T>
    {
        private Thread _RelayThread;
        private AutoResetEvent _ItemArriveEvent = new AutoResetEvent(false);
        private ManualResetEvent _ResumeEvent = new ManualResetEvent(true);
        private WaitHandle[] _WaitHandles;
        private bool _Stop = false;

        private LinkedList<T> _Buffer = new LinkedList<T>();
        private Func<T, bool> _RelayFunc;
        private Action<Exception> _HandleException;

        public RelayEngine(Func<T, bool> relayFunc, Action<Exception> handleException)
        {
            this._WaitHandles = new WaitHandle[] { this._ItemArriveEvent, this._ResumeEvent };
            this._RelayFunc = relayFunc;
            this._HandleException = handleException;
            this._RelayThread = new Thread(this.Run) { IsBackground = true };
            this._RelayThread.Start();
        }

        public void AddItem(T item)
        {
            lock (this)
            {
                this._Buffer.AddLast(item);
            }
            this._ItemArriveEvent.Set();
        }

        public void Suspend()
        {
            this._ResumeEvent.Reset();
        }

        public void Resume()
        {
            this._ResumeEvent.Set();
        }

        public void Stop()
        {
            this._Stop = true;
            this._ItemArriveEvent.Set();
            this._ResumeEvent.Set();
        }

        private void Run()
        {
            try
            {
                while (true)
                {
                    if (this._Buffer.Count == 0)
                    {
                        WaitHandle.WaitAll(this._WaitHandles);
                    }
                    else
                    {
                        this._ResumeEvent.WaitOne();
                    }

                    if (this._Stop) break;

                    T item = this._Buffer.First.Value;
                    if (this._RelayFunc(item))
                    {
                        lock (this)
                        {
                            this._Buffer.RemoveFirst();
                        }
                    }
                    else
                    {
                        this.Suspend();
                    }
                }
            }
            catch (Exception ex)
            {
                this._HandleException(ex);
            }
        }
    }
}
