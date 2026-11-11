using Datos;
using PacuIbera.Dominio;
using System.Globalization;

namespace PacuIbera.UI.Common;

public sealed class FormEditarProducto : Form
{
    private static readonly CultureInfo Argentina=CultureInfo.GetCultureInfo("es-AR");
    private sealed class NumeroFicha : NumericUpDown
    {
        public bool Leer(out decimal valor)
        {
            string texto=Text.Trim();int coma=texto.IndexOf(',');
            return decimal.TryParse(texto,NumberStyles.AllowLeadingSign|NumberStyles.AllowDecimalPoint,Argentina,out valor)
                &&valor>=Minimum&&valor<=Maximum
                &&(coma<0||(DecimalPlaces>0&&texto.Length-coma-1<=DecimalPlaces))
                &&decimal.Round(valor,DecimalPlaces)==valor;
        }
        protected override void ValidateEditText()
        {
            if(!Leer(out decimal valor)){BackColor=Color.MistyRose;return;}
            UserEdit=false;Value=valor;BackColor=Color.White;UpdateEditText();
        }
        protected override void UpdateEditText(){UserEdit=false;Text=Value.ToString(DecimalPlaces==0?"0":"0."+new string('#',DecimalPlaces),Argentina);}
        public override void UpButton(){if(Leer(out _))base.UpButton();else BackColor=Color.MistyRose;}
        public override void DownButton(){if(Leer(out _))base.DownButton();else BackColor=Color.MistyRose;}
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            // Formato es-AR. Pegados y texto pendiente tambien se comprueban en Leer().
            if (!char.IsControl(e.KeyChar) && !(e.KeyChar >= '0' && e.KeyChar <= '9')
                && !(e.KeyChar == ',' && DecimalPlaces > 0)) e.Handled=true;
            base.OnKeyPress(e);
        }
    }

    private readonly ProductoDatos datos=new();
    private readonly ProductoFicha? original;
    private readonly TextBox nombre=new(){Dock=DockStyle.Fill,MaxLength=100};
    private readonly ComboBox categoria=new(){Dock=DockStyle.Fill,DropDownStyle=ComboBoxStyle.DropDownList};
    private readonly TextBox descripcion=new(){Dock=DockStyle.Fill,MaxLength=255,Multiline=true,ScrollBars=ScrollBars.Vertical};
    private readonly TextBox codigo=new(){Dock=DockStyle.Fill,MaxLength=50};
    private readonly NumeroFicha precio=new(){DecimalPlaces=2,Minimum=0,Maximum=99999999.99M,Width=180};
    private readonly NumeroFicha minimo=new(){Minimum=0,Maximum=9999999.999M,Width=180};
    private readonly ComboBox peso=new(){DropDownStyle=ComboBoxStyle.DropDownList,Width=180};
    private readonly ComboBox vencimiento=new(){DropDownStyle=ComboBoxStyle.DropDownList,Width=180};
    private readonly ErrorProvider errores=new(){BlinkStyle=ErrorBlinkStyle.NeverBlink};
    private readonly Button guardar=new(){Text="Guardar",AutoSize=true};
    private readonly Button nuevaCategoria=new(){Text="Nueva categoría",AutoSize=true};
    private readonly TableLayoutPanel campos=new(){Dock=DockStyle.Fill,ColumnCount=2,RowCount=9,Padding=new Padding(18,12,18,8)};
    private bool ocupado;
    private bool resultadoIncierto;
    public int ProductoGuardadoId{get;private set;}

    public FormEditarProducto(ProductoFicha? producto,List<CategoriaProducto> categorias)
    {
        if(!ProductoDatos.PuedeEditarFichas)throw new UnauthorizedAccessException("Su rol no puede modificar productos.");
        if(producto!=null&&!producto.Activo)throw new InvalidOperationException("El producto está inactivo.");
        original=producto;
        errores.ContainerControl=this;
        Text=producto==null?"Nuevo producto":"Editar producto";
        Font=new Font("Segoe UI",9F);BackColor=Color.White;
        Size=new Size(720,640);MinimumSize=new Size(680,620);StartPosition=FormStartPosition.CenterParent;
        var titulo=new Label {Text=producto==null?"NUEVO PRODUCTO":"EDITAR PRODUCTO",Dock=DockStyle.Top,Height=52,Padding=new Padding(18,14,18,8),
            BackColor=Color.FromArgb(45,55,65),ForeColor=Color.White,Font=new Font(Font,FontStyle.Bold)};
        var nota=new Label {Text=producto==null
            ?"El producto se crea sin stock. La mercadería ingresa al local mediante remitos."
            :$"Stock local registrado: {producto.StockActual.ToString(producto.SeVendePorPeso?"0.###":"0",Argentina)}. Editar esta ficha no modifica stock ni lotes.",
            Dock=DockStyle.Top,Height=48,Padding=new Padding(18,8,18,0),ForeColor=Color.DimGray};
        campos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,210));campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        foreach(int alto in new[]{38,42,38,38,44,38,82,38,38})campos.RowStyles.Add(new RowStyle(SizeType.Absolute,alto));
        void Campo(string etiqueta,Control control,int fila)
        {
            campos.Controls.Add(new Label {Text=etiqueta,AutoSize=true,MaximumSize=new Size(195,0),Margin=new Padding(3,7,8,0)},0,fila);
            campos.Controls.Add(control,1,fila);
        }
        Campo("Nombre",nombre,0);
        var filaCategoria=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2};
        filaCategoria.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));filaCategoria.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        filaCategoria.Controls.Add(categoria,0,0);filaCategoria.Controls.Add(nuevaCategoria,1,0);
        Campo("Categoría",filaCategoria,1);Campo("Precio de venta ($)",precio,2);Campo("Tipo de venta",peso,3);
        var filaMinimo=new FlowLayoutPanel {Dock=DockStyle.Fill,WrapContents=false};
        filaMinimo.Controls.Add(minimo);filaMinimo.Controls.Add(new Label {Text="0 desactiva la alerta por mínimo",AutoSize=true,Margin=new Padding(8,7,0,0)});
        Campo("Mínimo del local",filaMinimo,4);Campo("Código de barras (opcional)",codigo,5);Campo("Descripción",descripcion,6);
        codigo.PlaceholderText="Opcional · hasta 50 caracteres";
        Campo("¿El producto requiere vencimiento?",vencimiento,7);
        vencimiento.Items.AddRange(new object[]{"Sí","No"});
        vencimiento.SelectedIndex=producto==null?-1:producto.ControlaVencimiento?0:1;
        if(producto!=null)campos.Controls.Add(new Label {Text="El tipo de venta se conserva para proteger el historial.",AutoSize=true,ForeColor=Color.DimGray},1,8);
        categoria.DataSource=categorias.ToList();categoria.DisplayMember=nameof(CategoriaProducto.Nombre);categoria.ValueMember=nameof(CategoriaProducto.Id);
        if(producto!=null)categoria.SelectedValue=producto.CategoriaId;else categoria.SelectedIndex=-1;
        peso.Items.AddRange(new object[]{"Por unidad","Por peso"});
        peso.SelectedIndex=producto==null?-1:producto.SeVendePorPeso?1:0;peso.Enabled=producto==null;
        minimo.DecimalPlaces=peso.SelectedIndex==1?3:0;minimo.Increment=peso.SelectedIndex==1?0.001M:1;
        nombre.Text=producto?.Nombre??"";descripcion.Text=producto?.Descripcion??"";codigo.Text=producto?.CodigoBarras??"";
        precio.Text=(producto?.PrecioVenta??0).ToString("0.##",Argentina);
        // Conservar el texto original aunque requiera corregir un minimo historico fraccionario.
        minimo.Text=(producto?.StockMinimo??0).ToString("0.###",Argentina);
        peso.SelectedIndexChanged+=(_,_)=>{string texto=minimo.Text;minimo.DecimalPlaces=peso.SelectedIndex==1?3:0;minimo.Increment=peso.SelectedIndex==1?0.001M:1;minimo.Text=texto;};
        var pie=new FlowLayoutPanel {Dock=DockStyle.Bottom,Height=66,Padding=new Padding(12),FlowDirection=FlowDirection.RightToLeft,BackColor=Color.WhiteSmoke};
        var cancelar=new Button {Text="Cancelar",AutoSize=true,DialogResult=DialogResult.Cancel};
        EstiloUI.AplicarEstiloBotonAccion(guardar,EstiloUI.TipoAccion.Positivo);
        EstiloUI.AplicarEstiloBotonAccion(cancelar,EstiloUI.TipoAccion.Editar);
        EstiloUI.AplicarEstiloBotonAccion(nuevaCategoria,EstiloUI.TipoAccion.Editar);
        nuevaCategoria.AutoSize=true;
        pie.Controls.Add(cancelar);pie.Controls.Add(guardar);
        Controls.Add(campos);Controls.Add(pie);Controls.Add(nota);Controls.Add(titulo);
        CancelButton=cancelar;
        guardar.Click+=async(_,_)=>await GuardarAsync();
        nuevaCategoria.Click+=async(_,_)=>await NuevaCategoriaAsync();
        FormClosing+=(_,e)=>{if(ocupado)e.Cancel=true;};
    }

    private void Bloquear(bool valor)
    {
        ocupado=valor;campos.Enabled=!valor;
        guardar.Enabled=!valor&&!resultadoIncierto&&ProductoDatos.PuedeEditarFichas;UseWaitCursor=valor;
    }
    private async Task GuardarAsync()
    {
        if(ocupado||resultadoIncierto||!ProductoDatos.PuedeEditarFichas)return;
        ProductoEdicion ficha;
        try { ficha=ValidarFormulario(); }
        catch(InvalidOperationException ex)
        {
            MessageBox.Show(this,ex.Message,"Complete los datos",MessageBoxButtons.OK,MessageBoxIcon.Warning);
            return;
        }
        bool enviado=false,exito=false;
        Bloquear(true);
        try
        {
            ProductoDatos.ValidarFicha(ficha);
            enviado=true;
            ProductoGuardadoId=await datos.GuardarFichaAsync(ficha,original);exito=true;
        }
        catch(InvalidOperationException ex){MessageBox.Show(this,ex.Message,"Revisar producto",MessageBoxButtons.OK,MessageBoxIcon.Warning);}
        catch(Exception ex)
        {
            resultadoIncierto=enviado;
            MessageBox.Show(this,enviado?"No se pudo confirmar el guardado. Cierre esta ventana y actualice la lista antes de volver a intentarlo; no dé por hecho que falló.":ex.Message,
                "Producto sin resultado confirmado",MessageBoxButtons.OK,MessageBoxIcon.Warning);
        }
        finally{Bloquear(false);}
        if(exito){DialogResult=DialogResult.OK;Close();}
    }

    private ProductoEdicion ValidarFormulario()
    {
        errores.Clear();
        var faltantes=new List<string>();
        Control? primero=null;
        void Comprobar(Control control,bool valido,string mensaje)
        {
            if(valido)return;
            errores.SetError(control,mensaje);faltantes.Add(mensaje);primero??=control;
        }
        string? errorNombre=ProductoDatos.ErrorNombreFicha(nombre.Text);
        Comprobar(nombre,errorNombre==null,errorNombre??"");
        Comprobar(categoria,categoria.SelectedItem is CategoriaProducto c&&c.Id>0,"Seleccione una categoría.");
        Comprobar(precio,precio.Leer(out decimal valorPrecio)&&valorPrecio>0,"Ingrese un precio mayor que cero, con hasta dos decimales.");
        Comprobar(descripcion,!string.IsNullOrWhiteSpace(descripcion.Text),"La descripción es obligatoria.");
        string? errorCodigo=ProductoDatos.ErrorCodigoBarrasFicha(codigo.Text);
        Comprobar(codigo,errorCodigo==null,errorCodigo??"");
        Comprobar(minimo,minimo.Leer(out decimal valorMinimo),"Ingrese el stock mínimo; admite cero. Use enteros para unidades y hasta tres decimales para peso.");
        Comprobar(peso,peso.SelectedIndex is 0 or 1,"Seleccione el tipo de venta: por unidad o por peso.");
        Comprobar(vencimiento,vencimiento.SelectedIndex is 0 or 1,"Indique si el producto requiere vencimiento: Sí o No.");
        if(faltantes.Count>0)
        {
            primero?.Focus();
            throw new InvalidOperationException(string.Join("\n",faltantes));
        }
        var ficha=new ProductoEdicion(nombre.Text.Trim(),((CategoriaProducto)categoria.SelectedItem!).Id,
            valorPrecio,valorMinimo,peso.SelectedIndex==1,descripcion.Text.Trim(),codigo.Text,vencimiento.SelectedIndex==0);
        ProductoDatos.ValidarFicha(ficha);
        return ficha;
    }

    protected override void Dispose(bool disposing)
    {
        if(disposing)errores.Dispose();
        base.Dispose(disposing);
    }

    private async Task NuevaCategoriaAsync()
    {
        if(ocupado||!ProductoDatos.PuedeEditarFichas)return;
        using var ventana=new Form {Text="Nueva categoría",Font=Font,Size=new Size(420,185),FormBorderStyle=FormBorderStyle.FixedDialog,
            MaximizeBox=false,MinimizeBox=false,StartPosition=FormStartPosition.CenterParent};
        var texto=new TextBox {Dock=DockStyle.Top,MaxLength=100,Margin=new Padding(12)};
        var contenido=new Panel {Dock=DockStyle.Fill,Padding=new Padding(14)};
        var etiqueta=new Label {Text="Nombre de la categoría",Dock=DockStyle.Top,Height=28};
        contenido.Controls.Add(texto);contenido.Controls.Add(etiqueta);
        var aceptar=new Button {Text="Aceptar",DialogResult=DialogResult.OK,AutoSize=true};
        var cancelar=new Button {Text="Cancelar",DialogResult=DialogResult.Cancel,AutoSize=true};
        var pie=new FlowLayoutPanel {Dock=DockStyle.Bottom,Height=50,Padding=new Padding(8),FlowDirection=FlowDirection.RightToLeft};
        pie.Controls.Add(cancelar);pie.Controls.Add(aceptar);ventana.Controls.Add(contenido);ventana.Controls.Add(pie);
        ventana.AcceptButton=aceptar;ventana.CancelButton=cancelar;
        if(ventana.ShowDialog(this)!=DialogResult.OK)return;
        Bloquear(true);
        try
        {
            int id=await datos.CrearCategoriaFichaAsync(texto.Text);
            var categorias=await datos.ConsultarCategoriasFichaAsync();
            if(IsDisposed)return;categoria.DataSource=categorias;categoria.SelectedValue=id;
        }
        catch(Exception){if(!IsDisposed)MessageBox.Show(this,"No se pudo confirmar la categoría. Consulte la lista antes de repetir el alta.","Categoría",MessageBoxButtons.OK,MessageBoxIcon.Warning);}
        finally{if(!IsDisposed)Bloquear(false);}
    }
}
