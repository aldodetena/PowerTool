using System.Management.Automation;
using System.Management.Automation.Runspaces;
using PowerTool.Models;
using PowerTool.Utilities; // Importar tu modelo de datos si es necesario

namespace PowerTool.Services
{
    public static class RemoteCommandService
    {
        public static string EjecutarComandoRemoto(Equipo equipo, string comando)
        {
            try
            {
                var runspace = RunspaceFactory.CreateRunspace();
                runspace.Open();

                var connectionInfo = new WSManConnectionInfo(
                    new Uri($"http://{equipo.Name}:5985/wsman"),
                    "http://schemas.microsoft.com/powershell/Microsoft.PowerShell",
                    (PSCredential)null
                );
                connectionInfo.AuthenticationMechanism = AuthenticationMechanism.Default;

                var remoteRunspace = RunspaceFactory.CreateRunspace(connectionInfo);
                remoteRunspace.Open();

                var pipeline = remoteRunspace.CreatePipeline();
                pipeline.Commands.Add(new Command(comando));
                var results = pipeline.Invoke();

                runspace.Close();
                remoteRunspace.Close();

                string resultado = string.Join(Environment.NewLine, results);
                return resultado;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error al ejecutar el comando en el equipo {equipo.Name}", ex);
                return $"Error: {ex.Message}";
            }
        }
    }
}
