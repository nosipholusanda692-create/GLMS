using System.ComponentModel.DataAnnotations;

namespace GLMS.Web.Models
{
    public enum ServiceRequestStatus
    {
        Pending,
        InProgress,
        Completed,
        Cancelled
    }

    public class ServiceRequest
    {
        public int Id { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Cost (USD)")]
        [DataType(DataType.Currency)]
        public decimal CostUSD { get; set; }

        // This gets auto-calculated from the currency API - Part 1 Strategy pattern
        [Display(Name = "Cost (ZAR)")]
        public decimal CostZAR { get; set; }

        [Display(Name = "Exchange Rate Used")]
        public decimal ExchangeRateUsed { get; set; }

        public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Pending;

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Foreign key to Contract
        [Required]
        [Display(Name = "Contract")]
        public int ContractId { get; set; }

        public Contract? Contract { get; set; }
    }
}
