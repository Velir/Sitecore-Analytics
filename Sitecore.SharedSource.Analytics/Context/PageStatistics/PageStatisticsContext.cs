using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Linq;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using log4net;
using Sitecore.Analytics;
using Sitecore.Analytics.Configuration;
using Sitecore.Analytics.Data.DataAccess;
using Sitecore.Analytics.Data.DataAccess.DataAdapters;
using Sitecore.Analytics.Data.DataAccess.DataSets;
using Sitecore.Analytics.Data.Items;
using Sitecore.Analytics.Testing;
using Sitecore.Analytics.Testing.Statistics;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Links;
using Sitecore.Resources.Media;
using Sitecore.SharedSource.Analytics.Context.Model;
using Sitecore.SharedSource.Commons.Extensions;
using Sitecore.Sites;

namespace Sitecore.SharedSource.Analytics.Context.PageStatistics
{
	public class PageStatisticsContext
	{
		// Fields
		private static readonly TimeSpan pageDailyVisitsCalculationPeriod = Settings.WebEdit.PageDailyVisitsCalculationPeriod;

		/// <summary>
		/// Sets the Goal Met
		/// </summary>
		/// <param name="currentItem"></param>
		/// <param name="linkedItem"></param>
		/// <param name="goalItem"></param>
		/// <param name="visitId"></param>
		/// <param name="visitorId"></param>
		/// <returns></returns>
		public bool SetGoalMet(Item currentItem, Item linkedItem, Item goalItem, Guid visitId, Guid visitorId)
		{
			try
			{
				using (AnalyticsDataContext AnalyticsDataContext = new AnalyticsDataContext(ConfigurationManager.ConnectionStrings["analytics"].ConnectionString))
				{
					//check to see if page exists
					Page currentPage = AnalyticsDataContext.Pages.Where(x => x.ItemId == currentItem.ID.Guid && x.VisitId == visitId).FirstOrDefault();
					if(currentPage == null)
					{
						Logger.Error("Page Statistics Context - Could not retrieve page");
						return false;
					}

					PageEventDefinition pageEventDefinition = AnalyticsDataContext.PageEventDefinitions.Where(x => x.Name == goalItem.Name).FirstOrDefault();
					if(pageEventDefinition == null)
					{
						Logger.Error("Page Statistics Context - Could not retrieve page event definition: " + goalItem.Name);
						return false;
					}

					//verify there is not already a page event 
					PageEvent existantPageEvent = AnalyticsDataContext.PageEvents.
						Where(x => x.VisitId == visitId &&
									x.PageId == currentPage.PageId &&
									x.PageEventDefinitionId == pageEventDefinition.PageEventDefinitionId &&
									x.ItemId == linkedItem.ID.Guid).FirstOrDefault();

					if(existantPageEvent != null)
					{
						Logger.Warn("Page Statistics Context - Page event already exists");
						return false;
					}

					//insert page event
					try
					{
						Guid pageEventId = Guid.NewGuid();

						PageEvent pageEvent = new PageEvent();
						pageEvent.PageEventId = pageEventId;
						pageEvent.VisitId = visitId;
						pageEvent.VisitorId = visitorId;
						pageEvent.PageEventDefinitionId = pageEventDefinition.PageEventDefinitionId;
						pageEvent.PageEventDefinition = pageEventDefinition;
						pageEvent.PageId = currentPage.PageId;
						pageEvent.Page = currentPage;
						pageEvent.ItemId = linkedItem.ID.Guid;
						pageEvent.IntegrationId = Guid.Empty;
						pageEvent.DataKey = linkedItem.Paths.FullPath;
						pageEvent.DateTime = DateTime.Now.ToUniversalTime();
						pageEvent.DataCode = 0;
						pageEvent.CustomSorting = 0;
						pageEvent.IntegrationLabel = string.Empty;
						pageEvent.Data = string.Empty;
						pageEvent.Timestamp = 634891148665341607;
						pageEvent.Text = string.Empty;

						AnalyticsDataContext.PageEvents.InsertOnSubmit(pageEvent);
						AnalyticsDataContext.SubmitChanges();
					}
					catch (Exception ex)
					{
						Logger.Error("Page Statistics Context - Could not create page record");
						Logger.Error(ex.Message);
						Logger.Error(ex.InnerException);
					}
				}
			}
			catch (Exception e)
			{
				Logger.Error("Page Statistics Context - Could not set goal met");
				Logger.Error(e.Message);
				Logger.Error(e.InnerException);
			}

			return false;
		}
		
