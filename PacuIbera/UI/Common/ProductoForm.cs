using Datos;
using PacuIbera.Dominio;
using System.Data;
using System.Globalization;

namespace PacuIbera.UI.Common;

public partial class ProductoForm : Form
{
    private readonly ProductoDatos datos = new();
    private readonly DataGridView tabla = CrearTabla();
    private readonly DataGridView tablaVencidos = CrearTabla();
    private readonly TextBox buscar = new() { Width=250, PlaceholderText="Buscar por nombre..." };
    private readonly ComboBox categoria = new() { Width=210,DropDownStyle=ComboBoxStyle.DropDownList };
    private readonly ComboBox estado = new() { Width=130,DropDownStyle=ComboBoxStyle.DropDownList };
    private readonly ComboBox alerta = new() { Width=185,DropDownStyle=ComboBoxStyle.DropDownList };
    private readonly Button nuevo = new() { Text="Nuevo producto",AutoSize=true };
    private readonly Button editar = new() { Text="Editar producto",AutoSize=true,Enabled=false };
    private readonly Button baja = new() { Text="Dar de baja",AutoSize=true,Enabled=false };
    private readonly Button lotes = new() { Text="Ver lotes locales",AutoSize=true,Enabled=false };
    private readonly Button actualizar = new() { Text="Actualizar",AutoSize=true };
    private readonly Button[] accesosAlertas = new[] { "Todos", "Sin stock", "Stock bajo", "Próximos a vencer", "Productos vencidos" }
        .Select(texto=>new Button { Text=texto,AutoSize=true }).ToArray();
    private readonly Label mensaje = new() { Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleLeft,AutoEllipsis=true };
    private readonly Label encabezado = new();
    private List<ProductoFicha> productos = new();
    private List<LoteVencidoProducto> historialVencidos = new();
    private bool ocupado;
    private bool cargandoFiltros;

