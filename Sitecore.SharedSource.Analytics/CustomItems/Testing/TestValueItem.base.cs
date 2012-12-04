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
public partial class TestValueItem : CustomItem
{

public static readonly string TemplateId = "{122C4EDD-B132-495E-AADE-840D0D4ADDDE}";


#region Boilerplate CustomItem Code

public TestValueItem(Item innerItem) : base(innerItem)
{

}

public static implicit operator TestValueItem(Item innerItem)
{
	return innerItem != null ? new TestValueItem(innerItem) : null;
}

public static implicit operator Item(TestValueItem customItem)
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