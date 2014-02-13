using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace Manager.Common
{
    public class Logger
    {
        class Log
        {
            public TraceEventType EventType;
            public string Format;
            public object[] Args;
        }

        private static object _Lock = new object();
        private static TraceSource TraceSource = new TraceSource("Logger");
        private static RelayEngine<Log> LogEngine = null;

        private static void HandlEngineException(Exception ex)
        {
            Logger.TraceSource.TraceEvent(TraceEventType.Error, 0, "Logger.HandlEngineException\r\n{0}", ex.ToString());
        }

        private static bool WriteLog(Log log)
        {
            try
            {
                Logger.TraceSource.TraceEvent(log.EventType, 0, log.Format, log.Args);
            }
            catch(Exception ex)
            {
                try
                {
                    Logger.TraceEvent(TraceEventType.Error, "Logger.WriteLog\r\n{0}", ex.ToString());
                }
                catch{}
            }
            return true;
        }

        public static void AddEvent(TraceEventType eventType, string format, params object[] args)
        {
            lock (Logger._Lock)
            {
                if (Logger.LogEngine == null)
                {
                    Logger.LogEngine = new RelayEngine<Log>(Logger.WriteLog, Logger.HandlEngineException);
                }
                Logger.LogEngine.AddItem(new Log { EventType = eventType, Format = format, Args = args });
            }
        }

        public static void TraceEvent(TraceEventType eventType, string format, params object[] args)
        {
            Logger.TraceSource.TraceEvent(eventType, 0, format, args);
        }
    }
}
