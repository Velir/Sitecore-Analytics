using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Analytics.Data.DataAccess.DataSets;

namespace Sitecore.SharedSource.Analytics.Context.Model
{
	public class VisitGoal
	{
		public Guid GoalId { get; set; }
		public string GoalName { get; set; }
		public DateTime? GoalMetDate { get; set; }
		public Int32 EngagementValue { get; set; }
	}
}
