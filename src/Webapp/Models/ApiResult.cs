using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

public class ApiModel
{
    [JsonProperty]
    public ApiResult Result { get; set; }

    public ApiModel()
    {
    }
    public ApiModel(ApiResult result)
    {
        this.Result = result;
    }
    public ApiModel(Exception e, bool includeExceptions)
    {
        this.Result = ApiResult.AsException(e, includeExceptions);
    }
    public ApiModel(AggregateException e, bool includeExceptions)
    {
        this.Result = ApiResult.AsException(e, includeExceptions);
    }

    // Helpers
    public static ApiModel AsError(string errorField, string errorMessage)
    {
        return new ApiModel(ApiResult.AsError(errorField, errorMessage));
    }
    public static ApiModel AsError(string errorMessage, int errorCode = 0)
    {
        return new ApiModel(ApiResult.AsError(errorMessage, errorCode));
    }
    public static ApiModel AsSuccess(string message = null)
    {
        return new ApiModel(ApiResult.AsSuccess(message));
    }

    public static ApiModel AsException(Exception exception, bool includeExceptions = false)
    {
        return new ApiModel(ApiResult.AsException(exception, includeExceptions));
    }

    public static ApiModel AsException(AggregateException exception, bool includeExceptions = false)
    {
        return new ApiModel(ApiResult.AsException(exception, includeExceptions));
    }
}

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class ApiResult
{
    [JsonProperty] // For easy javascript handling
    public string Status { get { return Errors.Keys.Any() ? "Error" : "OK"; } }

    public bool Success { get { return !Errors.Keys.Any(); } }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string Message { get; set; }
    /// <summary>
    /// Dictionary key is the field having the error
    /// Value is a list of errors. We don't support errors caused by a combination of fields like the Nancy ModelResult
    /// </summary>
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public Dictionary<string, List<string>> Errors { get; set; }

    //[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    //public Exception Exception { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public int Code { get; set; }

    public ApiResult(string errorField, string errorMessage) : this()
    {
        this.Errors.Add(errorField, new List<string>(new[] { errorMessage }));
    }

    public ApiResult(int errorCode, string errorMessage) : this()
    {
        this.Code = Code;
        this.Message = errorMessage;
        this.Errors.Add("", new List<string>(new[] { errorMessage }));
    }

    public ApiResult()
    {
        this.Errors = new Dictionary<string, List<string>>();
        this.Code = 0;
    }

    // Helper methods
    public static ApiResult AsError(string errorField, string errorMessage)
    {
        return new ApiResult(errorField, errorMessage);
    }
    public static ApiResult AsError(string errorMessage, int errorCode = 0)
    {
        return new ApiResult(errorCode, errorMessage);
    }
    public static ApiResult AsSuccess(string message = null)
    {
        return new ApiResult { Message = message };
    }

    public static ApiResult AsException(Exception exception, bool includeExceptions = false)
    {
        ApiResult result;
        if (includeExceptions)
        {
            result = new ApiResult { Message = "Exception(s) occurred" };
            result.Errors.Add("Exceptions", new List<string>(new[] { exception.Message }));
        }
        else
        {
            result = ApiResult.AsError("Server Error");
        }
        return result;
    }

    public static ApiResult AsException(AggregateException exception, bool includeExceptions = false)
    {
        ApiResult result;
        if (includeExceptions)
        {
            result = new ApiResult { Message = "Exception(s) occurred" };
            result.Errors.Add("Exceptions", new List<string>(exception.InnerExceptions.Select(e => e.Message)));
        }
        else
        {
            result = ApiResult.AsError("Server Error");
        }
        return result;
    }
}
