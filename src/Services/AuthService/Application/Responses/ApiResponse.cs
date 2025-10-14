namespace RateWatch.AuthService.Application.Responses
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        protected ApiResponse(bool success, string message)
        {
            Success = success;
            Message = message;
        }

        protected ApiResponse(bool success)
        {
            Success = success;
        }

        public static ApiResponse SuccessResponse(string message = "Successfully completed.")
        {
            return new ApiResponse(true, message);
        }

        public static ApiResponse FailResponse(string message = "Something went wrong.")
        {
            return new ApiResponse(false, message);
        }
    }
}
