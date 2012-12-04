using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Sitecore.Analytics.Data.Items;
using Sitecore.Data.Items;
using Sitecore.SharedSource.Analytics.Context.Model;
using Sitecore.SharedSource.Analytics.Controls;
using Sitecore.SharedSource.Commons.Extensions;
using Sitecore.SharedSource.Commons.Utilities;
using Sitecore.Workflows;
using GoalItem = Sitecore.SharedSource.Analytics.CustomItems.GoalItem;
using MultivariateTestDefinitionItem = Sitecore.SharedSource.Analytics.CustomItems.Testing.MultivariateTestDefinitionItem;
using MultivariateTestValueItem = Sitecore.SharedSource.Analytics.CustomItems.Testing.MultivariateTestValueItem;
using MultivariateTestVariableItem = Sitecore.SharedSource.Analytics.CustomItems.Testing.MultivariateTestVariableItem;

namespace Sitecore.SharedSource.Analytics.Controls.Web
{
	public partial class MultivariantReport : AnalyticsControl
	{
		private DateTime? _endDate;
		private DateTime? _startDate;
		private Item _testComponentItem;
		private Item _testDefinitionItem;

		/// <summary>
		/// overall test definition
		/// </summary>
		protected Item TestDefinitionItem
		{
			get
			{
				if(_testDefinitionItem.IsNotNull())
				{
					return _testDefinitionItem;
				}

				if (ddlTest.SelectedIndex == 0 || string.IsNullOrEmpty(ddlTest.SelectedValue))
				{
					return null;
				}

				_testDefinitionItem = Database.GetItem(ddlTest.SelectedValue);
				if (_testDefinitionItem.IsNull())
				{
					return null;
				}

				_testDefinitionItem = TestComponentItem.Parent;
				return _testDefinitionItem;
			}
		}

		/// <summary>
		/// Selected test item from the dropdown
		/// </summary>
		protected Item TestComponentItem
		{
			get
			{
				if(_testComponentItem.IsNotNull())
				{
					return _testComponentItem;
				}

				List<Item> items = SitecoreItemFinder.GetSubItemsOfTemplate(TestDefinitionItem, MultivariateTestVariableItem.TemplateId, true);
				if(items.Count == 0)
				{
					return null;
				}

				_testComponentItem = items[0];
				if (_testComponentItem.IsNull())
				{
					return null;
				}

				return _testComponentItem;
			}
		}

		/// <summary>
		/// Selected goal item from the dropdown
		/// </summary>
		protected Item GoalItem
		{
			get
			{
				if (ddlGoal.SelectedIndex == 0 || string.IsNullOrEmpty(ddlGoal.SelectedValue))
				{
					return null;
				}

				Item goalItem = Database.GetItem(ddlGoal.SelectedValue);
				if (goalItem.IsNull())
				{
					return null;
				}

				return goalItem;
			}
		}

		/// <summary>
		/// Finish Date
		/// </summary>
		protected DateTime? FinishDate
		{
			get
			{
				if (TestDefinitionItem.IsNull())
				{
					return null;
				}

				if (_endDate != null)
				{
					return _endDate;
				}

				IWorkflow workflow = TestDefinitionItem.State.GetWorkflow();
				if (workflow == null)
				{
					return null;
				}

				WorkflowEvent event2 = workflow.GetHistory(TestDefinitionItem).LastOrDefault<WorkflowEvent>(w => w.NewState == Sitecore.Analytics.Data.Items.TestDefinitionItem.StateIDs.Completed);
				if (event2 == null)
				{
					return null;
				}

				_endDate = event2.Date;
				return event2.Date;
			}
		}

		/// <summary>
		/// Start Date
		/// </summary>
		protected DateTime? TestStartDate
		{
			get
			{
				if (TestDefinitionItem.IsNull())
				{
					return null;
				}

				if (_startDate != null)
				{
					return _startDate;
				}

				IWorkflow workflow = TestDefinitionItem.State.GetWorkflow();
				if (workflow == null)
				{
					return null;
				}

				WorkflowEvent event2 = workflow.GetHistory(TestDefinitionItem).LastOrDefault<WorkflowEvent>(w => w.NewState == Sitecore.Analytics.Data.Items.TestDefinitionItem.StateIDs.Deployed);
				if (event2 == null)
				{
					return null;
				}

				_startDate = event2.Date;
				return event2.Date;
			}
		}

