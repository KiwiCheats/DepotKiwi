using System.IO;

namespace DepotKiwi.Services {
    public class RepositoryService {
        public RepositoryService(string directoryPath) {
            _directoryPath = Path.GetFullPath(directoryPath);
        }

        public StorageService CreateRepositoryStorageService(string name) {
            var path = Path.Join(_directoryPath, name);

            try {
                Directory.CreateDirectory(path);
                
                return new(path);
            }
            catch {
                return null;
            }
        }
        
        public StorageService GetRepositoryStorageService(string name) {
            var path = Path.Join(_directoryPath, name);
            
            if (!Directory.Exists(path))
                return null;
            
            return new(path);
        }

        private readonly string _directoryPath;
    }
}