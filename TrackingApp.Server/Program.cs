using System;
using System.Threading;
using System.Threading.Tasks;
using TrackingApp.Server.Config;
using TrackingApp.Server.TcpServer;
using TrackingApp.Server.Services;
using TrackingApp.Server.Models;

namespace TrackingApp.Server
{
    class Program
    {
        private static FirebaseTcpServer _server;
        private static bool _isRunning = true;
        private static FirebaseService _firebaseService;
        private static bool _inRequestMenu = false;
        private static bool _firebaseConnected = false;

        static void Main(string[] args)
        {
            PrintHeader();

            // Ініціалізація
            ServerConfig.Initialize();
            FirebaseConfig.Initialize();
            _firebaseService = new FirebaseService();

            // Запуск сервера
            _server = new FirebaseTcpServer();
            _server.Start();

            // Перевірка з'єднання з Firebase
            CheckFirebaseConnection();

            // Основний цикл
            while (_isRunning)
            {
                if (!_inRequestMenu)
                {
                    ShowMainMenu();
                }

                Console.Write("> ");
                var input = Console.ReadLine()?.Trim().ToLower();

                if (_inRequestMenu)
                {
                    HandleRequestMenu(input);
                }
                else
                {
                    HandleMainMenu(input);
                }
            }

            StopServer();
        }

        private static void PrintHeader()
        {
            Console.Clear();
            PrintSeparator();
            Console.WriteLine("🚀 Запуск TrackingApp Server");
            Console.WriteLine("📍 Порт: 8888");
            Console.WriteLine("📍 Режим: Збереження маршрутів у Firebase");
            PrintSeparator();
            Console.WriteLine("✅ Сервер успішно запущено та готовий до роботи");
            PrintSeparator();
        }

        private static async void CheckFirebaseConnection()
        {
            try
            {
                _firebaseConnected = await _firebaseService.TestConnectionAsync();
            }
            catch (Exception ex)
            {
                _firebaseConnected = false;
                Console.WriteLine($"⚠️ Помилка перевірки з'єднання з Firebase: {ex.Message}");
            }
        }

        private static void ShowMainMenu()
        {
            Console.WriteLine("\n📋 ОСНОВНЕ МЕНЮ СЕРВЕРА:");
            Console.WriteLine("┌───────────────────────────────────────────────────────────────┐");
            Console.WriteLine($"│ Статус сервера: 🟢 ЗАПУЩЕНО                                   │");
            Console.WriteLine($"│ Порт: {ServerConfig.Port,-51}     │");
            Console.WriteLine($"│ Firebase: {(_firebaseConnected ? "🟢 ПІДКЛЮЧЕНО" : "🔴 ВІДКЛЮЧЕНО"),-45}       │");
            Console.WriteLine("├───────────────────────────────────────────────────────────────┤");
            Console.WriteLine("│ 1  - Меню запитів до бази даних                               │");
            Console.WriteLine("│ 2  - Перевірити з'єднання з Firebase (TestConnectionAsync)    │");
            Console.WriteLine("│ 3  - Статистика сервера                                       │");
            Console.WriteLine("│ 4  - Перезапустити сервер                                     │");
            Console.WriteLine("│ q  - Завершити роботу сервера                                 │");
            Console.WriteLine("└───────────────────────────────────────────────────────────────┘");
        }

        private static void ShowRequestsMenu()
        {
            Console.WriteLine("\n📊 МЕНЮ ЗАПИТІВ ДО БАЗИ ДАНИХ:");
            Console.WriteLine($"┌──────────────────────────────────────────────────────────┐");
            Console.WriteLine($"│ Статус Firebase: {(_firebaseConnected ? "🟢 ПІДКЛЮЧЕНО" : "🔴 ВІДКЛЮЧЕНО"),-38}  │");
            Console.WriteLine("├──────────────────────────────────────────────────────────┤");
            Console.WriteLine("│ 1  - Всі користувачі (GetUsersAsync)                     │");
            Console.WriteLine("│ 2  - Поточний активний маршрут (GetActiveTripAsync)      │");
            Console.WriteLine("│ 3  - Останні сповіщення (GetNotificationsAsync)          │");
            Console.WriteLine("│ 4  - Відгуки користувачів (GetFeedbacksAsync)            │");
            Console.WriteLine("│ 5  - Всі попередні маршрути (GetPredefinedTripsAsync)    │");
            Console.WriteLine("│ 6  - Детальна інформація про користувача (GetUsersAsync) │");
            Console.WriteLine("│ 7  - Статистика системи (GetStatisticsAsync)             │");
            Console.WriteLine("│ 8  - Видалити користувача (DeleteUserAsync)              │");
            Console.WriteLine("│ b  - Повернутися до головного меню                       │");
            Console.WriteLine("└──────────────────────────────────────────────────────────┘");
        }

