using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Sitecore.Data.Items;

namespace Sitecore.SharedSource.Analytics.Mappings
{
	public class AMultiVariantMapping : IMultiVariantMapping
	{
		private string _variantTemplateId;
		private List<string> _templateIds = new List<string>();

		public AMultiVariantMapping()
		{
			
		}

		public AMultiVariantMapping(XmlNode xmlNode)
		{
			if (xmlNode.Name != "VariantMapping" || xmlNode.Attributes["template"] == null || xmlNode.Attributes["variantTemplate"] == null)
			{
				return;
			}

			string variantTemplate = xmlNode.Attributes["variantTemplate"].Value;
			if (string.IsNullOrEmpty(variantTemplate))
			{
				return;
			}

			string templateId = xmlNode.Attributes["template"].Value;
			if (string.IsNullOrEmpty(templateId))
			{
				return;
			}

			List<string> templateIds = templateId.Split('|').Where(x => !string.IsNullOrEmpty(x)).ToList();
			if (templateIds.Count == 0)
			{
				return;
			}

			_variantTemplateId = variantTemplate;
			_templateIds = templateIds;
		}

		/// <summary>
		/// Run against content items of these templates
		/// </summary>
		public virtual List<string> TemplateIds
		{
			get { return _templateIds; }
			set { _templateIds = value; }
		}

		/// <summary>
		/// Mapped variant template
		/// </summary>
		public virtual string VariantTemplateId
		{
			get { return _variantTemplateId; }
			set { _variantTemplateId = value; }
		}

		/// <summary>
		/// Returns the source for the current variation mapping
		/// </summary>
		/// <param name="currentItem"></param>
		/// <returns></returns>
		public virtual Item VariantSource(Item currentItem)
		{
			return currentItem;
		}

		/// <summary>
		/// This will create the variant
		/// </summary>
		/// <param name="currentItem"></param>
		/// <param name="variantName"></param>
		/// <returns></returns>
		public virtual Item CreateVariantItem(Item currentItem, string variantName)
		{
			return null;
		}
	}
}
