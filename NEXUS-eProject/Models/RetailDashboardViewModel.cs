using System.Collections.Generic;

namespace NEXUS_eProject.Models
{
    public class RetailDashboardViewModel
    {
        // =====================================================
        // SUMMARY
        // =====================================================

        public int TotalCustomers { get; set; }

        public int TotalOrders { get; set; }

        public int ActiveProducts { get; set; }

        public int ActiveConnections { get; set; }

        public int PendingBills { get; set; }

        public int TodayPayments { get; set; }

        public int TodayOrders { get; set; }

        public int PendingOrders { get; set; }


        // =====================================================
        // FINANCIAL
        // =====================================================

        public decimal OutstandingAmount { get; set; }

        public decimal TodayPaymentAmount { get; set; }


        // =====================================================
        // RECENT ORDERS
        // =====================================================

        public List<Order> RecentOrders { get; set; }
            = new List<Order>();


        // =====================================================
        // RECENT PAYMENTS
        // =====================================================

        public List<Payment> RecentPayments { get; set; }
            = new List<Payment>();
    }
}