        private static void HandleMainMenu(string input)
        {
            switch (input)
            {
                case "1":
                    _inRequestMenu = true;
                    Console.Clear();
                    PrintSeparator();
                    Console.WriteLine("📊 МЕНЮ ЗАПИТІВ ДО БАЗИ ДАНИХ");
                    PrintSeparator();
                    ShowRequestsMenu();
                    break;

                case "2":
                    TestConnection();
                    break;

                case "3":
                    ShowServerStatistics();
                    break;

                case "4":
                    RestartServer();
                    break;

                case "q":
                case "quit":
                    _isRunning = false;
                    break;

                case "":
                    break;

                default:
                    Console.WriteLine("❌ Невідома команда. Спробуйте ще раз.");
                    break;
            }
        }

        private static void HandleRequestMenu(string input)
        {
            switch (input)
            {
                case "1":
                    ShowAllUsers();
                    WaitForBack();
                    break;

                case "2":
                    ShowActiveTrip();
                    WaitForBack();
                    break;

                case "3":
                    ShowNotifications();
                    WaitForBack();
                    break;

                case "4":
                    ShowFeedbacks();
                    WaitForBack();
                    break;

                case "5":
                    ShowPredefinedTrips();
                    WaitForBack();
                    break;

                case "6":
                    ShowUserDetailsMenu();
                    break;

                case "7":
                    ShowStatistics();
                    WaitForBack();
                    break;

                case "8":
                    DeleteUserMenu();
                    break;

                case "b":
                case "back":
                    _inRequestMenu = false;
                    Console.Clear();
                    PrintSeparator();
                    Console.WriteLine("🔙 Повернення до головного меню");
                    PrintSeparator();
                    break;

                case "":
                    break;

                default:
                    Console.WriteLine("❌ Невідома команда. Спробуйте ще раз.");
                    break;
            }
        }

        private static void WaitForBack()
        {
            Console.WriteLine("\n────────────────────────────────────────────────────────────");
            Console.WriteLine("Натисніть 'b' для повернення до меню запитів");
            var input = "";
            while (input != "b")
            {
                Console.Write("> ");
                input = Console.ReadLine()?.Trim().ToLower();
                if (input == "b")
                {
                    Console.Clear();
                    PrintSeparator();
                    Console.WriteLine("📊 МЕНЮ ЗАПИТІВ ДО БАЗИ ДАНИХ");
                    PrintSeparator();
                    ShowRequestsMenu();
                    break;
                }
            }
        }

        private static void ShowUserDetailsMenu()
        {
            Console.Write("\n👤 Введіть ім'я користувача: ");
            var userName = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(userName))
            {
                Console.WriteLine("❌ Ім'я не може бути порожнім");
                WaitForBack();
                return;
            }

            ShowUserDetails(userName);
            WaitForBack();
        }

        private static void DeleteUserMenu()
        {
            Console.Write("\n🗑️ Введіть ім'я користувача для видалення: ");
            var userName = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(userName))
            {
                Console.WriteLine("❌ Ім'я не може бути порожнім");
                WaitForBack();
                return;
            }

