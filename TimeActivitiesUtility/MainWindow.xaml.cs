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
using System.Windows.Threading;

namespace TimeActivitiesUtility
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string strFilePath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + System.IO.Path.DirectorySeparatorChar + "TimeActivitiesUtility2.csv";

        public MainWindow()
        {
            InitializeComponent();

            if (System.IO.File.Exists(strFilePath)) {
                ReadData();
            }

            DispatcherTimer TheTimer = new DispatcherTimer()
            {
                Interval = new TimeSpan(0, 0, 1)
            };
            TheTimer.Tick += TheTimer_Tick;
            TheTimer.Start();
        }

        private void ReadData()
        {
            using (System.IO.StreamReader sReader = new System.IO.StreamReader(strFilePath))
            {
                using (CsvHelper.CsvReader csvReader = new CsvHelper.CsvReader(sReader, new CsvHelper.Configuration.Configuration() { HasHeaderRecord = false }))
                {
                    csvReader.Read(); // first line
                    while (csvReader.Read())
                    {
                        AddTimer(new ActivityTimer(TimeSpan.FromHours(csvReader.GetField<double>(1)), csvReader.GetField<string>(0)));
                    }
                }
            }
            UpdateUI();
        }

        private void WriteData()
        {
            using (var sWriter = new System.IO.StreamWriter(strFilePath)) {
                using (var csvWriter = new CsvHelper.CsvWriter(sWriter))
                {
                    csvWriter.WriteField("Activity Text");
                    csvWriter.WriteField("Total Hours");
                    csvWriter.NextRecord();

                    for (int i = 1; i < TimersStack.Children.Count; i++)
                    {
                        if (TimersStack.Children[i] is ActivityTimer currTimer)
                        {
                            csvWriter.WriteField(currTimer.ActivityDescription);
                            csvWriter.WriteField(currTimer.Timer.TotalHours);
                            csvWriter.NextRecord();
                        }
                    }
                }
            }
        }

        private void AddNewTimerButton_Click(object sender, RoutedEventArgs e)
        {
            AddTimer(new ActivityTimer());
        }

        private void AddTimer(ActivityTimer tmr)
        {
            tmr.DeleteRequested += DeleteTimer;
            TimersStack.Children.Add(tmr);
        }

        private void DeleteTimer(ActivityTimer activityTimer)
        {
            for (int i = 1; i < TimersStack.Children.Count; i++)
            {
                if (TimersStack.Children[i] is ActivityTimer currTimer)
                {
                    if (activityTimer == currTimer)
                    {
                        TimersStack.Children.RemoveAt(i);
                        UpdateUI();
                        return;
                    }
                }
            }
        }

        private void ResetAllTimersButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 1; i < TimersStack.Children.Count; i++)
            {
                if (TimersStack.Children[i] is ActivityTimer currTimer)
                {
                    currTimer.Reset();
                }
            }
            UpdateUI();
        }

        private void DeleteAllTimersButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteAllTimers();
        }

        private void DeleteAllTimers()
        {
            while (TimersStack.Children.Count > 1)
            {
                TimersStack.Children.RemoveAt(1);
            }
            UpdateUI();
        }

        private void TheTimer_Tick(object sender, EventArgs e)
        {
            TimeSpan total = TimeSpan.Zero;
            for (int i = 1; i < TimersStack.Children.Count; i++)
            {
                if (TimersStack.Children[i] is ActivityTimer currTimer)
                {
                    if (currTimer.IsTimerEnabled)
                    {
                        currTimer.Tick();
                    }
                }
            }
            UpdateUI();
        }

        private TimeSpan total = TimeSpan.Zero;
        private void UpdateUI()
        {
            TimeSpan prevTotal = total;
            total = TimeSpan.Zero;
            for (int i = 1; i < TimersStack.Children.Count; i++)
            {
                if (TimersStack.Children[i] is ActivityTimer currTimer)
                {
                    total = total.Add(currTimer.Timer);
                }
            }
            if (total.Seconds % 2 == 0)
            {
                TimeLabel.Content = string.Format("{0:00}:{1:00}", Convert.ToInt32(Math.Floor(total.TotalHours)), Convert.ToInt32(total.Minutes));
            }
            else
            {
                TimeLabel.Content = string.Format("{0:00} {1:00}", Convert.ToInt32(Math.Floor(total.TotalHours)), Convert.ToInt32(total.Minutes));
            }

            // if the visible ttoal time has changed, save the data...
            if ((prevTotal.TotalHours != total.TotalHours) || (prevTotal.Minutes != total.Minutes))
            {
                WriteData();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            WriteData();
        }
    }
}
