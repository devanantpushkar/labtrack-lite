namespace LabTrackApi.DTOs
{
    public class ChatbotRequest
    {
        public string Query { get; set; } = string.Empty;
    }

    public class ChatbotResponse
    {
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
        public string QueryType { get; set; } = string.Empty;
    }

    public class PaginatedResponse<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }
}
