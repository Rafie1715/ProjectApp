using System.Globalization;
using CutiApp.DTOs;
using CutiApp.Exceptions;
using CutiApp.Models;
using CutiApp.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CutiApp.Services;

public class LeaveService : ILeaveService
{
    private const string StatusPending = "Pending";
    private const string StatusApproved = "Approved";
    private const string StatusRejected = "Rejected";
    private const string DateFormat = "yyyy-MM-dd";
    private const int MinReasonLength = 10;
    private const int MaxReasonLength = 500;

    private readonly IRepository<LeaveRequest> _leaveRequestRepo;
    private readonly IRepository<LeaveBalance> _leaveBalanceRepo;
    private readonly IRepository<User> _userRepo;

    public LeaveService(
        IRepository<LeaveRequest> leaveRequestRepo,
        IRepository<LeaveBalance> leaveBalanceRepo,
        IRepository<User> userRepo)
    {
        _leaveRequestRepo = leaveRequestRepo;
        _leaveBalanceRepo = leaveBalanceRepo;
        _userRepo = userRepo;
    }

    public async Task<List<LeaveRequestDto>> GetAllAsync()
    {
        List<LeaveRequest> items = await _leaveRequestRepo.Query()
            .Include(lr => lr.User)
            .OrderByDescending(lr => lr.CreatedAt)
            .ToListAsync();

        return items.Select(MapToDto).ToList();
    }

    public async Task<List<LeaveRequestDto>> GetMineAsync(int userId)
    {
        List<LeaveRequest> items = await _leaveRequestRepo.Query()
            .Include(lr => lr.User)
            .Where(lr => lr.UserId == userId)
            .OrderByDescending(lr => lr.CreatedAt)
            .ToListAsync();

        return items.Select(MapToDto).ToList();
    }

    public async Task<LeaveBalanceDto> GetBalanceAsync(int userId)
    {
        LeaveBalance? balance = await _leaveBalanceRepo.Query()
            .FirstOrDefaultAsync(lb => lb.UserId == userId);

        if (balance == null)
        {
            throw new NotFoundException("Data saldo cuti tidak ditemukan.");
        }

        return new LeaveBalanceDto
        {
            RemainingDays = balance.RemainingDays
        };
    }

    public async Task<LeaveRequestDto> CreateAsync(int userId, CreateLeaveRequestDto request)
    {
        User? user = await _userRepo.GetByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("Data karyawan tidak ditemukan.");
        }

        (DateOnly startDate, DateOnly endDate) = ParseAndValidateDateRange(request.StartDate, request.EndDate);
        string reason = ValidateReason(request.Reason);

        LeaveRequest leaveRequest = new LeaveRequest
        {
            UserId = userId,
            StartDate = startDate,
            EndDate = endDate,
            Reason = reason,
            Status = StatusPending,
            CreatedAt = DateTime.UtcNow
        };

        await _leaveRequestRepo.AddAsync(leaveRequest);
        await _leaveRequestRepo.SaveChangesAsync();

