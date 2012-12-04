using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web.UI;
using Sitecore.Analytics.Data.Items;
using Sitecore.Analytics.Testing.TestingUtils;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Comparers;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Layouts;
using Sitecore.Pipelines;
using Sitecore.Pipelines.GetPlaceholderRenderings;
using Sitecore.Pipelines.GetRenderingDatasource;
using Sitecore.Resources;
using Sitecore.SharedSource.Analytics.Mappings;
using Sitecore.SharedSource.Commons.Extensions;
using Sitecore.Shell.Applications.Dialogs.Testing;
using Sitecore.StringExtensions;
using Sitecore.Web;
using Sitecore.Web.UI;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Pages;
using Sitecore.Web.UI.Sheer;

namespace Sitecore.SharedSource.Analytics.CustomSitecore.Dialogs.Testing
{
	public class SetTestDetailsForm : DialogForm
	{
		// Fields
		private static readonly string NewVariationDefaultName = Translate.Text("Variation Name");
		protected Checkbox ComponentReplacing;
		protected Button NewVariation;
		protected Border NoVariations;
		protected Border ResetContainer;
		protected Border Variations;
		private DeviceDefinition device;
		private LayoutDefinition layout;
		private RenderingDefinition rendering;

		protected ItemUri ContextItemUri
		{
			get { return (base.ServerProperties["itemUri"] as ItemUri); }
			set { base.ServerProperties["itemUri"] = value; }
		}

		protected DeviceDefinition Device
		{
			get
			{
				if (device == null)
				{
					LayoutDefinition layout = Layout;
					if ((layout != null) && !string.IsNullOrEmpty(DeviceId))
					{
						device = layout.GetDevice(DeviceId);
					}
				}
				return device;
			}
		}

		protected string DeviceId
		{
			get { return (base.ServerProperties["deviceid"] as string); }
			set
			{
				Assert.IsNotNullOrEmpty(value, "value");
				base.ServerProperties["deviceid"] = value;
			}
		}

		protected LayoutDefinition Layout
		{
			get
			{
				if (layout == null)
				{
					string sessionString = WebUtil.GetSessionString(LayoutSessionHandle);
					if (!string.IsNullOrEmpty(sessionString))
					{
						layout = LayoutDefinition.Parse(sessionString);
					}
				}
				return layout;
			}
		}

		protected string LayoutSessionHandle
		{
			get { return (base.ServerProperties["lsh"] as string); }
			set
			{
				Assert.IsNotNullOrEmpty(value, "value");
				base.ServerProperties["lsh"] = value;
			}
		}

		protected RenderingDefinition Rendering
		{
			get
			{
				if (rendering == null)
				{
					DeviceDefinition device = Device;
					string renderingUniqueId = RenderingUniqueId;
					if ((device != null) && !string.IsNullOrEmpty(renderingUniqueId))
					{
						rendering = device.GetRenderingByUniqueId(renderingUniqueId);
					}
				}
				return rendering;
			}
		}

		protected string RenderingUniqueId
		{
			get { return (base.ServerProperties["renderingid"] as string); }
			set
			{
				Assert.IsNotNullOrEmpty(value, "value");
				base.ServerProperties["renderingid"] = value;
			}
		}

		private List<VariableValueItemStub> VariableValues
		{
			get
			{
				var list = base.ServerProperties["variables"] as List<VariableValueItemStub>;
				return (list ?? new List<VariableValueItemStub>());
			}
			set
			{
				Assert.IsNotNull(value, "value");
				base.ServerProperties["variables"] = value;
			}
		}

		// Methods
		[UsedImplicitly]
		protected void AddVariation()
		{
			ID newID = ID.NewID;
			var item = new VariableValueItemStub(newID, NewVariationDefaultName);
			List<VariableValueItemStub> variableValues = VariableValues;
			variableValues.Insert(0, item);
			VariableValues = variableValues;
			string str = RenderVariableValue(item);
			SetControlsState();
			SheerResponse.Insert(Variations.ClientID, "afterBegin", str);
			SheerResponse.Eval("Sitecore.CollapsiblePanel.newAdded('{0}')".FormatWith(new object[] {newID.ToShortID()}));
		}

		protected void AllowComponentReplace()
		{
			if (!ComponentReplacing.Checked &&
			    (VariableValues.FindIndex(v => !string.IsNullOrEmpty(v.ReplacementComponent)) >= 0))
			{
				var parameters = new NameValueCollection();
				Sitecore.Context.ClientPage.Start(this, "ShowConfirm", parameters);
			}
			else
			{
				SheerResponse.Eval("scToggleTestComponentSection()");
			}
		}

		private static bool CanUpdateItem(Item item)
		{
			if ((!Sitecore.Context.IsAdministrator && item.Locking.IsLocked()) && !item.Locking.HasLock())
			{
				return false;
			}
			if (item.Appearance.ReadOnly)
			{
				return false;
			}
			return item.Access.CanWrite();
		}

