using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.SharedSource.Analytics.Mappings;
using Sitecore.SharedSource.Commons.Extensions;
using Velir.SitecoreLibrary.Modules.Contextualizer.Filters;

namespace Sitecore.SharedSource.Analytics.Filters
{
	public class VariantFilter : IFilter
	{
		public void Process(FilterArgs args)
		{
			//get master database
			Database database = Database.GetDatabase("master");
			if (database == null || args.ContentItem.IsNull())
			{
				args.HideCommand = false;
				return;
			}

			MultiVariantProcessor processor = new MultiVariantProcessor();
			TemplateItem templateItem = processor.GetTemplate(args.ContentItem.TemplateID.ToString(), database);

			args.HideCommand = (templateItem == null);
		}
	}
}