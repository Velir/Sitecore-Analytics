using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sitecore.SharedSource.Analytics.Context.Model
{
	public class VisitVariation
	{
		public String TestValues { get; set; }
		public Int32 VisitCount { get; set; }
		public Int32 AchievedGoalCount { get; set; }
	}
}
