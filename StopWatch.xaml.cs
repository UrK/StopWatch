using System.Threading;
using System.Diagnostics;
using System.ComponentModel;
using System;
using System.IO.Pipes;
using System.IO;
using System.Windows;

namespace StopWatch
{
    /// <summary>
    /// Interaction logic for StopWatchForm.xaml
    /// </summary>
    public partial class StopWatchForm
    {
        private readonly Stopwatch m_sw;
        private readonly Timer m_timer;
        private NamedPipeServerStream m_pipe;

        public enum Operation
        {
            Start,
            Stop,
            Pause
        }

        public StopWatchForm()
        {
            InitializeComponent();

            m_timer = new Timer(SecondsHandler, null, 1000, 1000);

            m_sw = new Stopwatch();
            m_sw.Start();

            CreatePipe();
        }

        private void CreatePipe()
        {

            try
            {
                m_pipe = new NamedPipeServerStream(
                    "stopwatchShutdownPipe",
                    PipeDirection.In,
                    1,
                    PipeTransmissionMode.Message,
                    PipeOptions.Asynchronous);
            }
            catch (Exception)
            {
                MessageBox.Show(
                    "Another instance of StopWatch running?",
                    "Unable to start",
                    MessageBoxButton.OK,
                    MessageBoxImage.Question);
                return;
            }
            m_pipe.BeginWaitForConnection(OperationHandler, null);
        }

        public static void StartupRequest(Operation op)
        {
            using (var pipe = new NamedPipeClientStream(".",
                "stopwatchShutdownPipe", PipeDirection.Out))
            {
                try
                {
                    pipe.Connect(100);
                }
                catch (TimeoutException)
                {
                    MessageBox.Show("No running StopWatch process found");
                    return;
                }
                using (var bw = new BinaryWriter(pipe))
                {
                    bw.Write((Int32)op);
                }
            }
        }

        private void OperationHandler(IAsyncResult result)
        {
            try
            {
                m_pipe.EndWaitForConnection(result);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            
            using (var br = new BinaryReader(m_pipe))
            {
                var op = (Operation) br.ReadInt32();
                m_pipe.Dispose();

                switch (op)
                {
                    case Operation.Start:
                        m_sw.Start();
                        CreatePipe();
                        StateLabel = "";
                        break;
                    case Operation.Stop:
                        Dispatcher.Invoke(new Action(Close));
                        Thread.Sleep(500);
                        break;
                    case Operation.Pause:
                        m_sw.Stop();
                        CreatePipe();
                        StateLabel = "Paused";
                        break;
                    default:
                        MessageBox.Show(String.Format("Invalid operation: {0}",
                            op));
                        CreatePipe();
                        break;
                }
            }
        }

        private String StateLabel
        {
            set
            {
                m_stateLabel.Dispatcher.BeginInvoke(
                    new Action(() => m_stateLabel.Content = value));
            }
        }

        private void SecondsHandler(object param)
        {
            Dispatcher.Invoke(new Action(() =>
                m_timeLabel.Content = string.Format("{0}:{1}:{2}",
                    m_sw.Elapsed.Hours.ToString("00"),
                    m_sw.Elapsed.Minutes.ToString("00"),
                    m_sw.Elapsed.Seconds.ToString("00"))));
        }

        private void ClosingHandler(object sender, CancelEventArgs e)
        {
            m_timer.Dispose();
            m_sw.Stop();
            m_pipe.Dispose();
        }
    }
}
