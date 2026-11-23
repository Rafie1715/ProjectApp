using System.Security.Claims;
using CutiApp.DTOs;
using CutiApp.Exceptions;
using CutiApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CutiApp.Controllers;

[ApiController]
[Route("api/leave-requests")]
[Authorize]
public class LeaveRequestsController : ControllerBase
{
    private readonly ILeaveService _leaveService;

    public LeaveRequestsController(ILeaveService leaveService)
    {
        _leaveService = leaveService;
    }

    [HttpGet]
    public async Task<ActionResult<List<LeaveRequestDto>>> GetAll()
    {
        var result = await _leaveService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("mine")]
    public async Task<ActionResult<List<LeaveRequestDto>>> GetMine()
    {
        var userId = GetCurrentUserId();
        var result = await _leaveService.GetMineAsync(userId);
        return Ok(result);
    }

    [HttpGet("balance")]
    public async Task<ActionResult<LeaveBalanceDto>> GetBalance()
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _leaveService.GetBalanceAsync(userId);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<LeaveRequestDto>> Create([FromBody] CreateLeaveRequestDto request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _leaveService.CreateAsync(userId, request);
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (BadRequestException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}/approve")]
    public async Task<ActionResult<LeaveRequestDto>> Approve(int id)
    {
        try
        {
            var result = await _leaveService.ApproveAsync(id);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidStatusException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (InsufficientBalanceException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (BadRequestException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}/reject")]
    public async Task<ActionResult<LeaveRequestDto>> Reject(int id)
    {
        try
        {
            var result = await _leaveService.RejectAsync(id);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidStatusException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancel(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _leaveService.CancelAsync(id, userId);
            return Ok(new { message = "Pengajuan cuti berhasil dibatalkan." });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ForbiddenException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (InvalidStatusException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(claim, out var userId))
        {
            throw new UnauthorizedException("Token autentikasi tidak valid.");
        }

        return userId;
    }
}

