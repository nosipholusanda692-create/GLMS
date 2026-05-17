using System.ComponentModel.DataAnnotations;

namespace GLMS.Web.Models
{
    public class Client
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Client Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Contact Details")]
        public string ContactDetails { get; set; } = string.Empty;

        [Required]
        public string Region { get; set; } = string.Empty;

        // Navigation property - one client can have many contracts
        public List<Contract> Contracts { get; set; } = new List<Contract>();
    }
}
