using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using Sitecore.SharedSource.Analytics.Context.PageStatistics;
using Sitecore.SharedSource.Analytics.CustomItems.Testing;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.SharedSource.Analytics.CustomItems;
using Sitecore.SharedSource.Analytics.Reference;
using Sitecore.SharedSource.Commons.Abstractions.Items;
using Sitecore.SharedSource.Commons.Extensions;
using Sitecore.SharedSource.Commons.Utilities;
using Sitecore.Web.UI.WebControls;

namespace Sitecore.SharedSource.Analytics.Controls
{
	public class AnalyticsControl : UserControl
	{
		private Database _database;
		private List<MultivariateTestDefinitionItem> _testItems;
		private List<GoalItem> _goalItems;
		private PageStatisticsContext _pageStatisticsContext;

		//Custom Event Handler
		public event EventHandler DebugInfoEvent;

		protected override void OnInit(EventArgs e)
		{
			DebugInfoEvent += new EventHandler(AnalyticsControl_DebugInfoEvent);

			//set database
			if (Sitecore.Context.ContentDatabase != null)
			{
				_database = Sitecore.Context.ContentDatabase;
			}
			else
			{
				_database = Sitecore.Context.Database;
			}

			//set context
			_pageStatisticsContext = new PageStatisticsContext();
		}

		#region Custom Events

		protected virtual void AnalyticsControl_DebugInfoEvent(object sender, EventArgs e)
		{

		}

		/// <summary>
		/// When called the calling UserControl is added to the list
		/// </summary>
		public void RaiseDebugInfoEvent_AddControl()
		{
			DebugInfoEvents.Add(this);
		}

		public void RaiseDebugInfoEvent()
		{
			foreach (AnalyticsControl control in DebugInfoEvents)
			{
				if (control.DebugInfoEvent != null)
					control.DebugInfoEvent(control, new EventArgs());
			}
		}

		private List<AnalyticsControl> DebugInfoEvents
		{
			get
			{
				if (HttpContext.Current.Items["DebugInfoEvents"] == null)
				{
					HttpContext.Current.Items["DebugInfoEvents"] = new List<AnalyticsControl>();
				}
				return (List<AnalyticsControl>)HttpContext.Current.Items["DebugInfoEvents"];
			}
			set
			{
				HttpContext.Current.Items["DebugInfoEvents"] = value;
			}
		}

		#endregion

		private Item _dataSource = null;
		public Item DataSource
		{
			get
			{
				if (_dataSource == null)
				{
					if (Parent is Sublayout)
					{
						_dataSource = Sitecore.Context.Database.GetItem(((Sublayout)Parent).DataSource);
					}

					if (_dataSource == null)
					{
						_dataSource = Sitecore.Context.Item;
					}
				}

				return _dataSource;
			}
		}

		/// <summary>
		/// Checks to see if DMS Debugging is enabled
		/// </summary>
		public bool IsDmsDebuggingEnabled
		{
			get
			{
				//get setting
				if(string.IsNullOrEmpty(Sitecore.Configuration.Settings.GetSetting("Analytics.Debugging.Enabled")))
				{
					//if there is no settings key, check the querystring
					return (Request.QueryString["dms"] != null);
				}

				bool isDmsDebuggingEnabled = false;
				if(!bool.TryParse(Sitecore.Configuration.Settings.GetSetting("Analytics.Debugging.Enabled"), out isDmsDebuggingEnabled))
				{
					//if there is no settings key, check the querystring
					return (Request.QueryString["dms"] != null);
				}

				//last attempt, if it is not enabled by the settings key, do we have an override in the querystring
				if(!isDmsDebuggingEnabled)
				{
					//if there is no settings key, check the querystring
					return (Request.QueryString["dms"] != null);
				}

				return isDmsDebuggingEnabled;
			}
		}

		/// <summary>
		/// Checks to see if DMS Flush button is enabled
		/// </summary>
		public bool IsDmsFlushButtonEnabled
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

		/// <summary>
		/// Checks to see if DMS New Visit button is enabled
		/// </summary>
		public bool IsDmsNewVisitButtonEnabled
		{
			get
			{
				//verify setting
				if (string.IsNullOrEmpty(Sitecore.Configuration.Settings.GetSetting("Analytics.Debugging.NewVisitEnabled")))
				{
					return false;
				}

				bool enabled = false;
				if (!bool.TryParse(Sitecore.Configuration.Settings.GetSetting("Analytics.Debugging.NewVisitEnabled"), out enabled))
				{
					return false;
				}

				return enabled;
			}
		}

		/// <summary>
		/// This could either be the content database or the context database
		/// </summary>
		protected Database Database
		{
			get { return _database; }
			set { _database = value; }
		}

		/// <summary>
		/// Context for DMS related statistical calculations
		/// </summary>
		protected PageStatisticsContext PageStatistics
		{
			get { return _pageStatisticsContext; }
		}

		/// <summary>
		/// Tests within Sitecore
		/// </summary>
		protected List<MultivariateTestDefinitionItem> Tests
		{
			get
			{
				if (_testItems != null)
				{
					return _testItems;
				}

				Item testLabItem = Database.GetItem(AnalyticsReference.System_MarketingCenter_TestLab.Guid);
				if(testLabItem.IsNull())
				{
					return new List<MultivariateTestDefinitionItem>();
				}

				List<Item> items = SitecoreItemFinder.GetSubItemsOfTemplate(testLabItem, MultivariateTestDefinitionItem.TemplateId, true);
				if (items == null || items.Count == 0)
				{
					return new List<MultivariateTestDefinitionItem>();
				}

				_testItems = new List<MultivariateTestDefinitionItem>();
				foreach (Item item in items)
				{
					_testItems.Add(item);
				}

				return _testItems;
			}
		}

		/// <summary>
		/// Goals within Sitecore
		/// </summary>
		protected List<GoalItem> Goals
		{
			get
			{
				if (_goalItems != null)
				{
					return _goalItems;
				}

				Item goalItem = Database.GetItem(AnalyticsReference.System_MarketingCenter_Goals.Guid);
				if (goalItem.IsNull())
				{
					return new List<GoalItem>();
				}

				List<Item> items = SitecoreItemFinder.GetSubItemsOfTemplate(goalItem, GoalItem.TemplateId, true);
				if (items == null || items.Count == 0)
				{
					return new List<GoalItem>();
				}

				_goalItems = new List<GoalItem>();
				foreach(Item item in items)
				{
					_goalItems.Add(item);
				}

				return _goalItems;
			}
		}
	}
}