		/// <summary>
		/// Returns a grouping of achieved goals against visits per variation
		/// </summary>
		/// <param name="goalItem"></param>
		/// <param name="testItem"></param>
		/// <param name="startDate"></param>
		/// <param name="endDate"></param>
		/// <returns></returns>
		public List<VisitVariation> GetAchievedGoals(Item goalItem, Item testItem, DateTime startDate, DateTime endDate)
		{
			if (goalItem.IsNull() || testItem.IsNull())
			{
				return new List<VisitVariation>();
			}

			try
			{
				using (AnalyticsDataContext AnalyticsDataContext = new AnalyticsDataContext(ConfigurationManager.ConnectionStrings["analytics"].ConnectionString))
				{
					//how many people saw the test
					string visitQuery = "select count(*) as VisitCount, dbo.fn_abc_hexadecimal(p.testValues) AS TestValues from pages p " +
										"where p.testSetId = {0} and p.[datetime] >= {1} and p.[datetime] <= {2} " +
										"group by testValues";

					//how many people performed action (EV) against the test?
					string evQuery = "select count(*) as AchievedGoalCount, dbo.fn_abc_hexadecimal(testValues) as TestValues from " +
										"(" +
											"select p.itemId, p.[DateTime], (select distinct(testValues) from pages where testSetId = {0} and visitId = p.visitId) as testValues " +
											"from pages p " +
												"inner join pageEvents pe on pe.pageId = p.pageId " +
											"where pe.pageEventDefinitionId = {1} and p.[datetime] >= {2} and p.[datetime] <= {3} " +
										") t " +
									"where testValues is not null " +
									"group by testValues";

					//get page hits
					var visitResults = AnalyticsDataContext.ExecuteQuery<VisitVariation>(visitQuery, testItem.ID.RemoveBrackets(), startDate, endDate).ToList();
					if (visitResults.Count == 0)
					{
						return new List<VisitVariation>();
					}

					//get hits that hit goal
					var engagementResults = AnalyticsDataContext.ExecuteQuery<VisitVariation>(evQuery, testItem.ID.RemoveBrackets(), goalItem.ID.RemoveBrackets(), startDate, endDate).ToList();

					List<VisitVariation> variations = new List<VisitVariation>();
					foreach (VisitVariation variationResult in visitResults)
					{
						VisitVariation result = variationResult;
						Int32 achievedCount = engagementResults.Where(x => x.TestValues == result.TestValues).Select(x => x.AchievedGoalCount).FirstOrDefault();
						result.AchievedGoalCount = achievedCount;

						variations.Add(result);
					}

					return variations;
				}
			}
			catch (Exception e)
			{
				Logger.Error("Page Statistics Context - Could not retrieve the achieved goals");
				Logger.Error(e.Message);
				Logger.Error(e.InnerException);
			}

			return new List<VisitVariation>();
		}

		/// <summary>
		/// Returns test variations for a specific test and item
		/// </summary>
		/// <param name="currentItem"></param>
		/// <param name="testItem"></param>
		/// <param name="startDate"></param>
		/// <param name="endDate"></param>
		/// <returns></returns>
		public List<TestVariationGrouping> GetTestVariations_ForCurrentItem(Item currentItem, Item testItem, DateTime startDate, DateTime endDate)
		{
			if (currentItem.IsNull())
			{
				return new List<TestVariationGrouping>();
			}

			try
			{
				using (AnalyticsDataContext AnalyticsDataContext = new AnalyticsDataContext(ConfigurationManager.ConnectionStrings["analytics"].ConnectionString))
				{
					var results = AnalyticsDataContext.Pages
						.Where(x => 
							x.TestSetId == testItem.Parent.ID.Guid 
							&& x.ItemId == currentItem.ID.Guid
							&& x.DateTime >= startDate
							&& x.DateTime <= endDate)
						.Select(x => new
						{
							TestSetID = x.TestSetId,
							TestValueString = AnalyticsDataContext.fn_abc_hexadecimal(x.TestValues),
							SomeValue = x.Visit.Value
						});

					var results1 = results.GroupBy(x => x.TestValueString).Select(g => new { TestSet = g.First().TestValueString, Count = g.Count(), ValueSum = g.Sum(x => x.SomeValue) }).ToList();

					List<TestVariationGrouping> groupings = new List<TestVariationGrouping>();
					foreach (var obj in results1)
					{
						var binary = obj.TestSet;
						var totalVisits = results.Where(x => x.TestValueString == binary).ToList();
						var inActiveVisits = results.Where(x => x.SomeValue == 0 && x.TestValueString == binary).ToList();

						Int32 activeVisits = 0;
						if (totalVisits.Count > 0)
						{
							activeVisits = totalVisits.Count - inActiveVisits.Count;
						}

						TestVariationGrouping grouping = new TestVariationGrouping(obj.TestSet, obj.ValueSum, obj.Count);
						grouping.TotalVisits = totalVisits.Count;
						grouping.ActiveVisits = activeVisits;
						grouping.InActiveVisits = inActiveVisits.Count;

						groupings.Add(grouping);
					}

					return groupings;
				}
			}
			catch (Exception e)
			{
				Logger.Error("Page Statistics Context - Could not retrieve the test variations");
				Logger.Error(e.Message);
				Logger.Error(e.InnerException);
			}

			return new List<TestVariationGrouping>();
		}