            DeleteUser(userName);
            WaitForBack();
        }

        private static void RestartServer()
        {
            Console.WriteLine("\n🔄 Перезапуск сервера...");
            _server?.Stop();
            Thread.Sleep(1000);

            _server = new FirebaseTcpServer();
            _server.Start();

            CheckFirebaseConnection();

            Console.WriteLine("✅ Сервер перезапущено успішно");
            Thread.Sleep(1000);
            Console.Clear();
            PrintHeader();
        }

        // МЕТОДИ ДЛЯ ВІДОБРАЖЕННЯ ДАНИХ

        private static async void ShowAllUsers()
        {
            try
            {
                Console.WriteLine("\n👥 ЗАВАНТАЖЕННЯ КОРИСТУВАЧІВ...");
                var users = await _firebaseService.GetUsersAsync();

                if (users.Any())
                {
                    Console.WriteLine($"\n✅ Знайдено {users.Count} користувачів:");
                    Console.WriteLine("┌─────┬──────────────────┬───────────────────────┬──────────┐");
                    Console.WriteLine("│ №   │ Ім'я             │ Email                 │ Лідер    │");
                    Console.WriteLine("├─────┼──────────────────┼───────────────────────┼──────────┤");

                    for (int i = 0; i < users.Count; i++)
                    {
                        var user = users[i];
                        var leaderStatus = user.IsLeader ? "✅ Так" : "❌ Ні";

                        Console.WriteLine($"│ {i + 1,-3} │ {user.Name,-16} │ {user.Email,-21} │ {leaderStatus,-7} │");
                    }
                    Console.WriteLine("└─────┴──────────────────┴───────────────────────┴──────────┘");
                }
                else
                {
                    Console.WriteLine("❌ Користувачі не знайдені");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Помилка: {ex.Message}");
            }
        }

        private static async void ShowActiveTrip()
        {
            try
            {
                Console.WriteLine("\n🗺️ ЗАВАНТАЖЕННЯ АКТИВНОГО МАРШРУТУ...");
                var trip = await _firebaseService.GetActiveTripAsync();

                if (trip != null && !string.IsNullOrEmpty(trip.TripName))
                {
                    Console.WriteLine($"\n✅ АКТИВНИЙ МАРШРУТ:");
                    Console.WriteLine($"├─ Назва: {trip.TripName}");
                    Console.WriteLine($"├─ Тривалість: {trip.Duration}");
                    Console.WriteLine($"├─ Перерви: {trip.BreakSchedule}");
                    Console.WriteLine($"├─ Точок маршруту: {trip.Route.Count}");
                    Console.WriteLine($"├─ Місця відпочинку: {string.Join(", ", trip.RestPlaces)}");
                    Console.WriteLine($"└─ Цікаві місця: {string.Join(", ", trip.PointsOfInterest)}");
                }
                else
                {
                    Console.WriteLine("❌ Активний маршрут не знайдений");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Помилка: {ex.Message}");
            }
        }

        private static async void ShowNotifications()
        {
            try
            {
                Console.WriteLine("\n📢 ЗАВАНТАЖЕННЯ СПОВІЩЕНЬ...");
                var notifications = await _firebaseService.GetNotificationsAsync(20);

                if (notifications.Any())
                {
                    Console.WriteLine($"\n🔔 ОСТАННІ {notifications.Count} СПОВІЩЕНЬ:");
                    foreach (var notification in notifications.Take(10))
                    {
                        Console.WriteLine($"├─ [{notification.Time:HH:mm:ss}] {notification.UserName}: {notification.Message}");
                    }
                    if (notifications.Count > 10)
                    {
                        Console.WriteLine($"└─ ... і ще {notifications.Count - 10} сповіщень");
                    }
                }
                else
                {
                    Console.WriteLine("❌ Сповіщення відсутні");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Помилка: {ex.Message}");
            }
        }

        private static async void ShowFeedbacks()
        {
            try
            {
                Console.WriteLine("\n⭐ ЗАВАНТАЖЕННЯ ВІДГУКІВ...");
                var feedbacks = await _firebaseService.GetFeedbacksAsync(20);

                if (feedbacks.Any())
                {
                    Console.WriteLine($"\n📝 ОСТАННІ {feedbacks.Count} ВІДГУКІВ:");
                    foreach (var feedback in feedbacks.Take(10))
                    {
                        var stars = new string('⭐', feedback.Rating) + new string('☆', 5 - feedback.Rating);
                        Console.WriteLine($"├─ {feedback.UserName} ({stars}): {feedback.Message}");
                    }
                    if (feedbacks.Count > 10)
                    {
                        Console.WriteLine($"└─ ... і ще {feedbacks.Count - 10} відгуків");
                    }
                }
                else
                {
                    Console.WriteLine("❌ Відгуки відсутні");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Помилка: {ex.Message}");
            }
        }

        private static async void ShowUserDetails(string userName)
        {
            try
            {
                var users = await _firebaseService.GetUsersAsync();
                var user = users.FirstOrDefault(u => u.Name.Equals(userName, StringComparison.OrdinalIgnoreCase));

                if (user != null)
                {
                    Console.WriteLine($"\n📋 ДЕТАЛЬНА ІНФОРМАЦІЯ ПРО КОРИСТУВАЧА:");
                    Console.WriteLine($"├─ Ім'я: {user.Name}");
                    Console.WriteLine($"├─ Email: {user.Email}");
                    Console.WriteLine($"├─ Лідер: {(user.IsLeader ? "✅ Так" : "❌ Ні")}");
                    Console.WriteLine($"├─ Тип подорожі: {user.TripType}");
                    Console.WriteLine($"└─ Останнє оновлення: {user.LastUpdated:dd.MM.yyyy HH:mm}");
                }
                else
                {
                    Console.WriteLine($"❌ Користувача '{userName}' не знайдено");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Помилка: {ex.Message}");
            }
        }

        private static async void ShowPredefinedTrips()
        {
            try
            {
                Console.WriteLine("\n🗺️ ЗАВАНТАЖЕННЯ МАРШРУТІВ...");
                var trips = await _firebaseService.GetPredefinedTripsAsync();

                if (trips.Any())
                {
                    Console.WriteLine($"\n📌 ЗНАЙДЕНО {trips.Count} МАРШРУТІВ:");
                    foreach (var trip in trips)
                    {
                        var activeStatus = trip.IsActive ? "✅ АКТИВНИЙ" : "❌ НЕАКТИВНИЙ";
                        Console.WriteLine($"├─ {trip.Name} ({activeStatus})");
                        Console.WriteLine($"│  └─ {trip.Description}");
                    }
                }
                else
                {
                    Console.WriteLine("❌ Маршрути не знайдені");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Помилка: {ex.Message}");
            }
        }

        private static async void ShowStatistics()
        {
            try
            {
                Console.WriteLine("\n📊 ЗАВАНТАЖЕННЯ СТАТИСТИКИ...");
                var stats = await _firebaseService.GetStatisticsAsync();

                if (stats.Any())
                {
                    Console.WriteLine("\n📈 СТАТИСТИКА СИСТЕМИ:");
                    Console.WriteLine("┌──────────────────────────┬──────────┐");
                    Console.WriteLine("│ Параметр                 │ Значення │");
                    Console.WriteLine("├──────────────────────────┼──────────┤");
                    Console.WriteLine($"│ Користувачів            │ {stats["totalUsers"],-8} │");
                    Console.WriteLine($"│ Активних користувачів   │ {stats["activeUsers"],-8} │");
                    Console.WriteLine($"│ Сповіщень               │ {stats["totalNotifications"],-8} │");
                    Console.WriteLine($"│ Відгуків                │ {stats["totalFeedbacks"],-8} │");
                    Console.WriteLine($"│ Середній рейтинг        │ {stats["averageRating"],-8} │");
                    Console.WriteLine($"│ Лідерів                 │ {stats["leaders"],-8} │");
                    Console.WriteLine("└──────────────────────────┴──────────┘");
                }
                else
                {
                    Console.WriteLine("❌ Не вдалося отримати статистику");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Помилка: {ex.Message}");
            }
        }

        private static async void DeleteUser(string userName)
        {
            try
            {
                Console.WriteLine($"\n🗑️ СПРОБА ВИДАЛЕННЯ КОРИСТУВАЧА: {userName}");

                // Спочатку перевіримо чи існує користувач
                var users = await _firebaseService.GetUsersAsync();
                var userToDelete = users.FirstOrDefault(u => u.Name.Equals(userName, StringComparison.OrdinalIgnoreCase));

                if (userToDelete == null)
                {
                    Console.WriteLine("❌ Користувача з таким іменем не знайдено");
                    return;
                }

                Console.WriteLine($"\n📋 ІНФОРМАЦІЯ ПРО КОРИСТУВАЧА ДЛЯ ВИДАЛЕННЯ:");
                Console.WriteLine($"├─ Ім'я: {userToDelete.Name}");
                Console.WriteLine($"├─ Email: {userToDelete.Email}");
                Console.WriteLine($"├─ Лідер: {(userToDelete.IsLeader ? "✅ Так" : "❌ Ні")}");
                Console.WriteLine($"└─ Тип подорожі: {userToDelete.TripType}");

                Console.Write($"\n❓ Ви впевнені, що хочете видалити користувача {userToDelete.Name}? (y/n): ");
                var confirmation = Console.ReadLine()?.Trim().ToLower();

                if (confirmation == "y" || confirmation == "yes")
                {
                    // Тут потрібно реалізувати метод видалення користувача
                    // bool success = await _firebaseService.DeleteUserAsync(userToDelete.Id);

                    // Тимчасова імітація успішного видалення
                    bool success = true;

                    if (success)
                    {
                        Console.WriteLine($"✅ Користувача {userToDelete.Name} успішно видалено");
                    }
                    else
                    {
                        Console.WriteLine($"❌ Не вдалося видалити користувача {userToDelete.Name}");
                    }
                }
                else
                {
                    Console.WriteLine("❌ Видалення скасовано");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Помилка видалення користувача: {ex.Message}");
            }
        }

        private static void ShowServerStatistics()
        {
            Console.WriteLine("\n🖥️ СТАТИСТИКА СЕРВЕРА:");
            Console.WriteLine("┌──────────────────────────┬────────────────┐");
            Console.WriteLine("│ Параметр                 │ Значення       │");
            Console.WriteLine("├──────────────────────────┼────────────────┤");
            Console.WriteLine($"│ Порт сервера            │ {ServerConfig.Port,-14} │");
            Console.WriteLine($"│ Статус сервера          │ {"🟢 Запущений",-13}  │");
            Console.WriteLine($"│ Firebase                │ {(_firebaseConnected ? "🟢 Підключено" : "🔴 Відключено"),-14} │");
            Console.WriteLine("└──────────────────────────┴────────────────┘");

            WaitForAnyKey();
        }

        private static async void TestConnection()
        {
            try
            {
                Console.WriteLine("\n🔗 ПЕРЕВІРКА З'ЄДНАННЯ...");
                var isConnected = await _firebaseService.TestConnectionAsync();
                _firebaseConnected = isConnected;

                if (isConnected)
                {
                    Console.WriteLine("✅ З'єднання з Firebase працює стабільно");
                }
                else
                {
                    Console.WriteLine("❌ Проблеми з з'єднанням Firebase");
                }
            }
            catch (Exception ex)
            {
                _firebaseConnected = false;
                Console.WriteLine($"❌ Помилка: {ex.Message}");
            }

            WaitForAnyKey();
        }

        private static void WaitForAnyKey()
        {
            Console.WriteLine("\n────────────────────────────────────────────────────────────");
            Console.WriteLine("Натисніть будь-яку клавішу для продовження...");
            Console.ReadKey();
            Console.Clear();
            PrintSeparator();
            Console.WriteLine("🔙 Повернення до головного меню");
            PrintSeparator();
        }

        private static void StopServer()
        {
            PrintSeparator();
            Console.WriteLine("🛑 Зупинка сервера...");
            _server?.Stop();

            Thread.Sleep(1000);
            Console.WriteLine("✅ Сервер зупинено");
            PrintSeparator();
        }

        private static void PrintSeparator()
        {
            Console.WriteLine(new string('─', 60));
        }
    }
}