using System;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Links;
using Sitecore.Resources.Media;
using Sitecore.SharedSource.Analytics.Mappings;
using Sitecore.SharedSource.Analytics.Reference;
using Sitecore.SharedSource.Commons.Extensions;
using Sitecore.Shell.Controls.RichTextEditor.InsertLink;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;

namespace Sitecore.SharedSource.Analytics.CustomSitecore.Dialogs.RichTextEditor
{
	public class InsertMultiVariantLinkForm : InsertLinkForm
	{
		protected TreePicker rootItemTreePicker;
		protected DataContext rootItemDataContext;
		private Item _currentItem;

		/// <summary>
		/// Current Item in the Content Manager
		/// </summary>
		private Item CurrentItem
		{
			get
			{
				if(Sitecore.Context.Request == null || Sitecore.Context.Request.QueryString["fo"] == null)
				{
					return null;
				}


				string currentItemId = Sitecore.Context.Request.QueryString["fo"];
				if(string.IsNullOrEmpty(currentItemId))
				{
					return null;
				}

				Item currentItem = Sitecore.Context.ContentDatabase.GetItem(currentItemId);
				if(currentItem.IsNull())
				{
					return null;
				}

				_currentItem = currentItem;
				return currentItem;
			}
		}

		protected override void OnOK(object sender, EventArgs args)
		{
			string mediaUrl;
			string displayName;
			Assert.ArgumentNotNull(sender, "sender");
			Assert.ArgumentNotNull(args, "args");
			string linkedId = string.Empty;
			string goalId = string.Empty;

			//verify goal has been selected
			Item goalItem = rootItemDataContext.GetFolder();
			if (goalItem == null || goalItem.ID.ToString() == AnalyticsReference.System_MarketingCenter_Goals.Guid)
			{
				//reset to null to avoid setting the goal item to the goal folder
				goalItem = null;
			}

			if (this.Tabs.Active == 0)
			{
				Item selectionItem = this.InternalLinkTreeview.GetSelectionItem();
				if (selectionItem == null)
				{
					SheerResponse.Alert("Select an item.", new string[0]);
					return;
				}

				//use variant mapping functionality
				MultiVariantProcessor processor = new MultiVariantProcessor();
				if (processor.IsVariantTemplate(selectionItem.TemplateID.ToString()))
				{
					SheerResponse.Alert("Select an non variant item.", new string[0]);
					return;
				}

				displayName = selectionItem.DisplayName;
				if (selectionItem.Paths.IsMediaItem)
				{
					mediaUrl = GetMediaUrl(selectionItem);
				}
				else
				{
					if (!selectionItem.Paths.IsContentItem)
					{
						SheerResponse.Alert("Select either a content item or a media item.", new string[0]);
						return;
					}
					LinkUrlOptions options = new LinkUrlOptions();
					mediaUrl = LinkManager.GetDynamicUrl(selectionItem, options);
					linkedId = selectionItem.ID.ToString();
				}
			}
			else
			{
				MediaItem item2 = this.MediaTreeview.GetSelectionItem();
				if (item2 == null)
				{
					SheerResponse.Alert("Select a media item.", new string[0]);
					return;
				}

				//use variant mapping functionality
				MultiVariantProcessor processor = new MultiVariantProcessor();
				if (processor.IsVariantTemplate(item2.InnerItem.TemplateID.ToString()))
				{
					SheerResponse.Alert("Select an non variant item.", new string[0]);
					return;
				}

				displayName = item2.DisplayName;
				mediaUrl = GetMediaUrl((Item)item2);
				linkedId = item2.ID.ToString();
			}
			if (this.Mode == "webedit")
			{
				SheerResponse.SetDialogValue(StringUtil.EscapeJavascriptString(mediaUrl));
				base.OnOK(sender, args);
			}
			else
			{
				string url = StringUtil.EscapeJavascriptString(mediaUrl);

				if (goalItem.IsNotNull())
				{
					goalId = goalItem.ID.ToString();
				}

				//put querystring on for only content items, media items will be picked up within a javascript event.
				if (this.Tabs.Active == 0)
				{
					//set default parameter key
					string parameterKey = "sc_trk";

					//check configuration file for parameter key
					if (!string.IsNullOrEmpty(Sitecore.Configuration.Settings.GetSetting("Analytics.EventQueryStringKey")))
					{
						parameterKey = Sitecore.Configuration.Settings.GetSetting("Analytics.EventQueryStringKey");
					}

					//if there is a goal, assign a querystring parameter
					//no check for existing parameters, this is because it will come across as link.aspx?_id=3D98F330EBA94EAA97C63F0D0FE1D5D8?sc_trk=SomeGoal
					//and when the field renderer processes this it will convert it as /research?sc_trk=SomeGoal
					string goalName = string.Empty;
					if (goalItem.IsNotNull() && !string.IsNullOrEmpty(goalItem.Name))
					{
						goalName = string.Format("?{1}={0}", goalItem.Name, parameterKey);
					}

					url = StringUtil.EscapeJavascriptString(string.Format("{0}{1}", mediaUrl, goalName));
				}

				SheerResponse.Eval("scClose(" + url + "," + StringUtil.EscapeJavascriptString(displayName) + ",'" + linkedId + "','" + goalId + "')");
			}
		}

		/// <summary>
		/// Load root item treepicker
		/// </summary>
		private void LoadRootItem()
		{
			Item goalsItem = Sitecore .Context.ContentDatabase.GetItem(AnalyticsReference.System_MarketingCenter_Goals.Guid);
			if (goalsItem == null || Sitecore.Context.ClientPage.IsPostBack)
			{
				return;
			}
			
			rootItemDataContext.Root = goalsItem.Paths.FullPath;
			rootItemDataContext.Refresh();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			LoadRootItem();
		}

		private static string GetMediaUrl(Item item)
		{
			Assert.ArgumentNotNull(item, "item");
			return MediaManager.GetMediaUrl(item, MediaUrlOptions.GetShellOptions());
		}
	}
}
