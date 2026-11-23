using System.ComponentModel.DataAnnotations;

namespace CutiApp.DTOs;

public class CreateLeaveRequestDto
{
    [Required(ErrorMessage = "Tanggal mulai wajib diisi.")]
    public string StartDate { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tanggal selesai wajib diisi.")]
    public string EndDate { get; set; } = string.Empty;

    [Required(ErrorMessage = "Alasan cuti wajib diisi.")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "Alasan cuti harus antara 10 sampai 500 karakter.")]
    public string Reason { get; set; } = string.Empty;
}

