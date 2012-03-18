using System.Windows;
using System;

namespace StopWatch
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (e.Args != null && e.Args.Length > 0)
            {
                if (e.Args[0].Equals("/stop"))
                {
                    StopWatchForm.StartupRequest(StopWatchForm.Operation.Stop);
                }
                else if (e.Args[0].Equals("/start"))
                {
                    StopWatchForm.StartupRequest(StopWatchForm.Operation.Start);
                }
                else if (e.Args[0].Equals("/pause"))
                {
                    StopWatchForm.StartupRequest(StopWatchForm.Operation.Pause);
                }
                else
                {
                    MessageBox.Show(
                        "Usage:\nStopWatch [/start] [/pause] [/stop]",
                        "Invalid command");
                }
            }
            else
            {
                new StopWatchForm().ShowDialog();
            }
            Shutdown();
        }
    }
}
