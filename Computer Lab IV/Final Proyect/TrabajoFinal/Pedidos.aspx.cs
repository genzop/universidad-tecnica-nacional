﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Pedidos : System.Web.UI.Page
{
    BaseDatosDataContext bd;

    //Si no hay una sesion iniciada se redirecciona a la pagina de LogIn
    protected void Page_Load(object sender, EventArgs e)
    {
        bd = new BaseDatosDataContext();
                
        if (Session["IdVendedor"] == null)
        {
            Response.Redirect("Ingresar.aspx");
        }
        else
        {           
            HyperLink pedidos = (HyperLink)Master.FindControl("hlPedidos");
            pedidos.CssClass = "active";
            
            if(Convert.ToInt32(Session["IdVendedor"]) == 20)
            {
                SqlDataSource1.SelectCommand = "SELECT PedidoVenta.IdPedidoVenta, PedidoVenta.FechaEstimadaEntrega, PedidoVenta.GastosEnvio, PedidoVenta.Estado, PedidoVenta.FechaPedido, PedidoVenta.NroPedido, PedidoVenta.SubTotal, PedidoVenta.MontoTotal, Cliente.RazonSocial FROM PedidoVenta INNER JOIN Cliente ON PedidoVenta.IdCliente = Cliente.IdCliente";
            }

        }
    }
    
    //Se redirecciona a la pagina de editar pedidos
    protected void imgEdit_Command(object sender, CommandEventArgs e)
    {
        Response.Redirect("EditarPedido.aspx?id=" + e.CommandArgument);
    }

    //Se elimina el pedido seleccionado
    protected void imgDelete_Command(object sender, CommandEventArgs e)
    {
        try
        {
            int registroSeleccionado = Convert.ToInt32(e.CommandArgument);

            //Se borran todos los detalles asociados con este pedido
            var detalles = from detalle in bd.PedidoVentaDetalles
                           where detalle.IdPedidoVenta == registroSeleccionado
                           select detalle;

            foreach (var detalle in detalles)
            {
                bd.PedidoVentaDetalles.DeleteOnSubmit(detalle);
                bd.SubmitChanges();
            }

            //Se borra el pedido y su domicilio correspondiente
            var temp = (from pedido in bd.PedidoVentas
                        where pedido.IdPedidoVenta == Convert.ToInt32(registroSeleccionado)
                        select pedido).Single();          

            int idCliente = Convert.ToInt32(temp.IdCliente);
            bd.PedidoVentas.DeleteOnSubmit(temp);            
            bd.SubmitChanges();

            calcularSaldoCliente(idCliente);
            grdPedidos.DataBind();
        }
        catch (Exception) { }
    }

    //Se calcula el saldo total del cliente
    private void calcularSaldoCliente(int id)
    {
        try
        {
            double saldo = 0;

            bool hayPedidos = (from ped in bd.PedidoVentas
                               where ped.IdCliente == id
                               select ped).Any();

            if (hayPedidos)
            {
                var pedidos = from ped in bd.PedidoVentas
                              where ped.IdCliente == id
                              select ped;

                foreach (var pedido in pedidos)
                {
                    saldo -= Convert.ToDouble(pedido.MontoTotal);
                }
            }

            Cliente cliente = (from cli in bd.Clientes
                               where cli.IdCliente == id
                               select cli).Single();

            cliente.Saldo = saldo;
            bd.SubmitChanges();
        }
        catch (Exception) { }       
    }

    //Se busca los pedidos que cumplan con el criterio indicado
    protected void imgFind_Click(object sender, ImageClickEventArgs e)
    {
        string query = "";

        if (Convert.ToInt32(Session["IdVendedor"]) == 20)
        {
            if (txtBuscar.Text == "")
            {
                query = "SELECT PedidoVenta.IdPedidoVenta, PedidoVenta.FechaEstimadaEntrega, PedidoVenta.GastosEnvio, PedidoVenta.Estado, PedidoVenta.FechaPedido, PedidoVenta.NroPedido, PedidoVenta.SubTotal, PedidoVenta.MontoTotal, Cliente.RazonSocial FROM PedidoVenta INNER JOIN Cliente ON PedidoVenta.IdCliente = Cliente.IdCliente";
            }
            else
            {
                query = "SELECT PedidoVenta.IdPedidoVenta, PedidoVenta.FechaEstimadaEntrega, PedidoVenta.GastosEnvio, PedidoVenta.Estado, PedidoVenta.FechaPedido, PedidoVenta.NroPedido, PedidoVenta.SubTotal, PedidoVenta.MontoTotal, Cliente.RazonSocial FROM PedidoVenta INNER JOIN Cliente ON PedidoVenta.IdCliente = Cliente.IdCliente WHERE " + ddlBuscar.SelectedValue + " LIKE '%" + txtBuscar.Text + "%'";
            }
        }
        else
        {
            if (txtBuscar.Text == "")
            {
                query = "SELECT PedidoVenta.IdPedidoVenta, PedidoVenta.FechaEstimadaEntrega, PedidoVenta.GastosEnvio, PedidoVenta.Estado, PedidoVenta.FechaPedido, PedidoVenta.NroPedido, PedidoVenta.SubTotal, PedidoVenta.MontoTotal, Cliente.RazonSocial FROM PedidoVenta INNER JOIN Cliente ON PedidoVenta.IdCliente = Cliente.IdCliente WHERE PedidoVenta.IdVendedor=@vendedor";
            }
            else
            {
                query = "SELECT PedidoVenta.IdPedidoVenta, PedidoVenta.FechaEstimadaEntrega, PedidoVenta.GastosEnvio, PedidoVenta.Estado, PedidoVenta.FechaPedido, PedidoVenta.NroPedido, PedidoVenta.SubTotal, PedidoVenta.MontoTotal, Cliente.RazonSocial FROM PedidoVenta INNER JOIN Cliente ON PedidoVenta.IdCliente = Cliente.IdCliente WHERE PedidoVenta.IdVendedor=@vendedor AND " + ddlBuscar.SelectedValue + " LIKE '%" + txtBuscar.Text + "%'";
            }
        }

        SqlDataSource1.SelectCommand = query;
        grdPedidos.DataBind();
    }

    protected void imgAdd_Click(object sender, ImageClickEventArgs e)
    {
        Response.Redirect("EditarPedido.aspx");
    }

    protected void imgPDF_Command(object sender, CommandEventArgs e)
    {
        Response.Redirect("ReportePedido.ashx?id=" + e.CommandArgument);
    }
}