		/// <summary>
		/// Sum of Visit Counts of Variations
		/// </summary>
		protected Int32 TotalVisitCount { get; set; }

		/// <summary>
		/// Sum of Active Visit Counts of Variations where engagement value was accumulated
		/// </summary>
		protected Int32 TotalAchievedGoalCount { get; set; }

		protected void Page_Load(object sender, EventArgs e)
		{
			if (IsPostBack)
			{
				return;
			}

			DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
			DateTime endDate = startDate.AddDays(1).AddMilliseconds(-1);
			beginDatePicker.SelectedDate = startDate;
			endDatePicker.SelectedDate = endDate;
			beginDatePicker.Enabled = false;
			endDatePicker.Enabled = false;

			LoadTestDropdown();
		}

		/// <summary>
		/// Change event for the Test dropdown
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void ddlTest_SelectedIndexChanged(object sender, EventArgs e)
		{
			//check report parameters
			plcErrorPanel.Visible = false;
			List<string> errorList = IsFormValid(true);
			if (errorList.Count > 0)
			{
				rptErrors.DataSource = errorList;
				rptErrors.DataBind();
				plcErrorPanel.Visible = true;
				plcReport.Visible = false;
				return;
			}

			//load goal dropdown
			LoadGoalDropdown();

			//set start date to when the test began
			if (TestStartDate.HasValue && TestStartDate != DateTime.MaxValue && TestStartDate != DateTime.MinValue)
			{
				beginDatePicker.Calendar.RangeMinDate = TestStartDate.Value;
				beginDatePicker.SelectedDate = TestStartDate.Value;
			}

			//set end date to the end of of the test
			if (FinishDate.HasValue && FinishDate != DateTime.MaxValue && FinishDate != DateTime.MinValue)
			{
				endDatePicker.Calendar.RangeMaxDate = FinishDate.Value;
				endDatePicker.SelectedDate = FinishDate.Value;
			}

			beginDatePicker.Enabled = true;
			endDatePicker.Enabled = true;
			ddlGoal.Enabled = true;
			btnRun.Enabled = true;
			btnClear.Enabled = true;
		}

		/// <summary>
		/// Reset form
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void btnClear_Click(object sender, EventArgs e)
		{
			Response.Redirect(Request.RawUrl);
		}

		/// <summary>
		/// Click Event for the Run button
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void btnRun_Click(object sender, EventArgs e)
		{
			RunReport();
		}

		/// <summary>
		/// Run Report
		/// </summary>
		private void RunReport()
		{
			//check report parameters
			plcErrorPanel.Visible = false;
			List<string> errorList = IsFormValid();
			if (errorList.Count > 0)
			{
				ShowErrors(errorList);
				return;
			}

			//load achieved goals data
			LoadAchievedGoals();

			//set information
			SetTestInformation();
		}

		/// <summary>
		/// Displays the Achieved Goals datatable
		/// </summary>
		private void LoadAchievedGoals()
		{
			if (TestComponentItem.IsNull() || TestDefinitionItem.IsNull() || GoalItem.IsNull())
			{
				return;
			}

			DateTime startTime = DateTime.MinValue;
			if (beginDatePicker.SelectedDate.HasValue)
			{
				startTime = beginDatePicker.SelectedDate.Value;
			}

			DateTime endTime = DateTime.MaxValue;
			if (endDatePicker.SelectedDate.HasValue)
			{
				endTime = endDatePicker.SelectedDate.Value;
			}

			List<VisitVariation> achievedGoals = PageStatistics.GetAchievedGoals(GoalItem, TestDefinitionItem, startTime,
			                                                                                endTime);
			if (achievedGoals == null || achievedGoals.Count == 0)
			{
				ShowErrors(new List<string> {"No data available for the selected parameters"});
				return;
			}

			litAchievedGoal.Text = TestComponentItem.Name;
			rptAchievedGoals.DataSource = achievedGoals;
			rptAchievedGoals.DataBind();
			plcAchievedGoalData.Visible = true;
			plcReport.Visible = true;

			//set totals which are summed up during the databinding process
			litTotalVisitCount.Text = TotalVisitCount.ToString("N0");
			litTotalAchievedCount.Text = TotalAchievedGoalCount.ToString("N0");
		}

