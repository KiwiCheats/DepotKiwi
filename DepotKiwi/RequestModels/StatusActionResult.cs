using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DepotKiwi.RequestModels {
    public class StatusActionResult : IActionResult {
        public StatusActionResult(StatusResponse status, int code) {
            _response = status;
            _code = code;
        }

        public Task ExecuteResultAsync(ActionContext context) {
            var objectResult = new ObjectResult(_response) {
                StatusCode = _code
            };

            return objectResult.ExecuteResultAsync(context);
        }

        private StatusResponse _response;
        private int _code;
    }
}