		public double GetAverageDailyVisits(Item item, bool ignoreCache)
		{
			Assert.ArgumentNotNull(item, "item");
			DateTime utcNow = DateTime.UtcNow;
			return GetAverageDailyVisits(item.ID, utcNow - pageDailyVisitsCalculationPeriod, utcNow);
		}

		public double GetAverageDailyVisits(ID itemId, DateTime startDate, DateTime endDate)
		{
			VisitStatistics statistics = DataAdapterManager.Testing.GetPageStatistics(itemId.ToGuid(), startDate, endDate);
			if (statistics.NumberOfVisits <= 0L)
			{
				return 0.0;
			}
			TimeSpan span = (statistics.DateOfLastVisit - statistics.DateOfFirstVisit);
			int days = span.Duration().Days;
			if (days == 0)
			{
				days = 1;
			}
			return ((statistics.NumberOfVisits) / ((double)days));
		}

		public TestSetStatistics GetTestStatistics(TestDefinitionItem testDefinitionItem)
		{
			return GetTestStatistics(testDefinitionItem, false, false);
		}

		public TestSetStatistics GetTestStatistics(TestDefinitionItem testDefinitionItem, bool ignoreSession, bool ignoreCache)
		{
			Assert.IsNotNull(testDefinitionItem, "testDefinitionItem");
			return TestManager.LoadTestStatistics(testDefinitionItem);
		}

		private static ILog _logger;
		private static ILog Logger
		{
			get
			{
				if (_logger == null)
				{
					_logger = LogManager.GetLogger(typeof(PageStatisticsContext));
				}
				return _logger;
			}
		}


		/// <summary>
		/// Returns Visit Record
		/// </summary>
		/// <param name="visitId"></param>
		/// <returns></returns>
		public Visit GetVisit_ById(string visitId)
		{
			ID id = null;
			if (string.IsNullOrEmpty(visitId) || !ID.TryParse(visitId, out id))
			{
				return null;
			}

			try
			{
				using (AnalyticsDataContext analyticsDataContext = new AnalyticsDataContext(ConfigurationManager.ConnectionStrings["analytics"].ConnectionString))
				{
					Visit visit = analyticsDataContext.Visits.Where(x => x.VisitId == id.Guid).FirstOrDefault();
					if (visit == null)
					{
						return null;
					}

					return visit;
				}
			}
			catch (Exception e)
			{
				Logger.Error("Page Statistics Context - Could not retrieve visit");
				Logger.Error(e.Message);
				Logger.Error(e.InnerException);
			}

			return null;
		}

		/// <summary>
		/// Returns a Visitor Record
		/// </summary>
		/// <param name="visitorId"></param>
		/// <returns></returns>
		public Visitor GetVisitor_ById(string visitorId)
		{
			ID id = null;
			if(string.IsNullOrEmpty(visitorId) || !ID.TryParse(visitorId, out id))
			{
				return null;
			}

			try
			{
				using (AnalyticsDataContext analyticsDataContext = new AnalyticsDataContext(ConfigurationManager.ConnectionStrings["analytics"].ConnectionString))
				{
					Visitor visitor = analyticsDataContext.Visitors.Where(x => x.VisitorId == id.Guid).FirstOrDefault();
					if(visitor == null)
					{
						return null;
					}

					return visitor;
				}
			}
			catch (Exception e)
			{
				Logger.Error("Page Statistics Context - Could not retrieve visitor");
				Logger.Error(e.Message);
				Logger.Error(e.InnerException);
			}

			return null;
		}

