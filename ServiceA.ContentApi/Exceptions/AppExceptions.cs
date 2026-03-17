namespace ServiceA.ContentApi.Exceptions
{
    /// <summary>
    /// Thrown when a requested resource could not be found.
    /// Results in a 404 Not Found response.
    /// </summary>
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }

    /// <summary>
    /// Thrown when request input fails validation rules.
    /// Results in a 400 Bad Request response.
    /// </summary>
    
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
    }
}