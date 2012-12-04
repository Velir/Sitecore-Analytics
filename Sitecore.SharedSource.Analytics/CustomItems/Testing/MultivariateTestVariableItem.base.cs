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
public partial class MultivariateTestVariableItem : CustomItem
{

public static readonly string TemplateId = "{72296CFA-6354-47AD-901A-17F1651E9505}";

#region Inherited Base Templates

private readonly TestVariableItem _TestVariableItem;
public TestVariableItem TestVariable { get { return _TestVariableItem; } }

#endregion

#region Boilerplate CustomItem Code

public MultivariateTestVariableItem(Item innerItem) : base(innerItem)
{
	_TestVariableItem = new TestVariableItem(innerItem);

}

public static implicit operator MultivariateTestVariableItem(Item innerItem)
{
	return innerItem != null ? new MultivariateTestVariableItem(innerItem) : null;
}

public static implicit operator Item(MultivariateTestVariableItem customItem)
{
	return customItem != null ? customItem.InnerItem : null;
}

#endregion //Boilerplate CustomItem Code


#region Field Instance Methods


#endregion //Field Instance Methods
}
}