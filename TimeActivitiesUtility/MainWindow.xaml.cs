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
        public MainWindow()
        {
            InitializeComponent();
            DataContext = ViewModel.MainWindowVM.Create(new Service.UpdateTimerDialogService());
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                // Use the RestoreBounds as the current values will be 0, 0 and the size of the screen
                Properties.Settings.Default.Top = RestoreBounds.Top;
                Properties.Settings.Default.Left = RestoreBounds.Left;
                Properties.Settings.Default.Height = RestoreBounds.Height;
                Properties.Settings.Default.Width = RestoreBounds.Width;
                Properties.Settings.Default.Maximized = true;
            }
            else
            {
                Properties.Settings.Default.Top = this.Top;
                Properties.Settings.Default.Left = this.Left;
                Properties.Settings.Default.Height = this.Height;
                Properties.Settings.Default.Width = this.Width;
                Properties.Settings.Default.Maximized = false;
            }

            Properties.Settings.Default.Save();
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            this.Top = Properties.Settings.Default.Top;
            this.Left = Properties.Settings.Default.Left;
            this.Height = Properties.Settings.Default.Height;
            this.Width = Properties.Settings.Default.Width;
            // Very quick and dirty - but it does the job
            if (Properties.Settings.Default.Maximized)
            {
                WindowState = WindowState.Maximized;
            }
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    var child = VisualTreeHelper.GetChild(depObj, i);

                    if (child != null && child is T)
                        yield return (T)child;

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                        yield return childOfChild;
                }
            }
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            foreach (TextBox tb in FindVisualChildren<TextBox>(this))
            {
                if (tb != sender) {
                    if (string.IsNullOrEmpty(Search.Text)) 
                    {
                        tb.Background = Brushes.Transparent;
                    }
                    else if (tb.Text.IndexOf(Search.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        tb.Background = Brushes.LightPink;
                        tb.Select(tb.Text.IndexOf(Search.Text, StringComparison.OrdinalIgnoreCase), Search.Text.Length);
                        tb.BringIntoView();
                    }
                    else
                    {
                        tb.Background = Brushes.Transparent;
                    }
                }
            }
        }

        private void Search_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (string.IsNullOrEmpty(Search.Text))
                {
                    return;
                }
                foreach (TextBox tb in FindVisualChildren<TextBox>(this))
                {
                    if (tb != sender)
                    {
                        if (tb.Text.IndexOf(Search.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            tb.Focus();
                            return;
                        }
                    }
                }
            }
        }
    }
}
