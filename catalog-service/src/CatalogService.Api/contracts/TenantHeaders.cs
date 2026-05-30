using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CatalogService.Api.Contracts;

public static class TenantHeaders
{
  public const string TenantId = "X-Tenant-Id";
}