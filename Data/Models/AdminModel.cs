using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models
{
    public class AdminOverviewModel
    {
        public int TotalAccounts { get; set; }
        public int TotalTrips { get; set; }
        public int TotalSupportRequests { get; set; }
        public int TotalEmergencyRequests { get; set; }
        public List<int> AccountDetails { get; set; } = new List<int>();
        public List<double> TripStatistics { get; set; } = new List<double>();
        public List<int> SupportStatusDetails { get; set; } = new List<int>();
        public List<int> EmergencyStatusDetails { get; set; } = new List<int>();
    }

    public class AdminLineChartModel
    {
        public List<long> MonthlyIncome { get; set; } = new List<long>();
    }
}
