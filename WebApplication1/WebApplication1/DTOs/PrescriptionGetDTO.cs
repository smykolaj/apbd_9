using WebApplication1.Models;

namespace WebApplication1.DTOs;

public class PrescriptionGetDTO
{
    public int IdPrescription { get; set; }
    public DateOnly Date { get; set; }
    public DateOnly DueDate { get; set; }
    public ICollection<MedicamentGetDTO> Medicaments { get; set; } = new List<MedicamentGetDTO>();
    public DoctorGetDTO Doctor { get; set; } = null!;

}