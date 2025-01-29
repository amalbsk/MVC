using Microsoft.AspNetCore.Mvc;
using Webapi.Models;
using Webapi.DTO;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using Microsoft.Extensions.Logging;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Ensures that only authenticated users can access these endpoints
public class InventoryController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<InventoryController> _logger;

    public InventoryController(
        AppDbContext context, 
        IMapper mapper,
        ILogger<InventoryController> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    // Get all items (authorized users only)
    [HttpGet("Display items")]
    public IActionResult GetItems()
    {
        _logger.LogInformation("Retrieving all inventory items");
        try
        {
            var items = _context.Inventories.ToList();
            _logger.LogInformation("Retrieved {Count} items", items.Count);
            return Ok(items);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error retrieving inventory items");
            return StatusCode(500, $"Internal server error: {exception.Message}");
        }
    }

    // Add a new item (authorized users only)
    [HttpPost("Add item")]
    public IActionResult AddItem([FromBody] AddItemDto itemDto)
    {
        _logger.LogInformation("Adding new inventory item: {ItemName}", itemDto.Name);
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
                
            var item = _mapper.Map<Inventory>(itemDto);
            _context.Inventories.Add(item);
            _context.SaveChanges();
            _logger.LogInformation("Successfully added item {ItemId}", item.ItemId);
            return CreatedAtAction(nameof(GetItems), new { id = item.ItemId }, item);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error adding inventory item");
            return StatusCode(500, $"Internal server error: {exception.Message}");
        }
    }

    // Update an existing item (authorized users only)
    [HttpPut("Upadte item{id}")]
    public IActionResult UpdateItem(int id, [FromBody] UpdateItemDto itemDto)
    {
        _logger.LogInformation("Attempting to update item with ID: {ItemId}", id);
        try
        {
            var existingItem = _context.Inventories.Find(id);
            if (existingItem == null)
            {
                _logger.LogWarning("Item with ID {ItemId} not found for update", id);
                return NotFound();
            }

            _mapper.Map(itemDto, existingItem);
            _context.SaveChanges();
            _logger.LogInformation("Successfully updated item {ItemId}", id);
            return Ok(existingItem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating item {ItemId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    // Get an item by ID (authorized users only)
    [HttpGet("Get by ID{id}")]
    public IActionResult GetItemById(int id)
    {
        _logger.LogInformation("Retrieving item with ID: {ItemId}", id);
        try
        {
            var item = _context.Inventories.Find(id);
            if (item == null)
            {
                _logger.LogWarning("Item with ID {ItemId} not found", id);
                return NotFound();
            }

            _logger.LogInformation("Successfully retrieved item {ItemId}", id);
            return Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving item {ItemId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    // Search items (authorized users only)
    [HttpGet("Search item")]
    public IActionResult SearchItems([FromQuery] string searchTerm)
    {
        _logger.LogInformation("Searching items with term: {SearchTerm}", searchTerm);
        try
        {
            var items = _context.Inventories
                .Where(i => i.Name.Contains(searchTerm))
                .ToList();

            _logger.LogInformation("Search completed. Found {Count} items", items.Count);
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching items with term {SearchTerm}", searchTerm);
            return StatusCode(500, "Internal server error");
        }
    }

    // Delete an item by ID (authorized users only)
    [HttpDelete("Delete{id}")]
    public IActionResult DeleteItem(int id)
    {
        _logger.LogInformation("Attempting to delete item with ID: {ItemId}", id);
        try
        {
            var item = _context.Inventories.Find(id);
            if (item == null)
            {
                _logger.LogWarning("Item with ID {ItemId} not found for deletion", id);
                return NotFound();
            }

            _context.Inventories.Remove(item);
            _context.SaveChanges();
            _logger.LogInformation("Successfully deleted item {ItemId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting item {ItemId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}
