namespace OutfitPlanner.Application.Common;

public class Result
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
    public string? Message { get; set; }

    public static Result Success() => new Result { IsSuccess = true };
    
    public static Result Success(string message) => new Result { IsSuccess = true, Message = message };
    
    public static Result Failure(string error) => new Result { IsSuccess = false, Error = error };
}
