namespace Lisere.StockApi.Application.Exceptions;

/// <summary>
/// Exception métier levée par le StockApi : quantité négative, article introuvable, etc.
/// Traduite en ProblemDetails (400) par l'ExceptionHandlingMiddleware.
/// </summary>
public class StockException : Exception
{
    public StockException(string message) : base(message)
    {
    }

    public StockException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