		[UsedImplicitly]
		protected void ChangeDisplayComponent(string variationId)
		{
			Assert.ArgumentNotNull(variationId, "variationId");
			ID id = ShortID.DecodeID(variationId);
			List<VariableValueItemStub> variableValues = VariableValues;
			VariableValueItemStub stub = variableValues.Find(v => v.Id == id);
			if (stub != null)
			{
				stub.HideComponent = !stub.HideComponent;
				using (var writer = new HtmlTextWriter(new StringWriter()))
				{
					RenderContentControls(writer, stub);
					SheerResponse.SetOuterHtml(variationId + "_content", writer.InnerWriter.ToString());
				}
				using (var writer2 = new HtmlTextWriter(new StringWriter()))
				{
					RenderComponentControls(writer2, stub);
					SheerResponse.SetOuterHtml(variationId + "_component", writer2.InnerWriter.ToString());
				}
				VariableValues = variableValues;
			}
		}

		private Menu GetActionsMenu(string id)
		{
			Assert.IsNotNullOrEmpty(id, "id");
			var menu = new Menu
			           	{
			           		ID = id + "_menu"
			           	};
			string themedImageSource = Images.GetThemedImageSource("/sitecore modules/shell/analytics/images/delete2.png");
			string click = "RemoveVariation(\\\"{0}\\\")".FormatWith(new object[] {id});
			menu.Add("Delete", themedImageSource, click);
			themedImageSource = string.Empty;
			click = "javascript:Sitecore.CollapsiblePanel.rename(this, event, \"{0}\")".FormatWith(new object[] {id});
			menu.Add("Rename", themedImageSource, click);
			return menu;
		}

		private Item GetCurrentContent(VariableValueItemStub value, out bool isFallback)
		{
			Assert.ArgumentNotNull(value, "value");
			isFallback = false;
			Item item = null;
			if (!string.IsNullOrEmpty(value.Datasource))
			{
				return Client.ContentDatabase.GetItem(value.Datasource);
			}
			if ((Rendering != null) && !string.IsNullOrEmpty(Rendering.Datasource))
			{
				item = Client.ContentDatabase.GetItem(Rendering.Datasource);
				isFallback = true;
			}
			return item;
		}

		private Item GetCurrentRenderingItem(VariableValueItemStub value, out bool isFallback)
		{
			isFallback = false;
			if (!string.IsNullOrEmpty(value.ReplacementComponent))
			{
				return Client.ContentDatabase.GetItem(value.ReplacementComponent);
			}
			RenderingDefinition rendering = Rendering;
			if ((rendering != null) && !string.IsNullOrEmpty(rendering.ItemID))
			{
				isFallback = true;
				return Client.ContentDatabase.GetItem(rendering.ItemID);
			}
			return null;
		}

		private static string GetThumbnailSrc(Item item)
		{
			string str = "/sitecore/shell/blank.gif";
			if (item == null)
			{
				return str;
			}
			if (!string.IsNullOrEmpty(item.Appearance.Thumbnail) && (item.Appearance.Thumbnail != Settings.DefaultThumbnail))
			{
				string str2 = UIUtil.GetThumbnailSrc(item, 0x80, 0x80);
				if (!string.IsNullOrEmpty(str2))
				{
					str = str2;
				}
				return str;
			}
			return Images.GetThemedImageSource(item.Appearance.Icon, ImageDimension.id48x48);
		}

		protected virtual void InitVariableValues()
		{
			if (Rendering != null)
			{
				IEnumerable<MultivariateTestValueItem> variableValues = TestingUtil.MultiVariateTesting.GetVariableValues(
					Rendering, Client.ContentDatabase);
				var list = new List<VariableValueItemStub>();
				foreach (MultivariateTestValueItem item in variableValues)
				{
					var stub2 = new VariableValueItemStub(item.ID, item["Name"])
					            	{
					            		Datasource = item.Datasource.Path,
					            		HideComponent = item.HideComponent
					            	};
					VariableValueItemStub stub = stub2;
					ID targetID = item.ReplacementComponent.TargetID;
					stub.ReplacementComponent = ID.IsNullOrEmpty(targetID) ? string.Empty : targetID.ToString();
					list.Add(stub);
				}
				VariableValues = list;
			}
		}

