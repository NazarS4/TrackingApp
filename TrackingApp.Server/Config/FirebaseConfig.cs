using Firebase.Database;

namespace TrackingApp.Server.Config
{
    public static class FirebaseConfig
    {
        private static FirebaseClient _firebaseClient;
        private static bool _initialized = false;

        public static string DatabaseUrl { get; private set; }

        public static void Initialize()
        {
            if (_initialized) return;

            try
            {
                // Встановлюємо ваше посилання
                DatabaseUrl = "https://tracking-app-1bea8-default-rtdb.europe-west1.firebasedatabase.app/";

                // Ініціалізація Firebase Client БЕЗ аутентифікації
                _firebaseClient = new FirebaseClient(DatabaseUrl);

                _initialized = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Помилка підключення до Firebase: {ex.Message}");
                throw;
            }
        }

        public static FirebaseClient GetClient()
        {
            if (!_initialized)
                throw new InvalidOperationException("Firebase не ініціалізовано. Викличте Initialize спочатку.");

            return _firebaseClient;
        }
    }
}