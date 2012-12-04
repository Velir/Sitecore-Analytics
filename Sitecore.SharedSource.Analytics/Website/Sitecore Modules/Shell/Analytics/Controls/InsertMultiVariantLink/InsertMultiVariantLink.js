function GetDialogArguments() {
    return getRadWindow().ClientParameters;
}

function getRadWindow() {
  if (window.radWindow) {
        return window.radWindow;
  }
    
    if (window.frameElement && window.frameElement.radWindow) {
        return window.frameElement.radWindow;
    }
    
    return null;
}

var isRadWindow = true;

var radWindow = getRadWindow();

if (radWindow) { 
  if (window.dialogArguments) { 
    radWindow.Window = window;
  } 
}

function scClose(url, text) {
	var returnValue = {
		url:url,
		text:text
	};

	getRadWindow().close(returnValue);
}

function scClose(url, text, linkedId, goalId) {
	var returnValue = {
		url: url,
		text: text,
		linkedId: linkedId,
		goalId: goalId
	};

	getRadWindow().close(returnValue);
}

function scCancel() {
  getRadWindow().close();
}

function scCloseWebEdit(url) {
  window.returnValue = url;
  window.close();
}

if (window.focus && Prototype.Browser.Gecko) {
  window.focus();
}