using System.Collections.Specialized;
using Sitecore.Data.Items;
using Sitecore.SharedSource.Analytics.Mappings;
using Sitecore.SharedSource.Commons.Extensions;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Sheer;

namespace Sitecore.SharedSource.Analytics.CustomSitecore.Commands
{
	public class InsertVariant : Command
	{
		public override void Execute(CommandContext context)
		{
			if(context.Items == null || context.Items.Length == 0)
			{
				return;
			}

			Item currentItem = context.Items[0];
			
			NameValueCollection nv = new NameValueCollection();
			nv.Add("currentid", currentItem.ID.ToString());

			//instantiate pipeline for processing
			Sitecore.Context.ClientPage.Start(this, "RunInsertPipeline", nv);
		}

		public void RunInsertPipeline(ClientPipelineArgs args)
		{
			if (!args.IsPostBack)
			{
				Sitecore.Context.ClientPage.ClientResponse.Input("Enter a name for the new variant: ", string.Empty);
				args.WaitForPostBack();
			}
			else
			{
				if (!args.HasResult || args.Result == null)
				{
					return;
				}

				//verify valid name
				string nameError = ItemUtil.GetItemNameError(args.Result);
				if(!string.IsNullOrEmpty(nameError))
				{
					SheerResponse.Alert(nameError, new string[0]);
					return;
				}

				string newItemName = args.Result;
				string currentItemId = args.Parameters["currentid"];

				Item currentItem = Sitecore.Context.ContentDatabase.GetItem(currentItemId);
				if(currentItem.IsNull())
				{
					return;
				}

				MultiVariantProcessor processor = new MultiVariantProcessor();
				IMultiVariantMapping multiVariantMapping = processor.GetMapping(currentItem.TemplateID.ToString(), Sitecore.Context.ContentDatabase);
				if (multiVariantMapping == null)
				{
					return;
				}

				//create a new item to be used as variant
				multiVariantMapping.CreateVariantItem(currentItem, newItemName);
			}
		}
	}
}
