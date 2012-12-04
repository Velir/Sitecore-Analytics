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
public partial class TestVariableItem : CustomItem
{

public static readonly string TemplateId = "{85DF313D-DBC6-4F27-AD77-3E0A8784A1DD}";


#region Boilerplate CustomItem Code

public TestVariableItem(Item innerItem) : base(innerItem)
{

}

public static implicit operator TestVariableItem(Item innerItem)
{
	return innerItem != null ? new TestVariableItem(innerItem) : null;
}

public static implicit operator Item(TestVariableItem customItem)
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


#endregion //Field Instance Methods
}
}