    public ProductoForm()
    {
        if(!ProductoDatos.PuedeConsultarFichas) throw new UnauthorizedAccessException("Su rol no puede consultar Productos.");
        InitializeComponent();
        var titulo=encabezado;
        titulo.Text="PRODUCTOS\nStock local utilizable: lotes activos, con cantidad y sin vencer. El depósito se consulta por separado.";
        titulo.Dock=DockStyle.Top;titulo.Height=64;titulo.Padding=new Padding(14,8,40,6);
        titulo.ForeColor=Color.FromArgb(45,55,65);titulo.BackColor=Color.WhiteSmoke;
        var filtros=new FlowLayoutPanel { Dock=DockStyle.Top,Height=86,Padding=new Padding(10,8,10,4),WrapContents=true,BackColor=Color.WhiteSmoke };
        filtros.Controls.Add(buscar); filtros.Controls.Add(categoria); filtros.Controls.Add(estado); filtros.Controls.Add(alerta);
        estado.Items.AddRange(new object[]{"Todos los estados","Activos","Inactivos"});estado.SelectedIndex=SesionActiva.Rol=="Gerente"?0:1;
        alerta.Items.AddRange(new object[]{"Todas las alertas","Sin stock","Stock bajo","Próximos a vencer","Productos vencidos"});alerta.SelectedIndex=0;
        Col("Nombre","Producto",240);Col("Categoria","Categoría",160);Col("PrecioVenta","Precio",105);
        Col("StockUtilizable","Stock local",110);Col("TipoVenta","Tipo de venta",120);Col("Estado","Estado",95);
        Col("RequiereVencimiento","Requiere vencimiento",150);
        Col("Alerta","Alerta",145);Col("ProximoVencimiento","Próximo vencimiento",150);
        Col("Descripcion","Descripción",240);
        foreach(var columna in new[]{("Producto","Producto",260),("NumeroLote","Número de lote",125),
            ("FechaVencimiento","Fecha de vencimiento",170),("CantidadRegistrada","Cantidad registrada",160),("Estado","Estado",180)})
            tablaVencidos.Columns.Add(new DataGridViewTextBoxColumn { Name=columna.Item1,DataPropertyName=columna.Item1,
                HeaderText=columna.Item2,Width=columna.Item3,SortMode=DataGridViewColumnSortMode.NotSortable });
        tablaVencidos.Columns["Producto"].AutoSizeMode=DataGridViewAutoSizeColumnMode.Fill;
        tablaVencidos.Columns["Producto"].MinimumWidth=220;
        tablaVencidos.Columns["FechaVencimiento"].DefaultCellStyle.Format="dd/MM/yyyy";
        foreach(DataGridViewColumn columna in tablaVencidos.Columns)
            if(columna.Name!="Producto")
            {
                columna.DefaultCellStyle.Alignment=DataGridViewContentAlignment.MiddleCenter;
                columna.HeaderCell.Style.Alignment=DataGridViewContentAlignment.MiddleCenter;
            }
        tablaVencidos.Visible=false;
        tablaVencidos.CellFormatting+=(_,e)=>
        {
            if(e.RowIndex<0||e.CellStyle==null||tablaVencidos.Rows[e.RowIndex].DataBoundItem is not LoteVencidoProducto lote)return;
            e.CellStyle.BackColor=Color.FromArgb(255,235,235);e.CellStyle.ForeColor=Color.Maroon;
            if(tablaVencidos.Columns[e.ColumnIndex].Name=="CantidadRegistrada")
            {
                e.CellStyle.Format=lote.SeVendePorPeso?"0.###":"0";
                e.CellStyle.FormatProvider=CultureInfo.GetCultureInfo("es-AR");
            }
        };
        tabla.Columns["Nombre"].MinimumWidth=220;tabla.Columns["Nombre"].AutoSizeMode=DataGridViewAutoSizeColumnMode.Fill;
        tabla.Columns["PrecioVenta"].DefaultCellStyle.Format="C2";
        tabla.Columns["PrecioVenta"].DefaultCellStyle.FormatProvider=CultureInfo.GetCultureInfo("es-AR");
        tabla.Columns["StockUtilizable"].ToolTipText="Cantidad utilizable de lotes activos, positivos y vigentes. No incluye lotes vencidos o inactivos.";
        foreach(string columna in new[]{"PrecioVenta","StockUtilizable","TipoVenta","Estado","RequiereVencimiento","Alerta","ProximoVencimiento"})
        {
            tabla.Columns[columna].DefaultCellStyle.Alignment=DataGridViewContentAlignment.MiddleCenter;
            tabla.Columns[columna].HeaderCell.Style.Alignment=DataGridViewContentAlignment.MiddleCenter;
        }
        tabla.Columns["ProximoVencimiento"].DefaultCellStyle.Format="dd/MM/yyyy";
        tabla.Columns["ProximoVencimiento"].DefaultCellStyle.NullValue="Sin fecha futura";
        tabla.Columns["ProximoVencimiento"].ToolTipText="Próxima fecha posterior a hoy en lotes activos con cantidad. La alerta abarca de mañana hasta 30 días.";
        tabla.CellFormatting+=(_,e)=>
        {
            if(e.RowIndex<0 || e.CellStyle==null || tabla.Rows[e.RowIndex].DataBoundItem is not ProductoFicha p)return;
            string columna=tabla.Columns[e.ColumnIndex].Name;
            if(columna=="StockUtilizable")
            {
                e.CellStyle.Format=p.SeVendePorPeso?"0.###":"0";
                e.CellStyle.FormatProvider=CultureInfo.GetCultureInfo("es-AR");
                e.CellStyle.Alignment=DataGridViewContentAlignment.MiddleCenter;
            }
            if(columna=="ProximoVencimiento" && p.ProximoVencimiento==null && p.SinVencimientoLocal)
            {
                e.Value="Sin vencimiento"; e.FormattingApplied=true;
            }
            if(columna=="Alerta" && p.Alerta!="Sin alerta")e.CellStyle.ForeColor=Color.FromArgb(150,95,15);
            if(columna=="Estado" && !p.Activo)e.CellStyle.ForeColor=Color.DimGray;
        };
        tabla.CellToolTipTextNeeded+=(_,e)=>
        {
            if(e.RowIndex<0||e.ColumnIndex<0||tabla.Columns[e.ColumnIndex].Name!="StockUtilizable"
                ||tabla.Rows[e.RowIndex].DataBoundItem is not ProductoFicha p)return;
            string Numero(decimal valor)=>valor.ToString(p.SeVendePorPeso?"0.###":"0",CultureInfo.GetCultureInfo("es-AR"));
            e.ToolTipText=$"Utilizable: {Numero(p.StockUtilizable)}\nSaldo registrado en la ficha: {Numero(p.StockActual)}"
                +$"\nCantidad actual de todos los lotes: {Numero(p.StockActualLotes)}\nCantidad de lotes activos: {Numero(p.StockLotesActivos)}"
                +"\nLos lotes inactivos o vencidos no forman parte del stock utilizable. No se corrigen saldos automáticamente.";
        };
        var pie=new TableLayoutPanel { Dock=DockStyle.Bottom,Height=104,ColumnCount=1,RowCount=2,Padding=new Padding(10) };
        pie.RowStyles.Add(new RowStyle(SizeType.Absolute,30));pie.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        pie.Controls.Add(mensaje,0,0);
        var acciones=new FlowLayoutPanel { Dock=DockStyle.Fill,WrapContents=true };
        foreach(var boton in new[]{nuevo,editar,baja,lotes,actualizar})
        {
            EstiloUI.AplicarEstiloBotonAccion(boton,boton==baja?EstiloUI.TipoAccion.Editar:EstiloUI.TipoAccion.Positivo);
            acciones.Controls.Add(boton);
        }
        pie.Controls.Add(acciones,0,1);
        var accesos=new FlowLayoutPanel { Dock=DockStyle.Top,Height=52,Padding=new Padding(10,2,10,2),WrapContents=false,AutoScroll=true,BackColor=Color.WhiteSmoke };
        for(int i=0;i<accesosAlertas.Length;i++)
        {
            int filtro=i;
            var boton=accesosAlertas[i];
            EstiloUI.AplicarEstiloBotonAccion(boton,EstiloUI.TipoAccion.Positivo);
            boton.Click+=(_,_)=>alerta.SelectedIndex=filtro;
            accesos.Controls.Add(boton);
        }
        Controls.Add(tabla);Controls.Add(tablaVencidos);Controls.Add(pie);Controls.Add(accesos);Controls.Add(filtros);Controls.Add(titulo);
        buscar.TextChanged+=(_,_)=>Filtrar();categoria.SelectedIndexChanged+=(_,_)=>Filtrar();
        estado.SelectedIndexChanged+=(_,_)=>Filtrar();alerta.SelectedIndexChanged+=(_,_)=>Filtrar();
        tabla.SelectionChanged+=(_,_)=>ActualizarAcciones();
        tabla.CellDoubleClick+=async(_,e)=>{if(e.RowIndex>=0 && ProductoDatos.PuedeEditarFichas)await AbrirEditorAsync(nuevoProducto:false);};
        nuevo.Click+=async(_,_)=>await AbrirEditorAsync(nuevoProducto:true);
        editar.Click+=async(_,_)=>await AbrirEditorAsync(nuevoProducto:false);
        baja.Click+=async(_,_)=>await DarBajaAsync();
        lotes.Click+=async(_,_)=>await VerLotesAsync();actualizar.Click+=async(_,_)=>await CargarAsync();
        Load+=async(_,_)=>{EstiloUI.CrearBotonCerrar(this);await CargarAsync();};
        ActualizarAcciones();
    }

