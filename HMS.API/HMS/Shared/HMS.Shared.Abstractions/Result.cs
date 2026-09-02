namespace HMS.Shared.Abstractions;

/// <summary>
/// Merepresentasikan hasil operasi yang sukses atau gagal,
/// tanpa mengandalkan exception untuk alur kontrol.
/// </summary>
public readonly struct Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }

    private Result(bool isSuccess, string error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, string.Empty);

    public static Result Failure(string error) => new(false, error);

    public static implicit operator Result(string error) => Failure(error);

    public static implicit operator Result(bool success) => success ? Success() : Failure("Operation failed");
}

/// <summary>
/// Result generik yang membawa nilai <typeparamref name="T"/> beserta status sukses/gagal.
/// </summary>
public readonly struct Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }
    public T? Value { get; }

    private Result(bool isSuccess, T? value, string error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) => new(true, value, string.Empty);

    public static Result<T> Failure(string error) => new(false, default, error);

    public static implicit operator Result<T>(T value) => Success(value);

    public static implicit operator Result<T>(string error) => Failure(error);

    public Result<TOut> Map<TOut>(Func<T, TOut> mapper) =>
        IsSuccess ? Result<TOut>.Success(mapper(Value!)) : Result<TOut>.Failure(Error);
}