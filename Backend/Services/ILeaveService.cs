using CutiApp.DTOs;

namespace CutiApp.Services;

public interface ILeaveService
{
    Task<List<LeaveRequestDto>> GetAllAsync();
    Task<List<LeaveRequestDto>> GetMineAsync(int userId);
    Task<LeaveBalanceDto> GetBalanceAsync(int userId);
    Task<LeaveRequestDto> CreateAsync(int userId, CreateLeaveRequestDto request);
    Task<LeaveRequestDto> ApproveAsync(int id);
    Task<LeaveRequestDto> RejectAsync(int id);
    Task CancelAsync(int id, int userId);
}

