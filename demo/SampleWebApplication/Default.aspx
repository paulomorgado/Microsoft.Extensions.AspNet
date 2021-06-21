<%@ Page Title="Web Forms" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="SampleWebApplication.Default" Async="True" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <h1><%: Title %></h1>

    <h2><a runnat="server" href="<%: SyndicationFeed.Links[0] %>"><%: SyndicationFeed.Description.Text %></a></h2>

    <asp:DataList ID="FeedItemsDataList" runat="server" EnableViewState="false">
        <ItemTemplate>
            <h3><a runnat="server" href="<%# DataBinder.Eval(Container.DataItem, "Links") %>"><%# DataBinder.Eval(Container.DataItem, "Title.Text") %></a></h3>
            <p><%# DataBinder.Eval(Container.DataItem, "Summary.Text") %></p>
        </ItemTemplate>
    </asp:DataList>

</asp:Content>
