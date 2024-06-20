namespace Auth.Domain
{
    public static class PrimitiveExtensions
    {
        public static bool IsEmpty(this string? value) => string.IsNullOrEmpty(value);

        public static bool IsNotEmpty(this string value) => !IsEmpty(value);

        public static bool NotNull(this object value) => value != null;

        public static bool NotEmpty(this Guid guid) => guid != Guid.Empty;
    }
}
