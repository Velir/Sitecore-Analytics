using Sitecore.Data.Items;

namespace Sitecore.SharedSource.Analytics.CustomItems
{
public partial class GoalItem : CustomItem
{

	public static readonly string TemplateId = "{475E9026-333F-432D-A4DC-52E03B75CB6B}";

#region Inherited Base Templates

private readonly PageEventItem _PageEvent;
public PageEventItem PageEvent { get { return _PageEvent; } }

#endregion

#region Boilerplate CustomItem Code

public GoalItem(Item innerItem)
	: base(innerItem)
{
	_PageEvent = new PageEventItem(innerItem);

}

public static implicit operator GoalItem(Item innerItem)
{
	return innerItem != null ? new GoalItem(innerItem) : null;
}

public static implicit operator Item(GoalItem customItem)
{
	return customItem != null ? customItem.InnerItem : null;
}

#endregion //Boilerplate CustomItem Code


#region Field Instance Methods


//Could not find Field Type for Item


#endregion //Field Instance Methods
}
}