		/// <summary>
		/// Logic to show the error panel
		/// </summary>
		/// <param name="errorList"></param>
		private void ShowErrors(List<string> errorList)
		{
			rptErrors.DataSource = errorList;
			rptErrors.DataBind();
			plcErrorPanel.Visible = true;
			plcReport.Visible = false;
		}

		protected void rptAchievedGoals_OnItemDataBound(object sender, RepeaterItemEventArgs e)
		{
			if (e.Item == null || e.Item.DataItem == null)
			{
				return;
			}

			VisitVariation visitVariation = (VisitVariation)e.Item.DataItem;
			Literal litVariationName = (Literal)e.Item.FindControl("litVariationName");
			Literal litVisitCount = (Literal)e.Item.FindControl("litVisitCount");
			Literal litActiveVisitCount = (Literal)e.Item.FindControl("litActiveVisitCount");
			Literal litEffectiveness = (Literal)e.Item.FindControl("litEffectiveness");

			TotalVisitCount += visitVariation.VisitCount;
			TotalAchievedGoalCount += visitVariation.AchievedGoalCount;

			litEffectiveness.Text = "N/A";
			if (visitVariation.VisitCount > 0)
			{
				double effectiveness = ((double) (visitVariation.AchievedGoalCount)/((double) visitVariation.VisitCount));
				litEffectiveness.Text = effectiveness.ToString("P1");
			}

			litVariationName.Text = GetVariationName(visitVariation.TestValues);
			litVisitCount.Text = visitVariation.VisitCount.ToString("N0");
			litActiveVisitCount.Text = visitVariation.AchievedGoalCount.ToString("N0");
		}

		protected void rptErrors_OnItemDataBound(object sender, RepeaterItemEventArgs e)
		{
			if (e.Item == null || e.Item.DataItem == null)
			{
				return;
			}

			string error = (string) e.Item.DataItem;
			Literal litError = (Literal)e.Item.FindControl("litError");

			litError.Text = error;
		}

		/// <summary>
		/// Method to load the test dropdown
		/// </summary>
		private void LoadTestDropdown()
		{
			ddlTest.Items.Clear();

			ddlTest.Items.Add(new ListItem(string.Empty));
			foreach (MultivariateTestDefinitionItem item in Tests)
			{
				if (item.IsNull())
				{
					continue;
				}

				ddlTest.Items.Add(new ListItem(item.TestDefinition.Name.Text, item.ID.ToString()));
			}
		}

		/// <summary>
		/// Loads the Goal Dropdown
		/// </summary>
		private void LoadGoalDropdown()
		{
			ddlGoal.Items.Clear();

			ddlGoal.Items.Add(new ListItem(string.Empty));
			foreach (GoalItem item in Goals)
			{
				if (item.IsNull())
				{
					continue;
				}

				ddlGoal.Items.Add(new ListItem(item.PageEvent.Name.Text, item.ID.ToString()));
			}
		}

		/// <summary>
		/// Returns a list of errors
		/// </summary>
		/// <returns></returns>
		private List<string> IsFormValid()
		{
			return IsFormValid(false);
		}

