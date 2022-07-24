namespace HealthCheck.Mapping
{
    public static class SqlConnectionString
    {
        public static string GetConnectionString()
        {
            return @"TrustServerCertificate=true; Persist Security Info= false;User ID=sa; Password = MyPassword; Initial Catalog = master; Server = .";
        }
    }
}