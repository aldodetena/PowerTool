using System.Windows;

namespace PowerTool;
public partial class ProgramListWindow : Window
{
    public ProgramListWindow(List<InstalledProgram> programas, string nombreEquipo)
    {
        InitializeComponent();
        this.Title = $"Programas instalados en {nombreEquipo}";
        this.DataContext = programas;
    }
}