using Microsoft.EntityFrameworkCore;

namespace OrganizationPlatform.Helpers;

public static class PostgreSqlFunctions
{
    [DbFunction("unaccent", "public")]
    public static string Unaccent(string value)
    {
        throw new NotSupportedException();
    }
}