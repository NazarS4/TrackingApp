using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TrackingApp.Server.Models;
using TrackingApp.Server.Services;
using TrackingApp.Server.Config;

namespace TrackingApp.Server.TcpServer
{
    public class FirebaseTcpServer
    {
        private readonly TcpListener _listener;
        private readonly FirebaseService _firebaseService;
        private bool _isRunning;

        public FirebaseTcpServer()
        {
            _listener = new TcpListener(IPAddress.Any, ServerConfig.Port);
            _firebaseService = new FirebaseService();
        }

        public void Start()
        {
            _isRunning = true;
            _listener.Start();

            _ = Task.Run(async () =>
            {
                await Task.Delay(500);
                await _firebaseService.InitializeDefaultTripsAsync();
            });

            Task.Run(ListenForClients);
        }

        public void Stop()
        {
            _isRunning = false;
            _listener?.Stop();
        }

        private async Task ListenForClients()
        {
            while (_isRunning)
            {
                try
                {
                    var client = await _listener.AcceptTcpClientAsync();
                    _ = Task.Run(() => HandleClient(client));
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    if (_isRunning)
                        Console.WriteLine($"❌ {ex.Message}");
                }
            }
        }

        private async Task HandleClient(TcpClient client)
        {
            try
            {
                using (client)
                using (var stream = client.GetStream())
                {
                    var buffer = new byte[ServerConfig.BufferSize];
                    var messageBuilder = new StringBuilder();

                    while (client.Connected && _isRunning)
                    {
                        var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                        if (bytesRead == 0) break;

                        var receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        messageBuilder.Append(receivedData);

                        var message = messageBuilder.ToString();

                        if (message.Contains("}"))
                        {
                            try
                            {
                                var cleanMessage = message.Trim();
                                cleanMessage = cleanMessage.TrimStart('\0', '\uFEFF', '\u200B');

                                if (!string.IsNullOrWhiteSpace(cleanMessage) &&
                                    cleanMessage.StartsWith("{") && cleanMessage.EndsWith("}"))
                                {
                                    var response = await ProcessRequest(cleanMessage);
                                    var responseJson = JsonSerializer.Serialize(response, new JsonSerializerOptions
                                    {
                                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                                    }) + "\n";

                                    var responseBytes = Encoding.UTF8.GetBytes(responseJson);
                                    await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
                                }

                                messageBuilder.Clear();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"❌ {ex.Message}");
                                messageBuilder.Clear();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ {ex.Message}");
            }
            finally
            {
                Console.WriteLine("────────────────────────────────────────────────────────────");
            }
        }

        private async Task<ApiResponse<object>> ProcessRequest(string requestJson)
        {
            try
            {
                var request = JsonSerializer.Deserialize<ApiRequest>(requestJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (request == null)
                    return ApiResponse<object>.ErrorResponse("Невірний формат запиту");

                Console.WriteLine($"🔧 {request.Command}");

                var response = request.Command?.ToLower() switch
                {
                    "ping" => await HandlePing(),
                    "get_users" => await HandleGetUsers(),
                    "get_active_trip" => await HandleGetActiveTrip(),
                    "get_notifications" => await HandleGetNotifications(),
                    "update_location" => await HandleUpdateLocation(request.Data),
                    "add_notification" => await HandleAddNotification(request.Data),
                    "get_statistics" => await HandleGetStatistics(),
                    "register_user" => await HandleRegisterUser(request.Data),
                    "add_feedback" => await HandleAddFeedback(request.Data),
                    "get_feedbacks" => await HandleGetFeedbacks(),
                    "get_predefined_trips" => await HandleGetPredefinedTrips(),
                    "set_active_trip" => await HandleSetActiveTrip(request.Data),
                    "update_trip_plan" => await HandleUpdateTripPlan(request.Data),
                    "save_predefined_trip" => await HandleSavePredefinedTrip(request.Data),
                    _ => ApiResponse<object>.ErrorResponse($"Невідома команда: {request.Command}")
                };

                // Додаємо роздільник після важливих команд
                if (request.Command?.ToLower() is "add_feedback" or "set_active_trip" or "update_trip_plan" or "save_predefined_trip")
                {
                    Console.WriteLine("────────────────────────────────────────────────────────────");
                }

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ {ex.Message}");
                return ApiResponse<object>.ErrorResponse($"Помилка: {ex.Message}");
            }
        }

        private async Task<ApiResponse<object>> HandlePing()
        {
            try
            {
                var testData = new
                {
                    message = "Сервер працює коректно",
                    timestamp = DateTime.UtcNow,
                    serverVersion = "1.0",
                    firebaseConnected = true
                };

                return ApiResponse<object>.SuccessResponse(testData, "Ping успішний");
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.ErrorResponse($"Помилка: {ex.Message}");
            }
        }

        private async Task<ApiResponse<object>> HandleGetUsers()
        {
            try
            {
                var users = await _firebaseService.GetUsersAsync();
                return ApiResponse<object>.SuccessResponse(users, $"Отримано {users.Count} користувачів");
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.ErrorResponse($"Помилка: {ex.Message}");
            }
        }

        private async Task<ApiResponse<object>> HandleGetActiveTrip()
        {
            try
            {
                var trip = await _firebaseService.GetActiveTripAsync();
                return ApiResponse<object>.SuccessResponse(trip, "Активний маршрут отримано");
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.ErrorResponse($"Помилка: {ex.Message}");
            }
        }

        private async Task<ApiResponse<object>> HandleGetNotifications()
        {
            try
            {
                var notifications = await _firebaseService.GetNotificationsAsync();
                return ApiResponse<object>.SuccessResponse(notifications, $"Отримано {notifications.Count} сповіщень");
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.ErrorResponse($"Помилка: {ex.Message}");
            }
        }

        private async Task<ApiResponse<object>> HandleUpdateLocation(object data)
        {
            try
            {
                var locationData = JsonSerializer.Deserialize<LocationUpdateData>(
                    JsonSerializer.Serialize(data),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (locationData == null || string.IsNullOrEmpty(locationData.UserId))
                    return ApiResponse<object>.ErrorResponse("Невірні дані локації");

                var success = await _firebaseService.UpdateUserLocationAsync(
                    locationData.UserId,
                    locationData.Location);

                return success
                    ? ApiResponse<object>.SuccessResponse(true, "Локація оновлена")
                    : ApiResponse<object>.ErrorResponse("Помилка оновлення");
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.ErrorResponse($"Помилка: {ex.Message}");
            }
        }

        private async Task<ApiResponse<object>> HandleAddNotification(object data)
        {
            try
            {
                var notificationData = JsonSerializer.Deserialize<NotificationData>(
                    JsonSerializer.Serialize(data),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (notificationData == null || string.IsNullOrEmpty(notificationData.Message))
                    return ApiResponse<object>.ErrorResponse("Невірні дані сповіщення");

                var notification = new Notification
                {
                    Message = notificationData.Message,
                    UserId = notificationData.UserId,
                    UserName = notificationData.UserName,
                    Time = DateTime.UtcNow
                };

                var success = await _firebaseService.AddNotificationAsync(notification);

                return success
                    ? ApiResponse<object>.SuccessResponse(true, "Сповіщення додано")
                    : ApiResponse<object>.ErrorResponse("Помилка додавання");
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.ErrorResponse($"Помилка: {ex.Message}");
            }
        }

        private async Task<ApiResponse<object>> HandleGetStatistics()
        {
            try
            {
                var statistics = await _firebaseService.GetStatisticsAsync();
                return ApiResponse<object>.SuccessResponse(statistics, "Статистика отримана");
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.ErrorResponse($"Помилка: {ex.Message}");
            }
        }

        private async Task<ApiResponse<object>> HandleRegisterUser(object data)
        {
            try
            {
                var userData = JsonSerializer.Deserialize<UserRegistrationData>(
                    JsonSerializer.Serialize(data),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (userData == null || string.IsNullOrEmpty(userData.Name) || string.IsNullOrEmpty(userData.Password))
                    return ApiResponse<object>.ErrorResponse("Невірні дані реєстрації");

                var existingUsers = await _firebaseService.GetUsersAsync();
                if (existingUsers.Any(u => u.Name == userData.Name))
                {
                    return ApiResponse<object>.ErrorResponse("Користувач вже існує");
                }

                var newUser = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = userData.Name,
                    Email = userData.Email,
                    Password = userData.Password,
                    IsLeader = userData.IsLeader,
                    TripType = userData.TripType,
                    CurrentLocation = new Location(49.842957, 24.031111),
                    Status = UserStatus.None,
                    LastUpdated = DateTime.UtcNow
                };

                var success = await _firebaseService.CreateUserAsync(newUser);

                if (success)
                {
                    await _firebaseService.AddNotificationAsync(new Notification
                    {
                        Message = $"Новий користувач: {newUser.Name}",
                        UserId = newUser.Id,
                        UserName = "System",
                        Time = DateTime.UtcNow
                    });

                    return ApiResponse<object>.SuccessResponse(newUser, "Користувача зареєстровано");
                }
                else
                {
                    return ApiResponse<object>.ErrorResponse("Помилка реєстрації");
                }
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.ErrorResponse($"Помилка: {ex.Message}");
            }
        }

        private async Task<ApiResponse<object>> HandleAddFeedback(object data)
        {
            try
            {
                var feedbackData = JsonSerializer.Deserialize<FeedbackData>(
                    JsonSerializer.Serialize(data),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (feedbackData == null || string.IsNullOrEmpty(feedbackData.Message))
                    return ApiResponse<object>.ErrorResponse("Невірні дані відгуку");

                var feedback = new Feedback
                {
                    UserId = feedbackData.UserId,
                    UserName = feedbackData.UserName,
                    Message = feedbackData.Message,
                    Rating = feedbackData.Rating,
                    TripName = feedbackData.TripName ?? "Невідома подорож"
                };

                var success = await _firebaseService.AddFeedbackAsync(feedback);

                Console.WriteLine($"⭐ {feedbackData.UserName} ({feedbackData.Rating}/5)");

                return success
                    ? ApiResponse<object>.SuccessResponse(true, "Відгук додано")
                    : ApiResponse<object>.ErrorResponse("Помилка додавання відгуку");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Помилка відгуку: {ex.Message}");
                return ApiResponse<object>.ErrorResponse($"Помилка: {ex.Message}");
            }
        }

        private async Task<ApiResponse<object>> HandleGetFeedbacks()
        {
            try
            {
                var feedbacks = await _firebaseService.GetFeedbacksAsync();
                return ApiResponse<object>.SuccessResponse(feedbacks, $"Отримано {feedbacks.Count} відгуків");
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.ErrorResponse($"Помилка: {ex.Message}");
            }
        }

        private async Task<ApiResponse<object>> HandleGetPredefinedTrips()
        {
            try
            {
                var trips = await _firebaseService.GetPredefinedTripsAsync();
                return ApiResponse<object>.SuccessResponse(trips, $"Отримано {trips.Count} маршрутів");
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.ErrorResponse($"Помилка: {ex.Message}");
            }
        }

        private async Task<ApiResponse<object>> HandleSetActiveTrip(object data)
        {
            try
            {
                var tripData = JsonSerializer.Deserialize<ActiveTripData>(
                    JsonSerializer.Serialize(data),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (tripData == null || string.IsNullOrEmpty(tripData.TripId))
                    return ApiResponse<object>.ErrorResponse("Невірні дані маршруту");

                var success = await _firebaseService.SetActiveTripAsync(tripData.TripId);

                if (success)
                {
                    var trips = await _firebaseService.GetPredefinedTripsAsync();
                    var trip = trips.FirstOrDefault(t => t.Id == tripData.TripId);
                    if (trip != null)
                    {
                        Console.WriteLine($"🔄 Активний маршрут: {trip.Name}");
                    }
                }

                return success
                    ? ApiResponse<object>.SuccessResponse(true, "Активний маршрут встановлено")
                    : ApiResponse<object>.ErrorResponse("Помилка встановлення маршруту");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Помилка маршруту: {ex.Message}");
                return ApiResponse<object>.ErrorResponse($"Помилка: {ex.Message}");
            }
        }

        private async Task<ApiResponse<object>> HandleUpdateTripPlan(object data)
        {
            try
            {
                var planData = JsonSerializer.Deserialize<TripPlanData>(
                    JsonSerializer.Serialize(data),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (planData == null || string.IsNullOrEmpty(planData.TripId))
                    return ApiResponse<object>.ErrorResponse("Невірні дані плану подорожі");

                var tripPlan = new TripPlan
                {
                    TripId = planData.TripId,
                    Duration = planData.Duration,
                    BreakSchedule = planData.BreakSchedule,
                    RestPlaces = planData.RestPlaces ?? new List<string>(),
                    PointsOfInterest = planData.PointsOfInterest ?? new List<string>(),
                    UpdatedBy = planData.UpdatedBy
                };

                var success = await _firebaseService.SaveTripPlanAsync(tripPlan);

                if (success)
                {
                    Console.WriteLine($"📝 Оновлено план: {planData.UpdatedBy}");
                }

                return success
                    ? ApiResponse<object>.SuccessResponse(true, "План подорожі оновлено")
                    : ApiResponse<object>.ErrorResponse("Помилка оновлення плану подорожі");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Помилка плану: {ex.Message}");
                return ApiResponse<object>.ErrorResponse($"Помилка: {ex.Message}");
            }
        }

        private async Task<ApiResponse<object>> HandleSavePredefinedTrip(object data)
        {
            try
            {
                var tripData = JsonSerializer.Deserialize<PredefinedTrip>(
                    JsonSerializer.Serialize(data),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (tripData == null || string.IsNullOrEmpty(tripData.Name))
                    return ApiResponse<object>.ErrorResponse("Невірні дані маршруту");

                var success = await _firebaseService.SavePredefinedTripAsync(tripData);

                if (success)
                {
                    Console.WriteLine($"💾 Новий маршрут: {tripData.Name}");
                }

                return success
                    ? ApiResponse<object>.SuccessResponse(true, "Маршрут збережено")
                    : ApiResponse<object>.ErrorResponse("Помилка збереження маршруту");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Помилка збереження: {ex.Message}");
                return ApiResponse<object>.ErrorResponse($"Помилка: {ex.Message}");
            }
        }
    }

    // Моделі для запитів
    public class TripPlanData
    {
        public string TripId { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public string BreakSchedule { get; set; } = string.Empty;
        public List<string> RestPlaces { get; set; } = new List<string>();
        public List<string> PointsOfInterest { get; set; } = new List<string>();
        public string UpdatedBy { get; set; } = string.Empty;
    }

    public class ApiRequest
    {
        public string Command { get; set; }
        public object Data { get; set; }
    }

    public class LocationUpdateData
    {
        public string UserId { get; set; }
        public Location Location { get; set; }
    }

    public class NotificationData
    {
        public string Message { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
    }

    public class UserRegistrationData
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsLeader { get; set; }
        public TripType TripType { get; set; }
    }

    public class FeedbackData
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Message { get; set; }
        public int Rating { get; set; }
        public string TripName { get; set; }
    }

    public class ActiveTripData
    {
        public string TripId { get; set; } = string.Empty;
    }
}