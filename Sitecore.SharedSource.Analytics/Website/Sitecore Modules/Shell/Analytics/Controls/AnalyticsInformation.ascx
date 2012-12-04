<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AnalyticsInformation.ascx.cs" Inherits="Sitecore.SharedSource.Analytics.Controls.Web.AnalyticsInformation" %>

<asp:PlaceHolder runat="server" ID="plcDmsPanel" Visible="false">
	<link rel="stylesheet" href="/sitecore modules/shell/analytics/styles/analytics.infoPanel.css">
	<script type="text/javascript" src="/sitecore modules/shell/analytics/scripts/analytics.js"></script>

	<asp:Panel runat="server" ID="errorPanel" CssClass="errorPanel analyticsPanel">
		<div class="dmsHeader">
			<div id="headerTitle">
				<h3 class="dmsTitle">DMS Debugging Information Panel</h3> <span>(* It may take time for Sitecore to write results to the database)</span>
			</div>
			<div id="refresh">
				<input type="submit" value="Refresh" onclick="analytics.refresh();return false;">
				<input runat="server" id="btnFlush" type="submit" value="Flush" onclick="analytics.flush();return false;">
				<asp:Button runat="server" ID="btnNewVisit" Text="New Visit" OnClick="btnNewVisit_OnClick" />
			</div>
		</div> 

		<div id="analyticsSettings">
			<span>Max Queue Size: <asp:Literal runat="server" ID="litMaxQueueSize" /></span>
			<span>Tracker Changes -  Max Rows: <asp:Literal runat="server" ID="litMaxRows" /></span>
			<span>Last Updated: <span class="analyticsUpdatedDate"><asp:Literal runat="server" ID="litUpdated" /></span></span>
		</div>

		<!-- First Row -->
		<div class="dmsInfoRow">
			<!-- Visitor Info -->
			<div class="infoSection">
				<h3>Visitor Information</h3>
				<ul>
					<asp:Repeater runat="server" ID="rptVisitorInfo" OnItemDataBound="rptDMS_OnItemDataBound">
						<ItemTemplate>
							<li><asp:Literal runat="server" ID="litPoint" /></li>
						</ItemTemplate>
					</asp:Repeater>
				</ul>
			</div>

			<!-- Visit Info -->
			<div class="infoSection">
				<h3>Visit Information</h3>
				<ul>
					<asp:Repeater runat="server" ID="rptVisitInfo" OnItemDataBound="rptDMS_OnItemDataBound">
						<ItemTemplate>
							<li><asp:Literal runat="server" ID="litPoint" /></li>
						</ItemTemplate>
					</asp:Repeater>
				</ul>
			</div>

			<!-- Page Info -->
			<div class="infoSection">
				<h3>Visit Page Information</h3>
				<ul>
					<asp:Repeater runat="server" ID="rptPageInfo" OnItemDataBound="rptDMS_OnItemDataBound">
						<ItemTemplate>
							<li><asp:Literal runat="server" ID="litPoint" /></li>
						</ItemTemplate>
					</asp:Repeater>
				</ul>
			</div>
		</div>

		<!--Second Row -->
		<div class="dmsInfoRow">
			<!-- Goals Info -->
			<div class="infoSection">
				<h3>Goals Met During Visit</h3>
				<ul class="analyticsGoals">
				</ul>
			</div>

			<!-- Second Section of Goals Info -->
			<div class="infoSection goalsSecondColumn">
				<ul class="analyticsGoals2">
				</ul>
			</div>
		</div>
	</asp:Panel>
</asp:PlaceHolder>