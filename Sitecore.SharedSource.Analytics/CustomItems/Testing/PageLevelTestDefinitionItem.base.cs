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
public partial class PageLevelTestDefinitionItem : CustomItem
{

public static readonly string TemplateId = "{E3D0E5DE-E552-4E88-B64E-81FA6B652122}";

#region Inherited Base Templates

private readonly TestDefinitionItem _TestDefinitionItem;
public TestDefinitionItem TestDefinition { get { return _TestDefinitionItem; } }

#endregion

#region Boilerplate CustomItem Code

public PageLevelTestDefinitionItem(Item innerItem) : base(innerItem)
{
	_TestDefinitionItem = new TestDefinitionItem(innerItem);

}

public static implicit operator PageLevelTestDefinitionItem(Item innerItem)
{
	return innerItem != null ? new PageLevelTestDefinitionItem(innerItem) : null;
}

public static implicit operator Item(PageLevelTestDefinitionItem customItem)
{
	return customItem != null ? customItem.InnerItem : null;
}

#endregion //Boilerplate CustomItem Code


#region Field Instance Methods


#endregion //Field Instance Methods
}
}