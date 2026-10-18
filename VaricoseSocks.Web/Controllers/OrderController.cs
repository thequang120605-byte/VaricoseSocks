using Microsoft.AspNetCore.Mvc;
using VaricoseSocks.Web.Models;
using VaricoseSocks.Web.Services;

namespace VaricoseSocks.Web.Controllers;

public class OrderController : Controller
{
    private readonly IApiService _apiService;

    public OrderController(IApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<IActionResult> Track(string? query)
    {
        var model = new OrderTrackingViewModel { SearchQuery = query };

        if (!string.IsNullOrWhiteSpace(query))
        {
            model.HasSearched = true;
            var order = await _apiService.GetOrderByCodeAsync(query.Trim());
            if (order != null)
            {
                model.Order = order;
            }
            else
            {
                model.ErrorMessage = "Không tìm thấy đơn hàng với mã \"" + query + "\". Vui lòng kiểm tra lại.";
            }
        }

        return View(model);
    }
}
