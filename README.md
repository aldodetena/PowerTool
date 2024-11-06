# PowerTool - Documentación

## Descripción General

**PowerTool** es una aplicación de gestión remota de sistemas diseñada para administradores de TI. Permite interactuar con equipos en un dominio de Active Directory, proporcionando herramientas avanzadas para la gestión de hardware, software y usuarios.

## Funcionalidades Principales

### 1. **Gestión de Equipos**
   - Visualización en tiempo real del estado de los equipos (en línea o apagados).
   - Información detallada sobre los equipos, incluyendo sistema operativo, última conexión, IP y MAC.

### 2. **Inventario de Hardware**
   - Obtiene información sobre la CPU, RAM y almacenamiento de los equipos.
   - Visualización de inventario en una ventana dedicada.

### 3. **Inventario de Software**
   - Muestra una lista detallada de programas instalados en equipos remotos.
   - Permite la instalación y desinstalación remota de software.

### 4. **Gestión de Servicios**
   - Visualización de servicios en ejecución en equipos remotos.
   - Permite iniciar, detener o reiniciar servicios.

### 5. **Gestión de Usuarios**
   - Carga y administración de cuentas de usuario en el dominio.
   - Habilitación, deshabilitación y desbloqueo de usuarios.
   - Restablecimiento de contraseñas de usuarios.

### 6. **Gestión de Procesos**
   - Visualización de procesos en ejecución en equipos remotos.
   - Posibilidad de finalizar procesos de manera remota.

### 7. **Registro de Eventos**
   - Consulta de registros de eventos críticos y de error en los equipos remotos.
   - Visualización en una interfaz gráfica para análisis detallado.

### 8. **Conexión Remota**
   - Conexión RDP (Escritorio remoto) con un solo clic.
   - Acceso rápido al sistema de archivos remoto.

### 9. **Automatización y Scripts**
   - Ejecución remota de comandos y scripts de PowerShell.
   - Ventana de comandos integrada para enviar comandos a equipos específicos.

## Instalación

### Requisitos
- **Windows 10/11** o **Windows Server** con soporte para .NET.
- **.NET SDK 7.0** o superior.
- Acceso a un dominio de Active Directory.

### Configuración Inicial
1. Clonar el repositorio.
2. Restaurar dependencias:
   /codigo
   dotnet restore
   /codigo
3. Compilar y ejecutar la aplicación:
   /codigo
   dotnet run --project PowerTool
   /codigo

## Bibliotecas Utilizadas

Estas son las principales bibliotecas utilizadas en el proyecto **PowerTool**:

1. **[SkiaSharp](https://www.nuget.org/packages/SkiaSharp/)**
   - Utilizada para renderizar gráficos 2D, incluyendo imágenes SVG, en la aplicación WPF.
   - Instalación:
     ```bash
     dotnet add package SkiaSharp
     ```

2. **[SkiaSharp.Views.WPF](https://www.nuget.org/packages/SkiaSharp.Views.WPF/)**
   - Extiende SkiaSharp para su uso en aplicaciones WPF.
   - Instalación:
     ```bash
     dotnet add package SkiaSharp.Views.WPF
     ```

3. **[Svg.Skia](https://www.nuget.org/packages/Svg.Skia/)**
   - Permite cargar y renderizar archivos SVG utilizando SkiaSharp.
   - Instalación:
     ```bash
     dotnet add package Svg.Skia
     ```

4. **[System.Management](https://www.nuget.org/packages/System.Management/)**
   - Proporciona acceso a WMI para realizar consultas y comandos en equipos remotos.
   - Instalación:
     ```bash
     dotnet add package System.Management
     ```

5. **[System.DirectoryServices](https://www.nuget.org/packages/System.DirectoryServices/)**
   - Permite interactuar con Active Directory y otros servicios de directorio en Windows.
   - Instalación:
     ```bash
     dotnet add package System.DirectoryServices
     ```

6. **[Microsoft.PowerShell.Commands.Diagnostics](https://www.nuget.org/packages/Microsoft.PowerShell.Commands.Diagnostics/)**
   - Necesario para alojar una instancia de PowerShell.
   - Instalación:
     ```bash
     dotnet add package Microsoft.PowerShell.Commands.Diagnostics --version 7.4.5
     ```

7. **[Microsoft.PowerShell.ConsoleHost](https://www.nuget.org/packages/Microsoft.PowerShell.ConsoleHost/)**
   - Necesario para alojar una instancia de PowerShell.
   - Instalación:
     ```bash
     dotnet add package Microsoft.PowerShell.ConsoleHost --version 7.4.5
     ```

8. **[Microsoft.PowerShell.Commands.Diagnostics](https://www.nuget.org/packages/Microsoft.PowerShell.Commands.Utility/)**
   - Necesario para alojar una instancia de PowerShell.
   - Instalación:
     ```bash
     dotnet add package Microsoft.PowerShell.Commands.Utility --version 7.4.5
     ```

9. **[Microsoft.PowerShell.Commands.Management](https://www.nuget.org/packages/Microsoft.PowerShell.Commands.Management/)**
   - Necesario para alojar una instancia de PowerShell.
   - Instalación:
     ```bash
     dotnet add package Microsoft.PowerShell.Commands.Management --version 7.4.5
     ```


10. **[Microsoft.WSMan.Management](https://www.nuget.org/packages/Microsoft.WSMan.Management/)**
   - Necesario para alojar una instancia de PowerShell.
   - Instalación:
     ```bash
     dotnet add package Microsoft.WSMan.Management --version 7.4.5
     ```

11. **[System.DirectoryServices.AccountManagement](https://www.nuget.org/packages/system.directoryservices.accountmanagement/)**
   - Proporciona acceso uniforme y manipulación de principios de seguridad en múltiples almacenes principales.
   - Instalación:
     ```bash
     dotnet add package System.DirectoryServices.AccountManagement --version 8.0.1
     ```

## Uso

### Gestión de Dominios
1. Al abrir la aplicación, se mostrará una ventana para seleccionar un dominio.
2. Puedes agregar, editar o eliminar dominios en esta ventana.
3. Una vez seleccionado el dominio, la aplicación cargará los equipos automáticamente.

### Funciones Contextuales
- **Click derecho** en un equipo para abrir un menú contextual con opciones como:
  - Conexión RDP.
  - Explorador de archivos.
  - Ejecutar comandos.
  - Gestionar servicios y procesos.

### Inventario y Reportes
- El inventario de hardware y software puede exportarse en formatos como CSV para un análisis más detallado.

## Capturas de Pantalla

### 1. Vista Principal
*Incluye una lista de equipos con su estado en tiempo real.*

### 2. Gestión de Servicios
*Permite administrar servicios directamente desde la interfaz gráfica.*

### 3. Inventario de Hardware
*Ventana dedicada que muestra información detallada del hardware.*

## Contribución
Si deseas contribuir al proyecto, por favor sigue estos pasos:
1. Haz un fork del repositorio.
2. Crea una nueva rama para tu funcionalidad:
   /codigo
   git checkout -b feature/nueva-funcionalidad
   /codigo
3. Realiza tus cambios y envía un pull request.