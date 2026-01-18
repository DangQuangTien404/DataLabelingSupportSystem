using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DTOs.Entities
{
    public class Project
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ManagerId { get; set; } = string.Empty;

        [ForeignKey("ManagerId")]
        public virtual User Manager { get; set; } = null!;

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerLabel { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalBudget { get; set; }
        public DateTime Deadline { get; set; }

        // Navigation
        public virtual ICollection<LabelClass> LabelClasses { get; set; } = new List<LabelClass>();
        public virtual ICollection<DataItem> DataItems { get; set; } = new List<DataItem>();
        public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
}