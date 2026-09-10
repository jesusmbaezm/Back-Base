namespace Services.DTOs.Shared
{
    public class ApiResponse<T>
    {
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = [];

        public static ApiResponse<T> Success(T data, string message = "") =>
            new() { Data = data, Message = message };

        public static ApiResponse<T> Failure(string message, List<string>? errors = null) =>
            new() { Message = message, Errors = errors ?? [] };
    }

    public class ApiResponse : ApiResponse<object>
    {
        public static ApiResponse SuccessEmpty(string message = "") =>
            new() { Message = message };
    }
}
