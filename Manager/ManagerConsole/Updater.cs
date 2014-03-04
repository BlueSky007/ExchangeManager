using Manager.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ManagerConsole
{
    public class Updater
    {
        private string _Server;
        private string _Port;
        private Action<bool> _ProcessExitCallback;
        public Updater(string server, string port, Action<bool> exitCallback)
        {
            this._Server = server;
            this._Port = port;
            this._ProcessExitCallback = exitCallback;
        }
        
        public void CheckUpdate()
        {
            try
            {
                Process updateProcess = new Process();

                StringBuilder commandLineBuilder = new StringBuilder();
                commandLineBuilder.AppendFormat("IpAddress={0};", this._Server);
                commandLineBuilder.AppendFormat("Port={0};", this._Port);
                commandLineBuilder.AppendFormat("ReTranslateTimes=5; Organization=UPS; ProcessId={0};", Process.GetCurrentProcess().Id);
                commandLineBuilder.AppendFormat("ProgramName={0}", Environment.GetCommandLineArgs()[0]);

                ProcessStartInfo startInfo = new ProcessStartInfo("AutoUpdateClient.exe", commandLineBuilder.ToString());
                updateProcess.StartInfo = startInfo;
                updateProcess.StartInfo.UseShellExecute = false;
                updateProcess.EnableRaisingEvents = true;
                if (updateProcess.Start())
                {
                    updateProcess.Exited += delegate(object sender, EventArgs e) { this._ProcessExitCallback(true); };
                }
            }
            catch (Exception exception)
            {
                this._ProcessExitCallback(false);
                Logger.TraceEvent(TraceEventType.Error, "Start AutoUpdateClient failed:\r\n{0}", exception);
            }
        }
    }
}
