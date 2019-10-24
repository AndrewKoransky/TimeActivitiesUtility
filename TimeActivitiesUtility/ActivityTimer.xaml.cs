using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace TimeActivitiesUtility
{
    /// <summary>
    /// Interaction logic for ActivityTimer.xaml
    /// </summary>
    public partial class ActivityTimer : UserControl
    {
        public ActivityTimer()
        {
            InitializeComponent();
        }

        public ActivityTimer(TimeSpan timer, string text)
        {
            InitializeComponent();
            Timer = timer;
            ActivityDescription = text;
            UpdateUI();
        }

        #region Model Interfacing
        public string ActivityDescription
        {
            get
            {
                return Description.Text;
            }
            set
            {
                Description.Text = value;
            }
        }

        public TimeSpan Timer { get; protected set; } = TimeSpan.Zero;

        public Data.ActivityTimerRow GetModel()
        {
            return new Data.ActivityTimerRow() { ActivityText=ActivityDescription, TotalHours= Timer.TotalHours };
        }
        #endregion

        public bool IsTimerEnabled { get; protected set; }

        public delegate void TimerUpdatedEventHandler(ActivityTimer activityTimer, TimeSpan elapsed);
        public event TimerUpdatedEventHandler TimerUpdated;

        public delegate void DeleteRequestedEventHandler(ActivityTimer activityTimer);
        public event DeleteRequestedEventHandler DeleteRequested;



        public void Tick()
        {
            Timer = Timer.Add(TimeSpan.FromSeconds(1));
            UpdateUI();
        }

        public void Reset()
        {
            IsTimerEnabled = false;
            Timer = TimeSpan.Zero;
            UpdateUI();
        }

        public void UpdateUI()
        {
            if ((IsTimerEnabled) && ("Start".Equals(StartStopButton.Content)))
            {
                StartStopButton.Content = "Stop";
                StartStopButton.FontWeight = FontWeights.Bold;
                StartStopButton.Background = Brushes.Red;
                TimeLabel.Foreground = Brushes.Red;
            }
            else if ((!IsTimerEnabled) && ("Stop".Equals(StartStopButton.Content)))
            {
                StartStopButton.Content = "Start";
                StartStopButton.FontWeight = FontWeights.Regular;
                StartStopButton.ClearValue(Button.BackgroundProperty);
                TimeLabel.ClearValue(Label.ForegroundProperty);
            }
            if (Timer.Seconds % 2 == 0) {
                TimeLabel.Content = string.Format("{0:00}:{1:00}", Convert.ToInt32(Math.Floor(Timer.TotalHours)), Convert.ToInt32(Timer.Minutes));
            } else {
                TimeLabel.Content = string.Format("{0:00} {1:00}", Convert.ToInt32(Math.Floor(Timer.TotalHours)), Convert.ToInt32(Timer.Minutes));
            }
        }

        private void TimeLabel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // HACK alert! VB InputBox cheat!
            string inputString = Microsoft.VisualBasic.Interaction.InputBox("Please enter time\n  (fractional hours or time span [hh:mm] supported)", "", Timer.Hours.ToString("00") + ":" + Timer.Minutes.ToString("00"));
            if (inputString.Length > 0)
            {
                try
                {
                    Timer = TimeSpan.FromHours(double.Parse(inputString));
                }
                catch
                {
                    try
                    {
                        Timer = TimeSpan.Parse(inputString);
                    }
                    catch
                    { }
                }
                UpdateUI();
                TimerUpdated?.Invoke(this, Timer);
            }
        }

        private void StartStopButton_Click(object sender, RoutedEventArgs e)
        {
            if ("Start".Equals(StartStopButton.Content)) {
                IsTimerEnabled = true;
            } else {
                IsTimerEnabled = false;
            }
            UpdateUI();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            Timer = TimeSpan.Zero;
            UpdateUI();
            TimerUpdated?.Invoke(this, Timer);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteRequested?.Invoke(this);
        }
    }
}
