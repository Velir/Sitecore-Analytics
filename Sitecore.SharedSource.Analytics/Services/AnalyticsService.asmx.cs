using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using Sitecore;
using Sitecore.Analytics;
using Sitecore.Analytics.Data;
using Sitecore.Analytics.Data.DataAccess.DataSets;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.SharedSource.Analytics.Context.Model;
using Sitecore.SharedSource.Analytics.Context.PageStatistics;
using Sitecore.SharedSource.Commons.Extensions;
using Sitecore.SharedSource.Commons.Utilities;

namespace Sitecore.SharedSource.Analytics.Services
{
	/// <summary>
	/// Summary description for AnalyticsService
	/// </summary>
	[WebService(Namespace = "http://tempuri.org/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[System.ComponentModel.ToolboxItem(false)]
	[System.Web.Script.Services.ScriptService]
	public class AnalyticsService : System.Web.Services.WebService
	{
		/// <summary>
		/// Sets goal against a visit, usually used for media items
		/// </summary>
		/// <param name="currentItemId"></param>
		/// <param name="linkedId"></param>
		/// <param name="goalId"></param>
		[WebMethod]
		public void SetGoal(string currentItemId, string linkedId, string goalId)
		{
			if (string.IsNullOrEmpty(currentItemId) || string.IsNullOrEmpty(linkedId) || string.IsNullOrEmpty(goalId))
			{
				return;
			}

			Database db = Sitecore.Context.Database;
			if(db == null || db.Name.ToLower() == "core")
			{
				db = Sitecore.Context.ContentDatabase;
			}

			if(db == null)
			{
				return;
			}

			Item currentItem = SitecoreItemFinder.GetItem(db, currentItemId);
			Item linkedItem = SitecoreItemFinder.GetItem(db, linkedId);
			Item goalItem = SitecoreItemFinder.GetItem(db, goalId);

			if (currentItem.IsNull() || linkedItem.IsNull() || goalItem.IsNull())
			{
				return;
			}

			PageStatisticsContext pageStatistics = new PageStatisticsContext();
			pageStatistics.SetGoalMet(currentItem, linkedItem, goalItem, Tracker.CurrentVisit.VisitId, Tracker.CurrentVisit.VisitorId);
		}

		/// <summary>
		/// Commits the data to the database
		/// </summary>
		[WebMethod]
		public void Flush()
		{
			if (!IsDmsFlushButtonEnabled)
			{
				return;
			}

			TrackerChanges.Flush();
		}

		/// <summary>
		/// Returns analytic information for a particular visit
		/// </summary>
		/// <param name="visitId"></param>
		/// <returns></returns>
		[WebMethod(EnableSession = true)]
		public Hashtable GetInformation(string visitId)
		{
			List<Hashtable> goals = new List<Hashtable>();
			List<Hashtable> goals2 = new List<Hashtable>();

			PageStatisticsContext pageStatistics = new PageStatisticsContext();

			Sitecore.SharedSource.Analytics.Context.Visit visit = pageStatistics.GetVisit_ById(visitId);
			if(visit == null)
			{
				return null;
			}

			Sitecore.SharedSource.Analytics.Context.Visitor visitor = pageStatistics.GetVisitor_ById(visit.VisitorId.ToString());
			if (visitor == null)
			{
				return null;
			}

			List<VisitGoal> visitGoalsMet = pageStatistics.GetGoalsMetDuringVisit(visit.VisitId).OrderByDescending(x => x.GoalMetDate).ToList();
			foreach(VisitGoal visitGoal in visitGoalsMet)
			{
				Hashtable goalTable = new Hashtable();
				goalTable.Add("Date", visitGoal.GoalMetDate.Value.ToString("MMMM dd, yyyy H:mm:ss"));
				goalTable.Add("Goal", visitGoal.GoalName);
				goalTable.Add("Amount", visitGoal.EngagementValue);
				goals.Add(goalTable);
			}

			const int startSecondColumnAt = 3;
			if (goals.Count > startSecondColumnAt)
			{
				double dividedValue = ((double)goals.Count) / 2;
				int amountPerColumn = (int)Math.Round(dividedValue, 0, MidpointRounding.AwayFromZero);
				goals2 = goals.GetRange(amountPerColumn, goals.Count - amountPerColumn);

				goals = goals.GetRange(0, amountPerColumn);
			}

			Hashtable hashtable = new Hashtable();
			hashtable.Add("VisitorEngagementValue", pageStatistics.GetVisitorEngagementValue(visitor.VisitorId).ToString("N0"));
			hashtable.Add("VisitEngagementValue", pageStatistics.GetVisitEngagementValue(visit.VisitId).ToString("N0"));
			hashtable.Add("Goals", goals);
			hashtable.Add("Goals2", goals2);

			return hashtable;
		}

		/// <summary>
		/// Checks to see if DMS Flush button is enabled
		/// </summary>
		private bool IsDmsFlushButtonEnabled
		{
			get
			{
				//verify setting
				if (string.IsNullOrEmpty(Sitecore.Configuration.Settings.GetSetting("Analytics.Debugging.FlushButtonEnabled")))
				{
					return false;
				}

				bool enabled = false;
				if (!bool.TryParse(Sitecore.Configuration.Settings.GetSetting("Analytics.Debugging.FlushButtonEnabled"), out enabled))
				{
					return false;
				}

				return enabled;
			}
		}
	}
}
