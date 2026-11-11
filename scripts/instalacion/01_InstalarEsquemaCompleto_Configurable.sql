/*
  Instalacion del esquema exportado. Requiere SSMS en modo SQLCMD.
  Fuente preservada sin cambios: PacuIbera_Estructura_Completa.sql.
  Primero crear la base vacia con 00_CrearBase_Configurable.sql y conectarse a ella.
*/
:on error exit
:setvar DatabaseName "PacuIbera_Entrega"

IF N'$(DatabaseName)' LIKE N'%[^A-Za-z0-9_]%' OR N'$(DatabaseName)' IN (N'master',N'model',N'msdb',N'tempdb',N'PacuIberaDB',N'PacuIberaDB_Pruebas')
    THROW 54030, 'DatabaseName debe ser un identificador simple y distinto de las bases protegidas.', 1;
IF DB_ID(N'$(DatabaseName)') IS NULL
    THROW 54031, 'La base destino no existe. Ejecute primero 00_CrearBase_Configurable.sql.', 1;
IF DB_NAME() <> N'$(DatabaseName)'
    THROW 54032, 'Conectese a DatabaseName antes de instalar el esquema.', 1;
IF N'$(DatabaseName)' IN (N'PacuIberaDB',N'PacuIberaDB_Pruebas')
    THROW 54035, 'No se permite instalar sobre las bases actuales de PacuIbera.', 1;
IF TRY_CONVERT(int, SERVERPROPERTY('ProductMajorVersion')) < 15
    THROW 54034, 'El esquema exportado requiere SQL Server 2019 o posterior (OPTIMIZE_FOR_SEQUENTIAL_KEY).', 1;
IF EXISTS (SELECT 1 FROM sys.tables WHERE is_ms_shipped=0)
   OR EXISTS (SELECT 1 FROM sys.procedures WHERE is_ms_shipped=0)
   OR EXISTS (SELECT 1 FROM sys.table_types WHERE is_user_defined=1)
    THROW 54033, 'La base destino no esta vacia. La instalacion no es incremental y se detiene para evitar objetos parciales.', 1;
GO

