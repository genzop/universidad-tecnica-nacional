﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Navegacion.master" AutoEventWireup="true" CodeFile="EditarRubro.aspx.cs" Inherits="EditarRubro" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cphCabecera" Runat="Server">
    <title>Editar Rubro</title>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cphContenido" Runat="Server">
    <br /><br /><br />
    <div class="create-page" style="padding-top: 0">
        <div class="create-form">

            <!-- Formulario Rubro -->
            <form runat="server">

                <div style="margin: 10px 0">
                    <asp:Label ID="lblTitulo" runat="server" Text="Titulo Temporal" Font-Names="Arial" Font-Bold="true" Font-Size="25px" /><br />
                    <br />
                </div>

                <div class="editContent">
                    <p class="editContentTitle" style="font-weight: bold">Codigo</p>
                    <asp:TextBox ID="txtCodigo" runat="server" CssClass="inputCentrado" />
                    <asp:RequiredFieldValidator ID="rfvCodigo" runat="server" ControlToValidate="txtCodigo" ErrorMessage="* Este campo es obligatorio" Display="Dynamic" ForeColor="#ff0000" Font-Bold="true" Font-Size="11px" />
                    <asp:RegularExpressionValidator ID="revCodigo" runat="server" ControlToValidate="txtCodigo" ValidationExpression="^(\s|.){1,20}$" ErrorMessage="* El codigo puede contener hasta un máximo de 20 caracteres" Display="Dynamic" ForeColor="#ff0000" Font-Bold="true" Font-Size="11px" />
                </div>

                <div class="editContent">
                    <p class="editContentTitle" style="font-weight: bold">Denominacion</p>
                    <asp:TextBox id="txtDenominacion" runat="server" TextMode="MultiLine" Rows="4" cssclass="inputCentrado" style="resize: none"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rfvDenominacion" runat="server" ControlToValidate="txtDenominacion" ErrorMessage="* Este campo es obligatorio" Display="Dynamic" ForeColor="#ff0000" Font-Bold="true" Font-Size="11px"/>         
                    <asp:RegularExpressionValidator ID="revDenominacion" runat="server" ControlToValidate="txtDenominacion" ValidationExpression="^(\s|.){1,200}$" ErrorMessage="* La denominacion puede contener hasta un máximo de 200 caracteres" Display="Dynamic" ForeColor="#ff0000" Font-Bold="true" Font-Size="11px"/>    
                </div>

                <div class="editContent">
                    <p class="editContentTitle" style="font-weight: bold">Rubro Superior</p>
                    <asp:DropDownList ID="ddlRubroSuperior" runat="server" CssClass="drownDownList" DataSourceID="LinqDataSource1" DataTextField="Denominacion" DataValueField="IdRubro" style="margin-bottom: 10px"></asp:DropDownList>
                    <asp:LinqDataSource ID="LinqDataSource1" runat="server" ContextTypeName="BaseDatosDataContext" EntityTypeName="" Select="new (IdRubro, Denominacion)" TableName="Rubros">
                    </asp:LinqDataSource>
                </div>               
                
                <asp:button id="btnAccion" runat="server" text="Texto temporal" cssclass="botonImportante" OnClick="btnAccion_Click"/>
            </form>
        </div>
    </div>
</asp:Content>
