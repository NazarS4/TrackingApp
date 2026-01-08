using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrackingApp.Server.Config;
using TrackingApp.Server.Models;

namespace TrackingApp.Server.Services
{
    public class FirebaseService
    {
        private readonly FirebaseClient _firebase;

        public FirebaseService()
        {
            _firebase = FirebaseConfig.GetClient();
        }

        public async Task InitializeDefaultTripsAsync()
        {
            try
            {
                var existingTrips = await GetPredefinedTripsAsync();

                if (!existingTrips.Any())
                {
                    var lvivTrip = new PredefinedTrip
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Львів - Демонстраційний маршрут",
                        Description = "Історичний центр Львова",
                        Duration = "3 години",
                        BreakSchedule = "Перерва з 12:00 до 13:00",
                        RestPlaces = new List<string> { "Парк Шевченка", "Кафе Центральне" },
                        PointsOfInterest = new List<string> { "Львівський оперний театр", "Ринок" },
                        Route = new List<Location>
                        {
                            new Location(49.842957, 24.031111),
                            new Location(49.844000, 24.032500),
                            new Location(49.845500, 24.034000),
                            new Location(49.847000, 24.035500),
                            new Location(49.848500, 24.037000)
                        },
                        IsActive = false
                    };

                    await SavePredefinedTripAsync(lvivTrip);

                    var carpathiansTrip = new PredefinedTrip
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Карпати - Гірський маршрут",
                        Description = "Гірська прогулянка Карпатами",
                        Duration = "6 годин",
                        BreakSchedule = "Перерва з 13:00 до 14:00",
                        RestPlaces = new List<string> { "Гірський притулок", "Смотрова площа" },
                        PointsOfInterest = new List<string> { "Говерла", "Піп Іван" },
                        Route = new List<Location>
                        {
                            new Location(48.922633, 22.573900),
                            new Location(48.925000, 22.575000),
                            new Location(48.927500, 22.577500),
                            new Location(48.930000, 22.580000),
                            new Location(48.932000, 22.582000)
                        },
                        IsActive = false
                    };

                    await SavePredefinedTripAsync(carpathiansTrip);

                    var bukovelTrip = new PredefinedTrip
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Буковель - Лижний маршрут",
                        Description = "Лижні траси Буковеля",
                        Duration = "4 години",
                        BreakSchedule = "Перерва з 12:30 до 13:30",
                        RestPlaces = new List<string> { "Лижна база", "Гірський ресторан" },
                        PointsOfInterest = new List<string> { "Лижні траси", "Канатна дорога" },
                        Route = new List<Location>
                        {
                            new Location(48.354600, 24.412700),
                            new Location(48.356000, 24.414000),
                            new Location(48.357500, 24.416500),
                            new Location(48.359000, 24.418000),
                            new Location(48.360500, 24.420000)
                        },
                        IsActive = false
                    };

