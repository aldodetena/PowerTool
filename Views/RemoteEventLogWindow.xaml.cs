using System.Collections.ObjectModel;
using System.Windows;
using PowerTool.Models;

namespace PowerTool.Views
{
    public partial class RemoteEventLogWindow : Window
    {
        public ObservableCollection<RemoteEventLogEntry> EventLogs { get; set; }

        public RemoteEventLogWindow(ObservableCollection<RemoteEventLogEntry> eventLogs)
        {
            InitializeComponent();
            EventLogs = eventLogs;
            DataContext = this;
            EventLogDataGrid.ItemsSource = EventLogs;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
