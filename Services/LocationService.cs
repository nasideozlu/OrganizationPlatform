using System.Net.Http.Json;

namespace OrganizationPlatform.Services;

public class LocationService
{
    private readonly HttpClient _httpClient;

    public LocationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<LocationItem>> GetProvincesAsync()
    {
        var response = await _httpClient
            .GetFromJsonAsync<LocationApiResponse>(
                "provinces");

        return response?.Data ?? new List<LocationItem>();
    }


    public async Task<List<LocationItem>> GetDistrictsAsync(int provinceId)
    {
        var response = await _httpClient
            .GetFromJsonAsync<LocationApiResponse>(
                $"provinces/{provinceId}/districts?fields=id,name");

        return response?.Data ?? new List<LocationItem>();
    }

    public async Task<List<LocationItem>> GetNeighborhoodsAsync(int districtId)
    {
        var response = await _httpClient
            .GetFromJsonAsync<LocationApiResponse>(
                $"districts/{districtId}/neighborhoods?fields=id,name");

        return response?.Data ?? new List<LocationItem>();
    }

}

public class LocationApiResponse
{
    public List<LocationItem> Data { get; set; } = new();
}

public class LocationItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}