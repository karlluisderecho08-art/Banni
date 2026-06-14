namespace PetHotel.Models;

public class Owner
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now; 
    public List<Pet> Pets { get; set; } = new();
}

public class Pet
{
    public int Id { get; set; }
    public int OwnerId { get; set; }
    public string Name { get; set; } = "";
    public string Species { get; set; } = "";
    public string? Breed { get; set; }
    public int? Age { get; set; }
    public decimal? Weight { get; set; }
    public string? SpecialNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public Owner? Owner { get; set; }
}

public class Room
{
    public int Id { get; set; }
    public string RoomNumber { get; set; } = "";
    public string RoomType { get; set; } = "";
    public string PetType { get; set; } = "";
    public decimal PricePerNight { get; set; }
    public bool IsAvailable { get; set; }
    public string? Description { get; set; }
}

public class Booking
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public int RoomId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = "Pending";
    public string? SpecialRequests { get; set; }
    public DateTime CreatedAt { get; set; }
    public string PetName { get; set; } = "";
    public string OwnerName { get; set; } = "";
    public string RoomNumber { get; set; } = "";
    public string RoomType { get; set; } = "";
    public int Nights => (CheckOutDate - CheckInDate).Days;
}

public class Service
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public decimal Price { get; set; }
}

public class BookingViewModel
{
    public Booking Booking { get; set; } = new();
    public List<Pet> Pets { get; set; } = new();
    public List<Room> AvailableRooms { get; set; } = new();
    public List<Service> Services { get; set; } = new();
    public List<int> SelectedServiceIds { get; set; } = new();
}

public class HomeViewModel
{
    public int TotalPets { get; set; }
    public int TotalBookings { get; set; }
    public int ActiveGuests { get; set; }
    public int AvailableRooms { get; set; }
    public List<Booking> RecentBookings { get; set; } = new();
}

public class RegisterViewModel
{
    public Owner Owner { get; set; } = new();
    public Pet Pet { get; set; } = new();
}