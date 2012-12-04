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
public partial class TestDefinitionItem : CustomItem
{

public static readonly string TemplateId = "{45FB02E9-70B3-4CFE-8050-06EAD4B5DB3E}";


#region Boilerplate CustomItem Code

public TestDefinitionItem(Item innerItem) : base(innerItem)
{

}

public static implicit operator TestDefinitionItem(Item innerItem)
{
	return innerItem != null ? new TestDefinitionItem(innerItem) : null;
}

public static implicit operator Item(TestDefinitionItem customItem)
{
	return customItem != null ? customItem.InnerItem : null;
}

#endregion //Boilerplate CustomItem Code


#region Field Instance Methods


public CustomTextField Name
{
	get
	{
		return new CustomTextField(InnerItem, InnerItem.Fields["Name"]);
	}
}


public CustomLookupField TestStrategy
{
	get
	{
		return new CustomLookupField(InnerItem, InnerItem.Fields["Test Strategy"]);
	}
}


#endregion //Field Instance Methods
}
}