		private static bool IsVariableValueChanged(MultivariateTestValueItem variableItem, VariableValueItemStub variableStub)
		{
			Assert.ArgumentNotNull(variableItem, "variableItem");
			return ((variableItem["Name"] != variableStub.Name) ||
			        ((variableItem.Datasource.Path != variableStub.Datasource) ||
			         ((variableItem.ReplacementComponent.Path != variableStub.ReplacementComponent) ||
			          (variableItem.HideComponent != variableStub.HideComponent))));
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!Sitecore.Context.ClientPage.IsEvent)
			{
				SetTestDetailsOptions options = SetTestDetailsOptions.Parse();
				DeviceId = options.DeviceId;
				ContextItemUri = ItemUri.Parse(options.ItemUri);
				RenderingUniqueId = options.RenderingUniqueId;
				LayoutSessionHandle = options.LayoutSessionHandle;
				InitVariableValues();
				if (VariableValues.FindIndex(v => !string.IsNullOrEmpty(v.ReplacementComponent)) > -1)
				{
					ComponentReplacing.Checked = true;
				}
				else
				{
					Variations.Class = "hide-test-component";
				}
				if (VariableValues.Count > 0)
				{
					ResetContainer.Visible = true;
				}
				if (Rendering != null)
				{
					Item variableItem = TestingUtil.MultiVariateTesting.GetVariableItem(Rendering, Client.ContentDatabase);
					if ((variableItem != null) && !variableItem.Access.CanCreate())
					{
						NewVariation.Disabled = true;
					}
				}
				SetControlsState();
				Render();
			}
		}

		protected override void OnOK(object sender, EventArgs args)
		{
			DeviceDefinition device = Device;
			Assert.IsNotNull(device, "device");
			MultivariateTestDefinitionItem testDefinition = TestingUtil.MultiVariateTesting.GetTestDefinition(device,
			                                                                                                  new ID(
			                                                                                                  	RenderingUniqueId),
			                                                                                                  Client.
			                                                                                                  	ContentDatabase);
			if (testDefinition == null)
			{
				ItemUri contextItemUri = ContextItemUri;
				if (contextItemUri == null)
				{
					return;
				}
				Item item = Client.ContentDatabase.GetItem(contextItemUri.ToDataUri());
				if (item != null)
				{
					testDefinition = TestingUtil.MultiVariateTesting.AddTestDefinition(item);
				}
			}
			if (testDefinition == null)
			{
				SheerResponse.Alert("The action cannot be executed because of security restrictions.", new string[0]);
			}
			else if (Rendering != null)
			{
				MultivariateTestVariableItem variableItem = TestingUtil.MultiVariateTesting.GetVariableItem(Rendering,
				                                                                                            Client.ContentDatabase);
				if (variableItem == null)
				{
					variableItem = TestingUtil.MultiVariateTesting.AddVariable(testDefinition, Rendering, Client.ContentDatabase);
				}
				if (variableItem == null)
				{
					SheerResponse.Alert("The action cannot be executed because of security restrictions.", new string[0]);
				}
				else
				{
					List<ID> list;
					if (!UpdateVariableValues(variableItem, out list))
					{
						SheerResponse.Alert("The action cannot be executed because of security restrictions.", new string[0]);
					}
					else
					{
						SheerResponse.SetDialogValue(SetTestDetailsOptions.GetDialogResut(variableItem.ID, list));
						SheerResponse.CloseWindow();
					}
				}
			}
		}

		[UsedImplicitly]
		protected void RemoveVariation(string variationId)
		{
			Assert.ArgumentNotNull(variationId, "variationId");
			ID id = ShortID.DecodeID(variationId);
			List<VariableValueItemStub> variableValues = VariableValues;
			int index = variableValues.FindIndex(value => value.Id == id);
			if (index < 0)
			{
				SheerResponse.Alert("Item not found.", new string[0]);
			}
			else
			{
				variableValues.RemoveAt(index);
				SheerResponse.Remove(variationId);
				VariableValues = variableValues;
				SetControlsState();
			}
		}

		[UsedImplicitly, HandleMessage("variation:rename")]
		protected void RenameVariation(Message message)
		{
			string argument = message.Arguments["variationId"];
			string str2 = message.Arguments["name"];
			Assert.ArgumentNotNull(argument, "variationId");
			Assert.ArgumentNotNull(str2, "name");
			ID id = ShortID.DecodeID(argument);
			List<VariableValueItemStub> variableValues = VariableValues;
			int num = variableValues.FindIndex(value => value.Id == id);
			if (num < 0)
			{
				SheerResponse.Alert("Item not found.", new string[0]);
			}
			else if (string.IsNullOrEmpty(str2))
			{
				SheerResponse.Alert("An item name cannot be blank.", new string[0]);
				SheerResponse.Eval("Sitecore.CollapsiblePanel.editName(\"{0}\")".FormatWith(new object[] {argument}));
			}
			else
			{
				variableValues[num].Name = str2;
				VariableValues = variableValues;
			}
		}

		protected virtual void Render()
		{
			var writer = new HtmlTextWriter(new StringWriter());
			foreach (VariableValueItemStub stub in VariableValues)
			{
				writer.Write(RenderVariableValue(stub));
			}
			string str = writer.InnerWriter.ToString();
			if (!string.IsNullOrEmpty(str))
			{
				Variations.InnerHtml = str;
			}
		}

