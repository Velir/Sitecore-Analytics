using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Sitecore;
using Sitecore.Analytics;
using Sitecore.Analytics.Data;
using Sitecore.Analytics.Data.DataAccess.DataSets;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.SharedSource.Analytics.Context;
using Sitecore.SharedSource.Analytics.Context.Model;
using Sitecore.SharedSource.Analytics.Controls;

namespace Sitecore.SharedSource.Analytics.Controls.Web
{
	public partial class AnalyticsInformation : AnalyticsControl
	{
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			RaiseDebugInfoEvent_AddControl();
		}

		protected void Page_Load(object sender, EventArgs e)
		{
		}

		protected override void OnPreRender(EventArgs e)
		{
			//run base functionality
			base.OnPreRender(e);

			if (IsPostBack || !IsDmsDebuggingEnabled || !Sitecore.Context.PageMode.IsNormal)
			{
				return;
			}

			//raise events on controls to output debug information
			RaiseDebugInfoEvent();
		}

		protected override void AnalyticsControl_DebugInfoEvent(object sender, EventArgs e)
		{
			errorPanel.Attributes.Add("data_visitId", Tracker.CurrentVisit.VisitId.ToString());

			ListVisitorInformation();
			ListVisitInformation();
			ListPageInformation();
			plcDmsPanel.Visible = true;

			//updated text
			litUpdated.Text = DateTime.Now.ToString("MM/dd/yyyy H:mm:ss");

			Int32 maxQueueSize = 0;
			litMaxQueueSize.Text = "N/A";
			if(Int32.TryParse(Sitecore.Configuration.Settings.GetSetting("Analytics.MaxQueueSize"), out maxQueueSize))
			{
				litMaxQueueSize.Text = maxQueueSize.ToString("N0");
			}

			Int32 maxRows = 0;
			litMaxRows.Text = "N/A";
			if(Int32.TryParse(Sitecore.Configuration.Settings.GetSetting("Analytics.TrackerChanges.MaxRows"), out maxRows))
			{
				litMaxRows.Text = maxRows.ToString("N0");
			}

			btnNewVisit.Visible = IsDmsNewVisitButtonEnabled;
			btnFlush.Visible = IsDmsFlushButtonEnabled;
		}

		protected void btnNewVisit_OnClick(object sender, EventArgs e)
		{
			Tracker.EndVisit(true);
			RaiseDebugInfoEvent();
		}

		/// <summary>
		/// Displays information about the current page
		/// </summary>
		private void ListPageInformation()
		{
			List<string> info = new List<string>();

			info.Add(string.Format("Page Id: {0}", Tracker.CurrentPage.PageId));
			info.Add(string.Format("Page Views: {0}", PageStatistics.GetPageViewCount(Sitecore.Context.Item).ToString("N0")));

			rptPageInfo.DataSource = info;
			rptPageInfo.DataBind();
		}

		/// <summary>
		/// Displays information about the current visit
		/// </summary>
		private void ListVisitInformation()
		{
			List<string> info = new List<string>();

			info.Add(string.Format("Visit Id: {0}", Tracker.CurrentVisit.VisitId));
			info.Add(string.Format("Visit Started: {0}", Tracker.CurrentVisit.StartDateTime.ToString("MM/dd/yyyy H:mm:ss")));
			info.Add("EV Accrued: <span class=\"visitEV\"></span>");

			rptVisitInfo.DataSource = info;
			rptVisitInfo.DataBind();
		}

		/// <summary>
		/// Displays information about the current visitor
		/// </summary>
		private void ListVisitorInformation()
		{
			List<string> info = new List<string>();

			//visitor
			Visitor visitor = PageStatistics.GetCurrentVisitor();
			if (visitor != null)
			{
				info.Add(string.Format("Visitor Id: {0}", visitor.VisitorId));
				info.Add("Visitor's Engagement Value: <span class=\"visitEV\"></span>");
			}

			rptVisitorInfo.DataSource = info;
			rptVisitorInfo.DataBind();
		}

		protected void rptDMS_OnItemDataBound(object sender, RepeaterItemEventArgs e)
		{
			if (e.Item == null || e.Item.DataItem == null)
			{
				return;
			}

			string point = (string)e.Item.DataItem;
			Literal litPoint = (Literal)e.Item.FindControl("litPoint");

			litPoint.Text = point;
		}
	}
}