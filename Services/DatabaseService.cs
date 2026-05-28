using Dapper;
using MySql.Data.MySqlClient;
using PetHotel.Models;

namespace PetHotel.Services;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection")!;
    }

    private MySqlConnection GetConnection() => new MySqlConnection(_connectionString);

    public async Task<HomeViewModel> GetDashboardAsync()
    {
        using var conn = GetConnection();
        var totalPets = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Pets");
        var totalBookings = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Bookings");
        var activeGuests = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Bookings WHERE Status IN ('Confirmed','Checked-In')");
        var availableRooms = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Rooms WHERE IsAvailable = 1");
        var recentBookings = (await conn.QueryAsync<Booking>(@"
            SELECT b.*, p.Name AS PetName, o.FullName AS OwnerName,
                   r.RoomNumber, r.RoomType
            FROM Bookings b
            JOIN Pets p ON b.PetId = p.Id
            JOIN Owners o ON p.OwnerId = o.Id
            JOIN Rooms r ON b.RoomId = r.Id
            ORDER BY b.CreatedAt DESC LIMIT 8")).ToList();
        return new HomeViewModel
        {
            TotalPets = totalPets,
            TotalBookings = totalBookings,
            ActiveGuests = activeGuests,
            AvailableRooms = availableRooms,
            RecentBookings = recentBookings
        };
    }

    public async Task<List<Owner>> GetAllOwnersAsync()
    {
        using var conn = GetConnection();
        return (await conn.QueryAsync<Owner>("SELECT * FROM Owners ORDER BY FullName")).ToList();
    }

    public async Task<Owner?> GetOwnerByIdAsync(int id)
    {
        using var conn = GetConnection();
        var owner = await conn.QueryFirstOrDefaultAsync<Owner>(
            "SELECT * FROM Owners WHERE Id = @Id", new { Id = id });
        if (owner != null)
            owner.Pets = (await conn.QueryAsync<Pet>(
                "SELECT * FROM Pets WHERE OwnerId = @OwnerId", new { OwnerId = id })).ToList();
        return owner;
    }

    public async Task<int> CreateOwnerAsync(Owner owner)
    {
        using var conn = GetConnection();
        return await conn.ExecuteScalarAsync<int>(@"
            INSERT INTO Owners (FullName, Email, Phone, Address, CreatedAt)
            VALUES (@FullName, @Email, @Phone, @Address, @CreatedAt);
            SELECT LAST_INSERT_ID();", owner);
    }

    public async Task<List<Pet>> GetAllPetsAsync()
    {
        using var conn = GetConnection();
        return (await conn.QueryAsync<Pet>(@"
            SELECT p.*, o.FullName AS OwnerName
            FROM Pets p JOIN Owners o ON p.OwnerId = o.Id
            ORDER BY p.Name")).ToList();
    }

    public async Task<int> CreatePetAsync(Pet pet)
    {
        using var conn = GetConnection();
        return await conn.ExecuteScalarAsync<int>(@"
            INSERT INTO Pets (OwnerId, Name, Species, Breed, Age, Weight, SpecialNotes)
            VALUES (@OwnerId, @Name, @Species, @Breed, @Age, @Weight, @SpecialNotes);
            SELECT LAST_INSERT_ID();", pet);
    }

    public async Task<List<Room>> GetAllRoomsAsync()
    {
        using var conn = GetConnection();
        return (await conn.QueryAsync<Room>("SELECT * FROM Rooms ORDER BY RoomNumber")).ToList();
    }

    public async Task<List<Room>> GetAvailableRoomsAsync(DateTime checkIn, DateTime checkOut)
    {
        using var conn = GetConnection();
        return (await conn.QueryAsync<Room>(@"
            SELECT * FROM Rooms WHERE IsAvailable = 1
            AND Id NOT IN (
                SELECT RoomId FROM Bookings
                WHERE Status NOT IN ('Cancelled','Completed')
                AND NOT (CheckOutDate <= @CheckIn OR CheckInDate >= @CheckOut)
            )
            ORDER BY PricePerNight", new { CheckIn = checkIn, CheckOut = checkOut })).ToList();
    }

    public async Task<List<Booking>> GetAllBookingsAsync()
    {
        using var conn = GetConnection();
        return (await conn.QueryAsync<Booking>(@"
            SELECT b.*, p.Name AS PetName, o.FullName AS OwnerName,
                   r.RoomNumber, r.RoomType
            FROM Bookings b
            JOIN Pets p ON b.PetId = p.Id
            JOIN Owners o ON p.OwnerId = o.Id
            JOIN Rooms r ON b.RoomId = r.Id
            ORDER BY b.CreatedAt DESC")).ToList();
    }

    public async Task<int> CreateBookingAsync(Booking booking, List<int> serviceIds)
    {
        using var conn = GetConnection();

        var room = await conn.QueryFirstOrDefaultAsync<Room>(
            "SELECT * FROM Rooms WHERE Id = @Id", new { Id = booking.RoomId });

        if (room == null)
            throw new InvalidOperationException($"Room with ID {booking.RoomId} was not found.");

        var nights = (booking.CheckOutDate - booking.CheckInDate).Days;
        decimal servicesTotal = 0;
        if (serviceIds.Any())
            servicesTotal = await conn.ExecuteScalarAsync<decimal>(
                $"SELECT SUM(Price) FROM Services WHERE Id IN ({string.Join(",", serviceIds)})");
        booking.TotalPrice = (room.PricePerNight * nights) + servicesTotal;
        var bookingId = await conn.ExecuteScalarAsync<int>(@"
            INSERT INTO Bookings (PetId, RoomId, CheckInDate, CheckOutDate, TotalPrice, Status, SpecialRequests)
            VALUES (@PetId, @RoomId, @CheckInDate, @CheckOutDate, @TotalPrice, 'Pending', @SpecialRequests);
            SELECT LAST_INSERT_ID();", booking);
        foreach (var sid in serviceIds)
            await conn.ExecuteAsync(
                "INSERT INTO BookingServices (BookingId, ServiceId) VALUES (@BookingId, @ServiceId)",
                new { BookingId = bookingId, ServiceId = sid });
        return bookingId;
    }

    public async Task UpdateBookingStatusAsync(int bookingId, string status)
    {
        using var conn = GetConnection();
        await conn.ExecuteAsync(
            "UPDATE Bookings SET Status = @Status WHERE Id = @Id",
            new { Id = bookingId, Status = status });
    }

    public async Task<List<Service>> GetAllServicesAsync()
    {
        using var conn = GetConnection();
        return (await conn.QueryAsync<Service>("SELECT * FROM Services")).ToList();
    }
}