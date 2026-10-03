using System.Collections.Generic;

namespace NEXUS_eProject.Models
{
    public class AdminDashboardViewModel
    {
        // =====================================================
        // EXISTING ADMIN / SYSTEM STATISTICS
        // =====================================================

        public int TotalEmployees { get; set; }

        public int TotalShops { get; set; }

        public int TotalVendors { get; set; }

        public int TotalProducts { get; set; }

        public int TotalPlans { get; set; }

        public int ActiveEmployees { get; set; }

        public int ActiveProducts { get; set; }

        public int ActivePlans { get; set; }


        // =====================================================
        // CUSTOMER STATISTICS
        // =====================================================

        public int TotalCustomers { get; set; }

        public int ActiveCustomers { get; set; }


        // =====================================================
        // ORDER / APPLICATION STATISTICS
        // =====================================================

        public int TotalOrders { get; set; }

        public int PendingOrders { get; set; }

        public int RetailAcceptedOrders { get; set; }

        public int FeasibleOrders { get; set; }

        public int RejectedOrders { get; set; }


        // =====================================================
        // CONNECTION STATISTICS
        // =====================================================

        public int TotalConnections { get; set; }

        public int ActiveConnections { get; set; }

        public int BillingPendingConnections { get; set; }

        public int PaymentPendingConnections { get; set; }

        public int PaymentVerifiedConnections { get; set; }

        public int InstallationPendingConnections { get; set; }

        public int InactiveConnections { get; set; }


        // =====================================================
        // BILLING STATISTICS
        // =====================================================

        public int TotalBills { get; set; }

        public int PaidBills { get; set; }

        public int UnpaidBills { get; set; }

        public int PartiallyPaidBills { get; set; }

        public decimal TotalBilledAmount { get; set; }

        public decimal OutstandingAmount { get; set; }


        // =====================================================
        // PAYMENT / REVENUE STATISTICS
        // =====================================================

        public int TotalPayments { get; set; }

        public int PendingPayments { get; set; }

        public int VerifiedPayments { get; set; }

        public int RejectedPayments { get; set; }

        public decimal TotalRevenue { get; set; }

        public decimal CurrentMonthRevenue { get; set; }


        // =====================================================
        // OPERATIONAL STATISTICS
        // =====================================================

        public int ConnectionsAwaitingInstallation { get; set; }

        public int ConnectionsActivatedThisMonth { get; set; }


        // =====================================================
        // APPLICATION / ORDER TREND CHART
        // Last 6 months
        // =====================================================

        public List<string> ApplicationChartLabels { get; set; }
            = new List<string>();

        public List<int> ApplicationChartData { get; set; }
            = new List<int>();


        // =====================================================
        // REVENUE TREND CHART
        // Last 6 months
        // =====================================================

        public List<string> RevenueChartLabels { get; set; }
            = new List<string>();

        public List<decimal> RevenueChartData { get; set; }
            = new List<decimal>();


        // =====================================================
        // CONNECTION STATUS CHART
        // =====================================================

        public List<string> ConnectionStatusLabels { get; set; }
            = new List<string>();

        public List<int> ConnectionStatusData { get; set; }
            = new List<int>();


        // =====================================================
        // CONNECTION TYPE CHART
        // =====================================================

        public List<string> ConnectionTypeLabels { get; set; }
            = new List<string>();

        public List<int> ConnectionTypeData { get; set; }
            = new List<int>();


        // =====================================================
        // BILL STATUS CHART
        // =====================================================

        public List<string> BillStatusLabels { get; set; }
            = new List<string>();

        public List<int> BillStatusData { get; set; }
            = new List<int>();


        // =====================================================
        // RECENT ACTIVITY
        // =====================================================

        public List<Order> RecentOrders { get; set; }
            = new List<Order>();

        public List<Payment> RecentPayments { get; set; }
            = new List<Payment>();

        public List<Connection> RecentConnections { get; set; }
            = new List<Connection>();
    }
}