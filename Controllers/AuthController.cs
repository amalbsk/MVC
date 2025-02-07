using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using InventoryManagementApp.Models;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace InventoryManagementApp.Controllers;

public class AuthController : Controller
{
    private readonly HttpClient _httpClient;

    // Updated Constructor to inject the named HttpClient
    public AuthController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("BackendAPI");
    }
    public IActionResult Login()
    {
        if (HttpContext.Session.TryGetValue("AuthToken", out _))
        {
            return RedirectToAction("Dashboard", "Inventory");
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string username, string password)
    {
        var loginDto = new
        {
            Username = username,
            Password = password
        };

        var content = new StringContent(JsonSerializer.Serialize(loginDto), Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync("api/User/Login", content);
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseContent);

            if (response.IsSuccessStatusCode && result.TryGetProperty("token", out JsonElement tokenElement))
            {
                var token = tokenElement.GetString();
                if (token != null)
                {
                    HttpContext.Session.SetString("AuthToken", token);
                    return RedirectToAction("Dashboard", "Inventory");
                }
            }

            // Handle error message
            string errorMessage = "No such user exists";
            if (result.TryGetProperty("message", out JsonElement messageElement))
            {
                errorMessage = messageElement.GetString() ?? errorMessage;
            }
            
            TempData["ErrorMessage"] = errorMessage;
            return View();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "An error occurred during login. Please try again.";
            return View();
        }
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (HttpContext.Session.TryGetValue("AuthToken", out _))
        {
            return RedirectToAction("Dashboard", "Inventory");
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(string username, string password)
    {
        var registerDto = new
        {
            Username = username,
            Password = password            
        };

        var content = new StringContent(JsonSerializer.Serialize(registerDto), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("api/User/Register", content);

        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);

        if (response.IsSuccessStatusCode)
        {
            // Redirect to Login page after successful registration
            return RedirectToAction("Login", "Auth");
        }
        else
        {
            // Show error messages
            ViewBag.ErrorMessages = new List<string> { result.GetProperty("message").GetString() };
            return View();
        }
    }

    // New Logout Action
    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Remove("AuthToken");
        return RedirectToAction("Login", "Auth");
    }
}
