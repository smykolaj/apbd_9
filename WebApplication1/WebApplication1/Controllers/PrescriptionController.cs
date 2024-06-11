using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[Route("api/pres")]
[ApiController]
public class PrescriptionController : ControllerBase
{
    private readonly IDbService _dbService;

    public PrescriptionController(IDbService dbService)
    {
        _dbService = dbService;
    }


    [HttpPost("issue")]
    public async Task<IActionResult> PostPrescription(IssuePrescriptionCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var id = await _dbService.AssignPrescriptionAsync(command, cancellationToken);
            return Ok($"Added prescription with id {id}");

        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
        
    }

}