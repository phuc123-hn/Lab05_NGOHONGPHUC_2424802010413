using MediatR;
using Microsoft.AspNetCore.Mvc;
using PricingService.Api.Commands;

namespace PricingService.Controllers;

[Route("api/[controller]")]
public class PricingController : ControllerBase
{
    private readonly IMediator mediator;

    public PricingController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CalculatePriceCommand command)
    {
        var result = await mediator.Send(command);
        return new JsonResult(result);
    }
}
