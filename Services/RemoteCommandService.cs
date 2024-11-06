using System.Management.Automation;
using System.Management.Automation.Runspaces;
using PowerTool.Models;
using PowerTool.Utilities; // Importar tu modelo de datos si es necesario

namespace PowerTool.Services
{
    /// <summary>
    /// Proporciona funcionalidad para ejecutar comandos en equipos remotos utilizando PowerShell y WinRM.
    /// </summary>
    public static class RemoteCommandService
    {
        /// <summary>
        /// Ejecuta un comando de PowerShell en un equipo remoto y devuelve el resultado.
        /// </summary>
        /// <param name="equipo">El equipo remoto en el que se ejecutará el comando.</param>
        /// <param name="comando">El comando de PowerShell a ejecutar.</param>
        /// <returns>El resultado del comando, o un mensaje de error si la ejecución falla.</returns>
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
