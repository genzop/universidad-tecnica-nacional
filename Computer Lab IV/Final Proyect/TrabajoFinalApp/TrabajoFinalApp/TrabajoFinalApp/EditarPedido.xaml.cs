﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrabajoFinalApp.Controladores;
using TrabajoFinalApp.Modelo;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TrabajoFinalApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditarPedido : ContentPage
    {
        //Propiedades
        protected List<Cliente> clientes;
        protected List<Articulo> articulos;         
                
        protected PedidoVenta tempPedido;
        protected Domicilio tempDomicilio;
        protected PedidoVentaDetalle tempDetalle;

        protected ObservableCollection<PedidoVentaDetalle> detalles;
        protected List<PedidoVentaDetalle> detallesEliminados;

        protected int IdVendedor { get; set; }
        

        //Constructor
        public EditarPedido(PedidoVenta pedido, int idVendedor)
        {
            //Se inicializan las cosas
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            //Se guarda el id del vendedor
            this.IdVendedor = idVendedor;

            //Se cargan los clientes para el picker
            cargarClientes();

            //Se cargan los articulos para el picker
            cargarArticulos();

            //Se verifica si se esta creando un pedido o si se esta modificando uno
            if (pedido == null)
            {
                lblTitulo.Text = "Agregar Pedido";
                
                this.tempPedido = new PedidoVenta();
                this.tempDomicilio = new Domicilio();
                btnEliminar.Text = "Cancelar";
                this.detalles = new ObservableCollection<PedidoVentaDetalle>();
                listDetalles.ItemsSource = this.detalles;                
            }
            else
            {
                lblTitulo.Text = "Editar Pedido";
                this.tempPedido = pedido;
                using(var domControlador = new ControladorDomicilio())
                {
                    this.tempDomicilio = domControlador.FindById(this.tempPedido.IdDomicilio);
                }
                rellenarCampos();
                cargarDetalles();
                this.detallesEliminados = new List<PedidoVentaDetalle>();
            }            
        }

        //Se cargan los articulos de la base de datos
        private void cargarArticulos()
        {
            using(var artControlador = new ControladorArticulo())
            {
                this.articulos = artControlador.ShowAll();
            }

            foreach (Articulo art in this.articulos)
            {
                pickerArticulo.Items.Add(art.Denominacion);
            }
        }

        //Cuando se presiona eliminar pedido
        private async void btnEliminar_Clicked(object sender, EventArgs e)
        {
            //Si se esta eliminando un pedido ya persistido
            if(btnEliminar.Text == "Eliminar")
            {
                //Se confirma la eliminacion del pedido
                var respuesta = await DisplayAlert("Confirmar eliminacion del pedido", "¿Está seguro que desea eliminar este pedido con sus respectivos detalles?", "Si", "Cancelar");

                if (respuesta)
                {
                    //Se elimina el domicilio de ese pedido
                    using (var domControlador = new ControladorDomicilio())
                    {
                        domControlador.Delete(this.tempDomicilio);
                    }
                    //Se elimina el pedido en si
                    using (var pedControlador = new ControladorPedidoVenta())
                    {
                        pedControlador.Delete(this.tempPedido);
                    }

                    await Navigation.PopModalAsync();
                }
            }
            else
            {
                //Se confirma que se quiera cancelar la creacion del pedido
                var respuesta = await DisplayAlert("Confirmar cancelacion del pedido", "¿Está seguro que desea cancelar la creacion de ste pedido?", "Si", "Cancelar");
                if (respuesta)
                {
                    await Navigation.PopModalAsync();
                }
            }            
        }

        //Cuando se presiona el boton guardar pedido
        private void btnGuardar_Clicked(object sender, EventArgs e)
        {
            if(lblTitulo.Text == "Agregar Pedido")
            {
                //Se crea un domicilio nuevo y se guardan los datos ingresados                
                tempDomicilio.Calle = txtCalle.Text;
                tempDomicilio.Numero = Convert.ToInt32(txtCalleNumero.Text);
                tempDomicilio.Localidad = pickerLocalidad.Items[pickerLocalidad.SelectedIndex];
                if(txtLatitud.Text != null)
                {
                    tempDomicilio.Latitud = Convert.ToDouble(txtLatitud.Text);
                }
                if(txtLongitud.Text != null)
                {
                    tempDomicilio.Longitud = Convert.ToDouble(txtLongitud.Text);
                }

                //Se persiste a la base de datos
                using(var domControlador = new ControladorDomicilio())
                {
                    domControlador.Insert(tempDomicilio);
                }

                //Se crea un pedido nuevo y se guardan los datos ingresados
                tempPedido.EsEditable = true;
                tempPedido.NroPedido = Convert.ToInt64(txtNumero.Text);
                tempPedido.IdCliente = clientes[pickerCliente.SelectedIndex].IdCliente;

                using(var cliControlador = new ControladorCliente())
                {
                    var clienteSeleccionado = cliControlador.FindById(tempPedido.IdCliente);
                    tempPedido.Cliente = clienteSeleccionado.RazonSocial;
                }

                tempPedido.IdVendedor = this.IdVendedor;
                tempPedido.IdDomicilio = tempDomicilio.IdDomicilio;
                tempPedido.Estado = pickerEstado.Items[pickerEstado.SelectedIndex];
                if(tempPedido.Estado == "Entregado")
                {
                    tempPedido.Entregado = true;
                }
                else
                {
                    tempPedido.Entregado = false;
                }
                tempPedido.FechaPedido = dateFechaPedido.Date;
                tempPedido.FechaEstimadaEntrega = dateFechaEntrega.Date;
                tempPedido.SubTotal = Convert.ToDouble(lblSubTotal.Text);
                tempPedido.GastosEnvio = Convert.ToDouble(txtGastosEnvio.Text);
                tempPedido.MontoTotal = Convert.ToDouble(lblTotal.Text);                
                
                //Se persiste el pedido a la base de datos
                using(var pedControlador = new ControladorPedidoVenta())
                {
                    pedControlador.Insert(tempPedido);
                }
            }
            else
            {
                //Se actualizan un domicilio y se guardan los datos ingresados                
                tempDomicilio.Calle = txtCalle.Text;
                tempDomicilio.Numero = Convert.ToInt32(txtCalleNumero.Text);
                tempDomicilio.Localidad = pickerLocalidad.Items[pickerLocalidad.SelectedIndex];
                if (txtLatitud.Text != null)
                {
                    tempDomicilio.Latitud = Convert.ToDouble(txtLatitud.Text);
                }
                if (txtLongitud.Text != null)
                {
                    tempDomicilio.Longitud = Convert.ToDouble(txtLongitud.Text);
                }

                using (var domControlador = new ControladorDomicilio())
                {
                    domControlador.Update(tempDomicilio);
                }



                tempPedido.NroPedido = Convert.ToInt64(txtNumero.Text);
                tempPedido.IdCliente = clientes[pickerCliente.SelectedIndex].IdCliente;

                using (var cliControlador = new ControladorCliente())
                {
                    var clienteSeleccionado = cliControlador.FindById(tempPedido.IdCliente);
                    tempPedido.Cliente = clienteSeleccionado.RazonSocial;
                }
                
                tempPedido.Estado = pickerEstado.Items[pickerEstado.SelectedIndex];
                if (tempPedido.Estado == "Entregado")
                {
                    tempPedido.Entregado = true;
                }
                else
                {
                    tempPedido.Entregado = false;
                }
                tempPedido.FechaPedido = dateFechaPedido.Date;
                tempPedido.FechaEstimadaEntrega = dateFechaEntrega.Date;
                tempPedido.SubTotal = Convert.ToDouble(lblSubTotal.Text);
                tempPedido.GastosEnvio = Convert.ToDouble(txtGastosEnvio.Text);
                tempPedido.MontoTotal = Convert.ToDouble(lblTotal.Text);

                //Se persiste el pedido a la base de datos
                using (var pedControlador = new ControladorPedidoVenta())
                {
                    pedControlador.Update(tempPedido);
                }

            }

            Navigation.PopModalAsync();      

        }

        private void listDetalles_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            //No se muestra el item seleccionado
            ((ListView)sender).SelectedItem = null;

            //Se guarda el detalle seleccionado en una variable
            this.tempDetalle = (PedidoVentaDetalle)e.Item;

            //Se muestra el formulario para editar un detalle
            tablaDetalles.IsVisible = false;

            editarDetalle.IsVisible = true;
            imgAddDetalle.IsVisible = false;
            lblTituloDetalle.Text = "Editar Detalle";

            //Se cargan los valores correspondientes
            for (int i = 0; i < articulos.Count; i++)
            {
                if(articulos[i].IdArticulo == this.tempDetalle.IdArticulo)
                {
                    pickerArticulo.SelectedIndex = i;
                }
            }

            txtCantidad.Text = this.tempDetalle.Cantidad.ToString();
            txtDescuento.Text = (this.tempDetalle.PorcentajeDescuento * 100).ToString();
        }

        public void cargarDetalles()
        {
           using(var detControlador = new ControladorPedidoVentaDetalle())
            {
                this.detalles = new ObservableCollection<PedidoVentaDetalle>(detControlador.FindByPedidoVenta(this.tempPedido.IdPedidoVenta));
                listDetalles.ItemsSource = this.detalles;
            }
        }

        //Se cargan los clientes de la base de datos para ser utilizados en el picker
        public void cargarClientes()
        {            
            using( var cClientes = new ControladorCliente())
            {
                this.clientes = cClientes.ShowAll();        
            }

            foreach (Cliente cli in clientes)
            {
                pickerCliente.Items.Add(cli.RazonSocial);
            }            
        }

        //Cuando cambian los gastos de envio, se actualizan los totales 
        private void txtGastosEnvio_TextChanged(object sender, TextChangedEventArgs e)
        {
            double subTotal = Convert.ToDouble(lblSubTotal.Text);
            double gastosEnvio = 0;
            if (txtGastosEnvio.Text != "")
            {
                gastosEnvio = Convert.ToDouble(txtGastosEnvio.Text);
            }
            double total = subTotal + gastosEnvio;
            lblTotal.Text = total.ToString();            
        }

        //Se muestra el formulario para editar un detalle
        private void imgAddDetalle_Tapped(object sender, EventArgs e)
        {
            tablaDetalles.IsVisible = false;
            editarDetalle.IsVisible = true;
            lblTituloDetalle.Text = "Agregar Detalle";
            imgAddDetalle.IsVisible = false;
            
            txtCantidad.Text = "0";
            txtDescuento.Text = "0";
            lblSubTotalDetalle.Text = "0";

            this.tempDetalle = new PedidoVentaDetalle();
        }

        //Se esconde el formulario para editar un detalle
        private void imgCerrarDetalle_Tapped(object sender, EventArgs e)
        {
            editarDetalle.IsVisible = false;
            imgAddDetalle.IsVisible = true;
            tablaDetalles.IsVisible = true;
        }

        //Se rellenan todos los campos con los datos del pedido seleccionado
        private void rellenarCampos()
        {
            txtNumero.Text = this.tempPedido.NroPedido.ToString();
            for (int i = 0; i < clientes.Count(); i++)
            {
                if(this.tempPedido.IdCliente == clientes[i].IdCliente)
                {
                    pickerCliente.SelectedIndex = i;
                }
            }
            switch (this.tempPedido.Estado)
            {
                case "Pendiente":
                    pickerEstado.SelectedIndex = 0;
                    break;
                case "Enviado":
                    pickerEstado.SelectedIndex = 1;
                    break;
                case "Entregado":
                    pickerEstado.SelectedIndex = 2;
                    break;
                case "Anulado":
                    pickerEstado.SelectedIndex = 3;
                    break;
            }

            dateFechaPedido.Date = this.tempPedido.FechaPedido;
            dateFechaEntrega.Date = this.tempPedido.FechaEstimadaEntrega;
            txtGastosEnvio.Text = this.tempPedido.GastosEnvio.ToString();
            
            txtCalle.Text = tempDomicilio.Calle;
            txtCalleNumero.Text = tempDomicilio.Numero.ToString();
            switch (tempDomicilio.Localidad)
            {
                case "Guaymallen":
                    pickerLocalidad.SelectedIndex = 0;
                    break;
                case "Capital":
                    pickerLocalidad.SelectedIndex = 1;
                    break;
            }
            if(tempDomicilio.Latitud != 0)
            {
                txtLatitud.Text = tempDomicilio.Latitud.ToString();
            }
            if(tempDomicilio.Longitud != 0)
            {
                txtLongitud.Text = tempDomicilio.Longitud.ToString();
            }
            lblSubTotal.Text = this.tempPedido.SubTotal.ToString();
            lblTotal.Text = this.tempPedido.MontoTotal.ToString();                   
        }

        //Se calcula el subtotal del detalles
        private void calcularSubTotalDetalle()
        {
            double precio = 0;
            int cantidad = 0;
            double descuento = 0;

            Articulo artSeleccionado = this.articulos[pickerArticulo.SelectedIndex];
            precio = artSeleccionado.PrecioVenta;
            if(txtCantidad.Text != "")
            {
                cantidad = Convert.ToInt32(txtCantidad.Text);
            }
            if(txtDescuento.Text != "")
            {
                descuento = (Convert.ToDouble(txtDescuento.Text)) / 100;
            }            

            double subTotal = precio * cantidad;
            subTotal = subTotal * (1 - descuento);
            lblSubTotalDetalle.Text = subTotal.ToString();

        }

        //Cambia el valor del picker de articulo
        private void pickerArticulo_SelectedIndexChanged(object sender, EventArgs e)
        {
            calcularSubTotalDetalle();
        }

        //Cambia el valor de la cantidad
        private void txtCantidad_TextChanged(object sender, TextChangedEventArgs e)
        {
            calcularSubTotalDetalle();
        }

        //Cambia el valor del descuento
        private void txtDescuento_TextChanged(object sender, TextChangedEventArgs e)
        {
            calcularSubTotalDetalle();
        }

        //Se presiona guardar detalle
        private void btnGuardarDetalle_Clicked(object sender, EventArgs e)
        {
            var articulo = articulos[pickerArticulo.SelectedIndex];

            if (lblTituloDetalle.Text == "Agregar Detalle")
            {
                tempDetalle.Cantidad = Convert.ToInt32(txtCantidad.Text);
                tempDetalle.SubTotal = Convert.ToDouble(lblSubTotalDetalle.Text);
                tempDetalle.PorcentajeDescuento = Convert.ToDouble(txtDescuento.Text);
                tempDetalle.IdArticulo = articulo.IdArticulo;
                tempDetalle.Articulo = articulo.Denominacion;
                tempDetalle.PrecioUnitario = articulo.PrecioVenta;

                this.detalles.Add(tempDetalle);
                
            }
            else
            {
                //Se guarda la posicion del pedido en la lista
                int posicion = this.detalles.IndexOf(this.tempDetalle);                

                tempDetalle.Cantidad = Convert.ToInt32(txtCantidad.Text);
                tempDetalle.SubTotal = Convert.ToDouble(lblSubTotalDetalle.Text);
                tempDetalle.PorcentajeDescuento = Convert.ToDouble(txtDescuento.Text);
                tempDetalle.IdArticulo = articulo.IdArticulo;
                tempDetalle.Articulo = articulo.Denominacion;
                tempDetalle.PrecioUnitario = articulo.PrecioVenta;

                detalles[posicion] = tempDetalle;

                this.tempDetalle = null;
            }

            editarDetalle.IsVisible = false;
            imgAddDetalle.IsVisible = true;
            tablaDetalles.IsVisible = true;
        }

        //Se presiona eliminar detalle
        private void btnEliminarDetalle_Clicked(object sender, EventArgs e)
        {
            //Se guarda la posicion del pedido en la lista
            int posicion = this.detalles.IndexOf(this.tempDetalle);

            if (this.tempDetalle.IdPedidoVentaDetalle == 0)
            {
                detalles.RemoveAt(posicion);
            }
            else
            {
                this.detallesEliminados.Add(detalles[posicion]);
                this.detalles.RemoveAt(posicion);
            }

            editarDetalle.IsVisible = false;
            imgAddDetalle.IsVisible = true;
            tablaDetalles.IsVisible = true;
        }                
    }
}

