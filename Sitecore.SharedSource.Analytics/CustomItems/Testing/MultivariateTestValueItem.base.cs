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
public partial class MultivariateTestValueItem : CustomItem
{

public static readonly string TemplateId = "{6D00D695-0CAB-47D7-8929-A03EA002EB19}";

#region Inherited Base Templates

private readonly TestValueItem _TestValueItem;
public TestValueItem TestValue { get { return _TestValueItem; } }

#endregion

#region Boilerplate CustomItem Code

public MultivariateTestValueItem(Item innerItem) : base(innerItem)
{
	_TestValueItem = new TestValueItem(innerItem);

}

public static implicit operator MultivariateTestValueItem(Item innerItem)
{
	return innerItem != null ? new MultivariateTestValueItem(innerItem) : null;
}

public static implicit operator Item(MultivariateTestValueItem customItem)
{
	return customItem != null ? customItem.InnerItem : null;
}

#endregion //Boilerplate CustomItem Code


#region Field Instance Methods


//Could not find Field Type for Datasource


public CustomCheckboxField HideComponent
{
	get
	{
		return new CustomCheckboxField(InnerItem, InnerItem.Fields["Hide Component"]);
	}
}


//Could not find Field Type for Replacement Component


#endregion //Field Instance Methods
}
}