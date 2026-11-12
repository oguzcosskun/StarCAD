using Microsoft.AspNetCore.Mvc;
using StarCAD.Core.Utilities.Results;
using CoreResult = StarCAD.Core.Utilities.Results.IResult;

namespace StarCAD.Api.Controllers;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected IActionResult FromResult<T>(IDataResult<T> result)
    {
        if (result.Success)
            return Ok(result.Data);

        var statusCode = result is ErrorDataResult<T> error ? error.StatusCode : StatusCodes.Status400BadRequest;
        return StatusCode(statusCode, new { message = result.Message });
    }

    protected IActionResult FromResult(CoreResult result)
    {
        if (result.Success)
            return Ok(new { message = result.Message });

        var statusCode = result is ErrorResult error ? error.StatusCode : StatusCodes.Status400BadRequest;
        return StatusCode(statusCode, new { message = result.Message });
    }
}