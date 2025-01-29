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
 
        // Use the named HttpClient to send the request
        var response = await _httpClient.PostAsync("api/User/Login", content);
 
        var responseContent = await response.Content.ReadAsStringAsync(); 
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
 
	if (response.IsSuccessStatusCode)
        {
		var token = result.GetProperty("token").GetString();
 
                // Store the token in session or cookies
                HttpContext.Session.SetString("AuthToken", token);
 
                // Redirect to a secure page
                return RedirectToAction("Index", "Home");
	}
	else
            {
                ViewBag.ErrorMessages = new List<string> { result.GetProperty("message").GetString() };
                return View();
            }
    }


    [HttpGet]
    public IActionResult Register()
    {
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


}