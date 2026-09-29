using System.Text.Json;
using VaricoseSocks.Web.Models;

namespace VaricoseSocks.Web.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        var baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7123/api";
        _httpClient.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    public async Task<List<CategoryViewModel>> GetCategoriesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("categories");
            if (!response.IsSuccessStatusCode) return new List<CategoryViewModel>();
            var stream = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<List<CategoryViewModel>>(stream, _jsonOptions) ?? new List<CategoryViewModel>();
        }
        catch
        {
            return new List<CategoryViewModel>();
        }
    }

    public async Task<List<ProductViewModel>> GetProductsAsync(int? categoryId = null, string? searchTerm = null)
    {
        try
        {
            var query = new List<string>();
            if (categoryId.HasValue) query.Add($"categoryId={categoryId.Value}");
            if (!string.IsNullOrWhiteSpace(searchTerm)) query.Add($"searchTerm={Uri.EscapeDataString(searchTerm)}");

            var url = "products" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return new List<ProductViewModel>();

            var stream = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<List<ProductViewModel>>(stream, _jsonOptions) ?? new List<ProductViewModel>();
        }
        catch
        {
            return new List<ProductViewModel>();
        }
    }

    public async Task<ProductViewModel?> GetProductByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"products/{id}");
            if (!response.IsSuccessStatusCode) return null;

            var stream = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<ProductViewModel>(stream, _jsonOptions);
        }
        catch
        {
            return null;
        }
    }
}