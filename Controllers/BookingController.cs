using Microsoft.AspNetCore.Mvc;
using PetHotel.Models;
using PetHotel.Services;

namespace PetHotel.Controllers;

public class BookingController : Controller
{
    private readonly DatabaseService _db;
    public BookingController(DatabaseService db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var bookings = await _db.GetAllBookingsAsync();
        return View(bookings);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new BookingViewModel
        {
            Pets = await _db.GetAllPetsAsync(),
            Services = await _db.GetAllServicesAsync(),
            AvailableRooms = await _db.GetAvailableRoomsAsync(DateTime.Today, DateTime.Today.AddDays(1))
        };
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Create(BookingViewModel vm, List<int> selectedServiceIds)
    {
        if (vm.Booking.RoomId == 0)
        {
            vm.Pets = await _db.GetAllPetsAsync();
            vm.Services = await _db.GetAllServicesAsync();
            vm.AvailableRooms = await _db.GetAvailableRoomsAsync(DateTime.Today, DateTime.Today.AddDays(1));
            ModelState.AddModelError("", "Please select a room before confirming.");
            return View(vm);
        }

        vm.Booking.CheckInDate = vm.Booking.CheckInDate == default ? DateTime.Today : vm.Booking.CheckInDate;
        vm.Booking.CheckOutDate = vm.Booking.CheckOutDate == default ? DateTime.Today.AddDays(1) : vm.Booking.CheckOutDate;
        var bookingId = await _db.CreateBookingAsync(vm.Booking, selectedServiceIds);
        TempData["Success"] = $"Booking #{bookingId} created successfully!";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        await _db.UpdateBookingStatusAsync(id, status);
        TempData["Success"] = $"Booking status updated to {status}.";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> GetAvailableRooms(DateTime checkIn, DateTime checkOut)
    {
        var rooms = await _db.GetAvailableRoomsAsync(checkIn, checkOut);
        return Json(rooms);
    }
}