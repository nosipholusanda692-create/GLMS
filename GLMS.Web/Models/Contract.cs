using System.ComponentModel.DataAnnotations;

namespace GLMS.Web.Models
{
    // Contract status enum - matches our Part 1 Zachman design
    public enum ContractStatus
    {
        Draft,
        Active,
        Expired,
        OnHold
    }

    public class Contract
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Contract Title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required]
        public ContractStatus Status { get; set; } = ContractStatus.Draft;

        [Required]
        [Display(Name = "Service Level")]
        public string ServiceLevel { get; set; } = string.Empty;

        // PDF file path saved on server
        [Display(Name = "Signed Agreement (PDF)")]
        public string? SignedAgreementPath { get; set; }

        // Foreign key to Client
        [Required]
        [Display(Name = "Client")]
        public int ClientId { get; set; }

        public Client? Client { get; set; }

        // One contract can have many service requests
        public List<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
    }
}