        leaveRequest.User = user;
        return MapToDto(leaveRequest);
    }

    public async Task<LeaveRequestDto> ApproveAsync(int id)
    {
        // Ubah status dan kurangi saldo dalam satu transaksi agar selalu konsisten
        await using IDbContextTransaction transaction = await _leaveRequestRepo.BeginTransactionAsync();

        try
        {
            LeaveRequest leaveRequest = await GetPendingLeaveRequestAsync(id);
            LeaveBalance? balance = await _leaveBalanceRepo.Query()
                .FirstOrDefaultAsync(lb => lb.UserId == leaveRequest.UserId);

            if (balance == null)
            {
                throw new NotFoundException("Saldo cuti karyawan tidak ditemukan.");
            }

            int totalDays = CalculateLeaveDays(leaveRequest.StartDate, leaveRequest.EndDate);
            if (balance.RemainingDays < totalDays)
            {
                throw new InsufficientBalanceException(
                    $"Saldo cuti tidak mencukupi. Sisa saldo {balance.RemainingDays} hari, pengajuan membutuhkan {totalDays} hari.");
            }

            leaveRequest.Status = StatusApproved;
            balance.RemainingDays -= totalDays;

            _leaveRequestRepo.Update(leaveRequest);
            _leaveBalanceRepo.Update(balance);

            await _leaveRequestRepo.SaveChangesAsync();
            await transaction.CommitAsync();

            return MapToDto(leaveRequest);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<LeaveRequestDto> RejectAsync(int id)
    {
        LeaveRequest leaveRequest = await GetPendingLeaveRequestAsync(id);

        leaveRequest.Status = StatusRejected;
        _leaveRequestRepo.Update(leaveRequest);
        await _leaveRequestRepo.SaveChangesAsync();

        return MapToDto(leaveRequest);
    }

    public async Task CancelAsync(int id, int userId)
    {
        LeaveRequest? leaveRequest = await _leaveRequestRepo.GetByIdAsync(id);
        if (leaveRequest == null)
        {
            throw new NotFoundException("Pengajuan cuti tidak ditemukan.");
        }

        if (leaveRequest.UserId != userId)
        {
            throw new ForbiddenException("Anda hanya dapat membatalkan pengajuan cuti milik sendiri.");
        }

        if (leaveRequest.Status != StatusPending)
        {
            throw new InvalidStatusException("Hanya pengajuan berstatus Pending yang dapat dibatalkan.");
        }

        _leaveRequestRepo.Delete(leaveRequest);
        await _leaveRequestRepo.SaveChangesAsync();
    }

    private async Task<LeaveRequest> GetPendingLeaveRequestAsync(int id)
    {
        LeaveRequest? leaveRequest = await _leaveRequestRepo.Query()
            .Include(lr => lr.User)
            .FirstOrDefaultAsync(lr => lr.Id == id);

        if (leaveRequest == null)
        {
            throw new NotFoundException("Pengajuan cuti tidak ditemukan.");
        }

        if (leaveRequest.Status != StatusPending)
        {
            throw new InvalidStatusException("Pengajuan cuti sudah diproses sebelumnya.");
        }

        return leaveRequest;
    }

    private static (DateOnly StartDate, DateOnly EndDate) ParseAndValidateDateRange(
        string startDateText,
        string endDateText)
    {
        bool isStartValid = DateOnly.TryParseExact(
            startDateText, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly startDate);
        bool isEndValid = DateOnly.TryParseExact(
            endDateText, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly endDate);

        if (!isStartValid || !isEndValid)
        {
            throw new BadRequestException($"Format tanggal harus {DateFormat}.");
        }

        if (endDate < startDate)
        {
            throw new BadRequestException("Tanggal selesai tidak boleh sebelum tanggal mulai.");
        }

        return (startDate, endDate);
    }

    private static string ValidateReason(string rawReason)
    {
        string reason = rawReason.Trim();
        if (reason.Length < MinReasonLength || reason.Length > MaxReasonLength)
        {
            throw new BadRequestException(
                $"Alasan cuti harus antara {MinReasonLength} sampai {MaxReasonLength} karakter.");
        }

        return reason;
    }

    // Tanggal mulai dan tanggal selesai ikut dihitung (inklusif)
    private static int CalculateLeaveDays(DateOnly startDate, DateOnly endDate)
    {
        return (endDate.DayNumber - startDate.DayNumber) + 1;
    }

    private static LeaveRequestDto MapToDto(LeaveRequest lr)
    {
        return new LeaveRequestDto
        {
            Id = lr.Id,
            UserId = lr.UserId,
            FullName = lr.User.FullName,
            StartDate = lr.StartDate,
            EndDate = lr.EndDate,
            Reason = lr.Reason,
            Status = lr.Status,
            CreatedAt = lr.CreatedAt
        };
    }
}
