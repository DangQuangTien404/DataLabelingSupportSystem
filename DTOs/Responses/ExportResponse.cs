namespace DTOs.Responses
{
    public class ExportDataItemDto
    {
        public int Id { get; set; }
        public string StorageUrl { get; set; } = string.Empty;
        public string? MetaData { get; set; }
        public List<ExportAnnotationDto> Annotations { get; set; } = new List<ExportAnnotationDto>();
    }

    public class ExportAnnotationDto
    {
        public string Label { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
