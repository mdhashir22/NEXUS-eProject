using System.Collections.Generic;

namespace NEXUS_eProject.Models
{
    public class TechnicalDashboardViewModel
    {
        public int PendingOrders { get; set; }

        public int FeasibilityPending { get; set; }

        public int FeasibleOrders { get; set; }

        public int RejectedOrders { get; set; }

        public int TotalConnections { get; set; }

        public int ActiveConnections { get; set; }

        public int InstallationPending { get; set; }

        public int PaymentVerified { get; set; }

        public int AvailableEquipment { get; set; }

        public List<Order> PendingTechnicalOrders { get; set; }
            = new List<Order>();

        public List<Order> RecentTechnicalOrders { get; set; }
            = new List<Order>();

        public List<Connection> RecentConnections { get; set; }
            = new List<Connection>();
    }
}