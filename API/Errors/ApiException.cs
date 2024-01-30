namespace API.Errors;

public class ApiException : Exception
{
    public ApiException(int statusCode, string message, string details)
    {
        StatusCode = statusCode;
        Message = message;
        this.details = details;
    }

    public int StatusCode { get; set; }
    public override string Message { get; }
    public string details { get; set; }
}
