using Microsoft.AspNetCore.Http; // <-- Import thư viện này (hết báo lỗi)
using System.ComponentModel.DataAnnotations;

namespace DTOs.Requests
{
    public class CreateProjectRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal PricePerLabel { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TotalBudget { get; set; }

        public DateTime Deadline { get; set; }
    }

    public class UpdateProjectRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal PricePerLabel { get; set; }
        public decimal TotalBudget { get; set; }
        public DateTime Deadline { get; set; }
    }

    public class ImportDataRequest
    {
        [Required]
        public List<string> StorageUrls { get; set; } = new List<string>();
    }
    public class UploadDataRequest
    {
        [Required]
        public List<IFormFile> Files { get; set; } = new List<IFormFile>();
    }
}