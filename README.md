# PacuIbera

PacuIbera es una aplicación de escritorio para gestionar una pescadería y sus operaciones de inventario.

## Tecnologías

- C# y .NET 8 para Windows.
- Windows Forms para la interfaz.
- SQL Server para los datos; acceso desde ADO.NET (`Microsoft.Data.SqlClient`).
- Visual Studio para abrir y compilar la solución.

## Funcionalidades

La aplicación incluye gestión de productos y proveedores, pedidos y recepción de proveedores, stock del local y del depósito, lotes y alertas de vencimiento, remitos internos entre depósito y local, ventas, apertura y cierre de caja, movimientos de efectivo e informes. Las operaciones de inventario y remitos se procesan mediante procedimientos almacenados.

Los módulos disponibles dependen del perfil:

- **Administrador:** gestiona catálogos y operaciones administrativas, como depósito, remitos, movimientos de efectivo y resolución de diferencias.
- **Gerente:** consulta información e historiales autorizados en modo de lectura.
- **Vendedor:** realiza ventas y consulta o recibe remitos destinados a su operación; puede abrir y cerrar su caja.

## Abrir y compilar

1. Abrir `PacuIbera.sln` en Visual Studio 2022 con la carga de trabajo de desarrollo de escritorio de .NET.
2. Restaurar los paquetes NuGet si Visual Studio lo solicita.
3. Seleccionar `PacuIbera` como proyecto de inicio y compilar la solución.

El proyecto apunta a `net8.0-windows` y requiere Windows para ejecutar la interfaz WinForms.

## Configurar SQL Server

La configuración se lee desde `PacuIbera/appsettings.json` durante el desarrollo y se copia junto al ejecutable al compilar o publicar. Editar la sección `Database`:

```json
{
  "Database": {
    "Profile": "Entrega",
    "Server": "(localdb)\\MSSQLLocalDB",
    "Name": "PacuIbera_Entrega",
    "IntegratedSecurity": true,
    "TrustServerCertificate": true
  }
}
```

Para desarrollo, el archivo del repositorio está configurado como `Profile=Pruebas` y `Name=PacuIberaDB_Pruebas`. La aplicación solo admite los pares de perfil/base previstos (`Pruebas` con `PacuIberaDB_Pruebas` y `Entrega` con `PacuIbera_Entrega`); no colocar contraseñas de SQL en este archivo. Se utiliza autenticación integrada de Windows.

## Instalación de la base

Los scripts están en `scripts/instalacion/`. En SSMS activar **SQLCMD Mode** y usar el mismo valor de `DatabaseName` en todos los pasos:

1. Ejecutar `00_CrearBase_Configurable.sql` conectado a `master`. El script crea una base nueva y se detiene si el nombre ya existe o está protegido.
2. Conectarse a la base nueva y ejecutar `01_InstalarEsquemaCompleto_Configurable.sql`.
3. Ejecutar `02_InsertarRolesConfirmados.sql`.
4. Revisar `04_InsertarDatosIniciales_Configurable.sql`. Su variable `@AutorizarSeed` está en `0`, por lo que el seed se detiene hasta que una persona responsable revise y autorice explícitamente la carga.
5. Ejecutar `99_VerificarInstalacion.sql` sobre la base destino.

`01_RecuperarEsquemaYProcedimientos_Faltantes.sql` y `03_ConsultarCatalogosSemilla_PacuIberaDB_Pruebas.sql` son herramientas de revisión, no pasos de instalación. No ejecutar scripts de prueba o regularización como parte de una instalación limpia.

**La instalación completa desde cero todavía no fue verificada.** El esquema requiere SQL Server 2019 o posterior y SSMS en modo SQLCMD. Validar primero el procedimiento en una instancia desechable; los scripts no deben ejecutarse sobre `PacuIberaDB` ni `PacuIberaDB_Pruebas`.

## Material de entrega

[Abrir la carpeta compartida de PacuIbera en Google Drive](https://drive.google.com/drive/folders/1xwRYMKpS5eBibgtvner5WxRH1XkzNuiQ)
