<%@ Page Title="" Language="C#" MasterPageFile="~/sitecore modules/shell/analytics/Analytics.Master" AutoEventWireup="true" CodeBehind="Multivariant.aspx.cs" Inherits="Sitecore.SharedSource.Analytics.Reports.Multivariant" %>
<%@ Register TagPrefix="analytics" TagName="Multivariant" Src="/sitecore modules/Shell/Analytics/Controls/MultivariantReport.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
	<title>Multivariant Report</title>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
	<div id="content">
		<div id="dmsHeader">
			<div id="reportLogo">
				<asp:PlaceHolder runat="server" ID="plcLogo" Visible="false">
					<asp:Image runat="server" ID="imgLogo" AlternateText="Set the Report Logo in the Analytics Configuration" />
				</asp:PlaceHolder>
			</div>
			<div id="reportTitle"><h1>Multivariant Report</h1></div>
		</div>
		<div id="reportContent">
			<analytics:Multivariant runat="server" />
		</div>
	</div>
</asp:Content>