		private void RenderComponentControls(HtmlTextWriter output, VariableValueItemStub value)
		{
			bool flag;
			ShortID tid = value.Id.ToShortID();
			Item currentRenderingItem = GetCurrentRenderingItem(value, out flag);
			string thumbnailSrc = GetThumbnailSrc(currentRenderingItem);
			string str2 = flag ? "default-values" : string.Empty;
			if (value.HideComponent)
			{
				str2 = str2 + " display-off";
			}
			output.Write("<div id='{0}_component' {1}>", tid,
			             string.IsNullOrEmpty(str2) ? string.Empty : ("class='" + str2 + "'"));
			output.Write("<div style=\"background-image:url('{0}')\" class='thumbnail-container'>", thumbnailSrc);
			output.Write("</div>");
			output.Write("<div class='picker-container'>");
			string click = value.HideComponent ? "javascript:void(0);" : ("variation:setcomponent(variationid=" + tid + ")");
			string reset = value.HideComponent
			               	? "javascript:void(0);"
			               	: "ResetVariationComponent(\\\"{0}\\\")".FormatWith(new object[] {tid});
			RenderPicker(output, currentRenderingItem, click, reset, false);
			output.Write("</div>");
			output.Write("</div>");
		}

		private void RenderContentControls(HtmlTextWriter output, VariableValueItemStub value)
		{
			bool flag;
			Assert.ArgumentNotNull(output, "output");
			Assert.ArgumentNotNull(value, "value");
			ShortID tid = value.Id.ToShortID();
			Item currentContent = null;
			currentContent = GetCurrentContent(value, out flag);
			string str = flag ? "default-values" : string.Empty;
			if (value.HideComponent)
			{
				str = str + " display-off";
			}
			output.Write("<div {0} id='{1}_content'>", (str == string.Empty) ? str : ("class='" + str + "'"), tid);
			string click = value.HideComponent ? "javascript:void(0);" : ("variation:setcontent(variationid=" + tid + ")");
			string reset = value.HideComponent
			               	? "javascript:void(0);"
			               	: "ResetVariationContent(\\\"{0}\\\")".FormatWith(new object[] {tid});
			RenderPicker(output, currentContent, click, reset, true);
			output.Write("</div>");
		}

		private void RenderDisplayControls(HtmlTextWriter output, VariableValueItemStub value)
		{
			Assert.ArgumentNotNull(output, "output");
			Assert.ArgumentNotNull(value, "value");
			ShortID tid = value.Id.ToShortID();
			output.Write(
				"<input type='checkbox' onfocus='this.blur();' onclick=\"javascript:return scSwitchRendering(this, event, '{0}')\" ",
				tid);
			if (value.HideComponent)
			{
				output.Write(" checked='checked' ");
			}
			output.Write("/>");
			output.Write("<span class='display-component-title'>");
			output.Write(Translate.Text("Hide Component"));
			output.Write("</span>");
		}

		private void RenderPicker(HtmlTextWriter writer, Item item, string click, string reset, bool prependEllipsis)
		{
			Assert.ArgumentNotNull(writer, "writer");
			Assert.ArgumentNotNull(click, "click");
			Assert.ArgumentNotNull(reset, "reset");
			string themedImageSource = Images.GetThemedImageSource((item != null) ? item.Appearance.Icon : string.Empty,
			                                                       ImageDimension.id16x16);
			string str2 = Translate.Text("[Not set]");
			string str3 = "item-picker";
			if (item != null)
			{
				str2 = prependEllipsis ? ".../" : string.Empty;
				str2 = str2 + item.DisplayName;
			}
			else
			{
				str3 = str3 + " not-set";
			}
			writer.Write(string.Format("<div style=\"background-image:url('{0}')\" class='{1}'>", themedImageSource, str3));
			writer.Write("<a href='#' class='pick-button' onclick=\"{0}\" title=\"{1}\">...</a>",
			             Sitecore.Context.ClientPage.GetClientEvent(click), Translate.Text("Select"));
			writer.Write("<a href='#' class='reset-button' onclick=\"{0}\" title=\"{1}\"></a>",
			             Sitecore.Context.ClientPage.GetClientEvent(reset), Translate.Text("Reset"));
			writer.Write("<span title=\"{0}\">{1}</span>", (item == null) ? string.Empty : item.DisplayName, str2);
			writer.Write("</div>");
		}

		private string RenderVariableValue(VariableValueItemStub value)
		{
			var renderer = new CollapsiblePanelRenderer();
			var context3 = new CollapsiblePanelRenderer.ActionsContext
			               	{
			               		IsVisible = true
			               	};
			CollapsiblePanelRenderer.ActionsContext actionsContext = context3;
			string id = value.Id.ToShortID().ToString();
			actionsContext.Menu = GetActionsMenu(id);
			var context4 = new CollapsiblePanelRenderer.NameContext(value.Name)
			               	{
			               		OnNameChanged = "javascript:Sitecore.CollapsiblePanel.nameChanged(this, event)"
			               	};
			CollapsiblePanelRenderer.NameContext nameContext = context4;
			string panelHtml = RenderVariableValueDetails(value);
			return renderer.Render(id, panelHtml, false, nameContext, actionsContext);
		}

