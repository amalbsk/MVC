var builder = WebApplication.CreateBuilder(args);
 
// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient("BackendAPI", client =>
{
    client.BaseAddress = new Uri("http://localhost:5283"); // Your backend API URL
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});
 
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
    options.Cookie.HttpOnly = true; // Restrict JavaScript access
    options.Cookie.IsEssential = true; // Ensure essential cookies
});
 
var app = builder.Build();
 
// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // Use error handler in production
    app.UseHsts(); // Enable HSTS for secure headers
}
 
app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS
app.UseStaticFiles(); // Serve static files like CSS, JS, images
app.UseRouting();
 
app.UseSession(); // Enable session middleware
app.UseAuthorization(); // Authorization middleware
 
// Configure the default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");
 
// Run the application
app.Run();