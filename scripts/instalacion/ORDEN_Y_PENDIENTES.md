# Instalación SQL de PacuIbera

## Estado de entrega

El exportado original `PacuIbera_Estructura_Completa.sql` se conserva sin cambios. Se preparó una copia de instalación configurable con sus objetos, sin crear una base aquí y sin ejecutar SQL. **La instalación limpia todavía no está probada.**

La copia conserva las 31 tablas, 50 procedimientos, 5 tipos tabulares, claves, restricciones, índices y la propiedad extendida exportada. Se quitaron las instrucciones de creación de la base fuente con rutas físicas, el bloque de opciones `ALTER DATABASE` ligado a la base exportada y los `USE master` de cierre. El DDL de los objetos se mantiene como fuente exportada. La base destino conserva sus opciones predeterminadas del servidor; si la entrega exige replicar también las opciones de base, deben revisarse y adaptarse por separado.

## Archivos y orden previsto

1. En SSMS activar **SQLCMD Mode**. `:on error exit` detiene la secuencia ante una falla. `00_CrearBase_Configurable.sql`: configurar `DatabaseName` y crear una base nueva. Rechaza nombres protegidos y bases ya existentes; no las sobrescribe.
2. Conectarse a la base creada. Ejecutar `01_InstalarEsquemaCompleto_Configurable.sql` en SQLCMD Mode con el mismo `DatabaseName`. Solo acepta una base vacía; contiene guardas de destino y requiere SQL Server 2019 o posterior por `OPTIMIZE_FOR_SEQUENTIAL_KEY`.
3. Ejecutar `02_InsertarRolesConfirmados.sql`. Inserta idempotentemente los IDs requeridos por el origen: 0 Administrador, 1 Vendedor y 2 Gerente. El segundo registro Gerente (Id 3) del reporte no se importa. Si el destino ya tiene nombres/IDs en conflicto, el script se detiene sin corregirlos automáticamente.
4. `PacuIbera_Proveedores.rpt` completa los CUIT y las asociaciones proveedor-producto. `03_ConsultarCatalogosSemilla_PacuIberaDB_Pruebas.sql` queda como consulta de solo lectura para contrastar esos datos si hace falta.
5. `04_InsertarDatosIniciales_Configurable.sql` contiene las cuentas demo y sus hashes SHA-256 compatibles con `Seguridad.GenerarHashSHA256`. Las credenciales demo actuales son: Administrador, DNI `11111111`, clave `1234`; Gerente, DNI `33333333`, clave `1234`; Vendedor, DNI `22222222`, clave `1234`. Son credenciales ficticias para demostración; no reutilizarlas en cuentas reales. `CredencialesDemo.local.txt` está excluido de Git y no debe publicarse. El seed usa `DatabaseName`, conserva los IDs, incorpora proveedores y sus tres relaciones reales, incluye los 16 IDs de métodos de pago que los procedimientos exportados de Caja validan (C# usa 1–4 para pagos nuevos), carga los 8 productos activos con `StockActual=0`, y no crea lotes ni movimientos históricos. `@AutorizarSeed` permanece en 0; cambiarlo solo durante la instalación aprobada.
6. Después de completar y revisar el seed, ejecutar `99_VerificarInstalacion.sql` sobre la base destino con el mismo `DatabaseName`.

`01_RecuperarEsquemaYProcedimientos_Faltantes.sql` queda como auditoría de solo lectura adicional sobre la base de pruebas, no como etapa del instalador. Si una instalación se interrumpe tras crear algunos objetos, no la reejecutes sobre esa base parcial: el guardado la rechazará; debe revisarse y descartarse manualmente la base de entrega vacía/temporal antes de comenzar de nuevo.

La instalación final aún no se probó: el seed sigue desautorizado por diseño y debe habilitarse únicamente durante la instalación aprobada.

## Bloqueos pendientes

### Datos iniciales y bloqueos pendientes

`04_InsertarDatosIniciales_Configurable.sql` prepara provincias 1–24, las localidades canónicas 1–88 (incluidos Corrientes/Corrientes Capital IDs 7/25 usados por C#), los 16 IDs de métodos de pago exigidos por los procedimientos de Caja exportados, cinco categorías y los ocho productos activos del reporte con sus precios, categorías, mínimos, venta por peso y control de vencimiento. Los IDs extra de localidades duplicadas no se copian. Los alias de métodos de pago sí son necesarios: los procedimientos de cierre/resumen rechazan si falta cualquiera de esos IDs. Los productos inician con stock cero; no se inventan lotes.

El seed de catálogos está completo con los datos disponibles en `pacutxt2.rpt` y `PacuIbera_Proveedores.rpt`: incluye seis proveedores activos con CUIT y las tres relaciones `ProveedorProducto` reportadas.

Las tres cuentas demo están preparadas con roles 0 Administrador, 2 Gerente y 1 Vendedor, DNIs ficticios únicos, ubicación válida (Provincia 7/Localidad 25) y hashes compatibles con el login: Administrador `11111111` / `1234`, Gerente `33333333` / `1234`, Vendedor `22222222` / `1234`. La contraseña es una credencial demo pública, no apta para cuentas reales. `CredencialesDemo.local.txt` permanece excluido de Git. La autorización del seed permanece desactivada.

### Conexión WinForms

La conexión se lee de `appsettings.json` junto al ejecutable; no contiene credenciales y se copia al directorio de salida. El archivo del proyecto inicia en `Profile=Pruebas`, `Name=PacuIberaDB_Pruebas` y `Server` vacío (conserva la detección actual de servidor para desarrollo). Para la entrega, el profesor modifica ese archivo en la carpeta de PacuIbera, junto a `PacuIbera.exe`, y configura `Profile=Entrega`, `Name=PacuIbera_Entrega` y `Server` con su instancia SQL Server (por ejemplo `(localdb)\\MSSQLLocalDB`). Se utiliza autenticación integrada de Windows.

El código solo acepta dos combinaciones: `Pruebas` + `PacuIberaDB_Pruebas`, o `Entrega` + `PacuIbera_Entrega`. Un nombre arbitrario, `PacuIberaDB` u otra base del sistema detiene la aplicación antes de habilitar operaciones. Las operaciones de negocio conservan sus comprobaciones de rol/usuario activo y verifican `DB_NAME()` contra la base configurada. Los scripts administrativos y de pruebas conservan sus validaciones SQL literales para `PacuIberaDB_Pruebas`; cambiar `appsettings.json` no permite ejecutarlos en otra base.

### Procedimientos heredados

Se preservaron las 50 definiciones del exportado tal como estaban. Entre ellas están procedimientos heredados que requieren revisión antes de habilitar su uso en la entrega, como `sp_TransferirDepositoALocal`, `sp_RecepcionarItemPedido` y `sp_ModificarProducto`, que podrían realizar movimientos fuera del flujo nuevo. El instalador no corrige ni omite objetos porque la tarea pidió conservar el exportado.

## Bloqueos restantes para la entrega

- No se probó el paquete en una instalación limpia. Hace falta una instancia desechable para validar el orden completo y el primer inicio de sesión.
- Antes de usar la entrega, configurar `appsettings.json` junto al ejecutable con el servidor del profesor y `Profile=Entrega`, `Name=PacuIbera_Entrega`.
- Las definiciones exportadas conservan procedimientos heredados (`sp_TransferirDepositoALocal`, `sp_RecepcionarItemPedido`, `sp_ModificarProducto`); revisar su habilitación antes de usar esas operaciones en la entrega.

## Confirmación de instalación

No se ejecutó ni probó una instalación limpia. La revisión hecha fue estática: conteos de objetos, inspección de referencias de base/ruta y orden DDL. Debe validarse en una instancia desechable antes de presentarlo como instalación reproducible.