		private string RenderVariableValueDetails(VariableValueItemStub value)
		{
			var output = new HtmlTextWriter(new StringWriter());
			output.Write("<table class='top-row'>");
			output.Write("<tr>");
			output.Write("<td class='left test-title'>");
			output.Write(Translate.Text("Test Content Item:"));
			output.Write("</td>");
			output.Write("<td class='right'>");
			output.Write("</td>");
			output.Write("</tr>");
			output.Write("<tr>");
			output.Write("<td class='left test-content'>");
			RenderContentControls(output, value);
			output.Write("</td>");
			output.Write("<td class='right display-component'>");
			RenderDisplayControls(output, value);
			output.Write("</td>");
			output.Write("</tr>");
			output.Write("<tr class='component-row'>");
			output.Write("<td class='left test-title'>");
			output.Write(Translate.Text("Test Component Design:"));
			output.Write("</td>");
			output.Write("<td rowspan='2' class='right'>");
			output.Write("</td>");
			output.Write("</tr>");
			output.Write("<tr class='component-row'>");
			output.Write("<td class='left test-component'>");
			RenderComponentControls(output, value);
			output.Write("</td>");
			output.Write("</tr>");
			output.Write("</table>");
			return output.InnerWriter.ToString();
		}

		[HandleMessage("variation:reset", true)]
		protected void Reset_Click(ClientPipelineArgs args)
		{
			if (args.IsPostBack)
			{
				if (args.HasResult && (args.Result != "no"))
				{
					RenderingDefinition rendering = Rendering;
					string str = "#reset#";
					if (rendering != null)
					{
						Item variableItem = TestingUtil.MultiVariateTesting.GetVariableItem(rendering, Client.ContentDatabase);
						if (variableItem == null)
						{
							SheerResponse.SetDialogValue(str);
							SheerResponse.CloseWindow();
							return;
						}
						if (!variableItem.Access.CanDelete())
						{
							SheerResponse.Alert("The action cannot be executed because of security restrictions.", new string[0]);
							return;
						}
						Item parent = variableItem.Parent;
						variableItem.Delete();
						if (((parent != null) && parent.Access.CanDelete()) && !parent.HasChildren)
						{
							parent.Delete();
						}
					}
					SheerResponse.SetDialogValue(str);
					SheerResponse.CloseWindow();
				}
			}
			else
			{
				SheerResponse.Confirm("Component will be removed from the test set. Are you sure you want to continue?");
				args.WaitForPostBack();
			}
		}

		protected void ResetVariationComponent(string variationId)
		{
			Assert.ArgumentNotNull(variationId, "variationId");
			ID id = ShortID.DecodeID(variationId);
			List<VariableValueItemStub> variableValues = VariableValues;
			VariableValueItemStub stub = variableValues.Find(v => v.Id == id);
			if (stub != null)
			{
				stub.ReplacementComponent = string.Empty;
				var output = new HtmlTextWriter(new StringWriter());
				RenderComponentControls(output, stub);
				SheerResponse.SetOuterHtml(variationId + "_component", output.InnerWriter.ToString());
				VariableValues = variableValues;
			}
		}

		protected void ResetVariationContent(string variationId)
		{
			Assert.ArgumentNotNull(variationId, "variationId");
			ID id = ShortID.DecodeID(variationId);
			List<VariableValueItemStub> variableValues = VariableValues;
			VariableValueItemStub stub = variableValues.Find(v => v.Id == id);
			if (stub != null)
			{
				stub.Datasource = string.Empty;
				var output = new HtmlTextWriter(new StringWriter());
				RenderContentControls(output, stub);
				SheerResponse.SetOuterHtml(variationId + "_content", output.InnerWriter.ToString());
				VariableValues = variableValues;
			}
		}

		[HandleMessage("variation:setcomponent", true)]
		protected void SetComponent(ClientPipelineArgs args)
		{
			string str = args.Parameters["variationid"];
			if (string.IsNullOrEmpty(str))
			{
				SheerResponse.Alert("Item not found.", new string[0]);
			}
			else if ((Rendering == null) || (Layout == null))
			{
				SheerResponse.Alert("An error ocurred.", new string[0]);
			}
			else if (!args.IsPostBack)
			{
				string placeholder = Rendering.Placeholder;
				Assert.IsNotNull(placeholder, "placeholder");
				string str3 = Layout.ToXml();
				var args2 = new GetPlaceholderRenderingsArgs(placeholder, str3, Client.ContentDatabase, ID.Parse(DeviceId));
				args2.OmitNonEditableRenderings = true;
				args2.CustomData["showOpenProperties"] = false;
				CorePipeline.Run("getPlaceholderRenderings", args2);
				string dialogURL = args2.DialogURL;
				if (string.IsNullOrEmpty(dialogURL))
				{
					SheerResponse.Alert("An error ocurred.", new string[0]);
				}
				else
				{
					SheerResponse.ShowModalDialog(dialogURL, "720px", "470px", string.Empty, true);
					args.WaitForPostBack();
				}
			}
			else if (args.HasResult)
			{
				ID id = ShortID.DecodeID(str);
				List<VariableValueItemStub> variableValues = VariableValues;
				VariableValueItemStub stub = variableValues.Find(v => v.Id == id);
				if (stub != null)
				{
					string result;
					if (args.Result.IndexOf(',') >= 0)
					{
						result = args.Result.Split(new[] {','})[0];
					}
					else
					{
						result = args.Result;
					}
					stub.ReplacementComponent = result;
					var output = new HtmlTextWriter(new StringWriter());
					RenderComponentControls(output, stub);
					SheerResponse.SetOuterHtml(str + "_component", output.InnerWriter.ToString());
					VariableValues = variableValues;
				}
			}
		}

