using System;
using System.IO;
using System.Text;

namespace DepotKiwi.Services {
    public class StorageService {
        public StorageService(string baseDirectory) {
            _baseDirectory = Path.GetFullPath(baseDirectory);
        }
        
        public Stream GetFileStream(string name) {
            try {
                return File.OpenRead(GetPath(name));
            }
            catch {
                return null;
            }
        }
        
        public bool DeleteFile(string name) {
            try {
                File.Delete(GetPath(name));

                return true;
            }
            catch {
                return false;
            }
        }

        public bool SaveFile(Span<byte> buffer, string name) {
            try {
                var path = GetPath(name);

                var directory = Path.GetDirectoryName(path);

                if (directory is null)
                    return false;

                Directory.CreateDirectory(directory);

                using var writer = new BinaryWriter(File.Create(path), Encoding.ASCII);

                writer.Write(buffer);

                return true;
            }
            catch {
                return false;
            }
        }

        private string GetPath(string name) {
            return Path.Join(_baseDirectory, name);
        }

        private readonly string _baseDirectory;
    }
}