		/// <summary>
		/// Returns the Current Visitor Record
		/// </summary>
		/// <returns></returns>
		public Visitor GetCurrentVisitor()
		{
			if (Tracker.CurrentVisit == null || string.IsNullOrEmpty(Tracker.CurrentVisit.VisitorId.ToString()))
			{
				return null;
			}

			return GetVisitor_ById(Tracker.CurrentVisit.VisitorId.ToString());
		}

		public VisitorDataSet.VisitsRow GetCurrentVisit()
		{
			if (Tracker.CurrentVisit == null || string.IsNullOrEmpty(Tracker.CurrentVisit.VisitorId.ToString()))
			{
				return null;
			}

			return Tracker.CurrentVisit;
		}

		/// <summary>
		/// Returns the page view count for the item passed
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public Int32 GetPageViewCount(Item item)
		{
			try
			{
				using (AnalyticsDataContext analyticsDataContext = new AnalyticsDataContext(ConfigurationManager.ConnectionStrings["analytics"].ConnectionString))
				{
					List<Page> pages = analyticsDataContext.Pages.Where(x => x.ItemId == item.ID.Guid).ToList();
					return pages.Count;
				}
			}
			catch (Exception e)
			{
				Logger.Error("Page Statistics Context - Could not retrieve page view count");
				Logger.Error(e.Message);
				Logger.Error(e.InnerException);
			}

			return 0;
		}

		/// <summary>
		/// Retrieves engagement value for the passed visitor
		/// </summary>
		/// <param name="visitorId"></param>
		/// <returns></returns>
		public Int32 GetVisitorEngagementValue(Guid visitorId)
		{
			using (AnalyticsDataContext analyticsDataContext = new AnalyticsDataContext(ConfigurationManager.ConnectionStrings["analytics"].ConnectionString))
			{
				List<Guid> visitIds = analyticsDataContext.Visits.Where(x => x.VisitorId == visitorId).Select(x => x.VisitId).Distinct().ToList();
				if (visitIds.Count == 0)
				{
					return 0;
				}

				Int32 engagementValue = 0;
				foreach (Guid visitId in visitIds)
				{
					engagementValue += GetVisitEngagementValue(visitId);
				}

				return engagementValue;
			}
		}

		/// <summary>
		/// Retrieves engagement value for the passed visit
		/// </summary>
		/// <param name="visitId"></param>
		/// <returns></returns>
		public Int32 GetVisitEngagementValue(Guid visitId)
		{
			try
			{
				List<VisitGoal> goalsMet = GetGoalsMetDuringVisit(visitId);
				if(goalsMet.Count == 0)
				{
					return 0;
				}

				return goalsMet.Sum(x => x.EngagementValue);
			}
			catch (Exception e)
			{
				Logger.Error("Page Statistics Context - Could not retrieve engagement value for this visit");
				Logger.Error(e.Message);
				Logger.Error(e.InnerException);
			}

			return 0;
		}

		/// <summary>
		/// Retrieves goals met during visit
		/// </summary>
		/// <param name="visitId"></param>
		public List<VisitGoal> GetGoalsMetDuringVisit(Guid visitId)
		{
			try
			{
				using (AnalyticsDataContext AnalyticsDataContext = new AnalyticsDataContext(ConfigurationManager.ConnectionStrings["analytics"].ConnectionString))
				{
					string sqlCommandText = "SELECT " +
												"pe.pageEventDefinitionId as GoalId, " +
												"ped.Name as GoalName, " +
												"pe.DateTime as GoalMetDate, " +
												"value as EngagementValue " +
											"FROM pageEvents pe " +
												"inner join PageEventDefinitions ped on pe.pageEventDefinitionId = ped.pageEventDefinitionId " +
											"where " +
												"isgoal = 1 " +
												"and pe.visitId = {0}";

					//get page hits
					var visitResults = AnalyticsDataContext.ExecuteQuery<VisitGoal>(sqlCommandText, visitId).ToList();
					if (visitResults.Count == 0)
					{
						return new List<VisitGoal>();
					}

					return visitResults;
				}
			}
			catch (Exception e)
			{
				Logger.Error("Page Statistics Context - Could not retrieve goals met during visit");
				Logger.Error(e.Message);
				Logger.Error(e.InnerException);
			}

			return new List<VisitGoal>();
		}
	}
}