    internal static DataGridView CrearTabla()
    {
        var grilla=new DataGridView { Dock=DockStyle.Fill,AutoGenerateColumns=false,AllowUserToAddRows=false,
            AllowUserToDeleteRows=false,ReadOnly=true,MultiSelect=false,RowHeadersVisible=false,
            SelectionMode=DataGridViewSelectionMode.FullRowSelect,BackgroundColor=Color.White };
        EstiloUI.AplicarEstiloGrid(grilla);
        grilla.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.None;
        grilla.ScrollBars=ScrollBars.Both;
        grilla.AlternatingRowsDefaultCellStyle.BackColor=Color.FromArgb(247,250,248);
        return grilla;
    }
    private void Col(string nombre,string titulo,int ancho)=>tabla.Columns.Add(new DataGridViewTextBoxColumn
        {Name=nombre,DataPropertyName=nombre,HeaderText=titulo,Width=ancho,SortMode=DataGridViewColumnSortMode.NotSortable});
    private bool VistaVencidos=>alerta.SelectedIndex==4;
    private ProductoFicha? Seleccionado()=>!VistaVencidos&&tabla.SelectedRows.Count==1?tabla.SelectedRows[0].DataBoundItem as ProductoFicha:null;
    private void ActualizarAcciones()
    {
        nuevo.Visible=editar.Visible=baja.Visible=!VistaVencidos&&ProductoDatos.PuedeEditarFichas;
        nuevo.Enabled=!VistaVencidos&&!ocupado && ProductoDatos.PuedeEditarFichas;
        editar.Enabled=baja.Enabled=!ocupado && ProductoDatos.PuedeEditarFichas && Seleccionado()?.Activo==true;
        lotes.Visible=!VistaVencidos&&SesionActiva.Rol!="Vendedor";
        lotes.Enabled=!ocupado && lotes.Visible && Seleccionado()!=null;
        actualizar.Enabled=buscar.Enabled=categoria.Enabled=estado.Enabled=alerta.Enabled=tabla.Enabled=!ocupado;
        categoria.Visible=estado.Visible=!VistaVencidos;
        tablaVencidos.Enabled=!ocupado;
        for(int i=0;i<accesosAlertas.Length;i++)
        {
            accesosAlertas[i].Enabled=!ocupado;
            accesosAlertas[i].BackColor=i==alerta.SelectedIndex?Color.FromArgb(16,125,90):Color.FromArgb(100,116,139);
        }
        UseWaitCursor=ocupado;
    }
    private async Task CargarAsync(int? seleccionar=null)
    {
        if(ocupado)return;ocupado=true;ActualizarAcciones();
        try
        {
            var categorias=await datos.ConsultarCategoriasFichaAsync();
            var resultado=await datos.ConsultarFichasAsync();
            var vencidos=await datos.ConsultarHistorialVencidosAsync();
            if(IsDisposed)return;
            int idCategoria=(categoria.SelectedItem as CategoriaProducto)?.Id??0;
            cargandoFiltros=true;
            categoria.DataSource=new[]{new CategoriaProducto(0,"Todas las categorías")}.Concat(categorias).ToList();
            categoria.DisplayMember=nameof(CategoriaProducto.Nombre);categoria.ValueMember=nameof(CategoriaProducto.Id);
            categoria.SelectedValue=categorias.Any(c=>c.Id==idCategoria)?idCategoria:0;
            productos=resultado;
            historialVencidos=vencidos;
            cargandoFiltros=false;Filtrar(seleccionar);
        }
        catch(Exception ex)
        {
            if(!IsDisposed){productos.Clear();historialVencidos.Clear();tabla.DataSource=null;tablaVencidos.DataSource=null;mensaje.Text="No se pudo cargar Productos.";
                MessageBox.Show(this,ex.Message,"Productos",MessageBoxButtons.OK,MessageBoxIcon.Warning);}
        }
        finally{cargandoFiltros=false;ocupado=false;if(!IsDisposed)ActualizarAcciones();}
    }
    private void Filtrar(int? seleccion=null)
    {
        if(cargandoFiltros)return;
        tabla.Visible=!VistaVencidos;
        tablaVencidos.Visible=VistaVencidos;
        encabezado.Text=VistaVencidos
            ? "PRODUCTOS VENCIDOS · HISTORIAL DE LOTES LOCALES\nLas cantidades registradas son históricas y no representan stock disponible para vender."
            : "PRODUCTOS\nStock local utilizable: lotes activos, con cantidad y sin vencer. El depósito se consulta por separado.";
        if(VistaVencidos)
        {
            var filas=historialVencidos.Where(l=>l.Producto.Contains(buscar.Text.Trim(),StringComparison.CurrentCultureIgnoreCase)).ToList();
            tablaVencidos.DataSource=filas;
            tablaVencidos.ClearSelection();tablaVencidos.CurrentCell=null;
            mensaje.Text=filas.Count==0?"No hay lotes vencidos con cantidad registrada para esta búsqueda.":$"{filas.Count} lotes vencidos · Cantidades históricas registradas: no son stock disponible para vender.";
            ActualizarAcciones();
            return;
        }
        seleccion??=Seleccionado()?.Id;
        int idCategoria=(categoria.SelectedItem as CategoriaProducto)?.Id??0;
        var visibles=productos.Where(p=>p.Nombre.Contains(buscar.Text.Trim(),StringComparison.CurrentCultureIgnoreCase)
            &&(idCategoria==0||p.CategoriaId==idCategoria)
            &&(estado.SelectedIndex==0||(estado.SelectedIndex==1?p.Activo:!p.Activo))
            &&(alerta.SelectedIndex switch
            {
                1=>p.SinStock,
                2=>p.StockBajo,
                3=>p.Activo&&p.LotesPorVencer>0,4=>p.LotesVencidos>0,_=>true
            })).ToList();
        tabla.DataSource=visibles;tabla.ClearSelection();tabla.CurrentCell=null;
        if(seleccion.HasValue)foreach(DataGridViewRow fila in tabla.Rows)
            if(fila.DataBoundItem is ProductoFicha p&&p.Id==seleccion){tabla.CurrentCell=fila.Cells[0];fila.Selected=true;break;}
        int sinStock=productos.Count(p=>p.SinStock);
        int bajos=productos.Count(p=>p.StockBajo);
        int proximos=productos.Count(p=>p.Activo&&p.LotesPorVencer>0);
        mensaje.Text=$"{visibles.Count} productos · Sin stock: {sinStock} · Stock bajo: {bajos} · Próximos a vencer: {proximos} (mañana a {ProductoDatos.DiasAlertaVencimientoLocal} días).";
        ActualizarAcciones();
    }
    private async Task AbrirEditorAsync(bool nuevoProducto)
    {
        if(ocupado||!ProductoDatos.PuedeEditarFichas)return;
        var original=nuevoProducto?null:Seleccionado();
        if(!nuevoProducto&&(original==null||!original.Activo))return;
        ocupado=true;ActualizarAcciones();int? guardado=null;
        try
        {
            var categorias=await datos.ConsultarCategoriasFichaAsync();
            if(IsDisposed)return;
            using var editor=new FormEditarProducto(original,categorias);
            if(editor.ShowDialog(this)==DialogResult.OK)guardado=editor.ProductoGuardadoId;
        }
        catch(Exception ex){if(!IsDisposed)MessageBox.Show(this,ex.Message,"Producto",MessageBoxButtons.OK,MessageBoxIcon.Warning);}
        finally{ocupado=false;if(!IsDisposed)ActualizarAcciones();}
        if(guardado.HasValue&&!IsDisposed)
        {
            // Mostrar el producto guardado aunque no coincida con el filtro anterior.
            buscar.Clear();categoria.SelectedIndex=0;estado.SelectedIndex=1;alerta.SelectedIndex=0;
            await CargarAsync(guardado);
        }
    }
    private async Task DarBajaAsync()
    {
        var producto=Seleccionado();if(ocupado||!ProductoDatos.PuedeEditarFichas||producto?.Activo!=true)return;
        ocupado=true;ActualizarAcciones();bool guardado=false;
        try
        {
            if(MessageBox.Show(this,$"¿Dar de baja {producto.Nombre}?\nSe conservará su historial. No se permite si tiene mercadería, pedidos o remitos pendientes.",
                "Dar de baja",MessageBoxButtons.YesNo,MessageBoxIcon.Question,MessageBoxDefaultButton.Button2)!=DialogResult.Yes)return;
            await datos.DarBajaFichaAsync(producto.Id);guardado=true;
        }
        catch(Exception ex){if(!IsDisposed)MessageBox.Show(this,"No se pudo confirmar la baja. "+ex.Message,"Producto",MessageBoxButtons.OK,MessageBoxIcon.Warning);}
        finally{ocupado=false;if(!IsDisposed)ActualizarAcciones();}
        if(guardado&&!IsDisposed)await CargarAsync();
    }
    private async Task VerLotesAsync()
    {
        var producto=Seleccionado();if(ocupado||producto==null||SesionActiva.Rol=="Vendedor")return;
        ocupado=true;ActualizarAcciones();
        try
        {
            using var datosLotes=await datos.ConsultarLotesFichaAsync(producto.Id);
            if(IsDisposed)return;
            using var ventana=new Form {Text="Lotes locales · "+producto.Nombre,Size=new Size(850,480),MinimumSize=new Size(700,350),
                Font=Font,StartPosition=FormStartPosition.CenterParent,BackColor=Color.White};
            var grilla=CrearTabla();
            foreach(var c in new[]{("Id","Lote"),("FechaIngreso","Ingreso"),("FechaVencimiento","Vencimiento"),
                ("StockActual","Cantidad actual"),("Estado","Estado")})
                grilla.Columns.Add(new DataGridViewTextBoxColumn {Name=c.Item1,DataPropertyName=c.Item1,HeaderText=c.Item2,Width=125});
            foreach(string c in new[]{"FechaIngreso","FechaVencimiento"}){grilla.Columns[c].DefaultCellStyle.Format="dd/MM/yyyy";grilla.Columns[c].DefaultCellStyle.NullValue="Sin vencimiento";}
            grilla.Columns["StockActual"].DefaultCellStyle.Format=producto.SeVendePorPeso?"0.###":"0";
            grilla.Columns["StockActual"].DefaultCellStyle.FormatProvider=CultureInfo.GetCultureInfo("es-AR");
            foreach(DataGridViewColumn columna in grilla.Columns)
            {
                columna.DefaultCellStyle.Alignment=DataGridViewContentAlignment.MiddleCenter;
                columna.HeaderCell.Style.Alignment=DataGridViewContentAlignment.MiddleCenter;
            }
            grilla.Columns["Estado"].AutoSizeMode=DataGridViewAutoSizeColumnMode.Fill;
            grilla.Columns["Estado"].MinimumWidth=110;
            grilla.DataSource=datosLotes;
            grilla.CellFormatting+=(_,e)=>
            {
                if(e.RowIndex<0||e.CellStyle==null||grilla.Rows[e.RowIndex].DataBoundItem is not DataRowView fila)return;
                bool vencido=Convert.ToBoolean(fila["EsVencido"]);
                bool inactivo=!Convert.ToBoolean(fila["Activo"]);
                bool agotado=Convert.ToDecimal(fila["StockActual"])<=0;
                (Color fondo,Color texto)=(vencido&&!agotado) switch
                {
                    true=>(Color.FromArgb(255,235,235),Color.Maroon),
                    _ when inactivo||agotado=>(Color.FromArgb(242,243,245),Color.DimGray),
                    _ when Convert.ToBoolean(fila["EsProximo"]) =>(Color.FromArgb(255,239,213),Color.FromArgb(145,83,8)),
                    _=>(Color.FromArgb(232,246,236),Color.FromArgb(20,115,75))
                };
                e.CellStyle.BackColor=fondo;
                e.CellStyle.ForeColor=texto;
            };
            grilla.CellToolTipTextNeeded+=(_,e)=>
            {
                if(e.RowIndex>=0&&e.ColumnIndex>=0&&grilla.Columns[e.ColumnIndex].Name=="StockActual"
                    &&grilla.Rows[e.RowIndex].DataBoundItem is System.Data.DataRowView fila)
                    e.ToolTipText="Cantidad inicial registrada: "+Convert.ToDecimal(fila["StockInicial"]).ToString(producto.SeVendePorPeso?"0.###":"0",CultureInfo.GetCultureInfo("es-AR"))
                        +". Es un dato histórico, no la cantidad disponible para vender.";
            };
            decimal fisico=datosLotes.AsEnumerable().Sum(f=>Convert.ToDecimal(f["StockActual"]));
            decimal utilizable=datosLotes.AsEnumerable().Where(f=>f.Field<bool>("EsUtilizable")).Sum(f=>Convert.ToDecimal(f["StockActual"]));
            bool historialAnomalo=datosLotes.AsEnumerable().Any(f=>Convert.ToDecimal(f["StockActual"])>Convert.ToDecimal(f["StockInicial"]));
            string Numero(decimal valor)=>valor.ToString(producto.SeVendePorPeso?"0.###":"0",CultureInfo.GetCultureInfo("es-AR"));
            var nota=new Label {Text=$"{producto.Nombre} · Saldo de la ficha (última consulta): {Numero(producto.StockActual)} · Cantidad actual en lotes: {Numero(fisico)} · Utilizable: {Numero(utilizable)}\n"
                +"Inactivos y vencidos no están disponibles para vender. Las cantidades iniciales son históricas."
                +$"\nRojo: vencido · Naranja: vence de mañana a {ProductoDatos.DiasAlertaVencimientoLocal} días · Verde: disponible (incluye hoy) · Gris: inactivo/agotado."
                +(historialAnomalo?"\nHay lotes con cantidad actual mayor que la inicial: requieren revisión histórica; no se ajustarán automáticamente.":"")
                +(!producto.Activo?"\nEl producto está inactivo.":""),Dock=DockStyle.Top,Height=historialAnomalo||!producto.Activo?106:78,Padding=new Padding(10)};
            var cerrar=new Button {Text="Cerrar",AutoSize=true,DialogResult=DialogResult.Cancel};EstiloUI.AplicarEstiloBotonAccion(cerrar,EstiloUI.TipoAccion.Editar);
            var pie=new FlowLayoutPanel {Dock=DockStyle.Bottom,Height=60,Padding=new Padding(10),FlowDirection=FlowDirection.RightToLeft};pie.Controls.Add(cerrar);
            ventana.Controls.Add(grilla);ventana.Controls.Add(pie);ventana.Controls.Add(nota);ventana.CancelButton=cerrar;ventana.ShowDialog(this);
        }
        catch(Exception ex){if(!IsDisposed)MessageBox.Show(this,ex.Message,"Lotes locales",MessageBoxButtons.OK,MessageBoxIcon.Warning);}
        finally{ocupado=false;if(!IsDisposed)ActualizarAcciones();}
    }
}
