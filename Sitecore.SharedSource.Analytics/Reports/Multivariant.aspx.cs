using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Sitecore.SharedSource.Analytics.Reports
{
	public partial class Multivariant : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if(IsPostBack)
			{
				return;
			}

			string logoPath = Sitecore.Configuration.Settings.GetSetting("Analytics.Reports.Logo");
			if(string.IsNullOrEmpty(logoPath))
			{
				return;
			}

			imgLogo.ImageUrl = logoPath;
			plcLogo.Visible = true;
		}
	}
}