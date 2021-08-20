using System.Collections.Generic;
using System.Threading.Tasks;

namespace DepotKiwi.Db {
    public interface IRepository<T> {
        Task<List<T>> Get();
        Task<T> Get(string id);
        T Create(T item);
        Task<bool> Delete(string id);
        Task<bool> Update(string id, T item);
    }
}