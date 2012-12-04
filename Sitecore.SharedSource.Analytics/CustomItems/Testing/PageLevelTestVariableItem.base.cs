using System;
using Sitecore.Data.Items;
using System.Collections.Generic;
using Sitecore.Data.Fields;
using Sitecore.Web.UI.WebControls;
using CustomItemGenerator.Fields.LinkTypes;
using CustomItemGenerator.Fields.ListTypes;
using CustomItemGenerator.Fields.SimpleTypes;
using Sitecore.SharedSource.Analytics.CustomItems.Testing;

namespace Sitecore.SharedSource.Analytics.CustomItems.Testing
{
public partial class PageLevelTestVariableItem : CustomItem
{

public static readonly string TemplateId = "{C1732433-D8BA-4EFC-902C-C9A6CE876471}";

#region Inherited Base Templates

private readonly TestVariableItem _TestVariableItem;
public TestVariableItem TestVariable { get { return _TestVariableItem; } }

#endregion

#region Boilerplate CustomItem Code

public PageLevelTestVariableItem(Item innerItem) : base(innerItem)
{
	_TestVariableItem = new TestVariableItem(innerItem);

}

public static implicit operator PageLevelTestVariableItem(Item innerItem)
{
	return innerItem != null ? new PageLevelTestVariableItem(innerItem) : null;
}

public static implicit operator Item(PageLevelTestVariableItem customItem)
{
	return customItem != null ? customItem.InnerItem : null;
}

#endregion //Boilerplate CustomItem Code


#region Field Instance Methods


#endregion //Field Instance Methods
}
}