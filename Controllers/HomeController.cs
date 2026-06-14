using Microsoft.AspNetCore.Mvc;
using PetHotel.Models;
using PetHotel.Services;

namespace PetHotel.Controllers;

public class HomeController : Controller
{
    private readonly DatabaseService _db;
    public HomeController(DatabaseService db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var vm = await _db.GetDashboardAsync();
        return View(vm);
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel vm)
    {
        vm.Owner.CreatedAt = DateTime.Now;
        vm.Pet.CreatedAt = DateTime.Now;

        var ownerId = await _db.CreateOwnerAsync(vm.Owner);
        vm.Pet.OwnerId = ownerId;
        await _db.CreatePetAsync(vm.Pet);
        
        TempData["Success"] = $"Welcome! {vm.Owner.FullName} and {vm.Pet.Name} are now registered.";
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Owners()
    {
        var owners = await _db.GetAllOwnersAsync();
        return View(owners);
    }

    public async Task<IActionResult> OwnerDetails(int id)
    {
        var owner = await _db.GetOwnerByIdAsync(id);
        if (owner == null) return NotFound();
        return View(owner);
    }

    public async Task<IActionResult> Pets()
    {
        var pets = await _db.GetAllPetsAsync();
        return View(pets);
    }

    public async Task<IActionResult> Rooms()
    {
        var rooms = await _db.GetAllRoomsAsync();
        return View(rooms);
    }
}