		[HandleMessage("variation:setcontent", true)]
		protected void SetContent(ClientPipelineArgs args)
		{
			Predicate<VariableValueItemStub> match = null;
			Predicate<VariableValueItemStub> predicate2 = null;
			ID id;
			string str = args.Parameters["variationid"];
			if (string.IsNullOrEmpty(str))
			{
				SheerResponse.Alert("Item not found.", new string[0]);
			}
			else
			{
				id = ShortID.DecodeID(str);
				if (args.IsPostBack)
				{
					if (args.HasResult)
					{
						List<VariableValueItemStub> variableValues = VariableValues;
						if (match == null)
						{
							match = v => v.Id == id;
						}
						VariableValueItemStub stub = variableValues.Find(match);
						if (stub != null)
						{
							stub.Datasource = args.Result;
							var output = new HtmlTextWriter(new StringWriter());
							RenderContentControls(output, stub);
							SheerResponse.SetOuterHtml(str + "_content", output.InnerWriter.ToString());
							VariableValues = variableValues;
						}
					}
				}
				else
				{
					if (predicate2 == null)
					{
						predicate2 = v => v.Id == id;
					}
					VariableValueItemStub stub2 = VariableValues.Find(predicate2);
					if ((stub2 != null) && ((Rendering != null) && !string.IsNullOrEmpty(Rendering.ItemID)))
					{
						Item renderingItem = Client.ContentDatabase.GetItem(Rendering.ItemID);
						if (renderingItem == null)
						{
							SheerResponse.Alert("Item not found.", new string[0]);
						}
						else
						{
							//get current content item
							Item variantSourceItem = (ContextItemUri == null) ? null : Client.ContentDatabase.GetItem(ContextItemUri.ToDataUri());

							if (variantSourceItem.IsNotNull())
							{
								//use variant mapping functionality to find proper source
								MultiVariantProcessor processor = new MultiVariantProcessor();
								IMultiVariantMapping mapping = processor.GetMapping(variantSourceItem.TemplateID.ToString(), Client.ContentDatabase);
								if(mapping != null)
								{
									//set mapping
									variantSourceItem = mapping.VariantSource(variantSourceItem);
								}
							}

							var args3 = new GetRenderingDatasourceArgs(renderingItem);
							args3.FallbackDatasourceRoots = new List<Item> {variantSourceItem};
							args3.ContentLanguage = ((variantSourceItem != null) ? variantSourceItem.Language : null);
							args3.ContextItemPath = (variantSourceItem != null) ? variantSourceItem.Paths.FullPath : string.Empty;
							args3.ShowDialogIfDatasourceSetOnRenderingItem = true;
							args3.CurrentDatasource = (string.IsNullOrEmpty(stub2.Datasource) ? Rendering.Datasource : stub2.Datasource);
							GetRenderingDatasourceArgs args2 = args3;
							CorePipeline.Run("getRenderingDatasource", args2);
							if (string.IsNullOrEmpty(args2.DialogUrl))
							{
								SheerResponse.Alert("An error ocurred.", new string[0]);
							}
							else
							{
								SheerResponse.ShowModalDialog(args2.DialogUrl, "460px", "460px", string.Empty, true);
								args.WaitForPostBack();
							}
						}
					}
				}
			}
		}

		private void SetControlsState()
		{
			int count = VariableValues.Count;
			base.OK.Disabled = count < 2;
			NoVariations.Visible = count < 1;
			NewVariation.Disabled = count > 0x100;
		}

		protected void ShowConfirm(ClientPipelineArgs args)
		{
			Assert.ArgumentNotNull(args, "args");
			if (args.IsPostBack)
			{
				if (args.HasResult && (args.Result != "no"))
				{
					SheerResponse.Eval("scToggleTestComponentSection()");
					List<VariableValueItemStub> variableValues = VariableValues;
					foreach (VariableValueItemStub stub in variableValues)
					{
						if (!string.IsNullOrEmpty(stub.ReplacementComponent))
						{
							stub.ReplacementComponent = string.Empty;
							using (var writer = new HtmlTextWriter(new StringWriter()))
							{
								RenderComponentControls(writer, stub);
								SheerResponse.SetOuterHtml(stub.Id.ToShortID() + "_component", writer.InnerWriter.ToString());
								continue;
							}
						}
					}
					VariableValues = variableValues;
				}
				else
				{
					ComponentReplacing.Checked = true;
				}
			}
			else
			{
				SheerResponse.Confirm("Test component settings will be removed. Are you sure you want to continue?");
				args.WaitForPostBack();
			}
		}

