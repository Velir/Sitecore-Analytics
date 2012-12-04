//*****************************************************
// Author: Tim Braga - Velir
// Date: 10/31/2012
//*****************************************************

var analytics = {};
var keys = new Array();
keys[0] = 17; //ctrl
keys[1] = 68; //'d'
keys[2] = 77; //'m'
keys[3] = 83; //'s'

var currentKeys = new Array();

jQuery(document).ready(function () {

	jQuery('.dmsMultiVariant a').click(function (event) {

		//verify link does not have a tracking parameter assigned
		var href = jQuery(this).attr('href');
		if (href.indexOf("sc_trk") == -1) {
			var currentItemId = jQuery('.dmsMultiVariant').attr('currentItemId');
			var linkedId = jQuery(this).attr('linkedId');
			var goalId = jQuery(this).attr('goalId');

			//verify we have a goal
			if (goalId != null && goalId != '') {

				//make ajax request
				analytics.setGoal(currentItemId, linkedId, goalId);
			}
		}
	});

	jQuery('.dmsMultiVariant a').mousedown(function (event) {
		switch (event.which) {

			//right mouse button pressed     
			case 3:
				//verify link does not have a tracking parameter assigned
				var href = jQuery(this).attr('href');
				if (href.indexOf("sc_trk") == -1) {
					var currentItemId = jQuery('.dmsMultiVariant').attr('currentItemId');
					var linkedId = jQuery(this).attr('linkedId');
					var goalId = jQuery(this).attr('goalId');

					//verify we have a goal
					if (goalId != null && goalId != '') {

						//make ajax request
						analytics.setGoal(currentItemId, linkedId, goalId);
					}
				}
				break;
		}
	});
	analytics.refresh();
	setInterval(analytics.refresh, 3500);
});

//will watch key commands to determine to launch the analytics window
jQuery(document).keydown(function (e) {

	if (e.ctrlKey) {
		currentKeys = new Array();
		currentKeys[0] = e.which;
	}
	else {
		currentKeys[currentKeys.length] = e.which;
	}

	if (analytics.areArraysEqual(keys, currentKeys)) {
		//analytics.launchInfoWindow();
	}
});

analytics.setGoal = function (currentItemId, linkedId, goalId) {

	//make ajax request
	var data = JSON.stringify({ currentItemId: currentItemId, linkedId: linkedId, goalId: goalId });
	jQuery.ajax({
		url: "/Sitecore Modules/Shell/Analytics/Services/AnalyticsService.asmx/SetGoal",
		contentType: "application/json; charset=utf-8",
		dataType: "json",
		type: 'POST',
		data: data,
		error: function () {
			console.log('AnalyticsService call SetGoal failed');
		}
	});
}

analytics.flush = function () {

	//make ajax request
	jQuery.ajax({
		url: "/Sitecore Modules/Shell/Analytics/Services/AnalyticsService.asmx/Flush",
		contentType: "application/json; charset=utf-8",
		dataType: "json",
		type: 'POST',
		success: function () { analytics.refresh(); },
		error: function () {
			console.log('AnalyticsService call Flush failed');
		}
	});
}

analytics.refresh = function () {
	//make ajax request
	var visitId = jQuery('.analyticsPanel').attr('data_visitId');
	if (visitId == null || visitId == '') {
		return;
	}

	var data = JSON.stringify({ visitId: visitId });
	jQuery.ajax({
		url: "/Sitecore Modules/Shell/Analytics/Services/AnalyticsService.asmx/GetInformation",
		contentType: "application/json; charset=utf-8",
		dataType: "json",
		type: 'POST',
		data: data,
		success: function (data) {
			var record = data.d;
			if (record == null) {
				return;
			}

			//set engagement value
			jQuery('.visitorEV').html(record.VisitorEngagementValue);
			jQuery('.visitEV').html(record.VisitEngagementValue);

			//clear and build list
			jQuery('.analyticsGoals').html('');
			for (var i = 0; i < record.Goals.length; i++) {
				//build comment and append to the comments area
				jQuery(".analyticsGoals").append(analytics.buildGoal(record.Goals[i]));
			}

			//clear and build list
			jQuery('.analyticsGoals2').html('');
			for (var i = 0; i < record.Goals2.length; i++) {
				//build comment and append to the comments area
				jQuery(".analyticsGoals2").append(analytics.buildGoal(record.Goals2[i]));
			}

			//build and set last modified date
			var currentdate = new Date();
			var datetime = (currentdate.getMonth() + 1) + "/"
					+ currentdate.getDate() + "/"
					+ currentdate.getFullYear() + " "
					+ currentdate.getHours() + ":"
					+ currentdate.getMinutes() + ":"
					+ currentdate.getSeconds();

			jQuery('.analyticsUpdatedDate').html(datetime);
		},
		error: function () {
			console.log('AnalyticsService call GetInformation failed');
		}
	});
}

analytics.buildGoal = function (goal) {
	return '<li>' + goal.Date + ' | ' + goal.Goal + ' | ' + goal.Amount +'</li>';
}

analytics.areArraysEqual = function (arrayA, arrayB) {
	var i = 0;
	while (i < arrayA.length) {
		if (arrayB[i] == null) {
			return false;
		}

		if (arrayA[i] != arrayB[i]) {
			return false;
		}
		i++;
	}

	return true;
}