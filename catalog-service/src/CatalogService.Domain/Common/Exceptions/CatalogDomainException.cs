using CatalogService.Domain.Errors;

namespace CatalogService.Domain.Common.Exceptions;

public sealed class CatalogDomainException : Exception
{
  public DomainError Error { get; }
  public string ErrorCode { get; }
  public CatalogDomainException(DomainError error)
      : base(error.Title)
  {
      Error = error;
      ErrorCode = error.Code;
  }
}