		private static void UpdateVariableValueItem(MultivariateTestValueItem variableValue,
		                                            VariableValueItemStub variableStub)
		{
			Assert.ArgumentNotNull(variableValue, "variableValue");
			UpdateVariableValueItem(variableValue, variableStub, variableValue.InnerItem.Appearance.Sortorder);
		}

		private static void UpdateVariableValueItem(MultivariateTestValueItem variableValue,
		                                            VariableValueItemStub variableStub, int sortOrder)
		{
			Assert.ArgumentNotNull(variableValue, "variableValue");
			using (new EditContext(variableValue))
			{
				variableValue["Name"] = variableStub.Name;
				variableValue.Datasource.Path = variableStub.Datasource;
				variableValue.HideComponent = variableStub.HideComponent;
				variableValue.ReplacementComponent.Path = variableStub.ReplacementComponent;
				variableValue.InnerItem.Appearance.Sortorder = sortOrder;
			}
		}

		protected virtual bool UpdateVariableValues(MultivariateTestVariableItem variableItem, out List<ID> modifiedVariations)
		{
			Assert.ArgumentNotNull(variableItem, "variableItem");
			modifiedVariations = new List<ID>();
			List<VariableValueItemStub> variableValues = VariableValues;
			var list2 = new List<MultivariateTestValueItem>(TestingUtil.MultiVariateTesting.GetVariableValues(variableItem));
			var comparer = new DefaultComparer();
			list2.Sort((lhs, rhs) => comparer.Compare(lhs, rhs));
			int num = (list2.Count > 0) ? (list2[0].InnerItem.Appearance.Sortorder - 1) : Settings.DefaultSortOrder;
			var templateID = new TemplateID(MultivariateTestValueItem.TemplateID);
			var list3 = new List<KeyValuePair<MultivariateTestValueItem, VariableValueItemStub>>();
			var list4 = new List<KeyValuePair<int, VariableValueItemStub>>();
			for (int i = variableValues.Count - 1; i >= 0; i--)
			{
				VariableValueItemStub stub = variableValues[i];
				ID currentId = stub.Id;
				int index = list2.FindIndex(item => item.ID == currentId);
				if (index < 0)
				{
					var pair = new KeyValuePair<int, VariableValueItemStub>(num--, stub);
					list4.Add(pair);
				}
				else
				{
					MultivariateTestValueItem item = list2[index];
					if (IsVariableValueChanged(item, stub))
					{
						list3.Add(new KeyValuePair<MultivariateTestValueItem, VariableValueItemStub>(item, stub));
					}
					list2.RemoveAt(index);
				}
			}
			if (list2.Count != 0)
			{
			}

			    foreach (Item item2 in list2)
			    {
			        modifiedVariations.Add(item2.ID);
			        item2.Delete();
			    }
			    foreach (var pair2 in list4)
			    {
			        VariableValueItemStub variableStub = pair2.Value;
			        int key = pair2.Key;
			        string name = variableStub.Name;
			        if (ContainsNonASCIISymbols(name))
			        {
			            Item item3 = variableItem.Database.GetItem(templateID.ID);
			            name = (item3 != null) ? item3.Name : "Unnamed item";
			        }
			        if (!ItemUtil.IsItemNameValid(name))
			        {
			            try
			            {
			                name = ItemUtil.ProposeValidItemName(name);
			            }
			            catch (Exception)
			            {
			                return false;
			            }
			        }
			        name = ItemUtil.GetUniqueName(variableItem, name);
			        Item item4 = variableItem.InnerItem.Add(name, templateID);
			        Assert.IsNotNull(item4, "newVariableValue");
			        UpdateVariableValueItem((MultivariateTestValueItem) item4, variableStub, key);
			    }
			    foreach (var pair3 in list3)
			    {
			        MultivariateTestValueItem variableValue = pair3.Key;
			        VariableValueItemStub stub3 = pair3.Value;
			        modifiedVariations.Add(variableValue.ID);
			        UpdateVariableValueItem(variableValue, stub3);
			    }
			return true;
		}

		public static bool ContainsNonASCIISymbols(string input)
		{
			Assert.ArgumentNotNull(input, "input");
			return input.Any<char>(c => (c > '\x00ff'));
		}

		// Properties

		// Nested Types

		#region Nested type: VariableValueItemStub

		[Serializable]
		private class VariableValueItemStub
		{
			// Fields
			private readonly string id;
			public string Datasource;
			public bool HideComponent;
			public string Name;
			public string ReplacementComponent;

