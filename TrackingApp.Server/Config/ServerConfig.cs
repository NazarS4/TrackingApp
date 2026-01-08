namespace TrackingApp.Server.Config
{
    public static class ServerConfig
    {
        public static int Port { get; private set; } = 8888;
        public static int BufferSize { get; private set; } = 4096;

        public static void Initialize()
        {
            // Можна змінити порт через змінну середовища
            var portEnv = Environment.GetEnvironmentVariable("TRACKING_SERVER_PORT");
            if (!string.IsNullOrEmpty(portEnv) && int.TryParse(portEnv, out int port))
            {
                Port = port;
            }
        }
    }
}