                    await SavePredefinedTripAsync(bukovelTrip);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Помилка ініціалізації маршрутів: {ex.Message}");
            }
        }

        public async Task<TripPlan> GetTripPlanAsync(string tripId)
        {
            try
            {
                var tripPlans = await _firebase
                    .Child("trip_plans")
                    .OnceAsync<TripPlan>();

                var plan = tripPlans
                    .Select(p =>
                    {
                        p.Object.Id = p.Key;
                        return p.Object;
                    })
                    .FirstOrDefault(p => p.TripId == tripId);

                return plan ?? new TripPlan { TripId = tripId };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Помилка отримання плану подорожі: {ex.Message}");
                return new TripPlan { TripId = tripId };
            }
        }

        public async Task<bool> SaveTripPlanAsync(TripPlan plan)
        {
            try
            {
                plan.LastUpdated = DateTime.UtcNow;

                var existingPlans = await _firebase
                    .Child("trip_plans")
                    .OnceAsync<TripPlan>();

                var existingPlan = existingPlans
                    .FirstOrDefault(p => p.Object.TripId == plan.TripId);

                if (existingPlan != null)
                {
                    await _firebase
                        .Child("trip_plans")
                        .Child(existingPlan.Key)
                        .PutAsync(plan);
                }
                else
                {
                    await _firebase
                        .Child("trip_plans")
                        .PostAsync(plan);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Помилка збереження плану подорожі: {ex.Message}");
                return false;
            }
        }

        public async Task<List<User>> GetUsersAsync()
        {
            try
            {
                var users = await _firebase
                    .Child("users")
                    .OnceAsync<User>();

                var result = users.Select(u =>
                {
                    u.Object.Id = u.Key;
                    return u.Object;
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Помилка отримання користувачів: {ex.Message}");
                return new List<User>();
            }
        }

        public async Task<bool> CreateUserAsync(User user)
        {
            try
            {
                user.LastUpdated = DateTime.UtcNow;

                if (string.IsNullOrEmpty(user.Id) || user.Id == Guid.Empty.ToString())
                {
                    user.Id = Guid.NewGuid().ToString();
                }

                await _firebase
                    .Child("users")
                    .Child(user.Id)
                    .PutAsync(user);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Помилка створення користувача: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> AddOrUpdateUserAsync(User user)
        {
            try
            {
                user.LastUpdated = DateTime.UtcNow;

                if (string.IsNullOrEmpty(user.Id) || user.Id == Guid.Empty.ToString())
                {
                    var result = await _firebase
                        .Child("users")
                        .PostAsync(user);

                    user.Id = result.Key;
                }
                else
                {
                    await _firebase
                        .Child("users")
                        .Child(user.Id)
                        .PutAsync(user);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Помилка оновлення користувача: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateUserLocationAsync(string userId, Location location)
        {
            try
            {
                var user = await GetUserAsync(userId);
                if (user == null) return false;

                user.CurrentLocation = location;
                user.LastUpdated = DateTime.UtcNow;

                await AddOrUpdateUserAsync(user);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Помилка оновлення локації: {ex.Message}");
                return false;
            }
        }

        private async Task<User> GetUserAsync(string userId)
        {
            try
            {
                var user = await _firebase
                    .Child("users")
                    .Child(userId)
                    .OnceSingleAsync<User>();

                if (user != null)
                    user.Id = userId;

                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Помилка отримання користувача: {ex.Message}");
                return null;
            }
        }

        public async Task<Trip> GetActiveTripAsync()
        {
            try
            {
                var predefinedTrips = await GetPredefinedTripsAsync();
                var activeTrip = predefinedTrips.FirstOrDefault(t => t.IsActive);

                if (activeTrip != null)
                {
                    var tripPlan = await GetTripPlanAsync(activeTrip.Id);

                    var trip = new Trip
                    {
                        TripName = activeTrip.Name,
                        Route = activeTrip.Route,
                        Duration = string.IsNullOrEmpty(tripPlan.Duration) ? activeTrip.Duration : tripPlan.Duration,
                        BreakSchedule = string.IsNullOrEmpty(tripPlan.BreakSchedule) ? activeTrip.BreakSchedule : tripPlan.BreakSchedule,
                        RestPlaces = tripPlan.RestPlaces.Any() ? tripPlan.RestPlaces : activeTrip.RestPlaces,
                        PointsOfInterest = tripPlan.PointsOfInterest.Any() ? tripPlan.PointsOfInterest : activeTrip.PointsOfInterest,
                        IsActive = true,
                        CreatedAt = activeTrip.CreatedAt
                    };

                    return trip;
                }

                return new Trip();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Помилка отримання активного маршруту: {ex.Message}");
                return new Trip();
            }
        }

        public async Task<bool> SavePredefinedTripAsync(PredefinedTrip trip)
        {
            try
            {
                trip.CreatedAt = DateTime.UtcNow;

                await _firebase
                    .Child("predefined_trips")
                    .Child(trip.Id)
                    .PutAsync(trip);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Помилка збереження маршруту: {ex.Message}");
                return false;
            }
        }

        public async Task<List<PredefinedTrip>> GetPredefinedTripsAsync()
        {
            try
            {
                var trips = await _firebase
                    .Child("predefined_trips")
                    .OnceAsync<PredefinedTrip>();

                var result = trips.Select(t =>
                {
                    t.Object.Id = t.Key;
                    return t.Object;
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Помилка отримання маршрутів: {ex.Message}");
                return new List<PredefinedTrip>();
            }
        }

        public async Task<bool> SetActiveTripAsync(string tripId)
        {
            try
            {
                var predefinedTrips = await GetPredefinedTripsAsync();

                foreach (var trip in predefinedTrips)
                {
                    if (trip.IsActive && trip.Id != tripId)
                    {
                        trip.IsActive = false;
                        await _firebase
                            .Child("predefined_trips")
                            .Child(trip.Id)
                            .PutAsync(trip);
                    }
                }

                var selectedTrip = predefinedTrips.FirstOrDefault(t => t.Id == tripId);
                if (selectedTrip != null)
                {
                    selectedTrip.IsActive = true;
                    await _firebase
                        .Child("predefined_trips")
                        .Child(selectedTrip.Id)
                        .PutAsync(selectedTrip);

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Помилка встановлення активного маршруту: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Notification>> GetNotificationsAsync(int limit = 50)
        {
            try
            {
                var notifications = await _firebase
                    .Child("notifications")
                    .OrderByKey()
                    .LimitToLast(limit)
                    .OnceAsync<Notification>();

                var result = notifications
                    .Select(n => n.Object)
                    .OrderByDescending(n => n.Time)
                    .ToList();

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Помилка отримання сповіщень: {ex.Message}");
                return new List<Notification>();
            }
        }

        public async Task<bool> AddNotificationAsync(Notification notification)
        {
            try
            {
                await _firebase
                    .Child("notifications")
                    .PostAsync(notification);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Помилка додавання сповіщення: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Feedback>> GetFeedbacksAsync(int limit = 50)
        {
            try
            {
                var feedbacks = await _firebase
                    .Child("feedbacks")
                    .OrderByKey()
                    .LimitToLast(limit)
                    .OnceAsync<Feedback>();

                var result = feedbacks
                    .Select(f =>
                    {
                        f.Object.Id = f.Key;
                        return f.Object;
                    })
                    .OrderByDescending(f => f.CreatedAt)
                    .ToList();

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Помилка отримання відгуків: {ex.Message}");
                return new List<Feedback>();
            }
        }

        public async Task<bool> AddFeedbackAsync(Feedback feedback)
        {
            try
            {
                if (string.IsNullOrEmpty(feedback.Id) || feedback.Id == Guid.Empty.ToString())
                {
                    feedback.Id = Guid.NewGuid().ToString();
                }

                feedback.CreatedAt = DateTime.UtcNow;

                await _firebase
                    .Child("feedbacks")
                    .Child(feedback.Id)
                    .PutAsync(feedback);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Помилка додавання відгуку: {ex.Message}");
                return false;
            }
        }

        public async Task<Dictionary<string, object>> GetStatisticsAsync()
        {
            try
            {
                var users = await GetUsersAsync();
                var notifications = await GetNotificationsAsync();
                var trip = await GetActiveTripAsync();
                var feedbacks = await GetFeedbacksAsync();

                var averageRating = feedbacks.Any() ? Math.Round(feedbacks.Average(f => f.Rating), 1) : 0;

                return new Dictionary<string, object>
                {
                    ["totalUsers"] = users.Count,
                    ["activeUsers"] = users.Count(u => u.LastUpdated > DateTime.UtcNow.AddHours(-1)),
                    ["totalNotifications"] = notifications.Count,
                    ["totalFeedbacks"] = feedbacks.Count,
                    ["averageRating"] = averageRating,
                    ["routePoints"] = trip.Route.Count,
                    ["leaders"] = users.Count(u => u.IsLeader)
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Помилка отримання статистики: {ex.Message}");
                return new Dictionary<string, object>();
            }
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                var testData = new { test = "connection", timestamp = DateTime.UtcNow };
                await _firebase
                    .Child("test_connection")
                    .Child(Guid.NewGuid().ToString())
                    .PutAsync(testData);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Помилка з'єднання з Firebase: {ex.Message}");
                return false;
            }
        }
    }
}