using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace EventTesting
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static int Timer_Value;
        static ProcessLogic PL;

        public MainWindow()
        {
            InitializeComponent();

            // a timer to display a stopwatch
            DispatcherTimer Timer = new DispatcherTimer();
            Timer.Interval = TimeSpan.FromMilliseconds(100); // 100ms time tick
            Timer.Tick += Timer_Tick;   // add the event handler to the timer
            Timer_Value = 0; // initalize the timer value
            Timer.Start();  // start the timer

            PL = new ProcessLogic();
            PL.ProcessUpdate += PL_ProcessUpdate;
            PL.ProcessCompleted += PL_ProcessCompleted;

            PL.Process2Update += PL_P2Update;
            PL.Process2Completed += PL_P2Done;


        }

        private void PL_ProcessCompleted()
        {
            // throw new NotImplementedException();
            TheLabel.Content = "Process Done";
        }

        private void PL_ProcessUpdate(string s)
        {
            // throw new NotImplementedException();
            TheLabel.Content = s;
        }

        private void PL_P2Update(string s)
        {
            Dispatcher.Invoke(() => TheTextBox.Text = s);
        }

        private void PL_P2Done()
        {
            Dispatcher.Invoke(() => TheTextBox.Text = "P2 Done!");
        }

        // setup a timer
        private void Timer_Tick(object sender, EventArgs e)
        {
            // this will get called every time tick.
            lbTimer.Content = string.Format("{0}", Timer_Value);
            Timer_Value += 1; 
        }


        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            // start a background process
            PL.StartProcess();
        }

        private void btnStart2_Click(object sender, RoutedEventArgs e)
        {
            PL.StartProcess2();
        }
    }


    public delegate void Notify(); // a delegate type 
    public delegate void Display(string s); // another delegate type that is passed a string
    // a class that does something ie process some logic
    public class ProcessLogic
    {
        public event Notify ProcessCompleted; // an event of type "Notify"
        public event Display ProcessUpdate;  // an event of type "Display"
        public event Display Process2Update;
        public event Notify Process2Completed;

        public delegate void tDoWorkCallback(int result);
        public delegate void tInWorkCallback(string s);

        static bool InT2 = false;

        static BackgroundWorker bw;

        Thread workThread;

        public ProcessLogic()
        {
            bw = new BackgroundWorker();    // instantiate the background worker bw
            
        }

        // this starts a process on in a new thread using the background worker
        public void StartProcess()
        {
            OnProcessUpdate("Starting");
            if (!bw.IsBusy)
            {
                bw.WorkerReportsProgress = true;
                bw.WorkerSupportsCancellation = false;

                bw.DoWork += ProcessWorker;
                bw.ProgressChanged += ProgessChanged;
                bw.RunWorkerCompleted += ProcessDone;

            
                bw.RunWorkerAsync();
            }
            else
            {
                MessageBox.Show("bw is busy!");
            }
        }

        private void ProcessWorker(object sender, DoWorkEventArgs e)
        {

            // throw new NotImplementedException(); // not quite sure what to do with this.
            for(int i = 0; i < 10; i++)
            {
                Thread.Sleep(1000);
               
                // MessageBox.Show($"In the worker! i = {i}");
                bw.ReportProgress(i);
                // OnProcessUpdate("This is it");
            }
        }

        private void ProgessChanged(object sender, ProgressChangedEventArgs e)
        {
            OnProcessUpdate($"i = {e.ProgressPercentage}");
        }

        private void ProcessDone(object sender, RunWorkerCompletedEventArgs e)
        {
            OnProcessCompleted();

            bw.DoWork -= ProcessWorker;         // remove from the background worker
            bw.RunWorkerCompleted -= ProcessDone;
            bw.ProgressChanged -= ProgessChanged;
        }

        protected virtual void OnProcessCompleted()
        {
            ProcessCompleted?.Invoke(); // if ProcessCompleted is not null then call the delegate
        }

        protected virtual void OnProcessUpdate(string x)    // because this is type Display, then we have to pass it a string.
        {
            ProcessUpdate?.Invoke(x);
        }

        // this starts a process that is in a different thread but not using the background worker
        public void StartProcess2()
        {
            // if (workThread == null)
            // {
            if (!InT2)
            {
                tDoWorkCallback callback = new tDoWorkCallback(displayWorkDone);
                tInWorkCallback callback2 = new tInWorkCallback(displayWorkUpdate);


                int tInput = 5;
                // tDoWork runs as a separate thread
                workThread = new Thread(() => tDoWork(tInput, callback, callback2));
                // }
                workThread.Start();
            }
            else
            {
                MessageBox.Show("Thread 2 is busy");
            }
            
        }

        private void tDoWork(int n, tDoWorkCallback cback, tInWorkCallback cback2)
        {
            InT2 = true;
            cback2("T2 Starting");
            for(int i = 0; i < n; i++)
            {
                Thread.Sleep(2000);
                cback2($"{i} of {n}");
            }
            cback(n);
            InT2 = false;
        }

        public void displayWorkDone(int result)
        {
            OnProcess2Completed();
        }

        public void displayWorkUpdate(string s)
        {
            OnProcess2Update(s);
        }

        protected virtual void OnProcess2Update(string x)
        {
            Process2Update?.Invoke(x);
        }

        protected virtual void OnProcess2Completed()
        {
            Process2Completed?.Invoke();
        }
    } 
}
