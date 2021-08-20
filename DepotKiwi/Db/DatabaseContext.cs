using DepotKiwi.Model;

using MongoDB.Driver;

namespace DepotKiwi.Db {
    public interface IDatabaseContext {
        IMongoCollection<Depot> DepotCollection { get; }
    }

    public class DatabaseContext : IDatabaseContext {
        public DatabaseContext(IDatabaseSettings settings) {
            _client = new MongoClient(settings.ConnectionString);

            _database = _client.GetDatabase(settings.DatabaseName);
        }

        public DepotRepository Depots => new(this);

        public IMongoCollection<Depot> DepotCollection => GetCollection<Depot>("depots");

        private IMongoCollection<T> GetCollection<T>(string name) {
            var collection = _database.GetCollection<T>(name);

            if (collection is null) {
                _database.CreateCollection(name);

                collection = _database.GetCollection<T>(name);
            }

            return collection;
        }

        private readonly IMongoDatabase _database;
        private readonly MongoClient _client;
    }
}