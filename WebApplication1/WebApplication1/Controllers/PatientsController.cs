using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly IDbService _service;

    public PatientsController(IDbService service)
    {
        _service = service;
    }

    [HttpGet("{patientId}")]
    public async Task<IActionResult> GetPatientData(int patientId, CancellationToken cancellationToken)
    {
        try
        {
            var output = await _service.GetPatientInfo(patientId, cancellationToken);
            return Ok(output);
        }
        catch (Exception e)
        {
            return NotFound(e.Message);
        }
    }
}