namespace projApiMongoDB.Api.Settings
{
    /// <summary>
    /// Mapeamento das configurações em appsettings.json para conexão com MongoDB.
    /// Use environment variables em produção para segurança.
    /// </summary>
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string InfectadosCollectionName { get; set; }
    }
}
