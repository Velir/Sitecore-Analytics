<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MultivariantReport.ascx.cs" Inherits="Sitecore.SharedSource.Analytics.Controls.Web.MultivariantReport" %>
<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI" Assembly="Telerik.Web.UI" %>

<div class="reportParameters">
	<h3>Report Parameters</h3>
	<!-- Test Selector -->
	<div class="line testSelector">
		<div class="lineTitle">Select Test:</div>
		<div><asp:DropDownList runat="server" ID="ddlTest" OnSelectedIndexChanged="ddlTest_SelectedIndexChanged" AutoPostBack="true"/></div>
	</div>

	<!-- Commands -->
	<div class="commands">
		<div><asp:Button runat="server" ID="btnRun" OnClick="btnRun_Click" Text="Run" Enabled="false" /></div>
		<div><asp:Button runat="server" ID="btnClear" OnClick="btnClear_Click" Text="Clear" Enabled="false" /></div>
	</div>

	<!-- Goal Selector -->
	<div class="line testSelector">
		<div class="lineTitle">Select Goal:</div>
		<div><asp:DropDownList runat="server" ID="ddlGoal" Enabled="false" /></div>
	</div>

	<div class="line datePicker">
		<div class="lineTitle">Start Date:</div>
		<div>
			<telerik:RadDateTimePicker ID="beginDatePicker" runat="server" DateInput-EmptyMessage="mm/dd/yyyy" 
					MinDate="01/01/1000" MaxDate="01/01/3000" DatePopupButton-ImageUrl="/sitecore modules/shell/analytics/images/calendar.gif"
					DatePopupButton-HoverImageUrl="/sitecore modules/shell/analytics/images/calendar.gif" EnableTyping="false" >
					<Calendar ShowRowHeaders="false"></Calendar>
				</telerik:RadDateTimePicker>
		</div>
	</div>
	<div class="line datePicker">
		<div class="lineTitle">End Date:</div>
		<div>
			<telerik:RadDateTimePicker ID="endDatePicker" runat="server" DateInput-EmptyMessage="mm/dd/yyyy"
					MinDate="01/01/1000" MaxDate="01/01/3000" DatePopupButton-ImageUrl="/sitecore modules/shell/analytics/images/calendar.gif"
					DatePopupButton-HoverImageUrl="/sitecore modules/shell/analytics/images/calendar.gif" EnableTyping="false">
					<Calendar ShowRowHeaders="false"></Calendar>
				</telerik:RadDateTimePicker>
		</div>
	</div>
</div>

<asp:PlaceHolder runat="server" ID="plcErrorPanel" Visible="false">
	<div class="errorPanel">
		<ul>
			<asp:Repeater runat="server" ID="rptErrors" OnItemDataBound="rptErrors_OnItemDataBound">
				<ItemTemplate>
					<li><asp:Literal runat="server" ID="litError" /></li>
				</ItemTemplate>
			</asp:Repeater>
		</ul>
	</div>
</asp:PlaceHolder>

<!-- Report Specific Section -->
<asp:PlaceHolder runat="server" ID="plcReport" Visible="false">
	<div id="reportInformation">
		<h3>Report Information</h3>

		<!-- Test Start Date -->
		<div class="line">
			<div class="lineTitle extended scWindowCaption">Test Started:</div>
			<div><asp:Literal runat="server" ID="litTestStarted" /></div>
		</div>

		<!-- Test End Date -->
		<div class="line">
			<div class="lineTitle extended scWindowCaption">Test Completed:</div>
			<div><asp:Literal runat="server" ID="litTestEnded" /></div>
		</div>

		<!-- Test Progress -->
		<div class="line">
			<div class="lineTitle extended scWindowCaption">Status:</div>
			<div><asp:Literal runat="server" ID="litProgress" /></div>
		</div>

		<!-- Report Data -->
		<div class="testData">
			<asp:PlaceHolder runat="server" ID="plcAchievedGoalData" Visible="false">
				<div class="testDataSection">
					<div class="sectionTitle"><img src="/sitecore modules/shell/analytics/images/breakpoint.png"/><h3>Test: <asp:Literal runat="server" ID="litAchievedGoal" /></h3></div>
					<div class="tableHeader">
						<div class="testDataColA">Variations</div>
						<div class="testDataColB">Visits</div>
						<div class="testDataColC">Achieved Goals</div>
						<div class="testDataColD">Effectiveness</div>
					</div>

					<asp:Repeater runat="server" ID="rptAchievedGoals" OnItemDataBound="rptAchievedGoals_OnItemDataBound">
						<ItemTemplate>
							<div class="tableData">
								<div class="testDataColA"><asp:Literal runat="server" ID="litVariationName" /></div>
								<div class="testDataColB"><asp:Literal runat="server" ID="litVisitCount" /></div>
								<div class="testDataColC"><asp:Literal runat="server" ID="litActiveVisitCount" /></div>
								<div class="testDataColD"><asp:Literal runat="server" ID="litEffectiveness" /></div>
							</div>
						</ItemTemplate>
					</asp:Repeater>

					<div class="tableData">
						<div class="testDataColA">Total:</div>
						<div class="testDataColB"><asp:Literal runat="server" ID="litTotalVisitCount" /></div>
						<div class="testDataColC"><asp:Literal runat="server" ID="litTotalAchievedCount" /></div>
					</div>
				</div>
			</asp:PlaceHolder>
		</div>
	</div>
</asp:PlaceHolder>