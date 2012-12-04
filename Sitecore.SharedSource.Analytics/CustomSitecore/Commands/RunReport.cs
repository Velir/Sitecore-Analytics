using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Shell.Applications.WebEdit.Commands;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Sheer;

namespace Sitecore.SharedSource.Analytics.CustomSitecore.Commands
{
	public class RunReport : WebEditCommand
	{
		public override void Execute(CommandContext context)
		{
			Assert.ArgumentNotNull(context, "context");
			Item item = (context.Items.Length > 0) ? context.Items[0] : null;
			if (item == null)
			{
				SheerResponse.Alert("Item not found.", new string[0]);
				return;
			}

			//instantiate pipeline for processing
			const string url = "/sitecore modules/shell/analytics/reports/multivariant.aspx";
			SheerResponse.Eval(string.Format("window.open('{0}', 'MultivariantReport','width=875,height=650,status,resizable')", url));
		}
	}
}
