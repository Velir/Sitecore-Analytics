/* This file is shared between older developer center rich text editor and the new EditorPage, that is used exclusively by Content Editor */

var scEditor = null;
var scTool = null;

 RadEditorCommandList["InsertMultiVariantSitecoreLink"] = function (commandName, editor, args) {
 	var d = Telerik.Web.UI.Editor.CommandList._getLinkArgument(editor);
 	Telerik.Web.UI.Editor.CommandList._getDialogArguments(d, "A", editor, "DocumentManager");

 	var html = editor.getSelectionHtml();

 	var id;

 	// internal link in form of <a href="~/link.aspx?_id=110D559FDEA542EA9C1C8A5DF7E70EF9">...</a>
 	if (html) {
 		id = GetMediaID(html);
 	}

 	// link to media in form of <a href="~/media/CC2393E7CA004EADB4A155BE4761086B.ashx">...</a>
 	if (!id) {
 		var regex = /~\/media\/([\w\d]+)\.ashx/;
 		var match = regex.exec(html);
 		if (match && match.length >= 1 && match[1]) {
 			id = match[1];
 		}
 	}

 	if (!id) {
 		id = scItemID;
 	}

 	scEditor = editor;

 	editor.showExternalDialog(
    "/sitecore/shell/default.aspx?xmlcontrol=Analytics.RichText.InsertMultiVariantLink&la=" + scLanguage + "&fo=" + id,
    null, //argument
    500,
    400,
    scInsertMultiVariantLink, //callback
    null, // callback args
    "Insert Link",
    true, //modal
    Telerik.Web.UI.WindowBehaviors.Close, // behaviors
    false, //showStatusBar
    false //showTitleBar
  );
 };

 function scInsertMultiVariantLink(sender, returnValue) {
 	if (!returnValue) {
 		return;
 	}

 	var d = scEditor.getSelection().getParentElement();

 	if ($telerik.isFirefox && d.tagName == "A") {
 		d.parentNode.removeChild(d);
 	} else {
 		scEditor.fire("Unlink");
 	}

 	var text = scEditor.getSelectionHtml();
 	
	var linkedId = '';
	if (returnValue.linkedId != null) {
		linkedId = returnValue.linkedId;
	}

	var className = '';
	var goalId = '';
	if (returnValue.goalId != null) {
		goalId = returnValue.goalId;

		if (goalId != '') {
			className = 'analytics';
		}
	}

 	if (text == "" || text == null || ((text != null) && (text.length == 15) && (text.substring(2, 15).toLowerCase() == "<p>&nbsp;</p>"))) {
 		text = returnValue.text;
 	}
 	else {
 		// if selected string is a full paragraph, we want to insert the link inside the paragraph, and not the other way around.
 		var regex = /^[\s]*<p>(.+)<\/p>[\s]*$/i;
 		var match = regex.exec(text);
 		if (match && match.length >= 2) {
 			scEditor.pasteHtml("<p><a class=\"" + className + "\" linkedId=\"" + linkedId + "\" goalId=\"" + goalId + "\" href=\"" + returnValue.url + "\">" + match[1] + "</a></p>", "DocumentManager");
 			return;
 		}
 	}

 	scEditor.pasteHtml("<a class=\"" + className + "\" linkedId=\"" + linkedId + "\" goalId=\"" + goalId + "\" href=\"" + returnValue.url + "\">" + text + "</a>", "DocumentManager");
 }