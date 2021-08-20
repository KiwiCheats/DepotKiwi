using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DepotKiwi.Model;
using MongoDB.Driver;

namespace DepotKiwi.Db {
      public class DepotRepository : IRepository<Depot> {
        public DepotRepository(DatabaseContext context) {
            _context = context;
        }

        public Task<List<Depot>> Get() {
            return _context.DepotCollection.Find(depot => true).ToListAsync();
        }

        public async Task<Depot> Get(string id) {
            try {
                return await _context.DepotCollection
                    .Find(depot => depot.Id == id)
                    .FirstOrDefaultAsync();
            }
            catch (FormatException) {
                return null;
            }
        }

        public Depot Create(Depot item) {
            _context.DepotCollection.InsertOne(item);

            return item;
        }

        public Task<bool> Delete(string id) {
            return Task.FromResult(_context.DepotCollection.DeleteOne(x => x.Id == id).DeletedCount > 0);
        }

        public async Task<bool> Update(string id, Depot item) {
            var result = await _context.DepotCollection.ReplaceOneAsync(
                loader => loader.Id == id,
                item,
                new ReplaceOptions { IsUpsert = true });

            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        private readonly DatabaseContext _context;
    }
}