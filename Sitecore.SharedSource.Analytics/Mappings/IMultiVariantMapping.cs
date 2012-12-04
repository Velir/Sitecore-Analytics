using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data.Items;

namespace Sitecore.SharedSource.Analytics.Mappings
{
	public interface IMultiVariantMapping
	{
		/// <summary>
		/// Run against content items of these templates
		/// </summary>
		List<string> TemplateIds { get; set; }

		/// <summary>
		/// Mapped variant template
		/// </summary>
		string VariantTemplateId { get; set; }

		/// <summary>
		/// Returns the source for the current variation mapping
		/// </summary>
		/// <param name="currentItem"></param>
		/// <returns></returns>
		Item VariantSource(Item currentItem);

		/// <summary>
		/// This will create the variant
		/// </summary>
		/// <param name="currentItem"></param>
		/// <param name="variantName"></param>
		/// <returns></returns>
		Item CreateVariantItem(Item currentItem, string variantName);
	}
}