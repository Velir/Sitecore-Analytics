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
public partial class MultivariateTestDefinitionItem : CustomItem
{

public static readonly string TemplateId = "{AFF050BA-A88C-4DB7-8ED6-9CB2B7CA2688}";

#region Inherited Base Templates

private readonly TestDefinitionItem _TestDefinitionItem;
public TestDefinitionItem TestDefinition { get { return _TestDefinitionItem; } }

#endregion

#region Boilerplate CustomItem Code

public MultivariateTestDefinitionItem(Item innerItem) : base(innerItem)
{
	_TestDefinitionItem = new TestDefinitionItem(innerItem);

}

public static implicit operator MultivariateTestDefinitionItem(Item innerItem)
{
	return innerItem != null ? new MultivariateTestDefinitionItem(innerItem) : null;
}

public static implicit operator Item(MultivariateTestDefinitionItem customItem)
{
	return customItem != null ? customItem.InnerItem : null;
}

#endregion //Boilerplate CustomItem Code


#region Field Instance Methods


//Could not find Field Type for Item


#endregion //Field Instance Methods
}
}