			// Methods
			public VariableValueItemStub(ID id, string name)
			{
				Assert.ArgumentNotNull(id, "id");
				Assert.ArgumentNotNull(name, "name");
				Datasource = string.Empty;
				HideComponent = false;
				ReplacementComponent = string.Empty;
				Name = name;
				this.id = id.ToShortID().ToString();
			}

			// Properties
			public ID Id
			{
				get
				{
					if (!string.IsNullOrEmpty(id))
					{
						return ShortID.DecodeID(id);
					}
					return ID.Null;
				}
			}
		}

		#endregion
	}

	internal class CollapsiblePanelRenderer
	{
		// Fields
		private string cssClass = "collapsible-panel";

		public string CssClass
		{
			get { return cssClass; }
			set
			{
				Assert.IsNotNullOrEmpty(value, "value");
				cssClass = value;
			}
		}

		// Methods
		public string Render(string id, string panelHtml, bool draggable, NameContext nameContext,
		                     ActionsContext actionsContext)
		{
			Assert.ArgumentNotNullOrEmpty(id, "id");
			Assert.ArgumentNotNullOrEmpty(panelHtml, "panelHtml");
			var output = new HtmlTextWriter(new StringWriter());
			output.Write("<div id='{0}' class='{1}'>", id, CssClass);
			output.Write("<div id='{0}_header' class='panel-header'>", id);
			output.Write("<div class='header-actions'>");
			output.Write("<a href='#'>");
			output.Write("<div class='icon-expand'></div>");
			output.Write("</a>");
			output.Write("</div>");
			RenderActions(output, id, actionsContext);
			if (draggable)
			{
				output.Write("<img src='/sitecore modules/shell/analytics/images/draghandle9x15.png' class='drag-handle' />");
			}
			RenderName(output, id, nameContext);
			output.Write("</div>");
			output.Write("<div id='{0}_panel' class='panel'>", id);
			output.Write(panelHtml);
			output.Write("</div>");
			output.Write("</div>");
			return output.InnerWriter.ToString();
		}

		protected virtual void RenderActions(HtmlTextWriter output, string id, ActionsContext context)
		{
			Assert.ArgumentNotNull(output, "output");
			Assert.ArgumentNotNullOrEmpty(id, "id");
			if (context.IsVisible)
			{
				output.Write("<div class='header-menu'>");
				string str = string.IsNullOrEmpty(context.OnActionClick)
				             	? string.Empty
				             	: ("onclick=\"" + context.OnActionClick + "\"");
				output.Write("<a class='action-combo' id='{0}_combo' data-meta-id='{0}' href='#'>", id);
				output.Write("<table {0}>", str);
				output.Write("<tr>");
				output.Write("<td class='action'>");
				output.Write(Translate.Text("Actions"));
				output.Write("</td>");
				output.Write("<td class='action-icon'>");
				output.Write("<img src='/sitecore modules/shell/analytics/images/menudropdown_black9x8.png' />");
				output.Write("</td>");
				output.Write("</tr>");
				output.Write("</table>");
				output.Write("</a>");
				if (context.Menu != null)
				{
					context.Menu.Style["display"] = "none";
					output.Write(HtmlUtil.RenderControl(context.Menu));
				}
				output.Write("</div>");
			}
		}

		protected virtual void RenderName(HtmlTextWriter output, string id, NameContext context)
		{
			Assert.ArgumentNotNull(output, "output");
			Assert.ArgumentNotNullOrEmpty(id, "id");
			if (!context.Editable)
			{
				output.Write("<span class='header-title'>");
				output.Write(context.Name);
				output.Write("</span>");
			}
			else
			{
				output.Write("<a href='#' class='header-title'>");
				output.Write(StringUtil.EscapeQuote(context.Name));
				output.Write("</a>");
				string str = "onkeydown='javascript:return Sitecore.CollapsiblePanel.editNameChanging(this, event);'";
				string str2 = string.IsNullOrEmpty(context.OnNameChanging)
				              	? string.Empty
				              	: ("onkeyup=\"" + context.OnNameChanging + "\"");
				string str3 = string.IsNullOrEmpty(context.OnNameChanged)
				              	? string.Empty
				              	: ("onchange=\"" + context.OnNameChanged + "\"");
				output.Write(
					"<input type='text' {0} {1} id='{2}_name' name='{2}_name' data-meta-id='{2}' data-validation-msg=\"{3}\" style='display:none' class='header-title-edit' value=\"{4}\" {5} />",
					new object[]
						{str2, str3, id, Translate.Text("The name cannot be blank."), StringUtil.EscapeQuote(context.Name), str});
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct ActionsContext
		{
			public bool IsVisible;
			public Menu Menu;
			public string OnActionClick;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct NameContext
		{
			public bool Editable;
			public string Name;
			public string OnNameChanged;
			public string OnNameChanging;

			public NameContext(string name)
			{
				Assert.IsNotNullOrEmpty(name, "name");
				Name = name;
				Editable = true;
				OnNameChanging = null;
				OnNameChanged = null;
			}
		}
	}
}