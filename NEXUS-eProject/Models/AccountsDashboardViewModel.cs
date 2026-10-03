using System.Collections.Generic;

namespace NEXUS_eProject.Models
{
    public class AccountsDashboardViewModel
    {
        public int BillingPending { get; set; }

        public int TotalBills { get; set; }

        public int UnpaidBills { get; set; }

        public int PaidBills { get; set; }

        public int PendingPayments { get; set; }

        public int VerifiedPayments { get; set; }

        public decimal OutstandingAmount { get; set; }

        public decimal TotalCollected { get; set; }

        public decimal TodayCollection { get; set; }

        public List<Connection> BillingPendingConnections { get; set; }
            = new List<Connection>();

        public List<Bill> RecentBills { get; set; }
            = new List<Bill>();

        public List<Payment> RecentPayments { get; set; }
            = new List<Payment>();
    }
}