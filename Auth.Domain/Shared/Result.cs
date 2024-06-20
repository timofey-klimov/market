using Auth.Domain.Exceptions;

namespace Auth.Domain.Shared
{
    public interface IResult<out T>
    {
        public IReadOnlyCollection<string> Errors { get; }

        public T Value { get; }

        public IResult<T> Validate(Func<bool> predicate, string errorMessage);
    }

    public class NullResult<T> : IResult<T>
    {
        public IReadOnlyCollection<string> Errors => throw new InvalidOperationException();

        public T Value => throw new InvalidOperationException();

        public IResult<T> Validate(Func<bool> predicate, string errorMessage)
        {
            throw new InvalidOperationException();
        }

        public static NullResult<T> Create() => new NullResult<T>();
    }

    public class Result<T> : IResult<T>
    {
        private readonly Func<T> _creator;
        private T _value;
        private List<string> _errors = new List<string>();
        public IReadOnlyCollection<string> Errors => _errors.AsReadOnly();

        public T Value
        {
            get
            {
                if (Errors.Any())
                    throw new InvalidOperationException("Cannot access Result Value with errors");
                return _value ??= _creator();
            }
        }

        private Result(Func<T> creator)
        {
            _creator = creator ?? throw new ArgumentNullException(nameof(creator));
        }

        private Result(string error)
        {
            _errors.Add(error);
        }

        private Result(T value)
            :this(() => value)
        {

        }

        public static Result<T> Bind(Func<T> creator)
        {
            return new Result<T>(creator);
        }

        public static Result<T> FromValue(T value) => new Result<T>(value);

        public IResult<T> Validate(Func<bool> predicate, string errorMessage)
        {
            if (predicate())
                return this;
            _errors.Add(errorMessage);
            return this;
        }

        public static Result<T> FromErrors(string error) => new Result<T>(error);
    }

    public static class ResultExtensions
    {
        public static void Handle<T>(this IResult<T> result, Func<IEnumerable<string>, Exception> handleError = null, Func<Exception> handleNull = null)
        {
            if (result is Result<T> &&  result.Errors.Any() && handleError is not null)
                throw handleError(result.Errors);
            if (result is NullResult<T> && handleNull is not null)
                throw handleNull();
        }

        public static bool IsNullResult<T>(this IResult<T> result) => result is NullResult<T>;
    }
}