USE [$(DatabaseName)];
GO
/****** Object:  UserDefinedTableType [dbo].[RecepcionDetalleEntrada]    Script Date: 11/11/2026 08:03:21 ******/
CREATE TYPE [dbo].[RecepcionDetalleEntrada] AS TABLE(
	[DetallePedidoProveedorId] [int] NOT NULL,
	[CantidadRecibida] [decimal](10, 3) NOT NULL,
	PRIMARY KEY CLUSTERED
(
	[DetallePedidoProveedorId] ASC
)WITH (IGNORE_DUP_KEY = OFF)
)
GO
/****** Object:  UserDefinedTableType [dbo].[RecepcionLoteEntrada]    Script Date: 11/11/2026 08:03:21 ******/
CREATE TYPE [dbo].[RecepcionLoteEntrada] AS TABLE(
	[DetallePedidoProveedorId] [int] NOT NULL,
	[LoteNumero] [int] NOT NULL,
	[Cantidad] [decimal](10, 3) NOT NULL,
	[TieneVencimiento] [bit] NOT NULL,
	[FechaVencimiento] [datetime2](0) NULL,
	PRIMARY KEY CLUSTERED
(
	[DetallePedidoProveedorId] ASC,
	[LoteNumero] ASC
)WITH (IGNORE_DUP_KEY = OFF)
)
GO
/****** Object:  UserDefinedTableType [dbo].[RecepcionRemitoLoteEntrada]    Script Date: 11/11/2026 08:03:21 ******/
CREATE TYPE [dbo].[RecepcionRemitoLoteEntrada] AS TABLE(
	[RemitoLoteId] [int] NOT NULL,
	[CantidadRecibida] [decimal](10, 3) NOT NULL,
	PRIMARY KEY CLUSTERED
(
	[RemitoLoteId] ASC
)WITH (IGNORE_DUP_KEY = OFF)
)
GO
/****** Object:  UserDefinedTableType [dbo].[RemitoProductoEntrada]    Script Date: 11/11/2026 08:03:21 ******/
CREATE TYPE [dbo].[RemitoProductoEntrada] AS TABLE(
	[ProductoId] [int] NOT NULL,
	[Cantidad] [decimal](10, 3) NOT NULL,
	PRIMARY KEY CLUSTERED
(
	[ProductoId] ASC
)WITH (IGNORE_DUP_KEY = OFF)
)
GO
/****** Object:  UserDefinedTableType [dbo].[ResolucionDiferenciaEntrada]    Script Date: 11/11/2026 08:03:21 ******/
CREATE TYPE [dbo].[ResolucionDiferenciaEntrada] AS TABLE(
	[RecepcionRemitoLoteId] [int] NOT NULL,
	[Cantidad] [decimal](10, 3) NOT NULL,
	PRIMARY KEY CLUSTERED
(
	[RecepcionRemitoLoteId] ASC
)WITH (IGNORE_DUP_KEY = OFF)
)
GO
/****** Object:  Table [dbo].[Caja]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Caja](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UsuarioId] [int] NOT NULL,
	[FechaHoraApertura] [datetime] NOT NULL,
	[FechaHoraCierre] [datetime] NULL,
	[SaldoInicial] [decimal](10, 2) NOT NULL,
	[SaldoFinal] [decimal](10, 2) NULL,
	[Estado] [varchar](30) NOT NULL,
PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Categoria]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Categoria](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [varchar](100) NOT NULL,
PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Cliente]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Cliente](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [varchar](100) NOT NULL,
	[Apellido] [varchar](100) NOT NULL,
	[DNI_CUIT] [varchar](20) NULL,
	[Telefono] [varchar](20) NULL,
	[ProvinciaId] [int] NOT NULL,
	[LocalidadId] [int] NOT NULL,
	[Email] [varchar](150) NULL,
	[Direccion] [varchar](150) NULL,
PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DepositoStock]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DepositoStock](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ProductoId] [int] NOT NULL,
	[FechaIngreso] [datetime2](0) NULL,
	[FechaVencimiento] [datetime2](0) NULL,
	[StockInicial] [decimal](10, 3) NOT NULL,
	[StockActual] [decimal](10, 3) NOT NULL,
	[Activo] [bit] NOT NULL,
 CONSTRAINT [PK_DepositoStock] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DepositoStockMinimo]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DepositoStockMinimo](
	[ProductoId] [int] NOT NULL,
	[StockMinimo] [decimal](10, 3) NOT NULL,
 CONSTRAINT [PK_DepositoStockMinimo] PRIMARY KEY CLUSTERED
(
	[ProductoId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DetallePedidoProveedor]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DetallePedidoProveedor](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PedidoId] [int] NOT NULL,
	[ProductoId] [int] NOT NULL,
	[Cantidad] [decimal](10, 3) NOT NULL,
	[CantidadRecibida] [decimal](10, 3) NOT NULL,
	[PrecioCosto] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_DetallePedidoProveedor] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DetalleVenta]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DetalleVenta](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[VentaId] [int] NOT NULL,
	[ProductoId] [int] NOT NULL,
	[LoteId] [int] NOT NULL,
	[Cantidad] [decimal](10, 3) NOT NULL,
	[PrecioUnitario] [decimal](10, 2) NOT NULL,
	[Subtotal] [decimal](10, 2) NOT NULL,
PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Localidad]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Localidad](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [varchar](100) NOT NULL,
	[ProvinciaId] [int] NOT NULL,
PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Lote]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Lote](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ProductoId] [int] NOT NULL,
	[FechaIngreso] [datetime] NOT NULL,
	[FechaVencimiento] [datetime] NULL,
	[StockInicial] [decimal](10, 3) NOT NULL,
	[StockActual] [decimal](10, 3) NOT NULL,
	[Activo] [bit] NOT NULL,
PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MetodoPago]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MetodoPago](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [varchar](50) NOT NULL,
PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MovimientoCaja]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MovimientoCaja](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CajaId] [int] NOT NULL,
	[UsuarioId] [int] NOT NULL,
	[TipoMovimiento] [nvarchar](10) NOT NULL,
	[Importe] [decimal](18, 2) NOT NULL,
	[Motivo] [nvarchar](500) NOT NULL,
	[FechaHora] [datetime2](0) NOT NULL,
	[ClaveOperacion] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_MovimientoCaja] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_MovimientoCaja_ClaveOperacion] UNIQUE NONCLUSTERED
(
	[ClaveOperacion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PedidoProveedor]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PedidoProveedor](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ProveedorId] [int] NOT NULL,
	[UsuarioId] [int] NOT NULL,
	[FechaPedido] [datetime2](0) NOT NULL,
	[Estado] [nvarchar](30) NOT NULL,
 CONSTRAINT [PK_PedidoProveedor] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Producto]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Producto](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [varchar](100) NOT NULL,
	[CategoriaId] [int] NOT NULL,
	[PrecioVenta] [decimal](10, 2) NOT NULL,
	[StockMinimo] [decimal](10, 3) NOT NULL,
	[SeVendePorPeso] [bit] NOT NULL,
	[Descripcion] [varchar](255) NULL,
	[Activo] [bit] NULL,
	[StockActual] [decimal](10, 3) NOT NULL,
	[CodigoBarras] [varchar](50) NULL,
	[ControlaVencimiento] [bit] NOT NULL,
PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Proveedor]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Proveedor](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RazonSocial] [varchar](100) NOT NULL,
	[CUIT] [varchar](20) NOT NULL,
	[Telefono] [varchar](20) NULL,
	[Email] [varchar](100) NULL,
	[Direccion] [varchar](150) NULL,
	[ProvinciaId] [int] NULL,
	[LocalidadId] [int] NULL,
	[Activo] [bit] NULL,
PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED
(
	[CUIT] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProveedorProducto]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProveedorProducto](
	[ProveedorId] [int] NOT NULL,
	[ProductoId] [int] NOT NULL,
	[Activo] [bit] NOT NULL,
 CONSTRAINT [PK_ProveedorProducto] PRIMARY KEY CLUSTERED
(
	[ProveedorId] ASC,
	[ProductoId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Provincia]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Provincia](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [varchar](100) NOT NULL,
PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecepcionDetalle]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecepcionDetalle](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RecepcionId] [int] NOT NULL,
	[DetallePedidoProveedorId] [int] NOT NULL,
	[CantidadSolicitada] [decimal](10, 3) NOT NULL,
	[CantidadRecibida] [decimal](10, 3) NOT NULL,
	[CantidadFaltante] [decimal](10, 3) NOT NULL,
 CONSTRAINT [PK_RecepcionDetalle] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_RecepcionDetalle_Recepcion_DetallePedido] UNIQUE NONCLUSTERED
(
	[RecepcionId] ASC,
	[DetallePedidoProveedorId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecepcionLote]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecepcionLote](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RecepcionDetalleId] [int] NOT NULL,
	[Cantidad] [decimal](10, 3) NOT NULL,
	[TieneVencimiento] [bit] NOT NULL,
	[FechaVencimiento] [datetime2](0) NULL,
	[FechaIngreso] [datetime2](0) NOT NULL,
	[DepositoStockId] [int] NULL,
 CONSTRAINT [PK_RecepcionLote] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecepcionPedido]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecepcionPedido](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PedidoId] [int] NOT NULL,
	[UsuarioReceptorId] [int] NOT NULL,
	[FechaRecepcion] [datetime2](0) NOT NULL,
	[EstadoResultado] [nvarchar](30) NOT NULL,
 CONSTRAINT [PK_RecepcionPedido] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_RecepcionPedido_PedidoId] UNIQUE NONCLUSTERED
(
	[PedidoId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecepcionRemito]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecepcionRemito](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RemitoId] [int] NOT NULL,
	[ClaveOperacion] [uniqueidentifier] NOT NULL,
	[UsuarioReceptorId] [int] NOT NULL,
	[FechaRecepcion] [datetime2](0) NOT NULL,
	[EstadoResultado] [nvarchar](30) NOT NULL,
 CONSTRAINT [PK_RecepcionRemito] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_RecepcionRemito_Operacion] UNIQUE NONCLUSTERED
(
	[ClaveOperacion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_RecepcionRemito_Remito] UNIQUE NONCLUSTERED
(
	[RemitoId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_RecepcionRemito_RemitoId] UNIQUE NONCLUSTERED
(
	[Id] ASC,
	[RemitoId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecepcionRemitoLote]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecepcionRemitoLote](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RecepcionRemitoId] [int] NOT NULL,
	[RemitoId] [int] NOT NULL,
	[RemitoLoteId] [int] NOT NULL,
	[CantidadEnviada] [decimal](10, 3) NOT NULL,
	[CantidadRecibida] [decimal](10, 3) NOT NULL,
	[CantidadDiferencia]  AS (CONVERT([decimal](10,3),[CantidadEnviada]-[CantidadRecibida])) PERSISTED,
	[LoteLocalId] [int] NULL,
	[Observacion] [nvarchar](500) NULL,
 CONSTRAINT [PK_RecepcionRemitoLote] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_RecepcionRemitoLote_Enviado] UNIQUE NONCLUSTERED
(
	[RemitoLoteId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_RecepcionRemitoLote_Remito] UNIQUE NONCLUSTERED
(
	[Id] ASC,
	[RemitoId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RemitoDetalle]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RemitoDetalle](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RemitoId] [int] NOT NULL,
	[ProductoId] [int] NOT NULL,
	[CantidadEnviada] [decimal](10, 3) NOT NULL,
 CONSTRAINT [PK_RemitoDetalle] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_RemitoDetalle_Producto] UNIQUE NONCLUSTERED
(
	[RemitoId] ASC,
	[ProductoId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_RemitoDetalle_Remito] UNIQUE NONCLUSTERED
(
	[Id] ASC,
	[RemitoId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RemitoInterno]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RemitoInterno](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ClaveOperacion] [uniqueidentifier] NOT NULL,
	[Estado] [nvarchar](30) NOT NULL,
	[UsuarioCreadorId] [int] NOT NULL,
	[FechaCreacion] [datetime2](0) NOT NULL,
	[UsuarioEmisorId] [int] NULL,
	[FechaEmision] [datetime2](0) NULL,
	[UsuarioCanceladorId] [int] NULL,
	[FechaCancelacion] [datetime2](0) NULL,
	[MotivoCancelacion] [nvarchar](500) NULL,
 CONSTRAINT [PK_RemitoInterno] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_RemitoInterno_Operacion] UNIQUE NONCLUSTERED
(
	[ClaveOperacion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RemitoLote]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RemitoLote](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RemitoId] [int] NOT NULL,
	[RemitoDetalleId] [int] NOT NULL,
	[DepositoStockId] [int] NOT NULL,
	[CantidadEnviada] [decimal](10, 3) NOT NULL,
	[FechaIngresoOrigen] [datetime2](0) NULL,
	[FechaVencimiento] [datetime2](0) NULL,
 CONSTRAINT [PK_RemitoLote] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_RemitoLote_Cantidad] UNIQUE NONCLUSTERED
(
	[Id] ASC,
	[RemitoId] ASC,
	[CantidadEnviada] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_RemitoLote_Origen] UNIQUE NONCLUSTERED
(
	[RemitoId] ASC,
	[DepositoStockId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ResolucionDiferencia]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ResolucionDiferencia](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RemitoId] [int] NOT NULL,
	[ClaveOperacion] [uniqueidentifier] NOT NULL,
	[UsuarioResponsableId] [int] NOT NULL,
	[FechaResolucion] [datetime2](0) NOT NULL,
	[Tipo] [nvarchar](30) NOT NULL,
	[Estado] [nvarchar](20) NOT NULL,
	[Motivo] [nvarchar](500) NOT NULL,
 CONSTRAINT [PK_ResolucionDiferencia] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_ResolucionDiferencia_Operacion] UNIQUE NONCLUSTERED
(
	[ClaveOperacion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_ResolucionDiferencia_Remito] UNIQUE NONCLUSTERED
(
	[Id] ASC,
	[RemitoId] ASC,
	[Tipo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ResolucionDiferenciaDetalle]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ResolucionDiferenciaDetalle](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ResolucionId] [int] NOT NULL,
	[RemitoId] [int] NOT NULL,
	[Tipo] [nvarchar](30) NOT NULL,
	[RecepcionRemitoLoteId] [int] NOT NULL,
	[Cantidad] [decimal](10, 3) NOT NULL,
	[LoteLocalDestinoId] [int] NULL,
	[DepositoStockDestinoId] [int] NULL,
 CONSTRAINT [PK_ResolucionDiferenciaDetalle] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_ResolucionDetalle_Lote] UNIQUE NONCLUSTERED
(
	[ResolucionId] ASC,
	[RecepcionRemitoLoteId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Rol]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Rol](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [varchar](50) NOT NULL,
PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Usuario]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Usuario](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [varchar](100) NOT NULL,
	[Apellido] [varchar](100) NOT NULL,
	[DNI] [varchar](20) NOT NULL,
	[ClaveHash] [varchar](255) NOT NULL,
	[RolId] [int] NOT NULL,
	[ProvinciaId] [int] NOT NULL,
	[LocalidadId] [int] NOT NULL,
	[Activo] [bit] NOT NULL,
	[Email] [varchar](150) NULL,
	[Telefono] [varchar](20) NULL,
	[FechaNacimiento] [date] NULL,
	[Direccion] [varchar](150) NULL,
PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED
(
	[Email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED
(
	[DNI] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Venta]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Venta](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CajaId] [int] NOT NULL,
	[ClienteId] [int] NULL,
	[UsuarioId] [int] NOT NULL,
	[FechaHora] [datetime] NOT NULL,
	[Total] [decimal](10, 2) NOT NULL,
	[Estado] [varchar](30) NOT NULL,
PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[VentaAnulacion]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VentaAnulacion](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[VentaId] [int] NOT NULL,
	[UsuarioId] [int] NOT NULL,
	[Motivo] [nvarchar](255) NOT NULL,
	[Fecha] [datetime] NULL,
PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[VentaPago]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VentaPago](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[VentaId] [int] NOT NULL,
	[MetodoPagoId] [int] NOT NULL,
	[MontoCobrado] [decimal](10, 2) NOT NULL,
PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Index [IX_DepositoStock_Producto_Activo]    Script Date: 11/11/2026 08:03:21 ******/
CREATE NONCLUSTERED INDEX [IX_DepositoStock_Producto_Activo] ON [dbo].[DepositoStock]
(
	[ProductoId] ASC,
	[Activo] ASC,
	[FechaVencimiento] ASC,
	[Id] ASC
)
INCLUDE([StockActual]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_DetallePedidoProveedor_PedidoId]    Script Date: 11/11/2026 08:03:21 ******/
CREATE NONCLUSTERED INDEX [IX_DetallePedidoProveedor_PedidoId] ON [dbo].[DetallePedidoProveedor]
(
	[PedidoId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_DetallePedidoProveedor_ProductoId]    Script Date: 11/11/2026 08:03:21 ******/
CREATE NONCLUSTERED INDEX [IX_DetallePedidoProveedor_ProductoId] ON [dbo].[DetallePedidoProveedor]
(
	[ProductoId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_MovimientoCaja_Caja_Fecha]    Script Date: 11/11/2026 08:03:21 ******/
CREATE NONCLUSTERED INDEX [IX_MovimientoCaja_Caja_Fecha] ON [dbo].[MovimientoCaja]
(
	[CajaId] ASC,
	[FechaHora] ASC,
	[Id] ASC
)
INCLUDE([TipoMovimiento],[Importe]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UX_RecepcionLote_DepositoStockId]    Script Date: 11/11/2026 08:03:21 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_RecepcionLote_DepositoStockId] ON [dbo].[RecepcionLote]
(
	[DepositoStockId] ASC
)
WHERE ([DepositoStockId] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_RecepcionRemito_Fecha]    Script Date: 11/11/2026 08:03:21 ******/
CREATE NONCLUSTERED INDEX [IX_RecepcionRemito_Fecha] ON [dbo].[RecepcionRemito]
(
	[FechaRecepcion] ASC,
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UX_RecepcionRemitoLote_Local]    Script Date: 11/11/2026 08:03:21 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_RecepcionRemitoLote_Local] ON [dbo].[RecepcionRemitoLote]
(
	[LoteLocalId] ASC
)
WHERE ([LoteLocalId] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_RemitoDetalle_Producto]    Script Date: 11/11/2026 08:03:21 ******/
CREATE NONCLUSTERED INDEX [IX_RemitoDetalle_Producto] ON [dbo].[RemitoDetalle]
(
	[ProductoId] ASC,
	[RemitoId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_RemitoInterno_Pendientes]    Script Date: 11/11/2026 08:03:21 ******/
CREATE NONCLUSTERED INDEX [IX_RemitoInterno_Pendientes] ON [dbo].[RemitoInterno]
(
	[Estado] ASC,
	[FechaEmision] ASC,
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_RemitoLote_Origen]    Script Date: 11/11/2026 08:03:21 ******/
CREATE NONCLUSTERED INDEX [IX_RemitoLote_Origen] ON [dbo].[RemitoLote]
(
	[DepositoStockId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ResolucionDiferencia_Remito]    Script Date: 11/11/2026 08:03:21 ******/
CREATE NONCLUSTERED INDEX [IX_ResolucionDiferencia_Remito] ON [dbo].[ResolucionDiferencia]
(
	[RemitoId] ASC,
	[FechaResolucion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ResolucionDetalle_Pendiente]    Script Date: 11/11/2026 08:03:21 ******/
CREATE NONCLUSTERED INDEX [IX_ResolucionDetalle_Pendiente] ON [dbo].[ResolucionDiferenciaDetalle]
(
	[RecepcionRemitoLoteId] ASC
)
INCLUDE([Cantidad],[ResolucionId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UX_ResolucionDetalle_Deposito]    Script Date: 11/11/2026 08:03:21 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_ResolucionDetalle_Deposito] ON [dbo].[ResolucionDiferenciaDetalle]
(
	[DepositoStockDestinoId] ASC
)
WHERE ([DepositoStockDestinoId] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UX_ResolucionDetalle_Local]    Script Date: 11/11/2026 08:03:21 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_ResolucionDetalle_Local] ON [dbo].[ResolucionDiferenciaDetalle]
(
	[LoteLocalDestinoId] ASC
)
WHERE ([LoteLocalDestinoId] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[DepositoStock] ADD  CONSTRAINT [DF_DepositoStock_StockInicial]  DEFAULT ((0)) FOR [StockInicial]
GO
ALTER TABLE [dbo].[DepositoStock] ADD  CONSTRAINT [DF_DepositoStock_StockActual]  DEFAULT ((0)) FOR [StockActual]
GO
ALTER TABLE [dbo].[DepositoStock] ADD  CONSTRAINT [DF_DepositoStock_Activo]  DEFAULT ((1)) FOR [Activo]
GO
ALTER TABLE [dbo].[DepositoStockMinimo] ADD  CONSTRAINT [DF_DepositoStockMinimo_StockMinimo]  DEFAULT ((0)) FOR [StockMinimo]
GO
ALTER TABLE [dbo].[DetallePedidoProveedor] ADD  CONSTRAINT [DF_DetallePedidoProveedor_CantidadRecibida]  DEFAULT ((0)) FOR [CantidadRecibida]
GO
ALTER TABLE [dbo].[Lote] ADD  DEFAULT ((1)) FOR [Activo]
GO
ALTER TABLE [dbo].[MovimientoCaja] ADD  CONSTRAINT [DF_MovimientoCaja_FechaHora]  DEFAULT (sysdatetime()) FOR [FechaHora]
GO
ALTER TABLE [dbo].[PedidoProveedor] ADD  CONSTRAINT [DF_PedidoProveedor_FechaPedido]  DEFAULT (sysdatetime()) FOR [FechaPedido]
GO
ALTER TABLE [dbo].[PedidoProveedor] ADD  CONSTRAINT [DF_PedidoProveedor_Estado]  DEFAULT (N'Pendiente') FOR [Estado]
GO
ALTER TABLE [dbo].[Producto] ADD  DEFAULT ((1)) FOR [Activo]
GO
ALTER TABLE [dbo].[Producto] ADD  DEFAULT ((0)) FOR [StockActual]
GO
ALTER TABLE [dbo].[Producto] ADD  CONSTRAINT [DF_Producto_ControlaVencimiento]  DEFAULT ((1)) FOR [ControlaVencimiento]
GO
ALTER TABLE [dbo].[Proveedor] ADD  DEFAULT ((1)) FOR [Activo]
GO
ALTER TABLE [dbo].[ProveedorProducto] ADD  CONSTRAINT [DF_ProveedorProducto_Activo]  DEFAULT ((1)) FOR [Activo]
GO
ALTER TABLE [dbo].[RecepcionLote] ADD  CONSTRAINT [DF_RecepcionLote_FechaIngreso]  DEFAULT (sysdatetime()) FOR [FechaIngreso]
GO
ALTER TABLE [dbo].[RecepcionPedido] ADD  CONSTRAINT [DF_RecepcionPedido_FechaRecepcion]  DEFAULT (sysdatetime()) FOR [FechaRecepcion]
GO
ALTER TABLE [dbo].[RecepcionRemito] ADD  CONSTRAINT [DF_RecepcionRemito_Fecha]  DEFAULT (sysdatetime()) FOR [FechaRecepcion]
GO
ALTER TABLE [dbo].[RemitoInterno] ADD  CONSTRAINT [DF_RemitoInterno_Estado]  DEFAULT (N'Borrador') FOR [Estado]
GO
ALTER TABLE [dbo].[RemitoInterno] ADD  CONSTRAINT [DF_RemitoInterno_Fecha]  DEFAULT (sysdatetime()) FOR [FechaCreacion]
GO
ALTER TABLE [dbo].[ResolucionDiferencia] ADD  CONSTRAINT [DF_ResolucionDiferencia_Fecha]  DEFAULT (sysdatetime()) FOR [FechaResolucion]
GO
ALTER TABLE [dbo].[ResolucionDiferencia] ADD  CONSTRAINT [DF_ResolucionDiferencia_Estado]  DEFAULT (N'Aplicada') FOR [Estado]
GO
ALTER TABLE [dbo].[Usuario] ADD  DEFAULT ((1)) FOR [Activo]
GO
ALTER TABLE [dbo].[Venta] ADD  DEFAULT (getdate()) FOR [FechaHora]
GO
ALTER TABLE [dbo].[VentaAnulacion] ADD  DEFAULT (getdate()) FOR [Fecha]
GO
ALTER TABLE [dbo].[Caja]  WITH CHECK ADD  CONSTRAINT [FK_Caja_Usuario] FOREIGN KEY([UsuarioId])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[Caja] CHECK CONSTRAINT [FK_Caja_Usuario]
GO
ALTER TABLE [dbo].[Cliente]  WITH CHECK ADD  CONSTRAINT [FK_Cliente_Localidad] FOREIGN KEY([LocalidadId])
REFERENCES [dbo].[Localidad] ([Id])
GO
ALTER TABLE [dbo].[Cliente] CHECK CONSTRAINT [FK_Cliente_Localidad]
GO
ALTER TABLE [dbo].[Cliente]  WITH CHECK ADD  CONSTRAINT [FK_Cliente_Provincia] FOREIGN KEY([ProvinciaId])
REFERENCES [dbo].[Provincia] ([Id])
GO
ALTER TABLE [dbo].[Cliente] CHECK CONSTRAINT [FK_Cliente_Provincia]
GO
ALTER TABLE [dbo].[DepositoStock]  WITH CHECK ADD  CONSTRAINT [FK_DepositoStock_Producto] FOREIGN KEY([ProductoId])
REFERENCES [dbo].[Producto] ([Id])
GO
ALTER TABLE [dbo].[DepositoStock] CHECK CONSTRAINT [FK_DepositoStock_Producto]
GO
ALTER TABLE [dbo].[DepositoStockMinimo]  WITH CHECK ADD  CONSTRAINT [FK_DepositoStockMinimo_Producto] FOREIGN KEY([ProductoId])
REFERENCES [dbo].[Producto] ([Id])
GO
ALTER TABLE [dbo].[DepositoStockMinimo] CHECK CONSTRAINT [FK_DepositoStockMinimo_Producto]
GO
ALTER TABLE [dbo].[DetallePedidoProveedor]  WITH CHECK ADD  CONSTRAINT [FK_DetallePedidoProveedor_Pedido] FOREIGN KEY([PedidoId])
REFERENCES [dbo].[PedidoProveedor] ([Id])
GO
ALTER TABLE [dbo].[DetallePedidoProveedor] CHECK CONSTRAINT [FK_DetallePedidoProveedor_Pedido]
GO
ALTER TABLE [dbo].[DetallePedidoProveedor]  WITH CHECK ADD  CONSTRAINT [FK_DetallePedidoProveedor_Producto] FOREIGN KEY([ProductoId])
REFERENCES [dbo].[Producto] ([Id])
GO
ALTER TABLE [dbo].[DetallePedidoProveedor] CHECK CONSTRAINT [FK_DetallePedidoProveedor_Producto]
GO
ALTER TABLE [dbo].[DetalleVenta]  WITH CHECK ADD  CONSTRAINT [FK_DetalleVenta_Lote] FOREIGN KEY([LoteId])
REFERENCES [dbo].[Lote] ([Id])
GO
ALTER TABLE [dbo].[DetalleVenta] CHECK CONSTRAINT [FK_DetalleVenta_Lote]
GO
ALTER TABLE [dbo].[DetalleVenta]  WITH CHECK ADD  CONSTRAINT [FK_DetalleVenta_Producto] FOREIGN KEY([ProductoId])
REFERENCES [dbo].[Producto] ([Id])
GO
ALTER TABLE [dbo].[DetalleVenta] CHECK CONSTRAINT [FK_DetalleVenta_Producto]
GO
ALTER TABLE [dbo].[DetalleVenta]  WITH CHECK ADD  CONSTRAINT [FK_DetalleVenta_Venta] FOREIGN KEY([VentaId])
REFERENCES [dbo].[Venta] ([Id])
GO
ALTER TABLE [dbo].[DetalleVenta] CHECK CONSTRAINT [FK_DetalleVenta_Venta]
GO
ALTER TABLE [dbo].[Localidad]  WITH CHECK ADD  CONSTRAINT [FK_Localidad_Provincia] FOREIGN KEY([ProvinciaId])
REFERENCES [dbo].[Provincia] ([Id])
GO
ALTER TABLE [dbo].[Localidad] CHECK CONSTRAINT [FK_Localidad_Provincia]
GO
ALTER TABLE [dbo].[Lote]  WITH CHECK ADD  CONSTRAINT [FK_Lote_Producto] FOREIGN KEY([ProductoId])
REFERENCES [dbo].[Producto] ([Id])
GO
ALTER TABLE [dbo].[Lote] CHECK CONSTRAINT [FK_Lote_Producto]
GO
ALTER TABLE [dbo].[MovimientoCaja]  WITH CHECK ADD  CONSTRAINT [FK_MovimientoCaja_Caja] FOREIGN KEY([CajaId])
REFERENCES [dbo].[Caja] ([Id])
GO
ALTER TABLE [dbo].[MovimientoCaja] CHECK CONSTRAINT [FK_MovimientoCaja_Caja]
GO
ALTER TABLE [dbo].[MovimientoCaja]  WITH CHECK ADD  CONSTRAINT [FK_MovimientoCaja_Usuario] FOREIGN KEY([UsuarioId])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[MovimientoCaja] CHECK CONSTRAINT [FK_MovimientoCaja_Usuario]
GO
ALTER TABLE [dbo].[PedidoProveedor]  WITH CHECK ADD  CONSTRAINT [FK_PedidoProveedor_Proveedor] FOREIGN KEY([ProveedorId])
REFERENCES [dbo].[Proveedor] ([Id])
GO
ALTER TABLE [dbo].[PedidoProveedor] CHECK CONSTRAINT [FK_PedidoProveedor_Proveedor]
GO
ALTER TABLE [dbo].[PedidoProveedor]  WITH CHECK ADD  CONSTRAINT [FK_PedidoProveedor_Usuario] FOREIGN KEY([UsuarioId])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[PedidoProveedor] CHECK CONSTRAINT [FK_PedidoProveedor_Usuario]
GO
ALTER TABLE [dbo].[Producto]  WITH CHECK ADD  CONSTRAINT [FK_Producto_Categoria] FOREIGN KEY([CategoriaId])
REFERENCES [dbo].[Categoria] ([Id])
GO
ALTER TABLE [dbo].[Producto] CHECK CONSTRAINT [FK_Producto_Categoria]
GO
ALTER TABLE [dbo].[Proveedor]  WITH CHECK ADD  CONSTRAINT [FK_Proveedor_Localidad] FOREIGN KEY([LocalidadId])
REFERENCES [dbo].[Localidad] ([Id])
GO
ALTER TABLE [dbo].[Proveedor] CHECK CONSTRAINT [FK_Proveedor_Localidad]
GO
ALTER TABLE [dbo].[Proveedor]  WITH CHECK ADD  CONSTRAINT [FK_Proveedor_Provincia] FOREIGN KEY([ProvinciaId])
REFERENCES [dbo].[Provincia] ([Id])
GO
ALTER TABLE [dbo].[Proveedor] CHECK CONSTRAINT [FK_Proveedor_Provincia]
GO
ALTER TABLE [dbo].[ProveedorProducto]  WITH CHECK ADD  CONSTRAINT [FK_ProveedorProducto_Producto] FOREIGN KEY([ProductoId])
REFERENCES [dbo].[Producto] ([Id])
GO
ALTER TABLE [dbo].[ProveedorProducto] CHECK CONSTRAINT [FK_ProveedorProducto_Producto]
GO
ALTER TABLE [dbo].[ProveedorProducto]  WITH CHECK ADD  CONSTRAINT [FK_ProveedorProducto_Proveedor] FOREIGN KEY([ProveedorId])
REFERENCES [dbo].[Proveedor] ([Id])
GO
ALTER TABLE [dbo].[ProveedorProducto] CHECK CONSTRAINT [FK_ProveedorProducto_Proveedor]
GO
ALTER TABLE [dbo].[RecepcionDetalle]  WITH CHECK ADD  CONSTRAINT [FK_RecepcionDetalle_DetallePedido] FOREIGN KEY([DetallePedidoProveedorId])
REFERENCES [dbo].[DetallePedidoProveedor] ([Id])
GO
ALTER TABLE [dbo].[RecepcionDetalle] CHECK CONSTRAINT [FK_RecepcionDetalle_DetallePedido]
GO
ALTER TABLE [dbo].[RecepcionDetalle]  WITH CHECK ADD  CONSTRAINT [FK_RecepcionDetalle_RecepcionPedido] FOREIGN KEY([RecepcionId])
REFERENCES [dbo].[RecepcionPedido] ([Id])
GO
ALTER TABLE [dbo].[RecepcionDetalle] CHECK CONSTRAINT [FK_RecepcionDetalle_RecepcionPedido]
GO
ALTER TABLE [dbo].[RecepcionLote]  WITH CHECK ADD  CONSTRAINT [FK_RecepcionLote_DepositoStock] FOREIGN KEY([DepositoStockId])
REFERENCES [dbo].[DepositoStock] ([Id])
GO
ALTER TABLE [dbo].[RecepcionLote] CHECK CONSTRAINT [FK_RecepcionLote_DepositoStock]
GO
ALTER TABLE [dbo].[RecepcionLote]  WITH CHECK ADD  CONSTRAINT [FK_RecepcionLote_RecepcionDetalle] FOREIGN KEY([RecepcionDetalleId])
REFERENCES [dbo].[RecepcionDetalle] ([Id])
GO
ALTER TABLE [dbo].[RecepcionLote] CHECK CONSTRAINT [FK_RecepcionLote_RecepcionDetalle]
GO
ALTER TABLE [dbo].[RecepcionPedido]  WITH CHECK ADD  CONSTRAINT [FK_RecepcionPedido_PedidoProveedor] FOREIGN KEY([PedidoId])
REFERENCES [dbo].[PedidoProveedor] ([Id])
GO
ALTER TABLE [dbo].[RecepcionPedido] CHECK CONSTRAINT [FK_RecepcionPedido_PedidoProveedor]
GO
ALTER TABLE [dbo].[RecepcionPedido]  WITH CHECK ADD  CONSTRAINT [FK_RecepcionPedido_Usuario] FOREIGN KEY([UsuarioReceptorId])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[RecepcionPedido] CHECK CONSTRAINT [FK_RecepcionPedido_Usuario]
GO
ALTER TABLE [dbo].[RecepcionRemito]  WITH CHECK ADD FOREIGN KEY([RemitoId])
REFERENCES [dbo].[RemitoInterno] ([Id])
GO
ALTER TABLE [dbo].[RecepcionRemito]  WITH CHECK ADD FOREIGN KEY([UsuarioReceptorId])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[RecepcionRemitoLote]  WITH CHECK ADD FOREIGN KEY([LoteLocalId])
REFERENCES [dbo].[Lote] ([Id])
GO
ALTER TABLE [dbo].[RecepcionRemitoLote]  WITH CHECK ADD  CONSTRAINT [FK_RecepcionRemitoLote_Enviado] FOREIGN KEY([RemitoLoteId], [RemitoId], [CantidadEnviada])
REFERENCES [dbo].[RemitoLote] ([Id], [RemitoId], [CantidadEnviada])
GO
ALTER TABLE [dbo].[RecepcionRemitoLote] CHECK CONSTRAINT [FK_RecepcionRemitoLote_Enviado]
GO
ALTER TABLE [dbo].[RecepcionRemitoLote]  WITH CHECK ADD  CONSTRAINT [FK_RecepcionRemitoLote_Recepcion] FOREIGN KEY([RecepcionRemitoId], [RemitoId])
REFERENCES [dbo].[RecepcionRemito] ([Id], [RemitoId])
GO
ALTER TABLE [dbo].[RecepcionRemitoLote] CHECK CONSTRAINT [FK_RecepcionRemitoLote_Recepcion]
GO
ALTER TABLE [dbo].[RemitoDetalle]  WITH CHECK ADD FOREIGN KEY([ProductoId])
REFERENCES [dbo].[Producto] ([Id])
GO
ALTER TABLE [dbo].[RemitoDetalle]  WITH CHECK ADD FOREIGN KEY([RemitoId])
REFERENCES [dbo].[RemitoInterno] ([Id])
GO
ALTER TABLE [dbo].[RemitoInterno]  WITH CHECK ADD FOREIGN KEY([UsuarioCreadorId])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[RemitoInterno]  WITH CHECK ADD FOREIGN KEY([UsuarioEmisorId])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[RemitoInterno]  WITH CHECK ADD FOREIGN KEY([UsuarioCanceladorId])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[RemitoLote]  WITH CHECK ADD FOREIGN KEY([DepositoStockId])
REFERENCES [dbo].[DepositoStock] ([Id])
GO
ALTER TABLE [dbo].[RemitoLote]  WITH CHECK ADD  CONSTRAINT [FK_RemitoLote_Detalle] FOREIGN KEY([RemitoDetalleId], [RemitoId])
REFERENCES [dbo].[RemitoDetalle] ([Id], [RemitoId])
GO
ALTER TABLE [dbo].[RemitoLote] CHECK CONSTRAINT [FK_RemitoLote_Detalle]
GO
ALTER TABLE [dbo].[ResolucionDiferencia]  WITH CHECK ADD FOREIGN KEY([RemitoId])
REFERENCES [dbo].[RemitoInterno] ([Id])
GO
ALTER TABLE [dbo].[ResolucionDiferencia]  WITH CHECK ADD FOREIGN KEY([UsuarioResponsableId])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[ResolucionDiferenciaDetalle]  WITH CHECK ADD FOREIGN KEY([DepositoStockDestinoId])
REFERENCES [dbo].[DepositoStock] ([Id])
GO
ALTER TABLE [dbo].[ResolucionDiferenciaDetalle]  WITH CHECK ADD FOREIGN KEY([LoteLocalDestinoId])
REFERENCES [dbo].[Lote] ([Id])
GO
ALTER TABLE [dbo].[ResolucionDiferenciaDetalle]  WITH CHECK ADD  CONSTRAINT [FK_ResolucionDetalle_Cabecera] FOREIGN KEY([ResolucionId], [RemitoId], [Tipo])
REFERENCES [dbo].[ResolucionDiferencia] ([Id], [RemitoId], [Tipo])
GO
ALTER TABLE [dbo].[ResolucionDiferenciaDetalle] CHECK CONSTRAINT [FK_ResolucionDetalle_Cabecera]
GO
ALTER TABLE [dbo].[ResolucionDiferenciaDetalle]  WITH CHECK ADD  CONSTRAINT [FK_ResolucionDetalle_Diferencia] FOREIGN KEY([RecepcionRemitoLoteId], [RemitoId])
REFERENCES [dbo].[RecepcionRemitoLote] ([Id], [RemitoId])
GO
ALTER TABLE [dbo].[ResolucionDiferenciaDetalle] CHECK CONSTRAINT [FK_ResolucionDetalle_Diferencia]
GO
ALTER TABLE [dbo].[Usuario]  WITH CHECK ADD  CONSTRAINT [FK_Usuario_Localidad] FOREIGN KEY([LocalidadId])
REFERENCES [dbo].[Localidad] ([Id])
GO
ALTER TABLE [dbo].[Usuario] CHECK CONSTRAINT [FK_Usuario_Localidad]
GO
ALTER TABLE [dbo].[Usuario]  WITH CHECK ADD  CONSTRAINT [FK_Usuario_Provincia] FOREIGN KEY([ProvinciaId])
REFERENCES [dbo].[Provincia] ([Id])
GO
ALTER TABLE [dbo].[Usuario] CHECK CONSTRAINT [FK_Usuario_Provincia]
GO
ALTER TABLE [dbo].[Usuario]  WITH CHECK ADD  CONSTRAINT [FK_Usuario_Rol] FOREIGN KEY([RolId])
REFERENCES [dbo].[Rol] ([Id])
GO
ALTER TABLE [dbo].[Usuario] CHECK CONSTRAINT [FK_Usuario_Rol]
GO
ALTER TABLE [dbo].[Venta]  WITH CHECK ADD  CONSTRAINT [FK_Venta_Caja] FOREIGN KEY([CajaId])
REFERENCES [dbo].[Caja] ([Id])
GO
ALTER TABLE [dbo].[Venta] CHECK CONSTRAINT [FK_Venta_Caja]
GO
ALTER TABLE [dbo].[Venta]  WITH CHECK ADD  CONSTRAINT [FK_Venta_Cliente] FOREIGN KEY([ClienteId])
REFERENCES [dbo].[Cliente] ([Id])
GO
ALTER TABLE [dbo].[Venta] CHECK CONSTRAINT [FK_Venta_Cliente]
GO
ALTER TABLE [dbo].[Venta]  WITH CHECK ADD  CONSTRAINT [FK_Venta_Usuario] FOREIGN KEY([UsuarioId])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[Venta] CHECK CONSTRAINT [FK_Venta_Usuario]
GO
ALTER TABLE [dbo].[VentaAnulacion]  WITH CHECK ADD FOREIGN KEY([UsuarioId])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[VentaAnulacion]  WITH CHECK ADD FOREIGN KEY([VentaId])
REFERENCES [dbo].[Venta] ([Id])
GO
ALTER TABLE [dbo].[VentaPago]  WITH CHECK ADD  CONSTRAINT [FK_VentaPago_MetodoPago] FOREIGN KEY([MetodoPagoId])
REFERENCES [dbo].[MetodoPago] ([Id])
GO
ALTER TABLE [dbo].[VentaPago] CHECK CONSTRAINT [FK_VentaPago_MetodoPago]
GO
ALTER TABLE [dbo].[VentaPago]  WITH CHECK ADD  CONSTRAINT [FK_VentaPago_Venta] FOREIGN KEY([VentaId])
REFERENCES [dbo].[Venta] ([Id])
GO
ALTER TABLE [dbo].[VentaPago] CHECK CONSTRAINT [FK_VentaPago_Venta]
GO
ALTER TABLE [dbo].[DepositoStock]  WITH CHECK ADD  CONSTRAINT [CK_DepositoStock_StockActual] CHECK  (([StockActual]>=(0)))
GO
ALTER TABLE [dbo].[DepositoStock] CHECK CONSTRAINT [CK_DepositoStock_StockActual]
GO
ALTER TABLE [dbo].[DepositoStock]  WITH CHECK ADD  CONSTRAINT [CK_DepositoStock_StockInicial] CHECK  (([StockInicial]>=(0)))
GO
ALTER TABLE [dbo].[DepositoStock] CHECK CONSTRAINT [CK_DepositoStock_StockInicial]
GO
ALTER TABLE [dbo].[DepositoStockMinimo]  WITH CHECK ADD  CONSTRAINT [CK_DepositoStockMinimo_StockMinimo] CHECK  (([StockMinimo]>=(0)))
GO
ALTER TABLE [dbo].[DepositoStockMinimo] CHECK CONSTRAINT [CK_DepositoStockMinimo_StockMinimo]
GO
ALTER TABLE [dbo].[DetallePedidoProveedor]  WITH CHECK ADD  CONSTRAINT [CK_DetallePedidoProveedor_Cantidad] CHECK  (([Cantidad]>(0)))
GO
ALTER TABLE [dbo].[DetallePedidoProveedor] CHECK CONSTRAINT [CK_DetallePedidoProveedor_Cantidad]
GO
ALTER TABLE [dbo].[DetallePedidoProveedor]  WITH CHECK ADD  CONSTRAINT [CK_DetallePedidoProveedor_CantidadRecibida] CHECK  (([CantidadRecibida]>=(0) AND [CantidadRecibida]<=[Cantidad]))
GO
ALTER TABLE [dbo].[DetallePedidoProveedor] CHECK CONSTRAINT [CK_DetallePedidoProveedor_CantidadRecibida]
GO
ALTER TABLE [dbo].[DetallePedidoProveedor]  WITH CHECK ADD  CONSTRAINT [CK_DetallePedidoProveedor_PrecioCosto] CHECK  (([PrecioCosto]>=(0)))
GO
ALTER TABLE [dbo].[DetallePedidoProveedor] CHECK CONSTRAINT [CK_DetallePedidoProveedor_PrecioCosto]
GO
ALTER TABLE [dbo].[MovimientoCaja]  WITH CHECK ADD  CONSTRAINT [CK_MovimientoCaja_Clave] CHECK  (([ClaveOperacion]<>'00000000-0000-0000-0000-000000000000'))
GO
ALTER TABLE [dbo].[MovimientoCaja] CHECK CONSTRAINT [CK_MovimientoCaja_Clave]
GO
ALTER TABLE [dbo].[MovimientoCaja]  WITH CHECK ADD  CONSTRAINT [CK_MovimientoCaja_Importe] CHECK  (([Importe]>(0)))
GO
ALTER TABLE [dbo].[MovimientoCaja] CHECK CONSTRAINT [CK_MovimientoCaja_Importe]
GO
ALTER TABLE [dbo].[MovimientoCaja]  WITH CHECK ADD  CONSTRAINT [CK_MovimientoCaja_Motivo] CHECK  ((len(ltrim(rtrim([Motivo])))>(0)))
GO
ALTER TABLE [dbo].[MovimientoCaja] CHECK CONSTRAINT [CK_MovimientoCaja_Motivo]
GO
ALTER TABLE [dbo].[MovimientoCaja]  WITH CHECK ADD  CONSTRAINT [CK_MovimientoCaja_Tipo] CHECK  (([TipoMovimiento]=N'Retiro' OR [TipoMovimiento]=N'Ingreso'))
GO
ALTER TABLE [dbo].[MovimientoCaja] CHECK CONSTRAINT [CK_MovimientoCaja_Tipo]
GO
ALTER TABLE [dbo].[PedidoProveedor]  WITH CHECK ADD  CONSTRAINT [CK_PedidoProveedor_Estado] CHECK  (([Estado]=N'Cerrado con faltantes' OR [Estado]=N'Completado' OR [Estado]=N'Parcialmente Recibido' OR [Estado]=N'Pendiente'))
GO
ALTER TABLE [dbo].[PedidoProveedor] CHECK CONSTRAINT [CK_PedidoProveedor_Estado]
GO
ALTER TABLE [dbo].[RecepcionDetalle]  WITH CHECK ADD  CONSTRAINT [CK_RecepcionDetalle_Cantidades] CHECK  (([CantidadSolicitada]>=(0) AND [CantidadRecibida]>=(0) AND [CantidadFaltante]>=(0) AND [CantidadRecibida]<=[CantidadSolicitada] AND [CantidadFaltante]=([CantidadSolicitada]-[CantidadRecibida])))
GO
ALTER TABLE [dbo].[RecepcionDetalle] CHECK CONSTRAINT [CK_RecepcionDetalle_Cantidades]
GO
ALTER TABLE [dbo].[RecepcionLote]  WITH CHECK ADD  CONSTRAINT [CK_RecepcionLote_Cantidad] CHECK  (([Cantidad]>(0)))
GO
ALTER TABLE [dbo].[RecepcionLote] CHECK CONSTRAINT [CK_RecepcionLote_Cantidad]
GO
ALTER TABLE [dbo].[RecepcionLote]  WITH CHECK ADD  CONSTRAINT [CK_RecepcionLote_Vencimiento] CHECK  (([TieneVencimiento]=(1) AND [FechaVencimiento] IS NOT NULL OR [TieneVencimiento]=(0) AND [FechaVencimiento] IS NULL))
GO
ALTER TABLE [dbo].[RecepcionLote] CHECK CONSTRAINT [CK_RecepcionLote_Vencimiento]
GO
ALTER TABLE [dbo].[RecepcionPedido]  WITH CHECK ADD  CONSTRAINT [CK_RecepcionPedido_EstadoResultado] CHECK  (([EstadoResultado]=N'Cerrado con faltantes' OR [EstadoResultado]=N'Completado'))
GO
ALTER TABLE [dbo].[RecepcionPedido] CHECK CONSTRAINT [CK_RecepcionPedido_EstadoResultado]
GO
ALTER TABLE [dbo].[RecepcionRemito]  WITH CHECK ADD  CONSTRAINT [CK_RecepcionRemito_Estado] CHECK  (([EstadoResultado]=N'Con diferencias' OR [EstadoResultado]=N'Recibido'))
GO
ALTER TABLE [dbo].[RecepcionRemito] CHECK CONSTRAINT [CK_RecepcionRemito_Estado]
GO
ALTER TABLE [dbo].[RecepcionRemito]  WITH CHECK ADD  CONSTRAINT [CK_RecepcionRemito_Operacion] CHECK  (([ClaveOperacion]<>'00000000-0000-0000-0000-000000000000'))
GO
ALTER TABLE [dbo].[RecepcionRemito] CHECK CONSTRAINT [CK_RecepcionRemito_Operacion]
GO
ALTER TABLE [dbo].[RecepcionRemitoLote]  WITH CHECK ADD  CONSTRAINT [CK_RecepcionRemitoLote_Cantidad] CHECK  (([CantidadEnviada]>(0) AND [CantidadRecibida]>=(0) AND [CantidadRecibida]<=[CantidadEnviada]))
GO
ALTER TABLE [dbo].[RecepcionRemitoLote] CHECK CONSTRAINT [CK_RecepcionRemitoLote_Cantidad]
GO
ALTER TABLE [dbo].[RecepcionRemitoLote]  WITH CHECK ADD  CONSTRAINT [CK_RecepcionRemitoLote_Destino] CHECK  (([CantidadRecibida]=(0) AND [LoteLocalId] IS NULL OR [CantidadRecibida]>(0) AND [LoteLocalId] IS NOT NULL))
GO
ALTER TABLE [dbo].[RecepcionRemitoLote] CHECK CONSTRAINT [CK_RecepcionRemitoLote_Destino]
GO
ALTER TABLE [dbo].[RemitoDetalle]  WITH CHECK ADD  CONSTRAINT [CK_RemitoDetalle_Cantidad] CHECK  (([CantidadEnviada]>(0)))
GO
ALTER TABLE [dbo].[RemitoDetalle] CHECK CONSTRAINT [CK_RemitoDetalle_Cantidad]
GO
ALTER TABLE [dbo].[RemitoInterno]  WITH CHECK ADD  CONSTRAINT [CK_RemitoInterno_Ciclo] CHECK  (([Estado]=N'Borrador' AND [UsuarioEmisorId] IS NULL AND [FechaEmision] IS NULL AND [UsuarioCanceladorId] IS NULL AND [FechaCancelacion] IS NULL AND [MotivoCancelacion] IS NULL OR [Estado]=N'Cancelado' AND [UsuarioEmisorId] IS NULL AND [FechaEmision] IS NULL AND [UsuarioCanceladorId] IS NOT NULL AND [FechaCancelacion] IS NOT NULL AND [MotivoCancelacion] IS NOT NULL AND len(ltrim(rtrim([MotivoCancelacion])))>(0) OR ([Estado]=N'Cerrado' OR [Estado]=N'Con diferencias' OR [Estado]=N'Recibido' OR [Estado]=N'En tránsito') AND [UsuarioEmisorId] IS NOT NULL AND [FechaEmision] IS NOT NULL AND [UsuarioCanceladorId] IS NULL AND [FechaCancelacion] IS NULL AND [MotivoCancelacion] IS NULL))
GO
ALTER TABLE [dbo].[RemitoInterno] CHECK CONSTRAINT [CK_RemitoInterno_Ciclo]
GO
ALTER TABLE [dbo].[RemitoInterno]  WITH CHECK ADD  CONSTRAINT [CK_RemitoInterno_Estado] CHECK  (([Estado]=N'Cerrado' OR [Estado]=N'Con diferencias' OR [Estado]=N'Recibido' OR [Estado]=N'En tránsito' OR [Estado]=N'Cancelado' OR [Estado]=N'Borrador'))
GO
ALTER TABLE [dbo].[RemitoInterno] CHECK CONSTRAINT [CK_RemitoInterno_Estado]
GO
ALTER TABLE [dbo].[RemitoInterno]  WITH CHECK ADD  CONSTRAINT [CK_RemitoInterno_Fechas] CHECK  ((([FechaEmision] IS NULL OR [FechaEmision]>=[FechaCreacion]) AND ([FechaCancelacion] IS NULL OR [FechaCancelacion]>=[FechaCreacion])))
GO
ALTER TABLE [dbo].[RemitoInterno] CHECK CONSTRAINT [CK_RemitoInterno_Fechas]
GO
ALTER TABLE [dbo].[RemitoInterno]  WITH CHECK ADD  CONSTRAINT [CK_RemitoInterno_Operacion] CHECK  (([ClaveOperacion]<>'00000000-0000-0000-0000-000000000000'))
GO
ALTER TABLE [dbo].[RemitoInterno] CHECK CONSTRAINT [CK_RemitoInterno_Operacion]
GO
ALTER TABLE [dbo].[RemitoLote]  WITH CHECK ADD  CONSTRAINT [CK_RemitoLote_Cantidad] CHECK  (([CantidadEnviada]>(0)))
GO
ALTER TABLE [dbo].[RemitoLote] CHECK CONSTRAINT [CK_RemitoLote_Cantidad]
GO
ALTER TABLE [dbo].[ResolucionDiferencia]  WITH CHECK ADD  CONSTRAINT [CK_ResolucionDiferencia_Estado] CHECK  (([Estado]=N'Aplicada'))
GO
ALTER TABLE [dbo].[ResolucionDiferencia] CHECK CONSTRAINT [CK_ResolucionDiferencia_Estado]
GO
ALTER TABLE [dbo].[ResolucionDiferencia]  WITH CHECK ADD  CONSTRAINT [CK_ResolucionDiferencia_Motivo] CHECK  ((len(ltrim(rtrim([Motivo])))>(0)))
GO
ALTER TABLE [dbo].[ResolucionDiferencia] CHECK CONSTRAINT [CK_ResolucionDiferencia_Motivo]
GO
ALTER TABLE [dbo].[ResolucionDiferencia]  WITH CHECK ADD  CONSTRAINT [CK_ResolucionDiferencia_Operacion] CHECK  (([ClaveOperacion]<>'00000000-0000-0000-0000-000000000000'))
GO
ALTER TABLE [dbo].[ResolucionDiferencia] CHECK CONSTRAINT [CK_ResolucionDiferencia_Operacion]
GO
ALTER TABLE [dbo].[ResolucionDiferencia]  WITH CHECK ADD  CONSTRAINT [CK_ResolucionDiferencia_Tipo] CHECK  (([Tipo]=N'Pérdida documentada' OR [Tipo]=N'Devolución depósito' OR [Tipo]=N'Recepción posterior'))
GO
ALTER TABLE [dbo].[ResolucionDiferencia] CHECK CONSTRAINT [CK_ResolucionDiferencia_Tipo]
GO
ALTER TABLE [dbo].[ResolucionDiferenciaDetalle]  WITH CHECK ADD  CONSTRAINT [CK_ResolucionDetalle_Cantidad] CHECK  (([Cantidad]>(0)))
GO
ALTER TABLE [dbo].[ResolucionDiferenciaDetalle] CHECK CONSTRAINT [CK_ResolucionDetalle_Cantidad]
GO
ALTER TABLE [dbo].[ResolucionDiferenciaDetalle]  WITH CHECK ADD  CONSTRAINT [CK_ResolucionDetalle_Destino] CHECK  (([Tipo]=N'Recepción posterior' AND [LoteLocalDestinoId] IS NOT NULL AND [DepositoStockDestinoId] IS NULL OR [Tipo]=N'Devolución depósito' AND [LoteLocalDestinoId] IS NULL AND [DepositoStockDestinoId] IS NOT NULL OR [Tipo]=N'Pérdida documentada' AND [LoteLocalDestinoId] IS NULL AND [DepositoStockDestinoId] IS NULL))
GO
ALTER TABLE [dbo].[ResolucionDiferenciaDetalle] CHECK CONSTRAINT [CK_ResolucionDetalle_Destino]
GO
/****** Object:  StoredProcedure [dbo].[sp_AbrirCaja]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[sp_AbrirCaja]
    @UsuarioId INT,
    @SaldoInicial DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @BloqueoUsuario int;
        DECLARE @Recurso nvarchar(255) = N'PacuIbera.Caja.Abrir.Usuario.' + CONVERT(nvarchar(20), @UsuarioId);

        EXEC @BloqueoUsuario = sys.sp_getapplock
            @Resource = @Recurso,
            @LockMode = N'Exclusive',
            @LockOwner = N'Transaction',
            @LockTimeout = 15000,
            @DbPrincipal = N'public';

        IF @BloqueoUsuario < 0
            THROW 52611, 'No se pudo asegurar la apertura de caja. Intente nuevamente.', 1;

        IF EXISTS
        (
            SELECT 1
            FROM dbo.Caja WITH (UPDLOCK, HOLDLOCK)
            WHERE UsuarioId = @UsuarioId
              AND Estado = 'Abierta'
        )
            THROW 52612, 'Este vendedor ya tiene una caja abierta. Continue con esa caja antes de abrir otra.', 1;

        INSERT INTO dbo.Caja (UsuarioId, FechaHoraApertura, SaldoInicial, Estado)
        VALUES (@UsuarioId, GETDATE(), @SaldoInicial, 'Abierta');

        COMMIT TRANSACTION;

        SELECT SCOPE_IDENTITY();
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_ActualizarClienteCRM]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 2. Agregamos ProvinciaId y LocalidadId para poder editarlos
CREATE PROCEDURE [dbo].[sp_ActualizarClienteCRM]
    @Id INT,
    @Nombre NVARCHAR(100),
    @Apellido NVARCHAR(100),
    @Telefono NVARCHAR(50),
    @Direccion NVARCHAR(200),
    @ProvinciaId INT,
    @LocalidadId INT
AS BEGIN
    UPDATE Cliente
    SET Nombre = @Nombre,
        Apellido = @Apellido,
        Telefono = @Telefono,
        Direccion = @Direccion,
        ProvinciaId = @ProvinciaId,
        LocalidadId = @LocalidadId
    WHERE Id = @Id;
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_ActualizarStockMinimoDeposito]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[sp_ActualizarStockMinimoDeposito]
    @ProductoId int,
    @StockMinimo decimal(10,3)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    DECLARE @TransaccionesEntrada int = @@TRANCOUNT;
    DECLARE @PorPeso bit;
    BEGIN TRY
        IF @TransaccionesEntrada = 0 BEGIN TRANSACTION;
        ELSE SAVE TRANSACTION StockMinimoSeguro;

        IF @StockMinimo IS NULL OR @StockMinimo < 0 OR @StockMinimo > 9999999.999
            THROW 51130, 'El minimo debe estar entre 0 y 9999999.999.', 1;

        SELECT @PorPeso = SeVendePorPeso FROM dbo.Producto WITH (HOLDLOCK)
        WHERE Id = @ProductoId;
        IF @PorPeso IS NULL
            THROW 51131, 'El producto no existe o no tiene definida su unidad de venta.', 1;
        IF @PorPeso = 0 AND @StockMinimo <> FLOOR(@StockMinimo)
            THROW 51510, 'Los productos por unidad requieren un minimo entero.', 1;

        IF EXISTS (SELECT 1 FROM dbo.DepositoStockMinimo WITH (UPDLOCK, HOLDLOCK)
                   WHERE ProductoId = @ProductoId)
            UPDATE dbo.DepositoStockMinimo SET StockMinimo = @StockMinimo WHERE ProductoId = @ProductoId;
        ELSE
            INSERT INTO dbo.DepositoStockMinimo (ProductoId, StockMinimo) VALUES (@ProductoId, @StockMinimo);

        IF @TransaccionesEntrada = 0 COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @TransaccionesEntrada = 0
        BEGIN
            IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        END
        ELSE IF XACT_STATE() = 1 ROLLBACK TRANSACTION StockMinimoSeguro;
        THROW;
    END CATCH;
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_ActualizarUsuario]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[sp_ActualizarUsuario]
    @Id INT, @Nombre VARCHAR(100), @Apellido VARCHAR(100), @DNI VARCHAR(20),
    @Telefono VARCHAR(20), @Email VARCHAR(150), @Direccion VARCHAR(150),
    @RolId INT, @Activo BIT, @ProvinciaId INT, @LocalidadId INT
AS
BEGIN
    UPDATE Usuario
    SET Nombre=@Nombre, Apellido=@Apellido, DNI=@DNI, Telefono=@Telefono, Email=@Email,
        Direccion=@Direccion, RolId=@RolId, Activo=@Activo, ProvinciaId=@ProvinciaId, LocalidadId=@LocalidadId
    WHERE Id=@Id;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_AgregarItemPedidoProveedor]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[sp_AgregarItemPedidoProveedor]
    @PedidoId int,
    @ProductoId int,
    @Cantidad decimal(10,3),
    @PrecioCosto decimal(18,2)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    -- Participar de GuardarPedidoAtomico sin confirmar su transaccion externa.
    DECLARE @TransaccionesEntrada int = @@TRANCOUNT;
    DECLARE @Estado nvarchar(30);

    BEGIN TRY
        IF @TransaccionesEntrada = 0
            BEGIN TRANSACTION;
        ELSE
            SAVE TRANSACTION AgregarItemPedidoSeguro;

        IF @Cantidad IS NULL OR @Cantidad <= 0
            THROW 51110, 'La cantidad del pedido debe ser mayor que cero.', 1;

        IF @PrecioCosto IS NULL OR @PrecioCosto < 0
            THROW 51111, 'El precio de costo no puede ser negativo.', 1;

        -- Orden compartido con la confirmacion: cabecera -> historial -> detalle.
        -- Mantener estos bloqueos hasta el fin de la transaccion externa, si existe.
        SELECT @Estado = Estado
        FROM dbo.PedidoProveedor WITH (UPDLOCK, HOLDLOCK)
        WHERE Id = @PedidoId;

        IF @Estado IS NULL
            THROW 51112, 'El pedido indicado no existe.', 1;

        IF @Estado <> N'Pendiente'
            THROW 51113, 'Solo se pueden agregar items a un pedido pendiente.', 1;

        IF EXISTS
        (
            SELECT 1
            FROM dbo.RecepcionPedido WITH (UPDLOCK, HOLDLOCK)
            WHERE PedidoId = @PedidoId
        )
            THROW 51411, 'No se pueden agregar items a un pedido con recepcion definitiva.', 1;

        IF NOT EXISTS (SELECT 1 FROM dbo.Producto WHERE Id = @ProductoId)
            THROW 51114, 'El producto indicado no existe.', 1;

        INSERT INTO dbo.DetallePedidoProveedor
            (PedidoId, ProductoId, Cantidad, CantidadRecibida, PrecioCosto)
        VALUES
            (@PedidoId, @ProductoId, @Cantidad, 0, @PrecioCosto);

        IF @TransaccionesEntrada = 0
            COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @TransaccionesEntrada = 0
        BEGIN
            IF XACT_STATE() <> 0
                ROLLBACK TRANSACTION;
        END
        ELSE IF XACT_STATE() = 1
            ROLLBACK TRANSACTION AgregarItemPedidoSeguro;
        -- Si XACT_STATE() = -1, el llamador debe revertir la transaccion externa.
        -- GuardarPedidoAtomico ya realiza ese rollback en su catch.
        THROW;
    END CATCH;
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_AnalisisCurvaVentas]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 1. Curva de Ventas del Mes (Para el gráfico de líneas)
CREATE   PROCEDURE [dbo].[sp_AnalisisCurvaVentas]
    @FechaDesde DATE,
    @FechaHasta DATE
AS BEGIN
    SELECT
        CAST(FechaHora AS DATE) AS Dia,
        SUM(Total) AS Recaudacion
    FROM Venta
    WHERE Estado != 'Anulada'
      AND CAST(FechaHora AS DATE) >= @FechaDesde
      AND CAST(FechaHora AS DATE) <= @FechaHasta
    GROUP BY CAST(FechaHora AS DATE)
    ORDER BY Dia ASC;
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_AnalisisPerdidaVencimientos]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Pérdida Económica por Vencimientos (Merma)
CREATE   PROCEDURE [dbo].[sp_AnalisisPerdidaVencimientos]
AS BEGIN
    SELECT
        P.Nombre AS Producto,
        L.Id AS Lote,
        L.FechaVencimiento,
        L.StockActual AS [Unidades Tiradas],
        (L.StockActual * P.PrecioVenta) AS [Dinero Perdido]
    FROM Lote L
    INNER JOIN Producto P ON L.ProductoId = P.Id
    WHERE L.FechaVencimiento < GETDATE() AND L.StockActual > 0
    ORDER BY [Dinero Perdido] DESC;
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_AnularVenta]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 2. Procedimiento BLINDADO para Anular y devolver stock
CREATE   PROCEDURE [dbo].[sp_AnularVenta]
    @VentaId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Verificamos que no intenten anular algo ya anulado
    DECLARE @EstadoActual VARCHAR(30);
    SELECT @EstadoActual = Estado FROM Venta WHERE Id = @VentaId;

    IF @EstadoActual = 'Anulada'
    BEGIN
        RAISERROR('La venta ya se encuentra anulada.', 16, 1);
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        -- A) Marcamos el ticket como Anulado
        UPDATE Venta SET Estado = 'Anulada' WHERE Id = @VentaId;

        -- B) Devolvemos la cantidad exacta a los Lotes que se usaron en esta venta
        UPDATE l
        SET l.StockActual = l.StockActual + dv.Cantidad,
            l.Activo = 1 -- Si el lote había quedado en 0 (inactivo), lo revivimos
        FROM Lote l
        INNER JOIN DetalleVenta dv ON l.Id = dv.LoteId
        WHERE dv.VentaId = @VentaId;

        -- C) Sincronizamos la tabla Producto con lo que se devolvió
        UPDATE p
        SET p.StockActual = p.StockActual + devolucion.TotalDevuelto
        FROM Producto p
        INNER JOIN (
            SELECT ProductoId, SUM(Cantidad) AS TotalDevuelto
            FROM DetalleVenta
            WHERE VentaId = @VentaId
            GROUP BY ProductoId
        ) devolucion ON p.Id = devolucion.ProductoId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[sp_AnularVentaConMotivo]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 2. Procedimiento seguro para anular y registrar el motivo al mismo tiempo
CREATE   PROCEDURE [dbo].[sp_AnularVentaConMotivo]
    @VentaId INT,
    @UsuarioId INT,
    @Motivo NVARCHAR(255)
AS BEGIN
    BEGIN TRY
        BEGIN TRANSACTION;

        -- A) Actualizamos el estado de la venta
        UPDATE Venta
        SET Estado = 'Anulada'
        WHERE Id = @VentaId;

        -- B) Registramos el motivo en la auditoría
        INSERT INTO VentaAnulacion (VentaId, UsuarioId, Motivo)
        VALUES (@VentaId, @UsuarioId, @Motivo);

        -- (Nota: Si tenías código acá para devolver el stock de los productos,
        -- tenés que agregarlo en este bloque).

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_BajaUsuario]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[sp_BajaUsuario]
    @Id INT
AS
BEGIN
    UPDATE Usuario SET Activo = 0 WHERE Id = @Id;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_BuscarProductosParaVenta]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[sp_BuscarProductosParaVenta]
    @Filtro VARCHAR(100)
AS
BEGIN
    SELECT
        p.Id,
        p.Nombre,
        p.PrecioVenta,
        p.SeVendePorPeso,
        ISNULL(SUM(l.StockActual), 0) AS StockTotal
    FROM Producto p
    LEFT JOIN Lote l ON p.Id = l.ProductoId AND l.Activo = 1 AND l.StockActual > 0
    WHERE p.Nombre LIKE '%' + @Filtro + '%' OR p.CodigoBarras = @Filtro
    GROUP BY p.Id, p.Nombre, p.PrecioVenta, p.SeVendePorPeso;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_CerrarCaja]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[sp_CerrarCaja]
    @CajaId int,
    @UsuarioId int,
    @SaldoFinal decimal(18,2)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @CajaId IS NULL OR @CajaId<=0 OR @UsuarioId IS NULL OR @UsuarioId<=0
        THROW 53120, 'Seleccione un Vendedor y una caja válidos.', 1;
    IF @SaldoFinal IS NULL OR @SaldoFinal<0
        THROW 53121, 'El efectivo contado debe ser cero o mayor.', 1;

    DECLARE @TransaccionAnterior int=@@TRANCOUNT;
    BEGIN TRY
        IF @TransaccionAnterior=0 BEGIN TRANSACTION;
        ELSE SAVE TRANSACTION CerrarCajaSave;

        DECLARE @Lock int, @Recurso nvarchar(255);
        SET @Recurso=N'PacuIbera.MovimientoCaja.Caja.'+CONVERT(nvarchar(20),@CajaId);
        EXEC @Lock=sys.sp_getapplock @Resource=@Recurso,@LockMode=N'Exclusive',
             @LockOwner=N'Transaction',@LockTimeout=15000,@DbPrincipal=N'public';
        IF @Lock<0 THROW 53122, 'No se pudo bloquear la caja para el cierre.', 1;

        DECLARE @Estado nvarchar(30),@UsuarioCaja int,@Apertura datetime2(7),@Fondo decimal(18,2);
        SELECT @Estado=Estado,@UsuarioCaja=UsuarioId,@Apertura=FechaHoraApertura,@Fondo=SaldoInicial
        FROM dbo.Caja WITH (XLOCK,HOLDLOCK) WHERE Id=@CajaId;
        IF @Estado IS NULL THROW 53123, 'La caja no existe.', 1;
        IF @Estado<>N'Abierta' THROW 53124, 'La caja ya fue cerrada; no se puede cerrar dos veces.', 1;
        IF @UsuarioCaja<>@UsuarioId
            THROW 53125, 'Solo el Vendedor responsable de esta caja puede cerrarla.', 1;
        IF @Fondo IS NULL OR @Fondo<0 THROW 53126, 'La caja no tiene un fondo inicial válido.', 1;
        IF NOT EXISTS
        (
            SELECT 1 FROM dbo.Usuario u WITH (HOLDLOCK)
            INNER JOIN dbo.Rol r WITH (HOLDLOCK) ON r.Id=u.RolId
            WHERE u.Id=@UsuarioId AND u.Activo=1 AND r.Nombre=N'Vendedor'
        ) THROW 53127, 'El Vendedor no está activo o ya no tiene el rol autorizado.', 1;

        DECLARE @MetodoValido bit=1;
        IF EXISTS
        (
            SELECT 1 FROM (VALUES
                (1,N'Efectivo'),(1002,N'Efectivo'),(1006,N'Efectivo'),(2006,N'Efectivo'),
                (2,N'Debito'),(1003,N'Debito'),(1007,N'Debito'),(2007,N'Debito'),
                (3,N'Credito'),(1004,N'Credito'),(1008,N'Credito'),(2008,N'Credito'),
                (4,N'Transferencia'),(1005,N'Transferencia'),(1009,N'Transferencia'),(2009,N'Transferencia')
            ) e(Id,Categoria)
            LEFT JOIN dbo.MetodoPago mp WITH (HOLDLOCK) ON mp.Id=e.Id
            WHERE mp.Id IS NULL
               OR (e.Categoria=N'Efectivo' AND LOWER(mp.Nombre) NOT LIKE N'%efectivo%')
               OR (e.Categoria=N'Debito' AND mp.Nombre COLLATE Latin1_General_100_CI_AI NOT LIKE N'%debito%')
               OR (e.Categoria=N'Credito' AND mp.Nombre COLLATE Latin1_General_100_CI_AI NOT LIKE N'%credito%')
               OR (e.Categoria=N'Transferencia' AND LOWER(mp.Nombre) NOT LIKE N'%transfer%'
                                                      AND LOWER(mp.Nombre) NOT LIKE N'%mercado pago%')
        ) SET @MetodoValido=0;
        IF @MetodoValido=0 THROW 53128, 'Los medios de pago requieren revisión; no se cerró la caja.', 1;

        IF EXISTS
        (
            SELECT 1 FROM dbo.Venta v WITH (HOLDLOCK)
            WHERE v.CajaId=@CajaId AND (v.FechaHora IS NULL OR v.FechaHora<@Apertura
                  OR v.Estado IS NULL OR v.Estado NOT IN (N'Completada',N'Anulada'))
        ) THROW 53129, 'Hay ventas fuera del turno o con estado no válido; no se cerró la caja.', 1;

        IF EXISTS
        (
            SELECT 1 FROM dbo.Venta v WITH (HOLDLOCK)
            WHERE v.CajaId=@CajaId AND (v.Total IS NULL OR v.Total<=0)
        ) THROW 53136, 'Hay ventas con un total nulo, cero o negativo; no se cerró la caja.', 1;

        IF EXISTS
        (
            SELECT 1 FROM dbo.Venta v WITH (HOLDLOCK)
            INNER JOIN dbo.VentaPago vp WITH (HOLDLOCK) ON vp.VentaId=v.Id
            LEFT JOIN dbo.MetodoPago mp WITH (HOLDLOCK) ON mp.Id=vp.MetodoPagoId
            WHERE v.CajaId=@CajaId AND
                (vp.MontoCobrado IS NULL OR vp.MontoCobrado<=0 OR mp.Id IS NULL OR mp.Id NOT IN
                 (1,1002,1006,2006,2,1003,1007,2007,3,1004,1008,2008,4,1005,1009,2009))
        ) THROW 53130, 'Hay pagos nulos, no positivos, desconocidos o incompletos; no se cerró la caja.', 1;

        IF EXISTS
        (
            SELECT 1 FROM dbo.MovimientoCaja WITH (HOLDLOCK)
            WHERE CajaId=@CajaId AND (TipoMovimiento IS NULL OR TipoMovimiento NOT IN (N'Ingreso',N'Retiro')
                  OR Importe IS NULL OR Importe<=0)
        ) THROW 53137, 'Hay movimientos de efectivo con tipo o importe inválido; no se cerró la caja.', 1;

        IF EXISTS
        (
            SELECT 1 FROM dbo.Venta v WITH (HOLDLOCK)
            INNER JOIN dbo.VentaPago vp WITH (HOLDLOCK) ON vp.VentaId=v.Id
            INNER JOIN dbo.MetodoPago mp WITH (HOLDLOCK) ON mp.Id=vp.MetodoPagoId
            WHERE v.CajaId=@CajaId AND v.Estado=N'Anulada'
              AND mp.Id IN (1,1002,1006,2006)
        ) THROW 53131, 'Hay efectivo asociado a ventas anuladas sin devolución comprobada; no se cerró la caja.', 1;

        DECLARE @PagosPorVenta TABLE
        (
            VentaId int NOT NULL PRIMARY KEY,Estado nvarchar(30) NULL,Total decimal(18,2) NULL,
            EfectivoEntregado decimal(38,2) NOT NULL,OtrosMedios decimal(38,2) NOT NULL,
            MedioDesconocido bit NOT NULL
        );
        INSERT INTO @PagosPorVenta(VentaId,Estado,Total,EfectivoEntregado,OtrosMedios,MedioDesconocido)
        SELECT v.Id,v.Estado,v.Total,
               COALESCE(SUM(CASE WHEN mp.Id IN (1,1002,1006,2006) THEN CONVERT(decimal(38,2),vp.MontoCobrado) ELSE CONVERT(decimal(38,2),0) END),0),
               COALESCE(SUM(CASE WHEN mp.Id IN (2,1003,1007,2007,3,1004,1008,2008,4,1005,1009,2009) THEN CONVERT(decimal(38,2),vp.MontoCobrado) ELSE CONVERT(decimal(38,2),0) END),0),
               CONVERT(bit,MAX(CASE WHEN vp.VentaId IS NOT NULL AND (mp.Id IS NULL OR mp.Id NOT IN
                  (1,1002,1006,2006,2,1003,1007,2007,3,1004,1008,2008,4,1005,1009,2009)) THEN 1 ELSE 0 END))
        FROM dbo.Venta v WITH (HOLDLOCK)
        LEFT JOIN dbo.VentaPago vp WITH (HOLDLOCK) ON vp.VentaId=v.Id
        LEFT JOIN dbo.MetodoPago mp WITH (HOLDLOCK) ON mp.Id=vp.MetodoPagoId
        WHERE v.CajaId=@CajaId AND v.FechaHora>=@Apertura
        GROUP BY v.Id,v.Estado,v.Total;

        IF EXISTS
        (
            SELECT 1 FROM @PagosPorVenta
            WHERE Estado NOT IN (N'Anulada',N'Completada') OR Estado IS NULL OR
                (Total IS NULL OR Total<=0 OR MedioDesconocido=1 OR OtrosMedios>Total
                 OR EfectivoEntregado+OtrosMedios<Total)
        ) THROW 53132, 'Hay ventas cuyos pagos no concilian con el total; no se cerró la caja.', 1;

        DECLARE @EfectivoConservado decimal(38,2)=0,
                @Ingresos decimal(38,2)=0,@Retiros decimal(38,2)=0,
                @EfectivoConservado18 decimal(18,2),@Ingresos18 decimal(18,2),@Retiros18 decimal(18,2),
                @Esperado decimal(18,2),@Diferencia decimal(18,2);
        SELECT @EfectivoConservado=COALESCE(SUM(
            CASE WHEN EfectivoEntregado>Total-OtrosMedios THEN Total-OtrosMedios ELSE EfectivoEntregado END),0)
        FROM @PagosPorVenta WHERE Estado<>N'Anulada';

        SELECT @Ingresos=COALESCE(SUM(CONVERT(decimal(38,2),Importe)),0)
        FROM dbo.MovimientoCaja WITH (UPDLOCK,HOLDLOCK)
        WHERE CajaId=@CajaId AND TipoMovimiento=N'Ingreso';
        SELECT @Retiros=COALESCE(SUM(CONVERT(decimal(38,2),Importe)),0)
        FROM dbo.MovimientoCaja WITH (UPDLOCK,HOLDLOCK)
        WHERE CajaId=@CajaId AND TipoMovimiento=N'Retiro';

        SET @EfectivoConservado18=TRY_CONVERT(decimal(18,2),@EfectivoConservado);
        SET @Ingresos18=TRY_CONVERT(decimal(18,2),@Ingresos);
        SET @Retiros18=TRY_CONVERT(decimal(18,2),@Retiros);
        IF @EfectivoConservado18 IS NULL OR @EfectivoConservado18<0
           OR @Ingresos18 IS NULL OR @Retiros18 IS NULL
            THROW 53135, 'Los totales de efectivo superan el rango permitido; no se cerró la caja.', 1;

        SET @Esperado=TRY_CONVERT(decimal(18,2),CONVERT(decimal(38,2),@Fondo)+@EfectivoConservado+@Ingresos-@Retiros);
        IF @Esperado IS NULL OR @Esperado<0
            THROW 53133, 'El efectivo esperado no puede calcularse dentro del rango permitido; no se cerró la caja.', 1;
        SET @Diferencia=@SaldoFinal-@Esperado;

        UPDATE dbo.Caja
        SET FechaHoraCierre=GETDATE(),SaldoFinal=@SaldoFinal,Estado=N'Cerrada'
        WHERE Id=@CajaId AND UsuarioId=@UsuarioId AND Estado=N'Abierta';
        IF @@ROWCOUNT<>1 THROW 53134, 'La caja cambió durante el cierre; no se guardó el conteo.', 1;

        IF @TransaccionAnterior=0 COMMIT TRANSACTION;
        SELECT @CajaId AS CajaId,@Fondo AS FondoInicial,
               @EfectivoConservado18 AS CobrosEfectivoConservados,
               @Ingresos18 AS Ingresos,
               @Retiros18 AS Retiros,
               @Esperado AS EfectivoEsperado,@SaldoFinal AS EfectivoContado,@Diferencia AS Diferencia;
    END TRY
    BEGIN CATCH
        IF @TransaccionAnterior=0 AND XACT_STATE()<>0 ROLLBACK TRANSACTION;
        ELSE IF @TransaccionAnterior>0 AND XACT_STATE()=1 ROLLBACK TRANSACTION CerrarCajaSave;
        THROW;
    END CATCH;
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_ConfirmarRecepcionPedido]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[sp_ConfirmarRecepcionPedido]
	@PedidoId int,
	@UsuarioReceptorId int,
	@Detalles dbo.RecepcionDetalleEntrada READONLY,
	@Lotes dbo.RecepcionLoteEntrada READONLY
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;
	SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

	DECLARE @Ahora datetime2(0) = SYSDATETIME();
	DECLARE @RecepcionId int;
	DECLARE @EstadoResultado nvarchar(30);
	DECLARE @CantidadDetallesPedido int;
	DECLARE @CantidadDetallesEntrada int;
	DECLARE @DetalleId int;
	DECLARE @LoteNumero int;
	DECLARE @ProductoId int;
	DECLARE @CantidadLote decimal(10,3);
	DECLARE @TieneVencimiento bit;
	DECLARE @FechaVencimiento datetime2(0);
	DECLARE @DepositoStockId int;
	DECLARE @ControlaVencimiento bit;

	BEGIN TRY
		BEGIN TRANSACTION;

		IF NOT EXISTS
		(
			SELECT 1
			FROM dbo.Usuario WITH (UPDLOCK, HOLDLOCK)
			WHERE Id = @UsuarioReceptorId
		)
			THROW 51310, 'El usuario receptor no existe.', 1;

		IF NOT EXISTS
		(
			SELECT 1
			FROM dbo.PedidoProveedor WITH (UPDLOCK, HOLDLOCK)
			WHERE Id = @PedidoId
			  AND Estado = N'Pendiente'
		)
			THROW 51311, 'El pedido no existe o no está pendiente.', 1;

		IF EXISTS
		(
			SELECT 1
			FROM dbo.RecepcionPedido WITH (UPDLOCK, HOLDLOCK)
			WHERE PedidoId = @PedidoId
		)
			THROW 51312, 'El pedido ya tiene una recepción definitiva.', 1;

		SELECT @CantidadDetallesPedido = COUNT(*)
		FROM dbo.DetallePedidoProveedor WITH (UPDLOCK, HOLDLOCK)
		WHERE PedidoId = @PedidoId;

		IF @CantidadDetallesPedido = 0
			THROW 51321, 'No se puede confirmar un pedido sin detalles.', 1;

		-- No mezclar ingresos anteriores con una confirmacion definitiva.
		-- La cabecera y todos sus detalles ya permanecen bloqueados.
		IF EXISTS
		(
			SELECT 1
			FROM dbo.DetallePedidoProveedor WITH (UPDLOCK, HOLDLOCK)
			WHERE PedidoId = @PedidoId
			  AND CantidadRecibida <> 0
		)
			THROW 51320, 'El pedido tiene cantidades recibidas anteriormente; requiere conciliacion antes de confirmar.', 1;

		SELECT @CantidadDetallesEntrada = COUNT(*)
		FROM @Detalles;

		IF @CantidadDetallesEntrada <> @CantidadDetallesPedido
			THROW 51313, 'La recepción debe representar todos los detalles del pedido una sola vez.', 1;

		IF EXISTS
		(
			SELECT 1
			FROM @Detalles e
			LEFT JOIN dbo.DetallePedidoProveedor d
				ON d.Id = e.DetallePedidoProveedorId
			   AND d.PedidoId = @PedidoId
			WHERE d.Id IS NULL
		)
			THROW 51314, 'Existe un detalle que no pertenece al pedido indicado.', 1;

		IF EXISTS
		(
			SELECT 1
			FROM dbo.DetallePedidoProveedor d
			LEFT JOIN @Detalles e ON e.DetallePedidoProveedorId = d.Id
			WHERE d.PedidoId = @PedidoId
			  AND e.DetallePedidoProveedorId IS NULL
		)
			THROW 51315, 'Falta representar un detalle del pedido.', 1;

		IF EXISTS
		(
			SELECT 1
			FROM @Detalles e
			INNER JOIN dbo.DetallePedidoProveedor d ON d.Id = e.DetallePedidoProveedorId
			WHERE e.CantidadRecibida < 0
			   OR e.CantidadRecibida > d.Cantidad
		)
			THROW 51316, 'La cantidad recibida es negativa o supera la cantidad solicitada.', 1;

		IF EXISTS
		(
			SELECT 1
			FROM @Lotes l
			LEFT JOIN @Detalles e ON e.DetallePedidoProveedorId = l.DetallePedidoProveedorId
			WHERE e.DetallePedidoProveedorId IS NULL
		)
			THROW 51317, 'Existe un lote para un detalle no incluido en la recepción.', 1;

        -- Leer y mantener estable la configuracion actual del producto hasta COMMIT.
        -- Orden: usuario -> pedido -> detalles -> productos por Id -> lotes de deposito.
        -- La edicion de ficha necesita X; estos HOLDLOCK impiden cambiar la regla
        -- entre la validacion y el ingreso. No se modifican fichas ni lotes previos.
        DECLARE curProductosControl CURSOR LOCAL FAST_FORWARD FOR
            SELECT DISTINCT ProductoId FROM dbo.DetallePedidoProveedor
            WHERE PedidoId=@PedidoId ORDER BY ProductoId;
        OPEN curProductosControl;
        FETCH NEXT FROM curProductosControl INTO @ProductoId;
        WHILE @@FETCH_STATUS=0
        BEGIN
            SET @ControlaVencimiento=NULL;
            SELECT @ControlaVencimiento=ControlaVencimiento
            FROM dbo.Producto WITH (HOLDLOCK) WHERE Id=@ProductoId;
            IF @ControlaVencimiento IS NULL
                THROW 52410,'No se pudo consultar la configuracion de vencimiento de un producto.',1;
            IF @ControlaVencimiento=1 AND EXISTS
            (
                SELECT 1 FROM @Lotes l JOIN dbo.DetallePedidoProveedor d
                    ON d.Id=l.DetallePedidoProveedorId AND d.PedidoId=@PedidoId
                WHERE d.ProductoId=@ProductoId
                    AND(l.TieneVencimiento=0 OR l.FechaVencimiento IS NULL)
            )
                THROW 52411,'Un producto requiere fecha de vencimiento en cada lote recibido. Revise la recepcion.',1;
            FETCH NEXT FROM curProductosControl INTO @ProductoId;
        END;
        CLOSE curProductosControl;
        DEALLOCATE curProductosControl;
        -- Usar el dia real despues de las esperas por bloqueos (incluye hoy como vigente).
        SET @Ahora=SYSDATETIME();

		IF EXISTS
		(
			SELECT 1
			FROM @Lotes l
			WHERE l.Cantidad <= 0
			   OR (l.TieneVencimiento = 1 AND l.FechaVencimiento IS NULL)
			   OR (l.TieneVencimiento = 0 AND l.FechaVencimiento IS NOT NULL)
			   OR (l.TieneVencimiento = 1 AND l.FechaVencimiento < CONVERT(date, @Ahora))
		)
			THROW 51318, 'Un lote tiene cantidad, vencimiento o fecha inválidos.', 1;

		IF EXISTS
		(
			SELECT 1
			FROM @Detalles e
			OUTER APPLY
			(
				SELECT SUM(l.Cantidad) AS CantidadLotes
				FROM @Lotes l
				WHERE l.DetallePedidoProveedorId = e.DetallePedidoProveedorId
			) s
			WHERE (e.CantidadRecibida = 0 AND ISNULL(s.CantidadLotes, 0) <> 0)
			   OR (e.CantidadRecibida > 0 AND ISNULL(s.CantidadLotes, 0) <> e.CantidadRecibida)
		)
			THROW 51319, 'La suma de los lotes no coincide con la cantidad recibida del detalle.', 1;

		SET @EstadoResultado = CASE
			WHEN EXISTS
			(
				SELECT 1
				FROM @Detalles e
				INNER JOIN dbo.DetallePedidoProveedor d ON d.Id = e.DetallePedidoProveedorId
				WHERE e.CantidadRecibida < d.Cantidad
			) THEN N'Cerrado con faltantes'
			ELSE N'Completado'
		END;

		INSERT INTO dbo.RecepcionPedido
			(PedidoId, UsuarioReceptorId, FechaRecepcion, EstadoResultado)
		VALUES
			(@PedidoId, @UsuarioReceptorId, @Ahora, @EstadoResultado);

		SET @RecepcionId = CONVERT(int, SCOPE_IDENTITY());

		INSERT INTO dbo.RecepcionDetalle
			(RecepcionId, DetallePedidoProveedorId, CantidadSolicitada, CantidadRecibida, CantidadFaltante)
		SELECT
			@RecepcionId,
			e.DetallePedidoProveedorId,
			d.Cantidad,
			e.CantidadRecibida,
			d.Cantidad - e.CantidadRecibida
		FROM @Detalles e
		INNER JOIN dbo.DetallePedidoProveedor d ON d.Id = e.DetallePedidoProveedorId;

		DECLARE curLotes CURSOR LOCAL FAST_FORWARD FOR
			SELECT
				l.DetallePedidoProveedorId,
				l.LoteNumero,
				d.ProductoId,
				l.Cantidad,
				l.TieneVencimiento,
				l.FechaVencimiento
			FROM @Lotes l
			INNER JOIN dbo.DetallePedidoProveedor d
				ON d.Id = l.DetallePedidoProveedorId
			   AND d.PedidoId = @PedidoId
			ORDER BY l.DetallePedidoProveedorId, l.LoteNumero;

		OPEN curLotes;
		FETCH NEXT FROM curLotes
			INTO @DetalleId, @LoteNumero, @ProductoId, @CantidadLote, @TieneVencimiento, @FechaVencimiento;

		WHILE @@FETCH_STATUS = 0
		BEGIN
			INSERT INTO dbo.DepositoStock
				(ProductoId, FechaIngreso, FechaVencimiento, StockInicial, StockActual, Activo)
			VALUES
				(@ProductoId, @Ahora, @FechaVencimiento, @CantidadLote, @CantidadLote, 1);

			SET @DepositoStockId = CONVERT(int, SCOPE_IDENTITY());

			INSERT INTO dbo.RecepcionLote
				(RecepcionDetalleId, Cantidad, TieneVencimiento, FechaVencimiento, FechaIngreso, DepositoStockId)
			SELECT
				rd.Id,
				@CantidadLote,
				@TieneVencimiento,
				@FechaVencimiento,
				@Ahora,
				@DepositoStockId
			FROM dbo.RecepcionDetalle rd
			WHERE rd.RecepcionId = @RecepcionId
			  AND rd.DetallePedidoProveedorId = @DetalleId;

			FETCH NEXT FROM curLotes
				INTO @DetalleId, @LoteNumero, @ProductoId, @CantidadLote, @TieneVencimiento, @FechaVencimiento;
		END;

		CLOSE curLotes;
		DEALLOCATE curLotes;

		UPDATE d
		SET d.CantidadRecibida = e.CantidadRecibida
		FROM dbo.DetallePedidoProveedor d
		INNER JOIN @Detalles e ON e.DetallePedidoProveedorId = d.Id
		WHERE d.PedidoId = @PedidoId;

		UPDATE dbo.PedidoProveedor
		SET Estado = @EstadoResultado
		WHERE Id = @PedidoId;

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
        IF CURSOR_STATUS('local','curProductosControl')>=0 CLOSE curProductosControl;
        IF CURSOR_STATUS('local','curProductosControl')>=-1 DEALLOCATE curProductosControl;
		IF CURSOR_STATUS('local', 'curLotes') >= 0
			CLOSE curLotes;
		IF CURSOR_STATUS('local', 'curLotes') >= -1
			DEALLOCATE curLotes;
		IF XACT_STATE() <> 0
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_ConfirmarRecepcionRemito]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[sp_ConfirmarRecepcionRemito]
    @RemitoId int,
    @ClaveOperacion uniqueidentifier,
    @UsuarioReceptorId int,
    @Lotes dbo.RecepcionRemitoLoteEntrada READONLY
AS
BEGIN
    SET NOCOUNT ON;
    IF @@TRANCOUNT<>0
    BEGIN
        RAISERROR('La recepcion requiere una llamada sin transaccion externa.',16,1);
        RETURN;
    END;
    SET IMPLICIT_TRANSACTIONS OFF;
    SET XACT_ABORT ON;
    SET ANSI_PADDING ON;
    SET ANSI_WARNINGS ON;
    SET ARITHABORT ON;
    SET CONCAT_NULL_YIELDS_NULL ON;
    SET NUMERIC_ROUNDABORT OFF;
    DECLARE @RecepcionId int,@RemitoOriginal int,@UsuarioOriginal int,@FechaOriginal datetime2(0);
    DECLARE @Estado nvarchar(30),@Resultado nvarchar(30),@Emision datetime2(0),@Ahora datetime2(0),@Hoy date;
    DECLARE @Bloqueo int,@Recurso nvarchar(255),@NumeroLotes int;
    DECLARE @ProductoId int,@PorPeso bit,@SaldoLocal decimal(10,3),@TotalProducto decimal(38,3);
    DECLARE @RemitoLoteId int,@Enviada decimal(10,3),@Recibida decimal(10,3),@Vence datetime2(0),@LoteLocalId int;
    DECLARE @Trabajo TABLE
        (RemitoLoteId int PRIMARY KEY,ProductoId int NOT NULL,Enviada decimal(10,3) NOT NULL,
         Recibida decimal(10,3) NOT NULL,Vencimiento datetime2(0) NULL);
    BEGIN TRY
        IF @RemitoId IS NULL OR @RemitoId<=0 OR @UsuarioReceptorId IS NULL
            THROW 51910,'Remito y usuario receptor son obligatorios.',1;
        IF @ClaveOperacion IS NULL OR @ClaveOperacion='00000000-0000-0000-0000-000000000000'
            THROW 51911,'La clave de operacion es obligatoria y no puede ser vacia.',1;
        IF NOT EXISTS (SELECT 1 FROM @Lotes)
            THROW 51912,'Debe informar todos los lotes enviados, incluso con cantidad recibida cero.',1;
        IF EXISTS (SELECT 1 FROM @Lotes WHERE RemitoLoteId<=0 OR CantidadRecibida<0)
            THROW 51913,'Los lotes deben ser validos y las cantidades no pueden ser negativas.',1;
        BEGIN TRANSACTION;
        SET @Recurso=N'PacuIbera.RecepcionRemito.'+CONVERT(nvarchar(36),@ClaveOperacion);
        EXEC @Bloqueo=sys.sp_getapplock @Resource=@Recurso,@LockMode=N'Exclusive',
            @LockOwner=N'Transaction',@LockTimeout=15000;
        IF @Bloqueo<0 THROW 51914,'Operacion ocupada. Consulte la clave antes de reintentar.',1;
        IF NOT EXISTS
        (
            SELECT 1 FROM dbo.Usuario u WITH (HOLDLOCK)
            INNER JOIN dbo.Rol r WITH (HOLDLOCK) ON r.Id=u.RolId
            WHERE u.Id=@UsuarioReceptorId AND u.Activo=1 AND r.Nombre=N'Vendedor'
        ) THROW 51915,'El receptor debe existir, estar activo y tener rol Vendedor.',1;
        SELECT @Estado=Estado,@Emision=FechaEmision
        FROM dbo.RemitoInterno WITH (UPDLOCK,HOLDLOCK) WHERE Id=@RemitoId;
        IF @Estado IS NULL THROW 51916,'El remito no existe.',1;

        SELECT @RecepcionId=Id,@RemitoOriginal=RemitoId,@UsuarioOriginal=UsuarioReceptorId,
               @FechaOriginal=FechaRecepcion,@Resultado=EstadoResultado
        FROM dbo.RecepcionRemito WITH (UPDLOCK,HOLDLOCK) WHERE ClaveOperacion=@ClaveOperacion;
        IF @RecepcionId IS NOT NULL
        BEGIN
            IF @RemitoOriginal<>@RemitoId OR @UsuarioOriginal<>@UsuarioReceptorId
                THROW 51917,'La clave pertenece a otro remito o receptor.',1;
            IF EXISTS (SELECT RemitoLoteId,CantidadRecibida FROM @Lotes
                       EXCEPT SELECT RemitoLoteId,CantidadRecibida FROM dbo.RecepcionRemitoLote WHERE RecepcionRemitoId=@RecepcionId)
               OR EXISTS (SELECT RemitoLoteId,CantidadRecibida FROM dbo.RecepcionRemitoLote WHERE RecepcionRemitoId=@RecepcionId
                          EXCEPT SELECT RemitoLoteId,CantidadRecibida FROM @Lotes)
                THROW 51918,'La clave ya fue usada con lotes o cantidades diferentes.',1;
            COMMIT TRANSACTION;
            SELECT @RecepcionId AS RecepcionRemitoId,@RemitoId AS RemitoId,@Resultado AS Estado,
                   @FechaOriginal AS FechaRecepcion,CONVERT(bit,1) AS Reutilizado;
            RETURN;
        END;
        IF EXISTS (SELECT 1 FROM dbo.RecepcionRemito WITH (UPDLOCK,HOLDLOCK) WHERE RemitoId=@RemitoId)
            THROW 51919,'El remito ya tiene una recepcion definitiva con otra clave.',1;
        IF @Estado<>N'En tránsito' OR @Emision IS NULL
            THROW 51920,'Solo se pueden recibir remitos emitidos En transito.',1;

        SELECT @NumeroLotes=COUNT(*) FROM dbo.RemitoLote WITH (HOLDLOCK) WHERE RemitoId=@RemitoId;
        IF @NumeroLotes=0 OR @NumeroLotes<>(SELECT COUNT(*) FROM @Lotes)
            THROW 51921,'La recepcion debe contener exactamente todos los lotes del remito.',1;
        INSERT @Trabajo (RemitoLoteId,ProductoId,Enviada,Recibida,Vencimiento)
        SELECT rl.Id,rd.ProductoId,rl.CantidadEnviada,e.CantidadRecibida,rl.FechaVencimiento
        FROM @Lotes e INNER JOIN dbo.RemitoLote rl WITH (HOLDLOCK) ON rl.Id=e.RemitoLoteId AND rl.RemitoId=@RemitoId
        INNER JOIN dbo.RemitoDetalle rd WITH (HOLDLOCK) ON rd.Id=rl.RemitoDetalleId AND rd.RemitoId=rl.RemitoId;
        IF (SELECT COUNT(*) FROM @Trabajo)<>@NumeroLotes
            THROW 51922,'Hay lotes ajenos al remito o detalles inconsistentes.',1;
        IF EXISTS (SELECT 1 FROM @Trabajo WHERE Enviada<=0 OR Recibida>Enviada)
            THROW 51923,'La cantidad recibida no puede superar la enviada.',1;
        IF EXISTS
        (
            SELECT 1 FROM dbo.RemitoLote rl INNER JOIN dbo.RemitoDetalle rd ON rd.Id=rl.RemitoDetalleId
            LEFT JOIN dbo.DepositoStock ds ON ds.Id=rl.DepositoStockId
            WHERE rl.RemitoId=@RemitoId AND (ds.Id IS NULL OR ds.ProductoId<>rd.ProductoId)
        ) THROW 51924,'El producto no coincide con su lote original de deposito.',1;
        IF EXISTS
        (
            SELECT rd.Id FROM dbo.RemitoDetalle rd WITH (HOLDLOCK)
            LEFT JOIN dbo.RemitoLote rl WITH (HOLDLOCK) ON rl.RemitoDetalleId=rd.Id AND rl.RemitoId=rd.RemitoId
            WHERE rd.RemitoId=@RemitoId GROUP BY rd.Id,rd.CantidadEnviada
            HAVING rd.CantidadEnviada<=0 OR ISNULL(SUM(rl.CantidadEnviada),0)<>rd.CantidadEnviada
        ) THROW 51925,'Las cantidades de los detalles no coinciden con los lotes enviados.',1;

        DECLARE validarProductos CURSOR LOCAL FAST_FORWARD FOR
            SELECT ProductoId,SUM(Recibida) FROM @Trabajo GROUP BY ProductoId ORDER BY ProductoId;
        OPEN validarProductos;
        FETCH NEXT FROM validarProductos INTO @ProductoId,@TotalProducto;
        WHILE @@FETCH_STATUS=0
        BEGIN
            SET @PorPeso=NULL;
            SELECT @PorPeso=SeVendePorPeso,@SaldoLocal=StockActual FROM dbo.Producto WITH (UPDLOCK,HOLDLOCK)
            WHERE Id=@ProductoId AND Activo=1;
            IF @PorPeso IS NULL OR @SaldoLocal IS NULL
                THROW 51926,'Un producto no existe, esta inactivo o tiene configuracion invalida.',1;
            IF @PorPeso=0 AND @SaldoLocal<>FLOOR(@SaldoLocal)
                THROW 51933,'Un producto por unidad tiene saldo local fraccionario. Requiere conciliacion antes de recibir.',1;
            IF @PorPeso=0 AND EXISTS (SELECT 1 FROM @Trabajo WHERE ProductoId=@ProductoId
                                     AND (Recibida<>FLOOR(Recibida) OR Enviada<>FLOOR(Enviada)))
                THROW 51927,'Los productos por unidad requieren cantidades enteras.',1;
            IF @SaldoLocal<0 OR @TotalProducto>9999999.999-CONVERT(decimal(18,3),@SaldoLocal)
                THROW 51928,'El saldo local es invalido o excederia decimal(10,3).',1;
            FETCH NEXT FROM validarProductos INTO @ProductoId,@TotalProducto;
        END;
        CLOSE validarProductos;
        DEALLOCATE validarProductos;

        SET @Ahora=SYSDATETIME();
        SET @Hoy=CONVERT(date,@Ahora);
        IF @Ahora<@Emision THROW 51929,'La fecha de recepcion no puede ser anterior a la emision.',1;
        IF EXISTS (SELECT 1 FROM @Trabajo WHERE Recibida>0 AND Vencimiento<@Hoy)
            THROW 51930,'Hay lotes vencidos: informe recibido cero para registrarlos como diferencia, sin acreditar stock vendible.',1;
        SET @Resultado=CASE WHEN EXISTS (SELECT 1 FROM @Trabajo WHERE Recibida<Enviada)
                             THEN N'Con diferencias' ELSE N'Recibido' END;
        INSERT dbo.RecepcionRemito (RemitoId,ClaveOperacion,UsuarioReceptorId,FechaRecepcion,EstadoResultado)
        VALUES (@RemitoId,@ClaveOperacion,@UsuarioReceptorId,@Ahora,@Resultado);
        SET @RecepcionId=CONVERT(int,SCOPE_IDENTITY());

        DECLARE acreditarLotes CURSOR LOCAL FAST_FORWARD FOR
            SELECT RemitoLoteId,ProductoId,Enviada,Recibida,Vencimiento FROM @Trabajo ORDER BY ProductoId,RemitoLoteId;
        OPEN acreditarLotes;
        FETCH NEXT FROM acreditarLotes INTO @RemitoLoteId,@ProductoId,@Enviada,@Recibida,@Vence;
        WHILE @@FETCH_STATUS=0
        BEGIN
            SET @LoteLocalId=NULL;
            IF @Recibida>0
            BEGIN
                INSERT dbo.Lote (ProductoId,FechaIngreso,FechaVencimiento,StockInicial,StockActual,Activo)
                VALUES (@ProductoId,@Ahora,@Vence,@Recibida,@Recibida,1);
                SET @LoteLocalId=CONVERT(int,SCOPE_IDENTITY());
            END;
            INSERT dbo.RecepcionRemitoLote
                (RecepcionRemitoId,RemitoId,RemitoLoteId,CantidadEnviada,CantidadRecibida,LoteLocalId,Observacion)
            VALUES (@RecepcionId,@RemitoId,@RemitoLoteId,@Enviada,@Recibida,@LoteLocalId,
                CASE WHEN @Vence<@Hoy THEN N'Lote vencido al recibir: no acreditado; diferencia pendiente.'
                     WHEN @Recibida<@Enviada THEN N'Diferencia pendiente de resolucion.' ELSE NULL END);
            FETCH NEXT FROM acreditarLotes INTO @RemitoLoteId,@ProductoId,@Enviada,@Recibida,@Vence;
        END;
        CLOSE acreditarLotes;
        DEALLOCATE acreditarLotes;
        UPDATE p SET StockActual=p.StockActual+s.Recibido
        FROM dbo.Producto p INNER JOIN
            -- SUM(decimal) devuelve decimal(38,3). Acotar antes de sumar al saldo
            -- evita que la aritmetica reduzca escala y redondee la tercera posicion.
            (SELECT ProductoId,CONVERT(decimal(18,3),SUM(Recibida)) AS Recibido
             FROM @Trabajo GROUP BY ProductoId) s ON s.ProductoId=p.Id
        WHERE s.Recibido>0;

        IF (SELECT COUNT(*) FROM dbo.RecepcionRemitoLote WHERE RecepcionRemitoId=@RecepcionId)<>@NumeroLotes
           OR (SELECT SUM(CantidadRecibida+CantidadDiferencia) FROM dbo.RecepcionRemitoLote WHERE RecepcionRemitoId=@RecepcionId)
              <> (SELECT SUM(Enviada) FROM @Trabajo)
            THROW 51931,'La suma recibida mas diferencias no coincide con el envio. Se revierte toda la recepcion.',1;
        UPDATE dbo.RemitoInterno SET Estado=@Resultado WHERE Id=@RemitoId AND Estado=N'En tránsito';
        IF @@ROWCOUNT<>1 THROW 51932,'El remito cambio de estado. Se revierte toda la recepcion.',1;
        COMMIT TRANSACTION;
        SELECT @RecepcionId AS RecepcionRemitoId,@RemitoId AS RemitoId,@Resultado AS Estado,
               @Ahora AS FechaRecepcion,CONVERT(bit,0) AS Reutilizado;
    END TRY
    BEGIN CATCH
        IF CURSOR_STATUS('local','validarProductos')>=0 CLOSE validarProductos;
        IF CURSOR_STATUS('local','validarProductos')>=-1 DEALLOCATE validarProductos;
        IF CURSOR_STATUS('local','acreditarLotes')>=0 CLOSE acreditarLotes;
        IF CURSOR_STATUS('local','acreditarLotes')>=-1 DEALLOCATE acreditarLotes;
        IF XACT_STATE()<>0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_CrearPedidoProveedor]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_CrearPedidoProveedor]
	@ProveedorId int,
	@UsuarioId int,
	@FechaPedido datetime2(0),
	@NuevoPedidoId int OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	BEGIN TRY
		IF @ProveedorId IS NULL OR @UsuarioId IS NULL OR @FechaPedido IS NULL
			THROW 51100, 'Proveedor, usuario y fecha son obligatorios.', 1;

		IF NOT EXISTS (SELECT 1 FROM dbo.Proveedor WHERE Id = @ProveedorId)
			THROW 51101, 'El proveedor indicado no existe.', 1;

		IF NOT EXISTS (SELECT 1 FROM dbo.Usuario WHERE Id = @UsuarioId)
			THROW 51102, 'El usuario indicado no existe.', 1;

		BEGIN TRANSACTION;
			INSERT INTO dbo.PedidoProveedor (ProveedorId, UsuarioId, FechaPedido, Estado)
			VALUES (@ProveedorId, @UsuarioId, @FechaPedido, N'Pendiente');

			SET @NuevoPedidoId = CONVERT(int, SCOPE_IDENTITY());
		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_EmitirRemitoInterno]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[sp_EmitirRemitoInterno]
    @ClaveOperacion uniqueidentifier,
    @UsuarioEmisorId int,
    @Productos dbo.RemitoProductoEntrada READONLY
AS
BEGIN
    SET NOCOUNT ON;
    -- El SP es la unidad atomica; no confirmar ni revertir transacciones ajenas.
    IF @@TRANCOUNT <> 0
    BEGIN
        RAISERROR('La emision requiere una llamada sin transaccion externa.', 16, 1);
        RETURN;
    END;
    SET IMPLICIT_TRANSACTIONS OFF;
    SET XACT_ABORT ON;
    SET ANSI_PADDING ON;
    SET ANSI_WARNINGS ON;
    SET ARITHABORT ON;
    SET CONCAT_NULL_YIELDS_NULL ON;
    SET NUMERIC_ROUNDABORT OFF;

    DECLARE @RemitoId int, @UsuarioOriginal int, @FechaOriginal datetime2(0), @Estado nvarchar(30);
    DECLARE @ProductoId int, @PorPeso bit, @Cantidad decimal(10,3), @Restante decimal(10,3);
    DECLARE @DetalleId int, @LoteId int, @Saldo decimal(10,3), @Tomar decimal(10,3);
    DECLARE @Ingreso datetime2(0), @Vencimiento datetime2(0), @Ahora datetime2(0), @Hoy date;
    DECLARE @ResultadoBloqueo int, @Recurso nvarchar(255), @LotesBloqueados bigint;
    BEGIN TRY
        IF @ClaveOperacion IS NULL OR @ClaveOperacion = '00000000-0000-0000-0000-000000000000'
            THROW 51710, 'La clave de operacion es obligatoria y no puede ser vacia.', 1;
        IF @UsuarioEmisorId IS NULL
            THROW 51711, 'El usuario emisor es obligatorio.', 1;
        IF NOT EXISTS (SELECT 1 FROM @Productos)
            THROW 51712, 'El remito debe contener al menos un producto.', 1;
        IF EXISTS (SELECT 1 FROM @Productos WHERE ProductoId <= 0 OR Cantidad <= 0)
            THROW 51713, 'Los productos y cantidades deben ser positivos.', 1;

        BEGIN TRANSACTION;
        SET @Recurso = N'PacuIbera.EmitirRemito.' + CONVERT(nvarchar(36), @ClaveOperacion);
        EXEC @ResultadoBloqueo = sys.sp_getapplock @Resource = @Recurso,
            @LockMode = N'Exclusive', @LockOwner = N'Transaction', @LockTimeout = 15000;
        IF @ResultadoBloqueo < 0
            THROW 51714, 'No se pudo bloquear la operacion. Consulte su resultado y reintente con la misma clave.', 1;

        SELECT @LotesBloqueados = COUNT_BIG(*) FROM dbo.DepositoStock WITH (TABLOCKX, HOLDLOCK);
        -- Calcular el dia despues de esperar los bloqueos; no usar una fecha anterior a la espera.
        SET @Ahora = SYSDATETIME();
        SET @Hoy = CONVERT(date, @Ahora);

        IF NOT EXISTS
        (
            SELECT 1 FROM dbo.Usuario u WITH (HOLDLOCK)
            INNER JOIN dbo.Rol r WITH (HOLDLOCK) ON r.Id = u.RolId
            WHERE u.Id = @UsuarioEmisorId AND u.Activo = 1 AND r.Nombre = N'Administrador'
        )
            THROW 51715, 'El emisor debe existir, estar activo y tener rol Administrador.', 1;

        SELECT @RemitoId = Id, @UsuarioOriginal = UsuarioEmisorId,
               @FechaOriginal = FechaEmision, @Estado = Estado
        FROM dbo.RemitoInterno WITH (UPDLOCK, HOLDLOCK) WHERE ClaveOperacion = @ClaveOperacion;
        IF @RemitoId IS NOT NULL
        BEGIN
            IF @FechaOriginal IS NULL OR @UsuarioOriginal IS NULL OR @UsuarioOriginal <> @UsuarioEmisorId
               OR @Estado NOT IN (N'En tránsito', N'Recibido', N'Con diferencias', N'Cerrado')
                THROW 51716, 'La clave corresponde a otro emisor o a una operacion no emitida.', 1;
            IF EXISTS
                (SELECT ProductoId, Cantidad FROM @Productos
                 EXCEPT SELECT ProductoId, CantidadEnviada FROM dbo.RemitoDetalle WHERE RemitoId = @RemitoId)
               OR EXISTS
                (SELECT ProductoId, CantidadEnviada FROM dbo.RemitoDetalle WHERE RemitoId = @RemitoId
                 EXCEPT SELECT ProductoId, Cantidad FROM @Productos)
                THROW 51717, 'La clave de operacion ya se utilizo con productos o cantidades diferentes.', 1;
            COMMIT TRANSACTION;
            SELECT @RemitoId AS RemitoId, @Estado AS Estado, @FechaOriginal AS FechaEmision,
                   CONVERT(bit, 1) AS Reutilizado;
            RETURN;
        END;

        INSERT INTO dbo.RemitoInterno
            (ClaveOperacion, Estado, UsuarioCreadorId, FechaCreacion, UsuarioEmisorId, FechaEmision)
        VALUES (@ClaveOperacion, N'En tránsito', @UsuarioEmisorId, @Ahora, @UsuarioEmisorId, @Ahora);
        SET @RemitoId = CONVERT(int, SCOPE_IDENTITY());

        DECLARE productos CURSOR LOCAL FAST_FORWARD FOR
            SELECT ProductoId, Cantidad FROM @Productos ORDER BY ProductoId;
        OPEN productos;
        FETCH NEXT FROM productos INTO @ProductoId, @Cantidad;
        WHILE @@FETCH_STATUS = 0
        BEGIN
            SET @PorPeso = NULL;
            SELECT @PorPeso = SeVendePorPeso FROM dbo.Producto WITH (HOLDLOCK)
            WHERE Id = @ProductoId AND Activo = 1;
            IF @PorPeso IS NULL
                THROW 51718, 'Un producto no existe, esta inactivo o no tiene unidad de venta definida.', 1;
            IF @PorPeso = 0 AND @Cantidad <> FLOOR(@Cantidad)
                THROW 51719, 'Los productos por unidad requieren cantidades enteras.', 1;
            IF (SELECT ISNULL(SUM(StockActual), 0) FROM dbo.DepositoStock
                WHERE ProductoId = @ProductoId AND Activo = 1 AND StockActual > 0
                  AND (FechaVencimiento IS NULL OR FechaVencimiento >= @Hoy)) < @Cantidad
                THROW 51720, 'No hay stock utilizable suficiente para uno de los productos.', 1;

            INSERT INTO dbo.RemitoDetalle (RemitoId, ProductoId, CantidadEnviada)
            VALUES (@RemitoId, @ProductoId, @Cantidad);
            SET @DetalleId = CONVERT(int, SCOPE_IDENTITY());
            SET @Restante = @Cantidad;
            WHILE @Restante > 0
            BEGIN
                SET @LoteId = NULL;
                SELECT TOP (1) @LoteId = Id, @Saldo = StockActual,
                               @Ingreso = FechaIngreso, @Vencimiento = FechaVencimiento
                FROM dbo.DepositoStock
                WHERE ProductoId = @ProductoId AND Activo = 1 AND StockActual > 0
                  AND (FechaVencimiento IS NULL OR FechaVencimiento >= @Hoy)
                ORDER BY CASE WHEN FechaVencimiento IS NULL THEN 1 ELSE 0 END,
                         FechaVencimiento,
                         CASE WHEN FechaVencimiento IS NULL THEN FechaIngreso END, Id;
                IF @LoteId IS NULL
                    THROW 51721, 'No se encontro un lote elegible. Se revertira toda la emision.', 1;
                -- Un saldo historico fraccionario por unidad no se corrige ni se saltea silenciosamente.
                IF @PorPeso = 0 AND @Saldo <> FLOOR(@Saldo)
                    THROW 51722, 'Un lote por unidad tiene saldo fraccionario; requiere conciliacion.', 1;
                SET @Tomar = CASE WHEN @Saldo < @Restante THEN @Saldo ELSE @Restante END;
                INSERT INTO dbo.RemitoLote
                    (RemitoId, RemitoDetalleId, DepositoStockId, CantidadEnviada, FechaIngresoOrigen, FechaVencimiento)
                VALUES (@RemitoId, @DetalleId, @LoteId, @Tomar, @Ingreso, @Vencimiento);
                UPDATE dbo.DepositoStock SET StockActual = StockActual - @Tomar
                WHERE Id = @LoteId AND StockActual >= @Tomar;
                IF @@ROWCOUNT <> 1
                    THROW 51723, 'No se pudo descontar el lote. Se revertira toda la emision.', 1;
                SET @Restante = @Restante - @Tomar;
            END;
            FETCH NEXT FROM productos INTO @ProductoId, @Cantidad;
        END;
        CLOSE productos;
        DEALLOCATE productos;
        COMMIT TRANSACTION;
        SELECT @RemitoId AS RemitoId, N'En tránsito' AS Estado, @Ahora AS FechaEmision,
               CONVERT(bit, 0) AS Reutilizado;
    END TRY
    BEGIN CATCH
        IF CURSOR_STATUS('local', 'productos') >= 0 CLOSE productos;
        IF CURSOR_STATUS('local', 'productos') >= -1 DEALLOCATE productos;
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_InsertarCliente]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[sp_InsertarCliente]
    @Nombre NVARCHAR(100),
    @Apellido NVARCHAR(100),
    @DNI_CUIT NVARCHAR(50) = '',
    @Telefono NVARCHAR(50) = '',
    @Email NVARCHAR(100) = '',
    @Direccion NVARCHAR(200) = '',
    @ProvinciaId INT = 1,
    @LocalidadId INT = 1
AS BEGIN
    INSERT INTO Cliente (Nombre, Apellido, DNI_CUIT, Telefono, Email, Direccion, ProvinciaId, LocalidadId)
    VALUES (@Nombre, @Apellido, @DNI_CUIT, @Telefono, @Email, @Direccion, @ProvinciaId, @LocalidadId);
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_InsertarClienteRapido]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =======================================================
-- PROCEDIMIENTO DEFINITIVO ALTA EXPRÉS (CON LOCACIÓN DINÁMICA)
-- =======================================================
CREATE   PROCEDURE [dbo].[sp_InsertarClienteRapido]
    @Nombre NVARCHAR(100),
    @Telefono NVARCHAR(50) = NULL,
    @Direccion NVARCHAR(200) = NULL,
    @ProvinciaId INT,
    @LocalidadId INT
AS
BEGIN
    -- Se inserta el Nombre, y el Apellido se fuerza como cadena vacía para que no aparezca la palabra "Cliente"
    INSERT INTO Cliente (Nombre, Apellido, Telefono, Direccion, ProvinciaId, LocalidadId)
    VALUES (@Nombre, '', @Telefono, @Direccion, @ProvinciaId, @LocalidadId)
END
GO
/****** Object:  StoredProcedure [dbo].[sp_InsertarUsuario]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[sp_InsertarUsuario]
    @Nombre VARCHAR(100), @Apellido VARCHAR(100), @DNI VARCHAR(20), @ClaveHash VARCHAR(255),
    @Email VARCHAR(150) = NULL, @Telefono VARCHAR(20) = NULL, @FechaNacimiento DATE = NULL,
    @Direccion VARCHAR(150) = NULL, @RolId INT, @ProvinciaId INT, @LocalidadId INT
AS
BEGIN
    INSERT INTO Usuario (Nombre, Apellido, DNI, ClaveHash, Email, Telefono, FechaNacimiento, Direccion, RolId, ProvinciaId, LocalidadId, Activo)
    VALUES (@Nombre, @Apellido, @DNI, @ClaveHash, @Email, @Telefono, @FechaNacimiento, @Direccion, @RolId, @ProvinciaId, @LocalidadId, 1);
END
GO
/****** Object:  StoredProcedure [dbo].[sp_LoginUsuario]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =======================================================
-- 3. PROCEDIMIENTOS ALMACENADOS
-- =======================================================

CREATE   PROCEDURE [dbo].[sp_LoginUsuario]
    @DNI VARCHAR(20)
AS
BEGIN
    SELECT u.Id, u.Nombre, u.Apellido, u.DNI, u.ClaveHash, r.Nombre AS Rol
    FROM Usuario u
    INNER JOIN Rol r ON u.RolId = r.Id
    WHERE u.DNI = @DNI AND u.Activo = 1;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_ModificarProducto]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[sp_ModificarProducto]
    @Id INT,
    @Nombre VARCHAR(100),
    @CategoriaId INT,
    @PrecioVenta DECIMAL(10,2),
    @StockMinimo DECIMAL(10,3),
    @SeVendePorPeso BIT,
    @Descripcion VARCHAR(50),
    @StockNuevo DECIMAL(10,3),
    @FechaVencimiento DATE
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- 1. PISAMOS el stock general del producto en la tabla principal
        UPDATE Producto
        SET Nombre = @Nombre,
            CategoriaId = @CategoriaId,
            PrecioVenta = @PrecioVenta,
            StockMinimo = @StockMinimo,
            SeVendePorPeso = @SeVendePorPeso,
            CodigoBarras = @Descripcion,
            StockActual = @StockNuevo
        WHERE Id = @Id;

        -- 2. PISAMOS el stock en el sistema de Lotes para que las Ventas no se rompan
        IF EXISTS (SELECT 1 FROM Lote WHERE ProductoId = @Id AND Activo = 1)
        BEGIN
            -- Si ya existía un lote, agarramos el último y le pisamos el stock
            DECLARE @UltimoLoteId INT = (SELECT TOP 1 Id FROM Lote WHERE ProductoId = @Id AND Activo = 1 ORDER BY Id DESC);

            UPDATE Lote
            SET StockActual = @StockNuevo,
                StockInicial = @StockNuevo
            WHERE Id = @UltimoLoteId;
        END
        ELSE IF @StockNuevo > 0
        BEGIN
            -- Si no tenía ningún lote previo y ahora le pusieron stock, creamos uno invisible
            INSERT INTO Lote (ProductoId, FechaIngreso, FechaVencimiento, StockInicial, StockActual, Activo)
            VALUES (@Id, GETDATE(), @FechaVencimiento, @StockNuevo, @StockNuevo, 1);
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[sp_ObtenerClientes]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[sp_ObtenerClientes]
AS
BEGIN
    SELECT c.Id, c.Nombre, c.Apellido, c.DNI_CUIT, c.Telefono, c.Email, c.Direccion, p.Nombre AS Provincia, l.Nombre AS Localidad
    FROM Cliente c
    INNER JOIN Provincia p ON c.ProvinciaId = p.Id
    INNER JOIN Localidad l ON c.LocalidadId = l.Id
    ORDER BY c.Apellido, c.Nombre;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_ObtenerClientesCRM]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 1. Agregamos Provincia y Localidad a la vista de la grilla
CREATE PROCEDURE [dbo].[sp_ObtenerClientesCRM]
AS BEGIN
    SELECT
        c.Id,
        c.Nombre,
        c.Apellido,
        c.Nombre + ' ' + c.Apellido AS Cliente,
        c.Telefono,
        c.Direccion,
        p.Nombre AS Provincia,
        l.Nombre AS Localidad
    FROM Cliente c
    INNER JOIN Provincia p ON c.ProvinciaId = p.Id
    INNER JOIN Localidad l ON c.LocalidadId = l.Id
    ORDER BY c.Nombre ASC
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_ObtenerDetallePedidoProveedor]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_ObtenerDetallePedidoProveedor]
	@PedidoId int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		dpp.Id AS DetalleId,
		dpp.ProductoId,
		pr.Nombre AS Producto,
		dpp.Cantidad,
		dpp.CantidadRecibida,
		dpp.Cantidad - dpp.CantidadRecibida AS Pendiente,
		dpp.PrecioCosto
	FROM dbo.DetallePedidoProveedor dpp
	INNER JOIN dbo.Producto pr ON pr.Id = dpp.ProductoId
	WHERE dpp.PedidoId = @PedidoId
	ORDER BY dpp.Id;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_ObtenerDetalleVenta]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[sp_ObtenerDetalleVenta]
    @VentaId INT
AS
BEGIN
    SELECT
        dv.Cantidad,
        p.Nombre,
        (dv.Cantidad * dv.PrecioUnitario) AS Subtotal
    FROM DetalleVenta dv
    INNER JOIN Producto p ON dv.ProductoId = p.Id
    WHERE dv.VentaId = @VentaId;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_ObtenerHistorialVentas]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 1. Procedimiento para llenar la grilla del Historial
CREATE   PROCEDURE [dbo].[sp_ObtenerHistorialVentas]
AS
BEGIN
    SELECT
        v.Id AS [Nro Venta],
        v.FechaHora AS Fecha,
        ISNULL(c.Nombre + ' ' + c.Apellido, 'CONSUMIDOR FINAL') AS Cliente,
        u.Nombre AS Cajero,
        v.Total,
        v.Estado
    FROM Venta v
    LEFT JOIN Cliente c ON v.ClienteId = c.Id
    INNER JOIN Usuario u ON v.UsuarioId = u.Id
    ORDER BY v.FechaHora DESC;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_ObtenerLocalidadesPorProvincia]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[sp_ObtenerLocalidadesPorProvincia]
    @ProvinciaId INT
AS
BEGIN
    SELECT Id, Nombre FROM Localidad WHERE ProvinciaId = @ProvinciaId;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_ObtenerPedidosProveedor]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_ObtenerPedidosProveedor]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		pp.Id,
		pp.ProveedorId,
		p.RazonSocial AS Proveedor,
		pp.UsuarioId,
		pp.FechaPedido,
		pp.Estado,
		COUNT(dpp.Id) AS CantidadItems,
		ISNULL(SUM(dpp.Cantidad * dpp.PrecioCosto), 0) AS TotalEstimado
	FROM dbo.PedidoProveedor pp
	INNER JOIN dbo.Proveedor p ON p.Id = pp.ProveedorId
	LEFT JOIN dbo.DetallePedidoProveedor dpp ON dpp.PedidoId = pp.Id
	GROUP BY
		pp.Id,
		pp.ProveedorId,
		p.RazonSocial,
		pp.UsuarioId,
		pp.FechaPedido,
		pp.Estado
	ORDER BY pp.FechaPedido DESC, pp.Id DESC;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_ObtenerProvincias]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[sp_ObtenerProvincias]
AS
BEGIN
    SELECT Id, Nombre FROM Provincia ORDER BY Nombre ASC;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_ObtenerRoles]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[sp_ObtenerRoles]
AS
BEGIN
    SELECT Id, Nombre FROM Rol;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_ObtenerStockDeposito]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_ObtenerStockDeposito]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		pr.Id AS ProductoId,
		pr.Nombre,
		pr.Descripcion,
		ca.Nombre AS Categoria,
		ISNULL(SUM(ds.StockActual), 0) AS StockDeposito,
		ISNULL(dsm.StockMinimo, 0) AS StockMinimoDeposito,
		CASE
			WHEN ISNULL(SUM(ds.StockActual), 0)
				 < ISNULL(dsm.StockMinimo, 0)
			THEN N'ALERTA'
			ELSE N'OK'
		END AS EstadoStock
	FROM dbo.Producto pr
	LEFT JOIN dbo.Categoria ca ON ca.Id = pr.CategoriaId
	LEFT JOIN dbo.DepositoStock ds
		ON ds.ProductoId = pr.Id
		AND ds.Activo = 1
		AND (ds.FechaVencimiento IS NULL
			 OR ds.FechaVencimiento >= CONVERT(date, SYSDATETIME()))
	LEFT JOIN dbo.DepositoStockMinimo dsm ON dsm.ProductoId = pr.Id
	WHERE pr.Activo = 1
	GROUP BY pr.Id, pr.Nombre, pr.Descripcion, ca.Nombre, dsm.StockMinimo
	ORDER BY pr.Nombre;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_ObtenerUsuariosDetalle]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[sp_ObtenerUsuariosDetalle]
AS
BEGIN
    SELECT u.Id, u.Nombre, u.Apellido, u.DNI, u.Telefono, u.Email, u.Direccion,
           r.Nombre AS Rol, p.Nombre AS Provincia, l.Nombre AS Localidad,
           CASE WHEN u.Activo = 1 THEN 'Activo' ELSE 'Inactivo' END AS Estado,
           u.ClaveHash AS Clave
    FROM Usuario u
    INNER JOIN Rol r ON u.RolId = r.Id
    INNER JOIN Provincia p ON u.ProvinciaId = p.Id
    INNER JOIN Localidad l ON u.LocalidadId = l.Id;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_RecepcionarItemPedido]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[sp_RecepcionarItemPedido]
    @DetalleId int,
    @CantidadIngresada int,
    @FechaVencimiento datetime2(0) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    THROW 51410, 'La recepcion por item esta deshabilitada. Utilice la confirmacion definitiva del pedido.', 1;
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_RegistrarDetalleVentaFIFO]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[sp_RegistrarDetalleVentaFIFO]
    @VentaId INT,
    @ProductoId INT,
    @CantidadRequerida DECIMAL(18,3),
    @PrecioUnitario DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @VentaId IS NULL OR @ProductoId IS NULL
       OR @CantidadRequerida IS NULL OR @CantidadRequerida <= 0
       OR @PrecioUnitario IS NULL OR @PrecioUnitario < 0
        THROW 52410, 'La venta, el producto y las cantidades deben ser válidos.', 1;

    DECLARE @FechaHoy DATE = CONVERT(date, SYSDATETIME());
    DECLARE @StockUtilizable DECIMAL(38,3);
    DECLARE @CantidadRestante DECIMAL(18,3) = @CantidadRequerida;
    DECLARE @LoteId INT, @StockLote DECIMAL(10,3);
    DECLARE @Tomar DECIMAL(10,3);

    IF @@TRANCOUNT = 0
        THROW 52415, 'La venta debe ejecutarse dentro de su transacción de aplicación.', 1;

    /*
      La aplicación ya tiene una transacción abierta. Estos bloqueos se conservan
      hasta que ella confirme o revierta venta, pagos y todos los detalles.
    */
    SELECT @StockUtilizable =
        COALESCE(SUM(CONVERT(DECIMAL(38,3), l.StockActual)), 0)
    FROM dbo.Lote l WITH (UPDLOCK, HOLDLOCK)
    WHERE l.ProductoId = @ProductoId
      AND l.Activo = 1
      AND l.StockActual > 0
      AND (
          l.FechaVencimiento IS NULL
          OR CONVERT(date, l.FechaVencimiento) =
             CONVERT(date, '20991231', 112)
          OR l.FechaVencimiento >= @FechaHoy
      );

    IF @StockUtilizable < @CantidadRequerida
        THROW 52411, 'No hay suficiente mercadería vigente disponible para completar la venta.', 1;

    WHILE @CantidadRestante > 0
    BEGIN
        SET @LoteId = NULL;
        SET @StockLote = NULL;

        SELECT TOP (1)
            @LoteId = l.Id,
            @StockLote = l.StockActual
        FROM dbo.Lote l WITH (UPDLOCK, HOLDLOCK)
        WHERE l.ProductoId = @ProductoId
          AND l.Activo = 1
          AND l.StockActual > 0
          AND (
              l.FechaVencimiento IS NULL
              OR CONVERT(date, l.FechaVencimiento) =
                 CONVERT(date, '20991231', 112)
              OR l.FechaVencimiento >= @FechaHoy
          )
        ORDER BY
            CASE
                WHEN l.FechaVencimiento IS NULL
                  OR CONVERT(date, l.FechaVencimiento) =
                     CONVERT(date, '20991231', 112)
                THEN 1
                ELSE 0
            END,
            CASE
                WHEN CONVERT(date, l.FechaVencimiento) =
                     CONVERT(date, '20991231', 112)
                THEN NULL
                ELSE l.FechaVencimiento
            END,
            l.Id;

        IF @LoteId IS NULL OR @StockLote IS NULL
            THROW 52412, 'No se pudo reservar un lote vigente para completar la venta.', 1;

        SET @Tomar =
            CASE
                WHEN @StockLote >= @CantidadRestante
                THEN @CantidadRestante
                ELSE @StockLote
            END;

        UPDATE dbo.Lote
        SET StockActual = StockActual - @Tomar,
            Activo =
                CASE
                    WHEN StockActual - @Tomar = 0
                    THEN 0
                    ELSE Activo
                END
        WHERE Id = @LoteId
          AND Activo = 1
          AND StockActual >= @Tomar
          AND (
              FechaVencimiento IS NULL
              OR CONVERT(date, FechaVencimiento) =
                 CONVERT(date, '20991231', 112)
              OR FechaVencimiento >= @FechaHoy
          );

        IF @@ROWCOUNT <> 1
            THROW 52413, 'El lote cambió mientras se procesaba la venta. Intente nuevamente.', 1;

        INSERT dbo.DetalleVenta
            (VentaId, ProductoId, LoteId, Cantidad, PrecioUnitario, Subtotal)
        VALUES
            (@VentaId, @ProductoId, @LoteId, @Tomar,
             @PrecioUnitario, @Tomar * @PrecioUnitario);

        SET @CantidadRestante -= @Tomar;
    END;

    /*
      Producto.StockActual representa el saldo utilizable del local.
      Recalcular con los mismos criterios evita contar lotes vencidos.
    */
    UPDATE p
    SET StockActual =
        (
            SELECT COALESCE(
                SUM(CONVERT(DECIMAL(10,3), l.StockActual)), 0
            )
            FROM dbo.Lote l WITH (UPDLOCK, HOLDLOCK)
            WHERE l.ProductoId = @ProductoId
              AND l.Activo = 1
              AND l.StockActual > 0
              AND (
                  l.FechaVencimiento IS NULL
                  OR CONVERT(date, l.FechaVencimiento) =
                     CONVERT(date, '20991231', 112)
                  OR l.FechaVencimiento >= @FechaHoy
              )
        )
    FROM dbo.Producto p WITH (UPDLOCK, HOLDLOCK)
    WHERE p.Id = @ProductoId;

    IF @@ROWCOUNT <> 1
        THROW 52414, 'No se encontró el producto para actualizar su saldo disponible.', 1;
END;

GO
/****** Object:  StoredProcedure [dbo].[sp_RegistrarMovimientoCaja]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[sp_RegistrarMovimientoCaja]
    @CajaId int,
    @UsuarioId int,
    @TipoMovimiento nvarchar(10),
    @Importe decimal(18,2),
    @Motivo nvarchar(500),
    @ClaveOperacion uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    -- PacuIbera.MovimientoCaja.v1

    SET @Motivo = LTRIM(RTRIM(@Motivo));

    IF @CajaId IS NULL OR @CajaId <= 0 OR @UsuarioId IS NULL OR @UsuarioId <= 0
        THROW 52910, 'Seleccione una caja y un usuario validos.', 1;
    IF @TipoMovimiento IS NULL OR @TipoMovimiento NOT IN (N'Ingreso', N'Retiro')
        THROW 52911, 'El tipo debe ser Ingreso o Retiro.', 1;
    IF @Importe IS NULL OR @Importe <= 0
        THROW 52912, 'El importe debe ser mayor que cero.', 1;
    IF @Motivo IS NULL OR LEN(@Motivo) = 0
        THROW 52913, 'El motivo es obligatorio.', 1;
    IF @ClaveOperacion IS NULL OR @ClaveOperacion = '00000000-0000-0000-0000-000000000000'
        THROW 52914, 'La clave de operacion no es valida.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @Lock int, @Recurso nvarchar(255);
        SET @Recurso = N'PacuIbera.MovimientoCaja.Operacion.' + CONVERT(nvarchar(36), @ClaveOperacion);
        EXEC @Lock = sys.sp_getapplock @Resource = @Recurso, @LockMode = N'Exclusive',
             @LockOwner = N'Transaction', @LockTimeout = 15000, @DbPrincipal = N'public';
        IF @Lock < 0
            THROW 52915, 'No se pudo asegurar la operacion. Intente nuevamente.', 1;

        IF NOT EXISTS
        (
            SELECT 1 FROM dbo.Usuario u WITH (HOLDLOCK)
            INNER JOIN dbo.Rol r WITH (HOLDLOCK) ON r.Id = u.RolId
            WHERE u.Id = @UsuarioId AND u.Activo = 1 AND r.Nombre = N'Administrador'
        )
            THROW 52916, 'Solo un Administrador activo puede registrar ingresos o retiros de efectivo.', 1;

        DECLARE @ExistenteId int, @CajaAnterior int, @UsuarioAnterior int,
                @TipoAnterior nvarchar(10), @ImporteAnterior decimal(18,2),
                @MotivoAnterior nvarchar(500);
        SELECT @ExistenteId = Id, @CajaAnterior = CajaId, @UsuarioAnterior = UsuarioId,
               @TipoAnterior = TipoMovimiento, @ImporteAnterior = Importe, @MotivoAnterior = Motivo
        FROM dbo.MovimientoCaja WITH (UPDLOCK, HOLDLOCK)
        WHERE ClaveOperacion = @ClaveOperacion;

        IF @ExistenteId IS NOT NULL
        BEGIN
            IF @CajaAnterior <> @CajaId OR @UsuarioAnterior <> @UsuarioId
               OR @TipoAnterior <> @TipoMovimiento OR @ImporteAnterior <> @Importe
               OR CONVERT(varbinary(max), @MotivoAnterior) <> CONVERT(varbinary(max), @Motivo)
                THROW 52917, 'La clave de operacion ya existe con datos diferentes.', 1;

            COMMIT TRANSACTION;
            SELECT Id AS MovimientoCajaId, CajaId, UsuarioId, TipoMovimiento, Importe,
                   Motivo, FechaHora, ClaveOperacion, CONVERT(bit, 1) AS Reutilizado
            FROM dbo.MovimientoCaja WHERE Id = @ExistenteId;
            RETURN;
        END;

        DECLARE @BloqueoCaja int;
        SET @Recurso = N'PacuIbera.MovimientoCaja.Caja.' + CONVERT(nvarchar(20), @CajaId);
        EXEC @BloqueoCaja = sys.sp_getapplock @Resource = @Recurso, @LockMode = N'Exclusive',
             @LockOwner = N'Transaction', @LockTimeout = 15000, @DbPrincipal = N'public';
        IF @BloqueoCaja < 0
            THROW 52918, 'No se pudo bloquear la caja. Intente nuevamente.', 1;

        DECLARE @EstadoCaja nvarchar(30), @Fondo decimal(18,2);
        SELECT @EstadoCaja = Estado, @Fondo = SaldoInicial
        FROM dbo.Caja WITH (XLOCK, HOLDLOCK)
        WHERE Id = @CajaId;
        IF @EstadoCaja IS NULL
            THROW 52919, 'La caja seleccionada no existe.', 1;
        IF @EstadoCaja <> N'Abierta'
            THROW 52920, 'Solo se pueden registrar movimientos en una caja abierta.', 1;
        IF @Fondo IS NULL
            THROW 52921, 'No se puede calcular el efectivo: la caja no tiene fondo inicial registrado.', 1;

        IF EXISTS
        (
            SELECT 1 FROM dbo.Venta v WITH (HOLDLOCK)
            WHERE v.CajaId = @CajaId AND v.Estado IS NULL
        )
            THROW 52927, 'No se puede calcular el efectivo: hay ventas sin estado definido en esta caja.', 1;

        IF EXISTS
        (
            SELECT 1
            FROM (VALUES
                (1, N'Efectivo'), (1002, N'Efectivo'), (1006, N'Efectivo'), (2006, N'Efectivo'),
                (2, N'Debito'), (1003, N'Debito'), (1007, N'Debito'), (2007, N'Debito'),
                (3, N'Credito'), (1004, N'Credito'), (1008, N'Credito'), (2008, N'Credito'),
                (4, N'Transferencia'), (1005, N'Transferencia'), (1009, N'Transferencia'), (2009, N'Transferencia')
            ) esperado(Id, Categoria)
            LEFT JOIN dbo.MetodoPago mp WITH (HOLDLOCK) ON mp.Id = esperado.Id
            WHERE mp.Id IS NULL
               OR (esperado.Categoria = N'Efectivo' AND LOWER(mp.Nombre) NOT LIKE N'%efectivo%')
               OR (esperado.Categoria = N'Debito' AND mp.Nombre COLLATE Latin1_General_100_CI_AI NOT LIKE N'%debito%')
               OR (esperado.Categoria = N'Credito' AND mp.Nombre COLLATE Latin1_General_100_CI_AI NOT LIKE N'%credito%')
               OR (esperado.Categoria = N'Transferencia'
                   AND LOWER(mp.Nombre) NOT LIKE N'%transfer%'
                   AND LOWER(mp.Nombre) NOT LIKE N'%mercado pago%')
        )
            THROW 52928, 'Los nombres de los medios de pago cambiaron; no se puede clasificar el efectivo con seguridad.', 1;

        -- No tomar decisiones mientras haya efectivo en ventas anuladas sin devolucion verificable.
        IF EXISTS
        (
            SELECT 1
            FROM dbo.Venta v WITH (HOLDLOCK)
            INNER JOIN dbo.VentaPago vp WITH (HOLDLOCK) ON vp.VentaId = v.Id
            INNER JOIN dbo.MetodoPago mp WITH (HOLDLOCK) ON mp.Id = vp.MetodoPagoId
            WHERE v.CajaId = @CajaId AND v.Estado = N'Anulada'
              AND mp.Id IN (1,1002,1006,2006)
        )
            THROW 52922, 'No se puede calcular el saldo con seguridad: hay efectivo asociado a ventas anuladas sin devolucion verificada.', 1;

        -- Todo pago vinculado a la caja debe pertenecer a uno de los medios conocidos.
        IF EXISTS
        (
            SELECT 1
            FROM dbo.Venta v WITH (HOLDLOCK)
            INNER JOIN dbo.VentaPago vp WITH (HOLDLOCK) ON vp.VentaId = v.Id
            LEFT JOIN dbo.MetodoPago mp WITH (HOLDLOCK) ON mp.Id = vp.MetodoPagoId
            WHERE v.CajaId = @CajaId
              AND (mp.Id IS NULL OR mp.Id NOT IN (1,1002,1006,2006,2,1003,1007,2007,3,1004,1008,2008,4,1005,1009,2009))
        )
            THROW 52923, 'No se puede calcular el efectivo: la caja tiene pagos con un medio no clasificado.', 1;

        DECLARE @CobrosEfectivo decimal(38,2) = 0,
                @Ingresos decimal(38,2) = 0,
                @Retiros decimal(38,2) = 0,
                @Disponible decimal(38,2),
                @MovimientoId int;

        -- Una fila por venta evita sumar la venta repetidamente al tener pagos mixtos.
        DECLARE @PagosPorVenta TABLE
        (
            VentaId int NOT NULL PRIMARY KEY,
            Estado nvarchar(30) NULL,
            Total decimal(18,2) NULL,
            EfectivoEntregado decimal(38,2) NOT NULL,
            OtrosMedios decimal(38,2) NOT NULL
        );

        INSERT INTO @PagosPorVenta (VentaId, Estado, Total, EfectivoEntregado, OtrosMedios)
        SELECT v.Id, v.Estado, v.Total,
               COALESCE(SUM(CASE WHEN mp.Id IN (1,1002,1006,2006)
                                 THEN vp.MontoCobrado ELSE CONVERT(decimal(18,2),0) END),0),
               COALESCE(SUM(CASE WHEN mp.Id IN (2,1003,1007,2007,3,1004,1008,2008,4,1005,1009,2009)
                                 THEN vp.MontoCobrado ELSE CONVERT(decimal(18,2),0) END),0)
        FROM dbo.Venta v WITH (HOLDLOCK)
        LEFT JOIN dbo.VentaPago vp WITH (HOLDLOCK) ON vp.VentaId = v.Id
        LEFT JOIN dbo.MetodoPago mp WITH (HOLDLOCK) ON mp.Id = vp.MetodoPagoId
        WHERE v.CajaId = @CajaId
        GROUP BY v.Id, v.Estado, v.Total;

        IF EXISTS
        (
            SELECT 1 FROM @PagosPorVenta
            WHERE Estado <> N'Anulada'
              AND (Total IS NULL OR OtrosMedios > Total OR EfectivoEntregado + OtrosMedios < Total)
        )
            THROW 52929, 'No se puede calcular el efectivo: hay ventas vigentes cuyos pagos no concilian con el total.', 1;

        SELECT @CobrosEfectivo = COALESCE(SUM(
                   CASE WHEN EfectivoEntregado > Total - OtrosMedios
                        THEN Total - OtrosMedios ELSE EfectivoEntregado END), 0)
        FROM @PagosPorVenta
        WHERE Estado <> N'Anulada';

        SELECT @Ingresos = COALESCE(SUM(Importe), 0)
        FROM dbo.MovimientoCaja WITH (UPDLOCK, HOLDLOCK)
        WHERE CajaId = @CajaId AND TipoMovimiento = N'Ingreso';

        SELECT @Retiros = COALESCE(SUM(Importe), 0)
        FROM dbo.MovimientoCaja WITH (UPDLOCK, HOLDLOCK)
        WHERE CajaId = @CajaId AND TipoMovimiento = N'Retiro';

        SET @Disponible = CONVERT(decimal(38,2), @Fondo) + @CobrosEfectivo + @Ingresos - @Retiros;
        IF @Disponible < 0
            THROW 52924, 'El efectivo calculado de la caja es inconsistente; no se registro el movimiento.', 1;
        IF @TipoMovimiento = N'Retiro' AND @Importe > @Disponible
            THROW 52925, 'El retiro supera el efectivo disponible en la caja.', 1;

        INSERT dbo.MovimientoCaja (CajaId, UsuarioId, TipoMovimiento, Importe, Motivo, ClaveOperacion)
        VALUES (@CajaId, @UsuarioId, @TipoMovimiento, @Importe, @Motivo, @ClaveOperacion);
        SET @MovimientoId = CONVERT(int, SCOPE_IDENTITY());

        COMMIT TRANSACTION;

        SELECT Id AS MovimientoCajaId, CajaId, UsuarioId, TipoMovimiento, Importe,
               Motivo, FechaHora, ClaveOperacion, CONVERT(bit, 0) AS Reutilizado
        FROM dbo.MovimientoCaja WHERE Id = @MovimientoId;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_RegistrarPagoVenta]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[sp_RegistrarPagoVenta]
    @VentaId INT, @MetodoPagoId INT, @MontoCobrado DECIMAL(10,2)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO VentaPago (VentaId, MetodoPagoId, MontoCobrado)
    VALUES (@VentaId, @MetodoPagoId, @MontoCobrado);
END
GO
/****** Object:  StoredProcedure [dbo].[sp_RegistrarProducto]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[sp_RegistrarProducto]
    @Nombre VARCHAR(100),
    @CategoriaId INT,
    @PrecioVenta DECIMAL(10,2),
    @StockMinimo DECIMAL(10,3),
    @SeVendePorPeso BIT,
    @Descripcion VARCHAR(50),
    @StockInicial DECIMAL(10,3),
    @FechaVencimiento DATE
AS
BEGIN
    SET NOCOUNT ON;

    -- ========================================================
    -- VALIDACIÓN ESTRICTA: No permitir cargar productos en 0
    -- ========================================================
    IF @StockInicial <= 0
    BEGIN
        -- El error 16 hace que C# lo atrape automáticamente en el catch(SqlException)
        RAISERROR('No se puede registrar un producto nuevo con stock 0. Ingrese el inventario inicial.', 16, 1);
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @NuevoProductoId INT;

        -- 1. Insertar el Producto Maestro
        INSERT INTO Producto (Nombre, CategoriaId, PrecioVenta, StockMinimo, SeVendePorPeso, CodigoBarras, Activo, StockActual)
        VALUES (@Nombre, @CategoriaId, @PrecioVenta, @StockMinimo, @SeVendePorPeso, @Descripcion, 1, @StockInicial);

        -- Capturamos el ID del producto recién creado
        SET @NuevoProductoId = SCOPE_IDENTITY();

        -- 2. Creamos el Lote obligatorio (como ya validamos que es > 0, lo inserta seguro)
        INSERT INTO Lote (ProductoId, FechaIngreso, FechaVencimiento, StockInicial, StockActual, Activo)
        VALUES (@NuevoProductoId, GETDATE(), @FechaVencimiento, @StockInicial, @StockInicial, 1);

        COMMIT TRANSACTION;

        -- Devolvemos el ID por si la aplicación lo necesita
        SELECT @NuevoProductoId AS NuevoId;

    END TRY
    BEGIN CATCH
        -- Si algo falla (ej. base de datos caída), deshacemos todo
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[sp_RegistrarVenta]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[sp_RegistrarVenta]
    @CajaId INT,
    @UsuarioId INT,
    @ClienteId INT = NULL,
    @Total DECIMAL(10,2)
AS
BEGIN
    SET NOCOUNT ON;
    -- PacuIbera.Caja.VentaConCajaAbierta.v1

    IF @@TRANCOUNT = 0
        THROW 53210, 'La venta debe ejecutarse dentro de la transaccion administrada por la aplicacion.', 1;
    IF @CajaId IS NULL OR @CajaId <= 0 OR @UsuarioId IS NULL OR @UsuarioId <= 0
        THROW 53211, 'La caja y el vendedor deben ser validos.', 1;

    DECLARE @Lock int, @Recurso nvarchar(255), @Estado nvarchar(30), @UsuarioCaja int;
    SET @Recurso = N'PacuIbera.MovimientoCaja.Caja.' + CONVERT(nvarchar(20), @CajaId);
    EXEC @Lock = sys.sp_getapplock
        @Resource = @Recurso, @LockMode = N'Exclusive',
        @LockOwner = N'Transaction', @LockTimeout = 15000, @DbPrincipal = N'public';
    IF @Lock < 0
        THROW 53212, 'No se pudo asegurar la caja para registrar la venta.', 1;

    -- Mismo orden de bloqueo que sp_CerrarCaja y sp_RegistrarMovimientoCaja:
    -- applock por CajaId y luego bloqueo de la fila Caja.
    SELECT @Estado = Estado, @UsuarioCaja = UsuarioId
    FROM dbo.Caja WITH (UPDLOCK, HOLDLOCK)
    WHERE Id = @CajaId;

    IF @Estado IS NULL
        THROW 53213, 'La caja seleccionada no existe.', 1;
    IF @Estado <> N'Abierta'
        THROW 53214, 'La caja ya esta cerrada. Actualice la caja antes de vender.', 1;
    IF @UsuarioCaja <> @UsuarioId
        THROW 53215, 'La caja abierta no pertenece al vendedor conectado.', 1;

    INSERT INTO dbo.Venta (CajaId, UsuarioId, ClienteId, FechaHora, Total, Estado)
    VALUES (@CajaId, @UsuarioId, @ClienteId, GETDATE(), @Total, N'Completada');

    SELECT SCOPE_IDENTITY() AS VentaId;
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_ReporteAuditoria]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 1. Auditoría con Total para el ticket
CREATE   PROCEDURE [dbo].[sp_ReporteAuditoria]
    @FechaDesde DATE,
    @FechaHasta DATE
AS BEGIN
    SELECT
        VA.Fecha,
        V.Id AS [Nro Venta],
        U.Nombre + ' ' + U.Apellido AS [Cajero que Anuló],
        V.Total,
        VA.Motivo
    FROM VentaAnulacion VA
    INNER JOIN Venta V ON VA.VentaId = V.Id
    INNER JOIN Usuario U ON VA.UsuarioId = U.Id
    WHERE CAST(VA.Fecha AS DATE) >= @FechaDesde
      AND CAST(VA.Fecha AS DATE) <= @FechaHasta
    ORDER BY VA.Fecha DESC;
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_ReporteLotesVencidos]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[sp_ReporteLotesVencidos]
AS BEGIN
    SELECT
        P.Nombre AS Producto,
        CAST(L.FechaVencimiento AS DATE) AS Vence,
        L.StockActual AS Stock,
        CASE
            WHEN L.FechaVencimiento < CAST(GETDATE() AS DATE) THEN 'Vencido'
            ELSE 'Pronto'
        END AS Estado
    FROM Lote L
    INNER JOIN Producto P ON L.ProductoId = P.Id
    WHERE L.StockActual > 0
      AND L.Activo = 1
      AND L.FechaVencimiento <= DATEADD(day, 15, GETDATE())
    ORDER BY L.FechaVencimiento ASC
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_ReporteMetodosPago]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Actualizamos Métodos de Pago para que respete el calendario
CREATE   PROCEDURE [dbo].[sp_ReporteMetodosPago]
    @FechaDesde DATE,
    @FechaHasta DATE
AS BEGIN
    SELECT
        M.Nombre,
        SUM(P.MontoCobrado) AS Total
    FROM VentaPago P
    INNER JOIN MetodoPago M ON P.MetodoPagoId = M.Id
    INNER JOIN Venta V ON P.VentaId = V.Id
    WHERE V.Estado != 'Anulada'
      AND CAST(V.FechaHora AS DATE) >= @FechaDesde
      AND CAST(V.FechaHora AS DATE) <= @FechaHasta
    GROUP BY M.Nombre;
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_ReporteRankingVendedores]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 2. Ranking de Rendimiento: Vendedor del Mes
CREATE   PROCEDURE [dbo].[sp_ReporteRankingVendedores]
    @FechaDesde DATE,
    @FechaHasta DATE
AS BEGIN
    SELECT TOP 5
        U.Nombre + ' ' + U.Apellido AS Vendedor,
        COUNT(V.Id) AS [Cantidad Ventas],
        SUM(V.Total) AS [Total Recaudado]
    FROM Venta V
    INNER JOIN Usuario U ON V.UsuarioId = U.Id
    WHERE V.Estado != 'Anulada'
      AND CAST(V.FechaHora AS DATE) >= @FechaDesde
      AND CAST(V.FechaHora AS DATE) <= @FechaHasta
    GROUP BY U.Nombre, U.Apellido
    ORDER BY [Total Recaudado] DESC;
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_ReporteResumenGlobal]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 3. Resumen Global: Termómetro del Negocio
CREATE   PROCEDURE [dbo].[sp_ReporteResumenGlobal]
    @FechaDesde DATE,
    @FechaHasta DATE
AS BEGIN
    SELECT
        ISNULL(SUM(Total), 0) AS TotalRecaudado,
        COUNT(Id) AS CantidadVentas,
        -- Calculamos el Ticket Promedio evitando el error de dividir por cero
        ISNULL(SUM(Total) / NULLIF(COUNT(Id), 0), 0) AS TicketPromedio
    FROM Venta
    WHERE Estado != 'Anulada'
      AND CAST(FechaHora AS DATE) >= @FechaDesde
      AND CAST(FechaHora AS DATE) <= @FechaHasta;
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_ReporteStockCritico]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[sp_ReporteStockCritico]
AS BEGIN
    SELECT
        Nombre AS Producto,
        StockActual AS Stock,
        'Crítico' AS Estado
    FROM Producto
    WHERE StockActual <= StockMinimo
      AND Activo = 1 -- LA MAGIA ESTÁ ACÁ: Ignora los productos dados de baja
    ORDER BY StockActual ASC;
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_ReporteTopClientes]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 2. Ranking de mejores clientes para la pestaña "Top Clientes"
CREATE   PROCEDURE [dbo].[sp_ReporteTopClientes]
AS BEGIN
    SELECT TOP 20
        C.Nombre + ' ' + C.Apellido AS Cliente,
        C.Telefono,
        COUNT(V.Id) AS CantidadCompras,
        SUM(V.Total) AS TotalGastado
    FROM Cliente C
    INNER JOIN Venta V ON C.Id = V.ClienteId
    WHERE V.Estado = 'Completada'
    GROUP BY C.Id, C.Nombre, C.Apellido, C.Telefono
    ORDER BY TotalGastado DESC
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_ReporteTopProductos]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Actualizamos Top Productos para que respete el calendario
CREATE   PROCEDURE [dbo].[sp_ReporteTopProductos]
    @FechaDesde DATE,
    @FechaHasta DATE
AS BEGIN
    SELECT TOP 5
        P.Nombre,
        SUM(D.Cantidad) AS TotalVendido
    FROM DetalleVenta D
    INNER JOIN Producto P ON D.ProductoId = P.Id
    INNER JOIN Venta V ON D.VentaId = V.Id
    WHERE V.Estado != 'Anulada'
      AND CAST(V.FechaHora AS DATE) >= @FechaDesde
      AND CAST(V.FechaHora AS DATE) <= @FechaHasta
    GROUP BY P.Nombre
    ORDER BY TotalVendido DESC;
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_ResolverDiferenciaRemito]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[sp_ResolverDiferenciaRemito]
 @RemitoId int,
 @ClaveOperacion uniqueidentifier,
 @UsuarioResponsableId int,
 @Tipo nvarchar(max),
 @Motivo nvarchar(max),
 @Lotes dbo.ResolucionDiferenciaEntrada READONLY
AS
BEGIN
 SET NOCOUNT ON;
 IF @@TRANCOUNT<>0
 BEGIN
  RAISERROR('La resolucion requiere llamada sin transaccion externa.',16,1);
  RETURN;
 END;
 SET IMPLICIT_TRANSACTIONS OFF;
 SET XACT_ABORT ON;
 SET ANSI_PADDING ON;
 SET ANSI_WARNINGS ON;
 SET ARITHABORT ON;
 SET CONCAT_NULL_YIELDS_NULL ON;
 SET NUMERIC_ROUNDABORT OFF;
 DECLARE @TipoCanon nvarchar(30),@Id int,@RemitoAnterior int,@UsuarioAnterior int,
  @TipoAnterior nvarchar(30),@MotivoAnterior nvarchar(500),@FechaAnterior datetime2(0),@EstadoAnterior nvarchar(20);
 DECLARE @EstadoRemito nvarchar(30),@RecepcionId int,@FechaRecepcion datetime2(0),@Ahora datetime2(0),@Hoy date;
 DECLARE @Lock int,@Recurso nvarchar(255),@ProductoId int,@PorPeso bit,@Activo bit,
  @SaldoLocal decimal(10,3),@TotalProducto decimal(38,3);
 DECLARE @DiferenciaId int,@Cantidad decimal(10,3),@Vence datetime2(0),@LocalId int,@DepositoId int;
 DECLARE @Pendientes TABLE
 (DiferenciaId int PRIMARY KEY,ProductoId int NOT NULL,RemitoLoteId int NOT NULL,DepositoOrigenId int NOT NULL,
  Enviada decimal(10,3) NOT NULL,Recibida decimal(10,3) NOT NULL,Diferencia decimal(10,3) NOT NULL,
  Resuelto decimal(38,3) NOT NULL,Vencimiento datetime2(0) NULL);
 DECLARE @Trabajo TABLE
 (DiferenciaId int PRIMARY KEY,ProductoId int NOT NULL,Cantidad decimal(10,3) NOT NULL,Vencimiento datetime2(0) NULL);
 BEGIN TRY
  IF @RemitoId IS NULL OR @RemitoId<=0 OR @UsuarioResponsableId IS NULL
   THROW 52010,'Remito y responsable son obligatorios.',1;
  IF @ClaveOperacion IS NULL OR @ClaveOperacion='00000000-0000-0000-0000-000000000000'
   THROW 52011,'La clave de operacion es obligatoria y no puede ser vacia.',1;
  SET @TipoCanon=CASE @Tipo WHEN N'Recepción posterior' THEN N'Recepción posterior'
   WHEN N'Devolución depósito' THEN N'Devolución depósito' WHEN N'Pérdida documentada' THEN N'Pérdida documentada' END;
  IF @TipoCanon IS NULL THROW 52012,'Tipo de resolucion no admitido.',1;
  IF @Motivo IS NULL OR LEN(LTRIM(RTRIM(@Motivo)))=0 OR DATALENGTH(@Motivo)>1000
   THROW 52013,'El motivo es obligatorio y debe caber en nvarchar(500), sin truncarlo.',1;
  IF NOT EXISTS (SELECT 1 FROM @Lotes) OR EXISTS (SELECT 1 FROM @Lotes WHERE RecepcionRemitoLoteId<=0 OR Cantidad<=0)
   THROW 52014,'Indique al menos una diferencia con cantidad positiva.',1;
  BEGIN TRANSACTION;
  SET @Recurso=N'PacuIbera.ResolverDiferencia.'+CONVERT(nvarchar(36),@ClaveOperacion);
  EXEC @Lock=sys.sp_getapplock @Resource=@Recurso,@LockMode=N'Exclusive',@LockOwner=N'Transaction',@LockTimeout=15000;
  IF @Lock<0 THROW 52015,'Operacion ocupada: consultar la clave antes de reintentar.',1;
  IF NOT EXISTS (SELECT 1 FROM dbo.Usuario u WITH (HOLDLOCK) INNER JOIN dbo.Rol r WITH (HOLDLOCK) ON r.Id=u.RolId
   WHERE u.Id=@UsuarioResponsableId AND u.Activo=1 AND r.Nombre=N'Administrador')
   THROW 52016,'El responsable debe ser un Administrador activo.',1;
  SELECT @EstadoRemito=Estado FROM dbo.RemitoInterno WITH (UPDLOCK,HOLDLOCK) WHERE Id=@RemitoId;
  IF @EstadoRemito IS NULL THROW 52017,'El remito no existe.',1;

  SELECT @Id=Id,@RemitoAnterior=RemitoId,@UsuarioAnterior=UsuarioResponsableId,@TipoAnterior=Tipo,
   @MotivoAnterior=Motivo,@FechaAnterior=FechaResolucion,@EstadoAnterior=Estado
  FROM dbo.ResolucionDiferencia WITH (UPDLOCK,HOLDLOCK) WHERE ClaveOperacion=@ClaveOperacion;
  IF @Id IS NOT NULL
  BEGIN
   IF @RemitoAnterior<>@RemitoId OR @UsuarioAnterior<>@UsuarioResponsableId OR @TipoAnterior<>@TipoCanon
      OR CONVERT(varbinary(max),@MotivoAnterior)<>CONVERT(varbinary(max),@Motivo)
    THROW 52018,'La clave ya corresponde a otro remito, responsable, tipo o motivo.',1;
   IF @EstadoAnterior<>N'Aplicada' THROW 52019,'La resolucion original no esta Aplicada; requiere revision.',1;
   IF EXISTS (SELECT RecepcionRemitoLoteId,Cantidad FROM @Lotes EXCEPT
              SELECT RecepcionRemitoLoteId,Cantidad FROM dbo.ResolucionDiferenciaDetalle WHERE ResolucionId=@Id)
      OR EXISTS (SELECT RecepcionRemitoLoteId,Cantidad FROM dbo.ResolucionDiferenciaDetalle WHERE ResolucionId=@Id
                 EXCEPT SELECT RecepcionRemitoLoteId,Cantidad FROM @Lotes)
    THROW 52020,'La clave ya fue usada con diferencias o cantidades distintas.',1;
   COMMIT TRANSACTION;
   SELECT @Id AS ResolucionId,@RemitoId AS RemitoId,@TipoAnterior AS Tipo,@FechaAnterior AS FechaResolucion,
    @EstadoAnterior AS EstadoResolucion,@EstadoRemito AS EstadoRemitoActual,CONVERT(bit,1) AS Reutilizado;
   RETURN;
  END;
  IF @EstadoRemito<>N'Con diferencias' THROW 52021,'Solo se resuelven remitos Con diferencias.',1;
  SELECT @RecepcionId=Id,@FechaRecepcion=FechaRecepcion FROM dbo.RecepcionRemito WITH (HOLDLOCK)
   WHERE RemitoId=@RemitoId AND EstadoResultado=N'Con diferencias';
  IF @RecepcionId IS NULL THROW 52022,'Falta una recepcion definitiva Con diferencias.',1;
  IF EXISTS (SELECT 1 FROM dbo.ResolucionDiferencia WITH (HOLDLOCK) WHERE RemitoId=@RemitoId AND Estado<>N'Aplicada')
   THROW 52023,'Existen resoluciones con estado no soportado.',1;
  INSERT @Pendientes (DiferenciaId,ProductoId,RemitoLoteId,DepositoOrigenId,Enviada,Recibida,Diferencia,Resuelto,Vencimiento)
  SELECT rr.Id,rd.ProductoId,rl.Id,rl.DepositoStockId,rr.CantidadEnviada,rr.CantidadRecibida,rr.CantidadDiferencia,
   ISNULL(s.Cantidad,0),rl.FechaVencimiento
  FROM dbo.RecepcionRemitoLote rr WITH (HOLDLOCK)
  INNER JOIN dbo.RemitoLote rl WITH (HOLDLOCK) ON rl.Id=rr.RemitoLoteId AND rl.RemitoId=rr.RemitoId
  INNER JOIN dbo.RemitoDetalle rd WITH (HOLDLOCK) ON rd.Id=rl.RemitoDetalleId AND rd.RemitoId=rl.RemitoId
  OUTER APPLY
  (
   SELECT SUM(d.Cantidad) AS Cantidad FROM dbo.ResolucionDiferenciaDetalle d WITH (HOLDLOCK)
   INNER JOIN dbo.ResolucionDiferencia h WITH (HOLDLOCK) ON h.Id=d.ResolucionId AND h.RemitoId=d.RemitoId
   WHERE d.RecepcionRemitoLoteId=rr.Id AND d.RemitoId=@RemitoId AND h.Estado=N'Aplicada'
  ) s
  WHERE rr.RemitoId=@RemitoId AND rr.RecepcionRemitoId=@RecepcionId;
  IF NOT EXISTS (SELECT 1 FROM @Pendientes)
     OR (SELECT COUNT(*) FROM @Pendientes)<>(SELECT COUNT(*) FROM dbo.RecepcionRemitoLote WHERE RecepcionRemitoId=@RecepcionId)
   THROW 52024,'Recepcion sin lotes o con trazabilidad inconsistente.',1;
  IF EXISTS (SELECT 1 FROM @Pendientes WHERE Diferencia<0 OR Recibida<0 OR Enviada<Recibida
     OR CONVERT(decimal(18,3),Enviada)-CONVERT(decimal(18,3),Recibida)<>Diferencia
     OR Resuelto<0 OR Resuelto>Diferencia)
   THROW 52025,'Las diferencias o resoluciones historicas no concilian; no se corrigen automaticamente.',1;
  INSERT @Trabajo SELECT p.DiferenciaId,p.ProductoId,e.Cantidad,p.Vencimiento
   FROM @Lotes e INNER JOIN @Pendientes p ON p.DiferenciaId=e.RecepcionRemitoLoteId;
  IF (SELECT COUNT(*) FROM @Trabajo)<>(SELECT COUNT(*) FROM @Lotes)
   THROW 52026,'Se informaron diferencias ajenas a la recepcion del remito.',1;
  -- Resuelto se compara antes de acotarlo: resta decimal(18,3), sin perder escala.
  IF EXISTS (SELECT 1 FROM @Trabajo t INNER JOIN @Pendientes p ON p.DiferenciaId=t.DiferenciaId
             WHERE t.Cantidad>CONVERT(decimal(18,3),p.Diferencia)-CONVERT(decimal(18,3),p.Resuelto))
   THROW 52027,'Una cantidad supera el saldo pendiente de la diferencia.',1;

  DECLARE validarProductos CURSOR LOCAL FAST_FORWARD FOR
   SELECT ProductoId,SUM(Cantidad) FROM @Trabajo GROUP BY ProductoId ORDER BY ProductoId;
  OPEN validarProductos;
  FETCH NEXT FROM validarProductos INTO @ProductoId,@TotalProducto;
  WHILE @@FETCH_STATUS=0
  BEGIN
   SET @PorPeso=NULL;
   SELECT @PorPeso=SeVendePorPeso,@Activo=Activo,@SaldoLocal=StockActual
   FROM dbo.Producto WITH (UPDLOCK,HOLDLOCK) WHERE Id=@ProductoId;
   IF @PorPeso IS NULL THROW 52028,'Producto inexistente o sin unidad de venta definida.',1;
   IF @PorPeso=0 AND
      (EXISTS (SELECT 1 FROM @Trabajo WHERE ProductoId=@ProductoId AND Cantidad<>FLOOR(Cantidad))
       OR EXISTS (SELECT 1 FROM @Pendientes WHERE ProductoId=@ProductoId
                  AND (Enviada<>FLOOR(Enviada) OR Recibida<>FLOOR(Recibida) OR Resuelto<>FLOOR(Resuelto))))
    THROW 52029,'Un producto por unidad tiene cantidades fraccionarias; requiere conciliacion.',1;
   IF @TipoCanon=N'Recepción posterior'
   BEGIN
    IF ISNULL(@Activo,0)<>1 OR @SaldoLocal IS NULL OR @SaldoLocal<0
     THROW 52030,'La entrega posterior requiere producto activo y saldo local valido.',1;
    IF @PorPeso=0 AND @SaldoLocal<>FLOOR(@SaldoLocal)
     THROW 52031,'El saldo local del producto por unidad es fraccionario.',1;
    IF @TotalProducto>9999999.999-CONVERT(decimal(18,3),@SaldoLocal)
     THROW 52032,'El ingreso excederia decimal(10,3) del saldo local.',1;
   END;
   FETCH NEXT FROM validarProductos INTO @ProductoId,@TotalProducto;
  END;
  CLOSE validarProductos;
  DEALLOCATE validarProductos;
  SET @Ahora=SYSDATETIME(); SET @Hoy=CONVERT(date,@Ahora);
  IF @Ahora<@FechaRecepcion THROW 52033,'La resolucion no puede ser anterior a la recepcion.',1;
  IF @TipoCanon=N'Recepción posterior' AND EXISTS (SELECT 1 FROM @Trabajo WHERE Vencimiento<@Hoy)
   THROW 52034,'No se acredita mercaderia vencida al stock vendible; corresponde otra resolucion justificada.',1;
  INSERT dbo.ResolucionDiferencia (RemitoId,ClaveOperacion,UsuarioResponsableId,FechaResolucion,Tipo,Estado,Motivo)
   VALUES (@RemitoId,@ClaveOperacion,@UsuarioResponsableId,@Ahora,@TipoCanon,N'Aplicada',CONVERT(nvarchar(500),@Motivo));
  SET @Id=CONVERT(int,SCOPE_IDENTITY());
  DECLARE aplicarLotes CURSOR LOCAL FAST_FORWARD FOR
   SELECT DiferenciaId,ProductoId,Cantidad,Vencimiento FROM @Trabajo ORDER BY ProductoId,DiferenciaId;
  OPEN aplicarLotes;
  FETCH NEXT FROM aplicarLotes INTO @DiferenciaId,@ProductoId,@Cantidad,@Vence;
  WHILE @@FETCH_STATUS=0
  BEGIN
   SET @LocalId=NULL; SET @DepositoId=NULL;
   IF @TipoCanon=N'Recepción posterior'
   BEGIN
    INSERT dbo.Lote (ProductoId,FechaIngreso,FechaVencimiento,StockInicial,StockActual,Activo)
     VALUES (@ProductoId,@Ahora,@Vence,@Cantidad,@Cantidad,1);
    SET @LocalId=CONVERT(int,SCOPE_IDENTITY());
   END;
   IF @TipoCanon=N'Devolución depósito'
   BEGIN
    SELECT @Activo=Activo FROM dbo.Producto WHERE Id=@ProductoId;
    INSERT dbo.DepositoStock (ProductoId,FechaIngreso,FechaVencimiento,StockInicial,StockActual,Activo)
     VALUES (@ProductoId,@Ahora,@Vence,@Cantidad,@Cantidad,
             CASE WHEN @Activo=1 AND (@Vence IS NULL OR @Vence>=@Hoy) THEN 1 ELSE 0 END);
    SET @DepositoId=CONVERT(int,SCOPE_IDENTITY());
   END;
   INSERT dbo.ResolucionDiferenciaDetalle
    (ResolucionId,RemitoId,Tipo,RecepcionRemitoLoteId,Cantidad,LoteLocalDestinoId,DepositoStockDestinoId)
    VALUES (@Id,@RemitoId,@TipoCanon,@DiferenciaId,@Cantidad,@LocalId,@DepositoId);
   FETCH NEXT FROM aplicarLotes INTO @DiferenciaId,@ProductoId,@Cantidad,@Vence;
  END;
  CLOSE aplicarLotes;
  DEALLOCATE aplicarLotes;
  IF @TipoCanon=N'Recepción posterior'
   UPDATE p SET StockActual=p.StockActual+s.Cantidad
   FROM dbo.Producto p INNER JOIN
    (SELECT ProductoId,CONVERT(decimal(18,3),SUM(Cantidad)) AS Cantidad FROM @Trabajo GROUP BY ProductoId) s
    ON s.ProductoId=p.Id;
  -- Las sumas son decimal(38,3). Comparar sin sumarlas al decimal(10,3) original.
  IF EXISTS
  (
   SELECT 1 FROM @Pendientes p OUTER APPLY
    (SELECT SUM(Cantidad) AS Cantidad FROM dbo.ResolucionDiferenciaDetalle
     WHERE RemitoId=@RemitoId AND RecepcionRemitoLoteId=p.DiferenciaId) s
   WHERE ISNULL(s.Cantidad,0)>p.Diferencia
  ) THROW 52035,'Se excedio una diferencia; se revierte toda la operacion.',1;
  SET @EstadoRemito=CASE WHEN EXISTS
  (
   SELECT 1 FROM @Pendientes p OUTER APPLY
    (SELECT SUM(Cantidad) AS Cantidad FROM dbo.ResolucionDiferenciaDetalle
     WHERE RemitoId=@RemitoId AND RecepcionRemitoLoteId=p.DiferenciaId) s
   WHERE ISNULL(s.Cantidad,0)<p.Diferencia
  ) THEN N'Con diferencias' ELSE N'Cerrado' END;
  UPDATE dbo.RemitoInterno SET Estado=@EstadoRemito WHERE Id=@RemitoId AND Estado=N'Con diferencias';
  IF @@ROWCOUNT<>1 THROW 52036,'Cambio inesperado del remito: se revierte la resolucion.',1;
  COMMIT TRANSACTION;
  SELECT @Id AS ResolucionId,@RemitoId AS RemitoId,@TipoCanon AS Tipo,@Ahora AS FechaResolucion,
   N'Aplicada' AS EstadoResolucion,@EstadoRemito AS EstadoRemitoActual,CONVERT(bit,0) AS Reutilizado;
 END TRY
 BEGIN CATCH
  IF CURSOR_STATUS('local','validarProductos')>=0 CLOSE validarProductos;
  IF CURSOR_STATUS('local','validarProductos')>=-1 DEALLOCATE validarProductos;
  IF CURSOR_STATUS('local','aplicarLotes')>=0 CLOSE aplicarLotes;
  IF CURSOR_STATUS('local','aplicarLotes')>=-1 DEALLOCATE aplicarLotes;
  IF XACT_STATE()<>0 ROLLBACK TRANSACTION;
  THROW;
 END CATCH;
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_ResumenCajas]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[sp_ResumenCajas]
    @Fecha date,
    @CajaId int = NULL,
    @UsuarioId int = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Inicio date=@Fecha,@Fin date=DATEADD(day,1,@Fecha);

    IF @Fecha IS NULL
        THROW 53140, 'Seleccione una fecha valida para el resumen de caja.', 1;
    IF (@CajaId IS NULL AND @UsuarioId IS NOT NULL)
       OR (@CajaId IS NOT NULL AND (@CajaId<=0 OR @UsuarioId IS NULL OR @UsuarioId<=0))
        THROW 53141, 'Para consultar un cierre debe indicar una caja y un vendedor validos.', 1;
    IF @CajaId IS NOT NULL AND NOT EXISTS
    (
        SELECT 1
        FROM dbo.Caja c
        INNER JOIN dbo.Usuario u ON u.Id=c.UsuarioId
        INNER JOIN dbo.Rol r ON r.Id=u.RolId
        WHERE c.Id=@CajaId AND c.UsuarioId=@UsuarioId AND c.Estado=N'Abierta'
          AND u.Activo=1 AND r.Nombre=N'Vendedor'
    )
        THROW 53142, 'La caja no esta abierta o no pertenece a un Vendedor activo.', 1;
    DECLARE @Metodos TABLE(Id int NOT NULL PRIMARY KEY,Categoria nvarchar(20) NOT NULL);
    INSERT INTO @Metodos(Id,Categoria)
    VALUES (1,N'Efectivo'),(1002,N'Efectivo'),(1006,N'Efectivo'),(2006,N'Efectivo'),
           (2,N'Debito'),(1003,N'Debito'),(1007,N'Debito'),(2007,N'Debito'),
           (3,N'Credito'),(1004,N'Credito'),(1008,N'Credito'),(2008,N'Credito'),
           (4,N'Transferencia'),(1005,N'Transferencia'),(1009,N'Transferencia'),(2009,N'Transferencia');
    DECLARE @MetodosValidos bit=1;
    IF EXISTS
    (
        SELECT 1 FROM @Metodos e LEFT JOIN dbo.MetodoPago mp ON mp.Id=e.Id
        WHERE mp.Id IS NULL
           OR (e.Categoria=N'Efectivo' AND LOWER(mp.Nombre) NOT LIKE N'%efectivo%')
           OR (e.Categoria=N'Debito' AND mp.Nombre COLLATE Latin1_General_100_CI_AI NOT LIKE N'%debito%')
           OR (e.Categoria=N'Credito' AND mp.Nombre COLLATE Latin1_General_100_CI_AI NOT LIKE N'%credito%')
           OR (e.Categoria=N'Transferencia' AND LOWER(mp.Nombre) NOT LIKE N'%transfer%' AND LOWER(mp.Nombre) NOT LIKE N'%mercado pago%')
    ) SET @MetodosValidos=0;

    ;WITH CajasDelDia AS
    (
        SELECT c.Id,c.UsuarioId,c.FechaHoraApertura,c.FechaHoraCierre,c.SaldoInicial,c.SaldoFinal,c.Estado,
               LTRIM(RTRIM(CONCAT(u.Nombre,N' ',u.Apellido))) AS Cajero
        FROM dbo.Caja c INNER JOIN dbo.Usuario u ON u.Id=c.UsuarioId
        WHERE (@CajaId IS NOT NULL AND c.Id=@CajaId)
           OR (@CajaId IS NULL AND c.FechaHoraApertura>=@Inicio AND c.FechaHoraApertura<@Fin)
    ), VentasPorCaja AS
    (
        SELECT v.CajaId,
               SUM(CASE WHEN v.FechaHora>=cd.FechaHoraApertura
                              AND (cd.FechaHoraCierre IS NULL OR v.FechaHora<=cd.FechaHoraCierre)
                              AND v.Estado<>N'Anulada'
                        THEN v.Total ELSE CONVERT(decimal(18,2),0) END) AS VentasSistema,
               SUM(CASE WHEN v.FechaHora>=cd.FechaHoraApertura
                              AND (cd.FechaHoraCierre IS NULL OR v.FechaHora<=cd.FechaHoraCierre)
                              AND v.Estado=N'Anulada' THEN 1 ELSE 0 END) AS CantidadVentasAnuladas,
               MAX(CASE WHEN v.FechaHora IS NULL OR v.FechaHora<cd.FechaHoraApertura
                              OR (cd.FechaHoraCierre IS NOT NULL AND v.FechaHora>cd.FechaHoraCierre)
                              OR v.Estado IS NULL OR v.Estado NOT IN (N'Completada',N'Anulada')
                        THEN 1 ELSE 0 END) AS VentaInconsistente
        FROM dbo.Venta v INNER JOIN CajasDelDia cd ON cd.Id=v.CajaId
        GROUP BY v.CajaId
    ), PagosPorVenta AS
    (
        SELECT v.CajaId,v.Id AS VentaId,v.Estado,v.Total,
               COALESCE(SUM(CASE WHEN m.Categoria=N'Efectivo' THEN CONVERT(decimal(38,2),vp.MontoCobrado) ELSE CONVERT(decimal(38,2),0) END),0) AS EfectivoEntregado,
               COALESCE(SUM(CASE WHEN m.Categoria IN (N'Debito',N'Credito',N'Transferencia') THEN CONVERT(decimal(38,2),vp.MontoCobrado) ELSE CONVERT(decimal(38,2),0) END),0) AS OtrosMedios,
               COALESCE(SUM(CASE WHEN m.Categoria IN (N'Debito',N'Credito') THEN CONVERT(decimal(38,2),vp.MontoCobrado) ELSE CONVERT(decimal(38,2),0) END),0) AS CobrosTarjeta,
               COALESCE(SUM(CASE WHEN m.Categoria=N'Transferencia' THEN CONVERT(decimal(38,2),vp.MontoCobrado) ELSE CONVERT(decimal(38,2),0) END),0) AS CobrosTransferencia,
               COALESCE(SUM(CASE WHEN v.Estado=N'Anulada' AND m.Categoria=N'Efectivo' THEN CONVERT(decimal(38,2),vp.MontoCobrado) ELSE CONVERT(decimal(38,2),0) END),0) AS EfectivoAnulado,
               MAX(CASE WHEN vp.VentaId IS NOT NULL AND
                             (m.Id IS NULL OR vp.MontoCobrado IS NULL OR vp.MontoCobrado<=0)
                        THEN 1 ELSE 0 END) AS PagoDesconocido
        FROM dbo.Venta v INNER JOIN CajasDelDia cd ON cd.Id=v.CajaId
        LEFT JOIN dbo.VentaPago vp ON vp.VentaId=v.Id
        LEFT JOIN @Metodos m ON m.Id=vp.MetodoPagoId
        WHERE v.FechaHora>=cd.FechaHoraApertura AND (cd.FechaHoraCierre IS NULL OR v.FechaHora<=cd.FechaHoraCierre)
        GROUP BY v.CajaId,v.Id,v.Estado,v.Total
    ), TotalesPago AS
    (
        SELECT CajaId,
               COALESCE(SUM(CASE WHEN Estado<>N'Anulada' AND Estado IS NOT NULL
                   THEN CASE WHEN EfectivoEntregado>Total-OtrosMedios THEN Total-OtrosMedios ELSE EfectivoEntregado END
                   ELSE CONVERT(decimal(38,2),0) END),0) AS EfectivoConservado,
               COALESCE(SUM(EfectivoEntregado),0) AS EfectivoEntregado,
               COALESCE(SUM(CobrosTarjeta),0) AS CobrosTarjeta,
               COALESCE(SUM(CobrosTransferencia),0) AS CobrosTransferencia,
               COALESCE(SUM(EfectivoAnulado),0) AS EfectivoAnulado,
               MAX(CASE WHEN Estado IS NULL OR Estado NOT IN (N'Anulada',N'Completada')
                    OR PagoDesconocido=1 OR Total IS NULL OR Total<=0 OR
                    OtrosMedios>Total OR EfectivoEntregado+OtrosMedios<Total
                    THEN 1 ELSE 0 END) AS PagoInconsistente
        FROM PagosPorVenta GROUP BY CajaId
    ), MovimientosCaja AS
    (
        SELECT CajaId,
               SUM(CASE WHEN TipoMovimiento=N'Ingreso' THEN CONVERT(decimal(38,2),Importe) ELSE CONVERT(decimal(38,2),0) END) AS IngresosCaja,
               SUM(CASE WHEN TipoMovimiento=N'Retiro' THEN CONVERT(decimal(38,2),Importe) ELSE CONVERT(decimal(38,2),0) END) AS RetirosCaja,
               MAX(CASE WHEN TipoMovimiento IS NULL OR TipoMovimiento NOT IN (N'Ingreso',N'Retiro')
                             OR Importe IS NULL OR Importe<=0 THEN 1 ELSE 0 END) AS MovimientoInconsistente
        FROM dbo.MovimientoCaja GROUP BY CajaId
    ), Base AS
    (
        SELECT cd.*,COALESCE(v.VentasSistema,CONVERT(decimal(18,2),0)) AS VentasSistema,
               COALESCE(v.CantidadVentasAnuladas,0) AS CantidadVentasAnuladas,
               COALESCE(p.EfectivoConservado,0) AS EfectivoConservado,
               COALESCE(p.EfectivoEntregado,0) AS EfectivoEntregado,
               COALESCE(p.CobrosTarjeta,0) AS CobrosTarjeta,
               COALESCE(p.CobrosTransferencia,0) AS CobrosTransferencia,
               COALESCE(p.EfectivoAnulado,0) AS EfectivoAnulado,
               COALESCE(p.PagoInconsistente,0) AS PagoInconsistente,
               COALESCE(m.IngresosCaja,0) AS IngresosCaja,
               COALESCE(m.RetirosCaja,0) AS RetirosCaja,
               COALESCE(m.MovimientoInconsistente,0) AS MovimientoInconsistente,
               COALESCE(v.VentaInconsistente,0) AS VentaInconsistente
        FROM CajasDelDia cd
        LEFT JOIN VentasPorCaja v ON v.CajaId=cd.Id
        LEFT JOIN TotalesPago p ON p.CajaId=cd.Id
        LEFT JOIN MovimientosCaja m ON m.CajaId=cd.Id
    ), Evaluado AS
    (
        SELECT b.*,
               CASE WHEN @MetodosValidos=0 OR b.EfectivoAnulado>0 OR b.PagoInconsistente=1
                         OR b.MovimientoInconsistente=1 OR b.VentaInconsistente=1
                         OR b.SaldoInicial IS NULL OR b.SaldoInicial<0
                         OR TRY_CONVERT(decimal(18,2),b.EfectivoConservado) IS NULL
                         OR TRY_CONVERT(decimal(18,2),b.EfectivoConservado)<0
                         OR TRY_CONVERT(decimal(18,2),b.IngresosCaja) IS NULL
                         OR TRY_CONVERT(decimal(18,2),b.RetirosCaja) IS NULL
                         OR CONVERT(decimal(38,2),b.SaldoInicial)+b.EfectivoConservado+b.IngresosCaja-b.RetirosCaja<0
                    THEN CONVERT(decimal(18,2),NULL)
                    ELSE TRY_CONVERT(decimal(18,2),CONVERT(decimal(38,2),b.SaldoInicial)+b.EfectivoConservado+b.IngresosCaja-b.RetirosCaja) END AS MontoEsperado,
               CASE WHEN @MetodosValidos=0 OR b.EfectivoAnulado>0 OR b.PagoInconsistente=1
                         OR b.MovimientoInconsistente=1 OR b.VentaInconsistente=1
                         OR b.SaldoInicial IS NULL OR b.SaldoInicial<0
                         OR TRY_CONVERT(decimal(18,2),b.EfectivoConservado) IS NULL
                         OR TRY_CONVERT(decimal(18,2),b.EfectivoConservado)<0
                         OR TRY_CONVERT(decimal(18,2),b.IngresosCaja) IS NULL
                         OR TRY_CONVERT(decimal(18,2),b.RetirosCaja) IS NULL
                         OR TRY_CONVERT(decimal(18,2),CONVERT(decimal(38,2),b.SaldoInicial)+b.EfectivoConservado+b.IngresosCaja-b.RetirosCaja) IS NULL
                         OR CONVERT(decimal(38,2),b.SaldoInicial)+b.EfectivoConservado+b.IngresosCaja-b.RetirosCaja<0
                    THEN CONVERT(bit,1) ELSE CONVERT(bit,0) END AS CajaBloqueadaCierre
        FROM Base b
    )
    SELECT e.Cajero AS Cajero,e.FechaHoraApertura AS Apertura,e.FechaHoraCierre AS Cierre,
           e.SaldoInicial AS FondoCaja,e.VentasSistema AS VentasSistema,e.SaldoFinal AS EfectivoDeclarado,
           e.MontoEsperado AS MontoEsperado,
           CASE WHEN e.SaldoFinal IS NULL OR e.CajaBloqueadaCierre=1 THEN CONVERT(decimal(18,2),NULL)
                ELSE e.SaldoFinal-e.MontoEsperado END AS Diferencia,
           e.Estado AS Estado,
           TRY_CONVERT(decimal(18,2),e.EfectivoConservado) AS CobrosEfectivo,
           CONVERT(decimal(18,2),e.CobrosTarjeta) AS CobrosTarjeta,
           CONVERT(decimal(18,2),e.CobrosTransferencia) AS CobrosTransferencia,
           CONVERT(decimal(18,2),e.EfectivoAnulado) AS EfectivoAnuladoPendienteVerificar,
           CASE WHEN @MetodosValidos=0 THEN N'Revisar clasificación de medios de pago'
                WHEN e.EfectivoAnulado>0 THEN N'Bloqueado: efectivo de ventas anuladas sin devolución verificada'
                WHEN e.PagoInconsistente=1 THEN N'Bloqueado: pagos desconocidos o que no concilian'
                WHEN e.MovimientoInconsistente=1 THEN N'Bloqueado: movimientos de efectivo invalidos'
                WHEN e.VentaInconsistente=1 THEN N'Bloqueado: ventas fuera del turno o con estado no valido'
                WHEN e.SaldoInicial IS NULL OR e.SaldoInicial<0 THEN N'Bloqueado: fondo inicial invalido'
                WHEN TRY_CONVERT(decimal(18,2),e.EfectivoConservado)<0 THEN N'Bloqueado: cobros conservados negativos'
                WHEN CONVERT(decimal(38,2),e.SaldoInicial)+e.EfectivoConservado+e.IngresosCaja-e.RetirosCaja<0
                    THEN N'Bloqueado: efectivo esperado negativo'
                WHEN e.MontoEsperado IS NULL THEN N'Bloqueado: efectivo esperado fuera del rango admitido'
                WHEN e.SaldoFinal IS NULL THEN N'Caja abierta: sin efectivo declarado'
                ELSE N'Efectivo esperado con vuelto descontado y movimientos incluidos' END AS ObservacionEfectivo,
           e.CantidadVentasAnuladas AS CantidadVentasAnuladas,
           TRY_CONVERT(decimal(18,2),e.EfectivoEntregado) AS EfectivoEntregado,
           TRY_CONVERT(decimal(18,2),e.IngresosCaja) AS IngresosCaja,
           TRY_CONVERT(decimal(18,2),e.RetirosCaja) AS RetirosCaja,
           e.CajaBloqueadaCierre AS CajaBloqueadaCierre
    FROM Evaluado e ORDER BY e.FechaHoraApertura,e.Id;
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_TransferirDepositoALocal]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_TransferirDepositoALocal]
	@UsuarioId int,
	@ProductoId int,
	@Cantidad int,
	@FechaVencimiento datetime2(0) = NULL
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;
	SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

	BEGIN TRY
		IF @UsuarioId IS NULL OR @ProductoId IS NULL
			THROW 51140, 'Usuario y producto son obligatorios.', 1;

		IF @Cantidad IS NULL OR @Cantidad <= 0
			THROW 51141, 'La cantidad a transferir debe ser mayor que cero.', 1;

		IF NOT EXISTS (SELECT 1 FROM dbo.Usuario WHERE Id = @UsuarioId)
			THROW 51142, 'El usuario indicado no existe.', 1;

		IF NOT EXISTS (SELECT 1 FROM dbo.Producto WHERE Id = @ProductoId)
			THROW 51143, 'El producto indicado no existe.', 1;

		DECLARE @Restante decimal(10,3) = CONVERT(decimal(10,3), @Cantidad),
				@LoteDepositoId int,
				@StockLote decimal(10,3),
				@VencimientoLote datetime2(0),
				@Tomar decimal(10,3);

		BEGIN TRANSACTION;

			IF
			(
				SELECT ISNULL(SUM(StockActual), 0)
				FROM dbo.DepositoStock WITH (UPDLOCK, HOLDLOCK)
				WHERE ProductoId = @ProductoId
				  AND Activo = 1
				  AND (FechaVencimiento IS NULL
					   OR FechaVencimiento >= CONVERT(date, SYSDATETIME()))
			) < @Restante
				THROW 51144, 'No hay stock suficiente en el depósito.', 1;

			WHILE @Restante > 0
			BEGIN
				SET @LoteDepositoId = NULL;

				SELECT TOP (1)
					@LoteDepositoId = Id,
					@StockLote = StockActual,
					@VencimientoLote = FechaVencimiento
				FROM dbo.DepositoStock WITH (UPDLOCK, HOLDLOCK)
				WHERE ProductoId = @ProductoId
				  AND Activo = 1
				  AND StockActual > 0
				  AND (FechaVencimiento IS NULL
					   OR FechaVencimiento >= CONVERT(date, SYSDATETIME()))
				ORDER BY
					CASE WHEN FechaVencimiento IS NULL THEN 1 ELSE 0 END,
					FechaVencimiento,
					Id;

				IF @LoteDepositoId IS NULL
					THROW 51145, 'No se pudo localizar un lote disponible en el depósito.', 1;

				IF @VencimientoLote IS NULL
				   AND @FechaVencimiento IS NOT NULL
				   AND @FechaVencimiento < CONVERT(date, SYSDATETIME())
					THROW 51146, 'La fecha de vencimiento manual no puede ser anterior al día actual.', 1;

				SET @Tomar = CASE
					WHEN @StockLote < @Restante THEN @StockLote
					ELSE @Restante
				END;

				UPDATE dbo.DepositoStock
				SET StockActual = StockActual - @Tomar
				WHERE Id = @LoteDepositoId;

			/*
				La fecha original del lote tiene prioridad. La fecha enviada por el
				formulario solo completa un lote cuyo vencimiento original era NULL;
				nunca puede convertir un lote vencido en vigente.
			*/
				INSERT INTO dbo.Lote
					(ProductoId, FechaIngreso, FechaVencimiento,
					 StockInicial, StockActual, Activo)
				VALUES
					(@ProductoId, SYSDATETIME(),
					  CASE
						 WHEN @VencimientoLote IS NULL THEN @FechaVencimiento
						 ELSE @VencimientoLote
					  END,
					 @Tomar, @Tomar, 1);

				UPDATE dbo.Producto
				SET StockActual = ISNULL(StockActual, 0) + @Tomar
				WHERE Id = @ProductoId;

				SET @Restante = @Restante - @Tomar;
			END;

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_VerificarCajaAbierta]    Script Date: 11/11/2026 08:03:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 1. Verificar caja abierta (Busca el texto 'Abierta')
CREATE   PROCEDURE [dbo].[sp_VerificarCajaAbierta]
    @UsuarioId INT
AS
BEGIN
    SELECT Id, SaldoInicial, FechaHoraApertura
    FROM Caja
    WHERE UsuarioId = @UsuarioId AND Estado = 'Abierta';
END
GO
EXEC sys.sp_addextendedproperty @name=N'PacuIbera.RecepcionPedido.ControlVencimiento', @value=N'0A84547E57A90310A649CCF333FDB2F099EEF3D9463F59968AB3D20E5468E862' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'PROCEDURE',@level1name=N'sp_ConfirmarRecepcionPedido'
GO
