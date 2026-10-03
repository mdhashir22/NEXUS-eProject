using System.Collections.Generic;

namespace NEXUS_eProject.Models
{
    public class CustomerDashboardViewModel
    {
        // Customer
        public Customer Customer { get; set; } = new Customer();

        // Dashboard Stats
        public int TotalApplications { get; set; }

        public string ConnectionStatus { get; set; } = "No Connection";

        public decimal OutstandingAmount { get; set; }

        public string AccountStatus { get; set; } = "Active";

        // Current / Latest Connection
        public Connection? Connection { get; set; }

        // Latest Bill
        public Bill? LatestBill { get; set; }

        // Latest Payment
        public Payment? LatestPayment { get; set; }

        // Recent Applications
        public List<Order> RecentApplications { get; set; }
            = new List<Order>();
    }
}