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
public partial class PageLevelTestValueItem : CustomItem
{

public static readonly string TemplateId = "{1BD410DD-2C0D-41DE-BD13-B9FC19976767}";

#region Inherited Base Templates

private readonly TestValueItem _TestValueItem;
public TestValueItem TestValue { get { return _TestValueItem; } }

#endregion

#region Boilerplate CustomItem Code

public PageLevelTestValueItem(Item innerItem) : base(innerItem)
{
	_TestValueItem = new TestValueItem(innerItem);

}

public static implicit operator PageLevelTestValueItem(Item innerItem)
{
	return innerItem != null ? new PageLevelTestValueItem(innerItem) : null;
}

public static implicit operator Item(PageLevelTestValueItem customItem)
{
	return customItem != null ? customItem.InnerItem : null;
}

#endregion //Boilerplate CustomItem Code


#region Field Instance Methods


//Could not find Field Type for Datasource


#endregion //Field Instance Methods
}
}