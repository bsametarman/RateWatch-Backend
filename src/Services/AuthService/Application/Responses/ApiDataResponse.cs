namespace RateWatch.AuthService.Application.Responses
{
    public class ApiDataResponse<T> : ApiResponse
    {
        public T Data { get; set; }

        protected ApiDataResponse(bool success, string message, T data) : base(success, message)
        {
            Data = data;
        }

        protected ApiDataResponse(bool success, T data) : base(success)
        {
            Data = data;
        }

        public static ApiDataResponse<T> SuccessResponse(T data)
        {
            return new ApiDataResponse<T>(true, data);
        }

        public static ApiDataResponse<T> SuccessWithMessage(T data, string message)
        {
            return new ApiDataResponse<T>(true, message, data);
        }

        public static ApiDataResponse<T> Fail(T data)
        {
            return new ApiDataResponse<T>(false, data);
        }

        public static ApiDataResponse<T> FailWithMessage(T data, string message)
        {
            return new ApiDataResponse<T>(false, message, data);
        }
    }
}
