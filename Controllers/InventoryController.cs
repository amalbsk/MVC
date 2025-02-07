using Microsoft.AspNetCore.Mvc;
using Webapi.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Webapi.DTO;


public class InventoryController : Controller
{
    private readonly HttpClient _httpClient;
    public InventoryController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("BackendAPI");
    }
    private void AddAuthorizationHeader()
    {
        var token = HttpContext.Session.GetString("AuthToken");
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }



    public async Task<IActionResult> Dashboard()
    {
        if (!HttpContext.Session.TryGetValue("AuthToken", out _))
        {
            return RedirectToAction("Login", "Auth");
        }
        try
        {
            AddAuthorizationHeader();

            var response = await _httpClient.GetAsync("api/Inventory/Display items");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var inventoryList = JsonSerializer.Deserialize<List<Inventory>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(inventoryList);
            }
            else
            {
                TempData["Error"] = "Failed to fetch inventory items.";
                return RedirectToAction("Error");
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = "An error occurred while fetching inventory items.";
            return RedirectToAction("Error");
        }
    }

    public IActionResult AddNewItem()
    {
        if (!HttpContext.Session.TryGetValue("AuthToken", out _))
        {
            return RedirectToAction("Login", "Auth");
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AddNewItem(AddItemDto model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            AddAuthorizationHeader();

            var jsonContent = JsonSerializer.Serialize(model);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/Inventory/Add item", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Message"] = "Item added successfully.";
                return RedirectToAction("Dashboard");
            }
            else
            {
                TempData["Error"] = "Failed to add item.";
                return View(model);
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = "An error occurred while adding the item.";
            return View(model);
        }
    }


    public async Task<IActionResult> SearchAnItem(string searchTerm)
    {
        if (!HttpContext.Session.TryGetValue("AuthToken", out _))
        {
            return RedirectToAction("Login", "Auth");
        }
        try
        {
            AddAuthorizationHeader();

            var response = await _httpClient.GetAsync($"api/Inventory/Search item?searchTerm={searchTerm}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var inventoryList = JsonSerializer.Deserialize<List<Inventory>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return View(inventoryList);
            }
            else
            {
                TempData["Error"] = "Failed to fetch search results.";
                return RedirectToAction("Dashboard");
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = "An error occurred while searching for inventory items.";
            return RedirectToAction("Dashboard");
        }
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteItem(int id)
    {
        if (!HttpContext.Session.TryGetValue("AuthToken", out _))
        {
            return RedirectToAction("Login", "Auth");
        }
        try
        {
            AddAuthorizationHeader();

            var response = await _httpClient.DeleteAsync($"api/Inventory/Delete{id}");
            if (response.IsSuccessStatusCode)
            {
                return Json(new { success = true, message = "Item deleted successfully." });
            }
            else
            {
                return Json(new { success = false, message = "Failed to delete the item." });
            }
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "An error occurred while deleting the item." });
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateItem([FromBody] UpdateItemRequest request)
    {
        if (!HttpContext.Session.TryGetValue("AuthToken", out _))
        {
            return RedirectToAction("Login", "Auth");
        }
        try
        {
            AddAuthorizationHeader();

            var jsonContent = JsonSerializer.Serialize(request.Model);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"api/Inventory/UpdateItem/{request.Id}", content);

            if (response.IsSuccessStatusCode)
            {
                return Json(new { success = true, message = "Item updated successfully." });
            }
            else
            {
                return Json(new { success = false, message = "Failed to update the item." });
            }
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Error: {ex.Message}" });
        }
    }

    // DTO class for request
    public class UpdateItemRequest
    {
        public int Id { get; set; }
        public UpdateItemDto Model { get; set; } // The model containing name, quantity, price
    }



    [HttpGet]
    public async Task<IActionResult> GetItem(int id)
    {
        if (!HttpContext.Session.TryGetValue("AuthToken", out _))
        {
            return RedirectToAction("Login", "Auth");
        }
        try
        {
            AddAuthorizationHeader();

            var response = await _httpClient.GetAsync($"api/Inventory/getitem/{id}");
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var item = JsonSerializer.Deserialize<Inventory>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return Json(item);
            }
            else
            {
                return Json(new { success = false, message = "Failed to fetch item details." });
            }
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "An error occurred while fetching the item details." });
        }
    }




}
