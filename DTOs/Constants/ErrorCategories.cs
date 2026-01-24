namespace DTOs.Constants
{
    public static class ErrorCategories
    {
        public const string IncorrectLabel = "Incorrect Label";
        public const string DrawingMisaligned = "Drawing Misaligned";
        public const string MissingObject = "Missing Object";
        public const string Occluded = "Occluded";
        public const string Other = "Other";

        public static readonly List<string> All = new()
        {
            IncorrectLabel,
            DrawingMisaligned,
            MissingObject,
            Occluded,
            Other
        };

        public static bool IsValid(string category)
        {
            return All.Contains(category);
        }
    }
}
