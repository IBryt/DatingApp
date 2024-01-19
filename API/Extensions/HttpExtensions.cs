using API.Helpers;
using System.Text.Json;

namespace API.Extensions;

public static class HttpExtensions
{
    public static void AddPaginationHeeader(
        this HttpResponse response,
        int currentPage,
        int itemsPerPage,
        int totalItems,
        int totalPages)
    {
        var paginationHeader = new PaginationHeader(currentPage, itemsPerPage, totalItems, totalPages);
        response.Headers.Add("Pagination", JsonSerializer.Serialize(paginationHeader));
        response.Headers.Add("Access-Control-Expose-Headers", "Pagination");
    }
}
