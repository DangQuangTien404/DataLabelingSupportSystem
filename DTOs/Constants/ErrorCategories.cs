namespace DTOs.Constants
{
    public static class ErrorCategories
    {

        public const string LU01_IncorrectLabel = "LU-01: Sai định nghĩa nhãn (Gán xe Buýt thành Xe con)";
        public const string LU02_Confusion = "LU-02: Nhầm lẫn nhãn (Xe máy vs Xe đạp điện)";

        public const string TE01_WrongBox = "TE-01: Sai vùng gán nhãn (Box lệch hẳn đối tượng)";
        public const string TE02_LooseBox = "TE-02: Box quá rộng (Dư nhiều nền)";
        public const string TE03_TightBox = "TE-03: Box quá hẹp (Cắt mất chi tiết xe)";
        public const string TE04_OcclusionError = "TE-04: Xử lý che khuất sai (Vẽ cả phần bị che)";

        public const string ME01_MissingObject = "ME-01: Bỏ sót đối tượng (Thiếu nhãn)";
        public const string ME02_ExtraLabel = "ME-02: Gán thừa (Vẽ vào khoảng trống/rác)";

        public const string PR01_ProcessError = "PR-01: Lỗi quy trình (Chưa hoàn thành hết ảnh)";
        public const string Other = "Other: Lỗi khác";

        public static readonly List<string> All = new()
        {
            LU01_IncorrectLabel, LU02_Confusion,
            TE01_WrongBox, TE02_LooseBox, TE03_TightBox, TE04_OcclusionError,
            ME01_MissingObject, ME02_ExtraLabel,
            PR01_ProcessError, Other
        };

        public static bool IsValid(string category) => All.Contains(category);

        public static int GetSeverityWeight(string category)
        {
            if (category.StartsWith("LU-01") || category.StartsWith("ME-01") ||
                category.StartsWith("TE-01") || category.StartsWith("PR-01"))
                return 10;

            if (category.StartsWith("TE-02") || category.StartsWith("TE-03") ||
                category.StartsWith("TE-04") || category.StartsWith("LU-02"))
                return 5;

            return 2;
        }
    }
}