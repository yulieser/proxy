using Microsoft.AspNetCore.Mvc;

namespace CRProxy.Infrastructure.Controllers
{
    [ApiController]
    [Route("/health")]
    public class HealthController : ControllerBase
    {
        private readonly Observability.ProxyMetrics _metrics;

        public HealthController(Observability.ProxyMetrics metrics)
        {
            _metrics = metrics;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var model = new
            {
                status = "ok",
                activeConnections = _metrics.ActiveConnections,
                errors = _metrics.Errors
            };
            return Ok(model);
        }
    }
}
