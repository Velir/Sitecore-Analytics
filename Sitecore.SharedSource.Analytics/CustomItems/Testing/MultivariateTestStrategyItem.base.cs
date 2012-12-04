using System;
using Sitecore.Data.Items;
using System.Collections.Generic;
using Sitecore.Data.Fields;
using Sitecore.Web.UI.WebControls;
using CustomItemGenerator.Fields.LinkTypes;
using CustomItemGenerator.Fields.ListTypes;
using CustomItemGenerator.Fields.SimpleTypes;

namespace Sitecore.SharedSource.Analytics.CustomItems.Testing
{
public partial class MultivariateTestStrategyItem : CustomItem
{

public static readonly string TemplateId = "{314BE5B0-8536-45CF-900D-A84B2B335210}";


#region Boilerplate CustomItem Code

public MultivariateTestStrategyItem(Item innerItem) : base(innerItem)
{

}

public static implicit operator MultivariateTestStrategyItem(Item innerItem)
{
	return innerItem != null ? new MultivariateTestStrategyItem(innerItem) : null;
}

public static implicit operator Item(MultivariateTestStrategyItem customItem)
{
	return customItem != null ? customItem.InnerItem : null;
}

#endregion //Boilerplate CustomItem Code


#region Field Instance Methods


public CustomTextField Type
{
	get
	{
		return new CustomTextField(InnerItem, InnerItem.Fields["Type"]);
	}
}


public CustomTextField Code
{
	get
	{
		return new CustomTextField(InnerItem, InnerItem.Fields["Code"]);
	}
}


public CustomTextField References
{
	get
	{
		return new CustomTextField(InnerItem, InnerItem.Fields["References"]);
	}
}


public CustomTextField Language
{
	get
	{
		return new CustomTextField(InnerItem, InnerItem.Fields["Language"]);
	}
}


#endregion //Field Instance Methods
}
}