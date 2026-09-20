namespace Comercio.Api.Service.Results
{
    public enum ErrorType
    {
        Ninguno,
        NotFound,
        Conflict,
        Validation,
        InvalidCredentials
    }
    public class Result
    {
        public bool IsSuccess { get; }
        public string? ErrorMessage { get; }
        public ErrorType ErrorType { get; set; }

        protected Result(bool isSuccess, string? errorMessage, ErrorType errorType)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
            ErrorType = errorType;
        }

        public static Result Success() => new Result(true, null, ErrorType.Ninguno);
        public static Result Failure(string errorMessage, ErrorType errorType)
            => new Result(false, errorMessage, errorType);
    }

    public class Result<T> : Result
    {
        public T? Data { get; }

        private Result(bool isSuccess, string? errorMessage, ErrorType errorType, T? data) : base(isSuccess, errorMessage, errorType)
        {
            Data = data;
        }

        public static Result<T> Success(T data) => new Result<T>(true, null, ErrorType.Ninguno, data);
        public new static Result<T> Failure(string errorMessage, ErrorType errorType)
            => new Result<T>(false, errorMessage, errorType, default);
    }
}
