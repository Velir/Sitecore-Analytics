using CustomItemGenerator.Fields.SimpleTypes;
using Sitecore.Data.Items;

namespace Sitecore.SharedSource.Analytics.CustomItems
{
	public class PageEventItem : CustomItem
	{
		public static readonly string TemplateId = "{059CFBDF-49FC-4F14-A4E5-B63E1E1AFB1E}";

		#region Boilerplate CustomItem Code

		public PageEventItem(Item innerItem)
			: base(innerItem)
		{
		}

		public static implicit operator PageEventItem(Item innerItem)
		{
			return innerItem != null ? new PageEventItem(innerItem) : null;
		}

		public static implicit operator Item(PageEventItem customItem)
		{
			return customItem != null ? customItem.InnerItem : null;
		}

		#endregion //Boilerplate CustomItem Code

		#region Field Instance Methods

		public CustomTextField Name
		{
			get { return new CustomTextField(InnerItem, InnerItem.Fields["Name"]); }
		}

		public CustomTextField Points
		{
			get { return new CustomTextField(InnerItem, InnerItem.Fields["Points"]); }
		}

		public CustomTextField Category
		{
			get { return new CustomTextField(InnerItem, InnerItem.Fields["Category"]); }
		}

		public CustomTextField Description
		{
			get { return new CustomTextField(InnerItem, InnerItem.Fields["Description"]); }
		}

		public CustomCheckboxField IsAuthorFeedback
		{
			get { return new CustomCheckboxField(InnerItem, InnerItem.Fields["IsAuthorFeedback"]); }
		}

		public CustomCheckboxField IsFailure
		{
			get { return new CustomCheckboxField(InnerItem, InnerItem.Fields["IsFailure"]); }
		}

		public CustomCheckboxField IsGoal
		{
			get { return new CustomCheckboxField(InnerItem, InnerItem.Fields["IsGoal"]); }
		}

		public CustomCheckboxField IsSystem
		{
			get { return new CustomCheckboxField(InnerItem, InnerItem.Fields["IsSystem"]); }
		}

		#endregion //Field Instance Methods
	}
}