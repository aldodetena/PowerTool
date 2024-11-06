using System.Collections.ObjectModel;
using System.Windows;
using PowerTool.Models;

namespace PowerTool.Views
{
    /// <summary>
    /// Representa una ventana para visualizar los registros de eventos remotos de un equipo.
    /// </summary>
    public partial class RemoteEventLogWindow : Window
    {
        /// <summary>
        /// Obtiene o establece la colección de registros de eventos remotos.
        /// </summary>
        public ObservableCollection<RemoteEventLogEntry> EventLogs { get; set; }

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="RemoteEventLogWindow"/> con una colección de registros de eventos.
        /// </summary>
        /// <param name="eventLogs">Colección de registros de eventos remotos.</param>
        public RemoteEventLogWindow(ObservableCollection<RemoteEventLogEntry> eventLogs)
        {
            InitializeComponent();
            EventLogs = eventLogs;
            DataContext = this;
            EventLogDataGrid.ItemsSource = EventLogs;
        }

        /// <summary>
        /// Maneja el evento de clic del botón de cierre, cerrando la ventana.
        /// </summary>
        /// <param name="sender">El botón que activó el evento.</param>
        /// <param name="e">Datos del evento RoutedEventArgs.</param>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
