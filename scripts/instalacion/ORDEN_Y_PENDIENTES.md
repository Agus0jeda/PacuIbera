# Instalación SQL de PacuIbera — entrega revisada el 16/11/2026

El paquete instala una base nueva `PacuIbera_Entrega`. Requiere SQL Server 2019 o posterior y SSMS con **SQLCMD Mode**. La revisión y compilación del programa no sustituyen una prueba de instalación limpia: no se ejecutó SQL durante esta preparación.

## Requisitos

Windows, SQL Server 2019 o posterior (Express es suficiente) o SQL Server Express LocalDB 2019 o posterior, y SQL Server Management Studio. LocalDB debe estar instalado y la instancia `MSSQLLocalDB` disponible bajo la misma cuenta de Windows que usará la aplicación. Para un servidor compartido usar la instancia SQL Server del compañero; LocalDB es local al usuario. La cuenta necesita permisos para crear la base e instalar tablas, tipos y procedimientos. Para ejecutar el programa se necesita .NET Desktop Runtime 8; para compilar, .NET SDK 8 o compatible con el destino `net8.0-windows`.

En SSMS activar **Consulta → Modo SQLCMD** antes de ejecutar cada archivo completo. Cada SQL del paquete define por sí mismo `:setvar DatabaseName "PacuIbera_Entrega"`; no depende de variables de una ventana anterior ni de rutas locales o archivos externos. El paso 00 crea la base desde `master`; desde el paso 01 seleccionar `PacuIbera_Entrega` en la conexión. Mantener ese nombre, que es el admitido por el perfil Entrega de la aplicación.

## Archivos para Google Drive

Subir o reemplazar exactamente estos nueve archivos en `Base de datos/Entrega Final`, tomándolos de `scripts/instalacion`:

1. `00_CrearBase_Configurable.sql`
2. `01_InstalarEsquemaCompleto_Configurable.sql`
3. `02_InsertarRolesConfirmados.sql`
4. `04_InsertarDatosIniciales_Configurable.sql`
5. `05_InstalarHistorialCierres_Configurable.sql`
6. `06_InstalarAnulacionVentaSegura_Configurable.sql`
7. `07_ActualizarAnalisisVencimientos_Configurable.sql`
8. `99_VerificarInstalacion.sql`
9. `ORDEN_Y_PENDIENTES.md`

Esta es la lista vigente del paquete. No usar los SQL antiguos del 11/11 como alternativa al instalador actualizado. No subir `CredencialesDemo.local.txt`, archivos `.rpt`, `PacuIbera_Estructura_Completa.sql`, backups, bases físicas, scripts de laboratorio o regularización ni archivos privados. Los archivos `01_RecuperarEsquemaYProcedimientos_Faltantes.sql` y `03_ConsultarCatalogosSemilla_PacuIberaDB_Pruebas.sql` son auditorías del origen, no pasos de entrega. Los scripts externos con destino exclusivo `PacuIberaDB_Pruebas` tampoco forman parte de este paquete.

## Orden de instalación

Usar el mismo `DatabaseName=PacuIbera_Entrega` en todos los pasos. No ejecutar sobre las bases existentes de desarrollo o pruebas.

1. Ejecutar `00`: crea una base nueva y rechaza una base ya existente. Luego conectarse a `PacuIbera_Entrega`.
2. Ejecutar `01`: exige una base vacía e instala el esquema y los procedimientos. No importa ventas, lotes, stock ni movimientos históricos. El Top 5 ya está corregido aquí; no requiere el parche exclusivo de pruebas.
3. Ejecutar `02`: carga los roles con los IDs requeridos por la aplicación (0 Administrador, 1 Vendedor, 2 Gerente), rechazando conflictos.
4. Revisar `04` antes de autorizar `@AutorizarSeed=1`. Contiene catálogos iniciales y cuentas de demostración con identidad sintética; no es un volcado de datos operativos. Conserva los IDs requeridos por los procedimientos, productos con stock cero y ningún historial de ventas o cajas. No compartir credenciales locales. Los hashes demo públicos existentes no son credenciales personales.
5. Ejecutar `05`: instala la consulta histórica de cierres para Administrador o Gerente activo.
6. Revisar `06`, incluidas las auditorías duplicadas informadas, y autorizar manualmente `@AutorizarInstalacion=1`. Agrega el detalle de auditoría de anulaciones y actualiza la ruta segura; instalarlo no ejecuta anulaciones ni modifica pagos o cajas. Incluye su propia variable SQLCMD y detención ante errores.
7. Revisar y autorizar `07` de la misma manera. Corrige el reporte de vencimientos, incluye su unidad y excluye lotes inactivos, sin alterar inventario.
8. Ejecutar `99`: consulta existencia de tablas, procedimientos, tipos y catálogos, y comprueba los metadatos del Top 5 y la unidad del reporte de vencimientos. No ejecuta procedimientos de negocio. Revisar cualquier resultado `Falta` o `Revisar definicion`; los controles textuales no prueban por sí solos el comportamiento de los procedimientos.

