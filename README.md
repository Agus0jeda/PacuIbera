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

Los scripts están en `scripts/instalacion/`. En SSMS activar **SQLCMD Mode**. Los scripts de entrega definen internamente `DatabaseName=PacuIbera_Entrega`; respetar ese nombre y las autorizaciones manuales indicadas:

1. Ejecutar `00_CrearBase_Configurable.sql` conectado a `master`. El script crea una base nueva y se detiene si el nombre ya existe o está protegido.
2. Conectarse a la base nueva y ejecutar `01_InstalarEsquemaCompleto_Configurable.sql`.
3. Ejecutar `02_InsertarRolesConfirmados.sql`.
4. Revisar `04_InsertarDatosIniciales_Configurable.sql`. Su variable `@AutorizarSeed` está en `0`, por lo que el seed se detiene hasta que una persona responsable revise y autorice explícitamente la carga.
5. Ejecutar `05_InstalarHistorialCierres_Configurable.sql` en SQLCMD Mode, conectado a la base destino y usando el mismo `DatabaseName`. Agrega la consulta histórica de cierres para Administrador y Gerente activos.
6. Revisar las auditorías duplicadas y ejecutar `06_InstalarAnulacionVentaSegura_Configurable.sql` conectado a la base destino en SQLCMD Mode. Mantiene `@AutorizarInstalacion=0`; solo cambiarlo a `1` después de revisar el destino y el preflight.
7. Revisar y ejecutar `07_ActualizarAnalisisVencimientos_Configurable.sql` conectado a la base destino en SQLCMD Mode. Mantiene la autorizacion desactivada hasta su revision.
8. Ejecutar `99_VerificarInstalacion.sql` sobre la base destino.

`01_RecuperarEsquemaYProcedimientos_Faltantes.sql` y `03_ConsultarCatalogosSemilla_PacuIberaDB_Pruebas.sql` son herramientas de revisión, no pasos de instalación. No ejecutar scripts de prueba o regularización como parte de una instalación limpia.

**La instalación completa desde cero todavía no fue verificada.** El esquema requiere SQL Server 2019 o posterior y SSMS en modo SQLCMD. Validar primero el procedimiento en una instancia desechable; los scripts no deben ejecutarse sobre `PacuIberaDB` ni `PacuIberaDB_Pruebas`.

## Estado de la entrega (16/11/2026)

- La solución compila y se han comprobado manualmente operaciones principales de ventas, stock, caja y reportes sobre la base de pruebas.
- El Dashboard incluye el Top 5 de productos por **cantidad vendida**. Las cantidades en kilogramos y unidades se identifican por separado: no representan la misma magnitud ni un ranking por facturación.
- La anulación de ventas registra el motivo y la decisión de devolución por cada detalle/lote. Solo reincorpora mercadería declarada recuperada y apta cuando el lote está activo y vigente. No reactiva automáticamente lotes inactivos o vencidos.
- **Anular una venta no registra automáticamente la devolución del dinero.** El reintegro y la conciliación de pagos siguen siendo una limitación; deben respetarse las protecciones del cierre de caja.
- **La instalación limpia de la base `PacuIbera_Entrega` todavía no está probada de punta a punta.** Una compilación correcta y la verificación estática de SQL no sustituyen esa prueba.

## Documentación para el equipo

- **Instalación detallada:** [`scripts/instalacion/ORDEN_Y_PENDIENTES.md`](scripts/instalacion/ORDEN_Y_PENDIENTES.md). Allí están el orden exacto, los pasos que requieren autorización y la verificación final.
- **Código de la aplicación:** la interfaz está en `PacuIbera/UI`, el acceso a SQL Server en `PacuIbera/Datos` y las clases compartidas en `PacuIbera/Dominio`. La configuración se prepara con `PacuIbera/appsettings.json`.
- **Manual de usuario:** documentación de los recorridos de Administrador, Gerente y Vendedor en preparación; no se incluye como manual terminado en este repositorio.

## Material de entrega

[Abrir la carpeta compartida de PacuIbera en Google Drive](https://drive.google.com/drive/folders/1xwRYMKpS5eBibgtvner5WxRH1XkzNuiQ)
