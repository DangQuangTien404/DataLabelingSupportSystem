namespace DTOs.Responses
{
    public class ProjectDetailResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal PricePerLabel { get; set; }
        public decimal TotalBudget { get; set; }
        public DateTime Deadline { get; set; }

        public string ManagerId { get; set; } = string.Empty;
        public string ManagerName { get; set; } = string.Empty;
        public string ManagerEmail { get; set; } = string.Empty;

        public List<LabelResponse> Labels { get; set; } = new List<LabelResponse>();
        public int TotalDataItems { get; set; }
        public int ProcessedItems { get; set; }
    }

    public class ProjectSummaryResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Deadline { get; set; }
        public string Status { get; set; } = "Active";
        public int TotalDataItems { get; set; }
        public decimal Progress { get; set; }
    }

    public class DataItemResponse
    {
        public int Id { get; set; }
        public string StorageUrl { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? MetaData { get; set; }
    }
}