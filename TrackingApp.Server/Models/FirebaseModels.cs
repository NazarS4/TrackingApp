using System.Text.Json.Serialization;

namespace TrackingApp.Server.Models
{
    public class Location
    {
        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        public Location() { }

        public Location(double lat, double lon)
        {
            Latitude = lat;
            Longitude = lon;
        }

        public override string ToString() => $"{Latitude:F5}, {Longitude:F5}";
    }

    public class User
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;

        [JsonPropertyName("currentLocation")]
        public Location CurrentLocation { get; set; } = new Location();

        [JsonPropertyName("isLeader")]
        public bool IsLeader { get; set; }

        [JsonPropertyName("status")]
        public UserStatus Status { get; set; }

        [JsonPropertyName("tripType")]
        public TripType TripType { get; set; }

        [JsonPropertyName("lastUpdated")]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    public enum UserStatus
    {
        None = 0,
        Help = 1,
        Stop = 2
    }

    public enum TripType
    {
        Walking = 0,
        Cycling = 1,
        Excursion = 2
    }

    public class Trip
    {
        [JsonPropertyName("tripName")]
        public string TripName { get; set; } = "Trip";

        [JsonPropertyName("route")]
        public List<Location> Route { get; set; } = new List<Location>();

        [JsonPropertyName("duration")]
        public string Duration { get; set; } = "3 години";

        [JsonPropertyName("breakSchedule")]
        public string BreakSchedule { get; set; } = "Перерва з 12:00 до 13:00";

        [JsonPropertyName("restPlaces")]
        public List<string> RestPlaces { get; set; } = new List<string>();

        [JsonPropertyName("pointsOfInterest")]
        public List<string> PointsOfInterest { get; set; } = new List<string>();

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; } = true;

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class PredefinedTrip
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("route")]
        public List<Location> Route { get; set; } = new List<Location>();

        [JsonPropertyName("duration")]
        public string Duration { get; set; } = string.Empty;

        [JsonPropertyName("breakSchedule")]
        public string BreakSchedule { get; set; } = string.Empty;

        [JsonPropertyName("restPlaces")]
        public List<string> RestPlaces { get; set; } = new List<string>();

        [JsonPropertyName("pointsOfInterest")]
        public List<string> PointsOfInterest { get; set; } = new List<string>();

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }

        [JsonPropertyName("isPredefined")]
        public bool IsPredefined { get; set; } = true;
    }

    public class Notification
    {
        [JsonPropertyName("time")]
        public DateTime Time { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("userName")]
        public string UserName { get; set; } = string.Empty;

        [JsonPropertyName("isRead")]
        public bool IsRead { get; set; }
    }

    public class Feedback
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("userName")]
        public string UserName { get; set; } = string.Empty;

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("rating")]
        public int Rating { get; set; } = 5;

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("tripName")]
        public string TripName { get; set; } = string.Empty;
    }

    // ДОДАНО: Клас для плану подорожі
    public class TripPlan
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("tripId")]
        public string TripId { get; set; } = string.Empty;

        [JsonPropertyName("duration")]
        public string Duration { get; set; } = string.Empty;

        [JsonPropertyName("breakSchedule")]
        public string BreakSchedule { get; set; } = string.Empty;

        [JsonPropertyName("restPlaces")]
        public List<string> RestPlaces { get; set; } = new List<string>();

        [JsonPropertyName("pointsOfInterest")]
        public List<string> PointsOfInterest { get; set; } = new List<string>();

        [JsonPropertyName("lastUpdated")]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("updatedBy")]
        public string UpdatedBy { get; set; } = string.Empty;
    }

    public class ApiResponse<T>
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public T Data { get; set; }

        [JsonPropertyName("errorCode")]
        public string ErrorCode { get; set; } = string.Empty;

        public static ApiResponse<T> SuccessResponse(T data, string message = "Success")
            => new ApiResponse<T> { Success = true, Message = message, Data = data };

        public static ApiResponse<T> ErrorResponse(string message, string errorCode = "")
            => new ApiResponse<T> { Success = false, Message = message, ErrorCode = errorCode };
    }

    public class ActiveTripData
    {
        [JsonPropertyName("tripId")]
        public string TripId { get; set; } = string.Empty;
    }
}