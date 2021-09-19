using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using DepotKiwi.Db;
using DepotKiwi.RequestModels;
using DepotKiwi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DepotKiwi.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class DepotController {
        public DepotController(DatabaseContext databaseContext, RepositoryService repositoryService) {
            _databaseContext = databaseContext;
            _repositoryService = repositoryService;
        }
        
        [AllowAnonymous]
        [HttpPost("create")]
        public async Task<StatusResponse> Create(DepotCreateRequest request) {
            var depot = _databaseContext.Depots.Get().Result.FirstOrDefault(x => x.Name == request.Name);

            if (depot is not null) {
                return new() {
                    Success = false,
                    Message = "Depot with this name already exists."
                };
            }

            depot = _databaseContext.Depots.Create(new() {
                Name = request.Name
            });

            _repositoryService.CreateRepositoryStorageService(depot.Id);
            
            return new() {
                Success = true,
                Message = "Successfully created depot."
            };
        }

        [HttpPost("delete")]
        public async Task<StatusResponse> Delete(DepotDeleteRequest request) {
            var success = await _databaseContext.Depots.Delete(request.Id);

            return new() {
                Success = success,
                Message = success ? "Successfully deleted depot." : "Invalid depot id."
            };
        }
        
        [AllowAnonymous]
        [HttpGet("list")]
        public async Task<List<DepotListResponse>> List() {
            var depots = await _databaseContext.Depots.Get();

            return depots.ConvertAll(x => new DepotListResponse {
                Id = x.Id,
                Name = x.Name
            });
        }

        [AllowAnonymous]
        [HttpGet("info/{depotId}")]
        public async Task<DepotInfoResponse> Info(string depotId) {
            var depot = await _databaseContext.Depots.Get(depotId) ?? await _databaseContext.Depots.GetByName(depotId);

            return new() {
                Success = depot is not null,
                Depot = depot
            };
        }

        [AllowAnonymous]
        [HttpGet("file/download/{depotId}/{**file}")]
        public async Task<IActionResult> Download(string depotId, string file) {
            var repository = _repositoryService.GetRepositoryStorageService(depotId);

            if (repository is null) {
                var depot = await _databaseContext.Depots.GetByName(depotId);

                if (depot is not null)
                    repository = _repositoryService.GetRepositoryStorageService(depot.Id) ?? _repositoryService.CreateRepositoryStorageService(depot.Id);
            }

            if (repository is null) {
                return new StatusActionResult(new StatusResponse {
                    Success = false,
                    Message = "Depot is invalid."
                }, 404);
            }

            var stream = repository.GetFileStream(file);

            if (stream is null) {
                return new StatusActionResult(new StatusResponse {
                    Success = false,
                    Message = "File does not exist in this depot."
                }, 404);
            }

            return new FileStreamResult(stream, "application/octet-stream");
        }
        
        
        [AllowAnonymous]
        [HttpPost("file/upload/{depotId}/{**file}")]
        public async Task<StatusResponse> Upload(string depotId, string file, [FromBody] DepotFileUploadRequest request) {
            var depot = await _databaseContext.Depots.Get(depotId) ?? await _databaseContext.Depots.GetByName(depotId);

            if (depot is null) {
                return new() {
                    Success = false,
                    Message = "Depot is invalid."
                };
            }
            
            if (string.IsNullOrWhiteSpace(file)) {
                return new() {
                    Success = false,
                    Message = "File name cannot be empty."
                };
            }

            byte[] buffer;

            try {
                buffer = Convert.FromBase64String(request.Data);
            }
            catch {
                return new() {
                    Success = false,
                    Message = "Invalid file encoding."
                };
            }

            var repository = _repositoryService.GetRepositoryStorageService(depotId) ?? _repositoryService.CreateRepositoryStorageService(depotId);

            if (!repository.SaveFile(buffer, file)) {
                return new() {
                    Success = false,
                    Message = "Failed to save file."
                };
            }

            var fileInfo = depot.Files.FirstOrDefault(x => x.Name == file);

            if (fileInfo is null) {
                fileInfo = new();
                
                depot.Files.Add(fileInfo);
            }
            
            fileInfo.Name = file;
            fileInfo.Sha256 = CalculateSha256(buffer);

            await _databaseContext.Depots.Update(depot.Id, depot);

            return new() {
                Success = true,
                Message = "Successfully uploaded file."
            };
        }

        [AllowAnonymous]
        [HttpPost("file/delete/{depotId}/{**file}")]
        public async Task<StatusResponse> Delete(string depotId, string file) {
            var depot = await _databaseContext.Depots.Get(depotId) ?? await _databaseContext.Depots.GetByName(depotId);

            if (depot is null) {
                return new() {
                    Success = false,
                    Message = "Depot is invalid."
                };
            }

            if (string.IsNullOrWhiteSpace(file)) {
                return new() {
                    Success = false,
                    Message = "File name cannot be empty."
                };
            }
            
            var fileInfo = depot.Files.FirstOrDefault(x => x.Name == file);

            if (fileInfo is null) {
                return new() {
                    Success = false,
                    Message = "File doesn't exist."
                };
            }

            var repository = _repositoryService.GetRepositoryStorageService(depot.Id) ?? _repositoryService.CreateRepositoryStorageService(depot.Id);

            repository.DeleteFile(file);

            depot.Files.Remove(fileInfo);

            await _databaseContext.Depots.Update(depot.Id, depot);

            return new() {
                Success = true,
                Message = "Successfully deleted file."
            };
        }

        private static string CalculateSha256(byte[] buffer) {
            using var crypt = new SHA256Managed();

            var builder = new StringBuilder();

            foreach (var b in crypt.ComputeHash(buffer)) {
                builder.Append(b.ToString("x2"));
            }

            return builder.ToString();
        }

        private readonly RepositoryService _repositoryService;
        private readonly DatabaseContext _databaseContext;
    }
}