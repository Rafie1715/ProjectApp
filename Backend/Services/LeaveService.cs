using System.Globalization;
using CutiApp.DTOs;
using CutiApp.Exceptions;
using CutiApp.Models;
using CutiApp.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CutiApp.Services;

public class LeaveService : ILeaveService
{
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
        var items = await _leaveRequestRepo.Query()
            .Include(lr => lr.User)
            .OrderByDescending(lr => lr.CreatedAt)
            .ToListAsync();

        return items.Select(MapToDto).ToList();
    }

    public async Task<List<LeaveRequestDto>> GetMineAsync(int userId)
    {
        var items = await _leaveRequestRepo.Query()
            .Include(lr => lr.User)
            .Where(lr => lr.UserId == userId)
            .OrderByDescending(lr => lr.CreatedAt)
            .ToListAsync();

        return items.Select(MapToDto).ToList();
    }

    public async Task<LeaveBalanceDto> GetBalanceAsync(int userId)
    {
        var balance = await _leaveBalanceRepo.Query()
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
        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("Data karyawan tidak ditemukan.");
        }

        if (!DateOnly.TryParseExact(request.StartDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate) ||
            !DateOnly.TryParseExact(request.EndDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate))
        {
            throw new BadRequestException("Format tanggal harus yyyy-MM-dd.");
        }

        if (endDate < startDate)
        {
            throw new BadRequestException("Tanggal selesai tidak boleh sebelum tanggal mulai.");
        }

        var reason = request.Reason.Trim();
        if (reason.Length < 10 || reason.Length > 500)
        {
            throw new BadRequestException("Alasan cuti harus antara 10 sampai 500 karakter.");
        }

        var leaveRequest = new LeaveRequest
        {
            UserId = userId,
            StartDate = startDate,
            EndDate = endDate,
            Reason = reason,
            Status = "Pending",
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
        await using var transaction = await _leaveRequestRepo.BeginTransactionAsync();

        try
        {
            var leaveRequest = await _leaveRequestRepo.Query()
                .Include(lr => lr.User)
                .FirstOrDefaultAsync(lr => lr.Id == id);

            if (leaveRequest == null)
            {
                throw new NotFoundException("Pengajuan cuti tidak ditemukan.");
            }

            if (leaveRequest.Status != "Pending")
            {
                throw new InvalidStatusException("Pengajuan cuti sudah diproses sebelumnya.");
            }

            var balance = await _leaveBalanceRepo.Query()
                .FirstOrDefaultAsync(lb => lb.UserId == leaveRequest.UserId);

            if (balance == null)
            {
                throw new NotFoundException("Saldo cuti karyawan tidak ditemukan.");
            }

            // Tanggal mulai dan tanggal selesai ikut dihitung (inklusif)
            var totalDays = (leaveRequest.EndDate.DayNumber - leaveRequest.StartDate.DayNumber) + 1;

            if (balance.RemainingDays < totalDays)
            {
                throw new InsufficientBalanceException(
                    $"Saldo cuti tidak mencukupi. Sisa saldo {balance.RemainingDays} hari, pengajuan membutuhkan {totalDays} hari.");
            }

            leaveRequest.Status = "Approved";
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
        var leaveRequest = await _leaveRequestRepo.Query()
            .Include(lr => lr.User)
            .FirstOrDefaultAsync(lr => lr.Id == id);

        if (leaveRequest == null)
        {
            throw new NotFoundException("Pengajuan cuti tidak ditemukan.");
        }

        if (leaveRequest.Status != "Pending")
        {
            throw new InvalidStatusException("Pengajuan cuti sudah diproses sebelumnya.");
        }

        leaveRequest.Status = "Rejected";
        _leaveRequestRepo.Update(leaveRequest);
        await _leaveRequestRepo.SaveChangesAsync();

        return MapToDto(leaveRequest);
    }

    public async Task CancelAsync(int id, int userId)
    {
        var leaveRequest = await _leaveRequestRepo.GetByIdAsync(id);
        if (leaveRequest == null)
        {
            throw new NotFoundException("Pengajuan cuti tidak ditemukan.");
        }

        if (leaveRequest.UserId != userId)
        {
            throw new ForbiddenException("Anda hanya dapat membatalkan pengajuan cuti milik sendiri.");
        }

        if (leaveRequest.Status != "Pending")
        {
            throw new InvalidStatusException("Hanya pengajuan berstatus Pending yang dapat dibatalkan.");
        }

        _leaveRequestRepo.Delete(leaveRequest);
        await _leaveRequestRepo.SaveChangesAsync();
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