		/// <summary>
		/// Returns a list of errors
		/// </summary>
		/// <returns></returns>
		private List<string> IsFormValid(bool phase1Only)
		{
			List<string> errorList = new List<string>();

			//test dropdown
			if (string.IsNullOrEmpty(ddlTest.SelectedValue))
			{
				errorList.Add("Select a test");
			}

			//test item
			if (!string.IsNullOrEmpty(ddlTest.SelectedValue) && (TestComponentItem.IsNull() || TestDefinitionItem.IsNull()))
			{
				errorList.Add("The test item is null");
			}

			if (phase1Only)
			{
				return errorList;
			}

			//test dropdown
			if (string.IsNullOrEmpty(ddlGoal.SelectedValue))
			{
				errorList.Add("Select a goal");
			}

			//test item
			if (!string.IsNullOrEmpty(ddlGoal.SelectedValue) && Database.GetItem(ddlGoal.SelectedValue).IsNull())
			{
				errorList.Add("The goal item is null");
			}

			//start date
			if (beginDatePicker.SelectedDate == null || beginDatePicker.SelectedDate == DateTime.MinValue)
			{
				errorList.Add("Select a start date");
			}

			//enddate
			if (endDatePicker.SelectedDate == null || endDatePicker.SelectedDate == DateTime.MinValue)
			{
				errorList.Add("Select an end date");
			}

			if (beginDatePicker.SelectedDate != null
			    && endDatePicker.SelectedDate != null
			    && beginDatePicker.SelectedDate > endDatePicker.SelectedDate)
			{
				errorList.Add("Select a start date that is before the end date");
			}

			return errorList;
		}

		/// <summary>
		/// Takes a variation code from the database and returns the proper variation item name from sitecore
		/// </summary>
		/// <param name="code"></param>
		/// <returns></returns>
		private string GetVariationName(string code)
		{
			if (string.IsNullOrEmpty(code))
			{
				return "Default";
			}

			if (TestComponentItem.IsNull())
			{
				return string.Empty;
			}

			List<Item> variationItems = SitecoreItemFinder.GetSubItemsOfTemplate(TestComponentItem, Sitecore.SharedSource.Analytics.CustomItems.Testing.MultivariateTestValueItem.TemplateId, true);
			if (variationItems == null || variationItems.Count == 0)
			{
				return string.Empty;
			}

			code = code.Substring(3, 1);

			int codeValue = 0;
			int.TryParse(code, out codeValue);
			if (variationItems[codeValue] == null)
			{
				return string.Empty;
			}

			Sitecore.SharedSource.Analytics.CustomItems.Testing.MultivariateTestValueItem variationItem = variationItems[codeValue];
			return variationItem.TestValue.Name.Text;
		}

		/// <summary>
		/// Fills out the report information section of the report
		/// </summary>
		private void SetTestInformation()
		{
			if(TestDefinitionItem.IsNull())
			{
				return;
			}

			TestDefinitionItem testDefinition = new TestDefinitionItem(TestDefinitionItem);
			IWorkflow workflow = TestDefinitionItem.State.GetWorkflow();
			DateTime testStartDate = DateTime.MinValue;
			DateTime testFinishDate = DateTime.MinValue;
			if (workflow != null)
			{
				WorkflowEvent testStartEvent = workflow.GetHistory(TestDefinitionItem).LastOrDefault<WorkflowEvent>(w => w.NewState == Sitecore.Analytics.Data.Items.TestDefinitionItem.StateIDs.Deployed);
				if (testStartEvent != null)
				{
					testStartDate = testStartEvent.Date;
				}

				WorkflowEvent testFinishEvent = workflow.GetHistory(TestDefinitionItem).LastOrDefault<WorkflowEvent>(w => w.NewState == Sitecore.Analytics.Data.Items.TestDefinitionItem.StateIDs.Completed);
				if (testFinishEvent != null)
				{
					testFinishDate = testFinishEvent.Date;
				}
			}

			//start date
			litTestStarted.Text = "Test has not started";
			if (testStartDate != DateTime.MinValue && testStartDate != DateTime.MaxValue)
			{
				litTestStarted.Text = testStartDate.ToString("MMMM dd, yyyy");
			}

			//end date
			litTestEnded.Text = "Test has not completed";
			if (testFinishDate != DateTime.MinValue && testFinishDate != DateTime.MaxValue)
			{
				litTestEnded.Text = testFinishDate.ToString("MMMM dd, yyyy");
			}

			if (testDefinition.IsRunning)
			{
				litProgress.Text = "Running";
			}
			else if (testDefinition.IsFinished)
			{
				litProgress.Text = "Finished";
			}
			else if (testDefinition.IsDraft)
			{
				litProgress.Text = "Draft";
			}
			else if (testDefinition.IsDeployed)
			{
				litProgress.Text = "Deployed";
			}
			else
			{
				litProgress.Text = "N/A";
			}
		}
	}
}