No reejecutar `01` sobre una instalación parcial: su guarda la rechaza. Ante errores, detenerse y revisar el destino; este paquete no borra ni repara automáticamente bases existentes.

## Correcciones incluidas

El esquema base incluye movimientos administrativos de efectivo, apertura de caja protegida, registro de ventas coordinado con el cierre, cierre y resumen de caja con pagos mixtos, FIFO y remitos internos. Los pasos 05–07 completan historial de cierres, anulación segura y análisis de vencimientos. El conjunto esperado es de 32 tablas, 51 procedimientos y 6 tipos tabulares después del paso 06.

`sp_ReporteTopProductos` devuelve `ProductoId`, `Nombre`, `SeVendePorPeso` y `TotalVendido`. Agrupa por ID, nombre y modalidad, mantiene `TOP (5)`, excluye anuladas e incluye ambos días del período seleccionado. Ordena por cantidad descendente y desempata por ID. El formulario conserva las cantidades originales, identifica kg y unidades, muestra todos los nombres y agrega tooltips. Kg y unidades son magnitudes diferentes; el ranking no es por facturación.

El formulario actualizado requiere el procedimiento actualizado para identificar las unidades. `ReportesDatos.ObtenerTopProductos` devuelve las columnas adicionales sin necesitar cambios.

El botón «Ver Ticket» de Anulaciones lee el registro enlazado del reporte y conserva el ID de venta original. Identifica al autor de la anulación, su fecha y motivo; no confunde esa fecha con la fecha original de venta. El usuario confirmó manualmente que abre correctamente desde el rol Gerente una anulación realizada por Administrador. Esta confirmación no equivale a validar una instalación limpia.

## Configuración y validación pendiente

Configurar `appsettings.json` junto a `PacuIbera.exe` con `Database.Profile=Entrega`, `Database.Name=PacuIbera_Entrega`, el servidor del destinatario y autenticación integrada de Windows. La aplicación acepta únicamente los perfiles Pruebas y Entrega con sus bases correspondientes. No distribuir configuraciones privadas del equipo de desarrollo.

Ejemplo público para LocalDB (crear o editar el archivo en el equipo destinatario):

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

Para Express, reemplazar únicamente `Server` por la instancia real, por ejemplo `EQUIPO\\SQLEXPRESS` en JSON. El archivo del repositorio inicia en perfil Pruebas: copiarlo sin adaptarlo no configura la entrega. No agregar usuarios ni contraseñas SQL; la aplicación exige autenticación integrada.

La instalación se considera completada solo después de que los pasos 00, 01, 02, 04, 05, 06 y 07 terminen sin errores y con sus autorizaciones revisadas, y que 99 informe todos los objetos y columnas como `Existe`, la definición Top 5 como `Coincide`, los tres roles y los catálogos esperados (5 categorías, 6 proveedores activos y 8 productos activos). En una base recién instalada esos productos tienen stock cero: los gráficos de ventas vacíos son normales. El verificador no muestra secretos ni valida los hashes de inicio de sesión. Después debe probarse el login y cada flujo previsto desde la aplicación; cualquier resultado faltante o error significa que aún no se completó la instalación.

La compatibilidad de destino y los contratos se revisaron estáticamente. Sigue pendiente probar una instalación limpia en una instancia desechable, el inicio de sesión y los flujos funcionales con datos creados expresamente para esa validación. Los procedimientos heredados como `sp_TransferirDepositoALocal`, `sp_RecepcionarItemPedido`, `sp_ModificarProducto` y `sp_AnularVenta` permanecen en el esquema; revisar sus dependencias antes de habilitar rutas externas. No se introdujeron funcionalidades nuevas en esta preparación.
