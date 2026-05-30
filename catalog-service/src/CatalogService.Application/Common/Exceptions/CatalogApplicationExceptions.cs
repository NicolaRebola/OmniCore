using CatalogService.Application.Errors;

namespace CatalogService.Application.Common.Exceptions;

public sealed class CatalogApplicationException : Exception
{
  public ApplicationError Error { get; }
  public string ErrorCode { get; }
  public CatalogApplicationException(ApplicationError error)
      : base(error.Title)
  {
      Error = error;
      ErrorCode = error.Code;
  }
}