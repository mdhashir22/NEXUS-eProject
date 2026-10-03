using System;

namespace NEXUS_eProject.Models
{
    public class TechnicalInstallationViewModel
    {
        public int ConnectionId { get; set; }

        public int OrderId { get; set; }

        public string AccountId { get; set; } = string.Empty;

        public string OrderNumber { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string ConnectionType { get; set; } = string.Empty;

        public string PlanName { get; set; } = string.Empty;

        public string Speed { get; set; } = string.Empty;

        public string Equipment { get; set; } = string.Empty;

        public string RouterSerial { get; set; } = string.Empty;

        public string ConnectionStatus { get; set; } = string.Empty;

        public string OrderStatus { get; set; } = string.Empty;

        public DateTime? InstallationDate { get; set; }

        public DateTime? ActivatedAt { get; set; }
    }
}