namespace WebApplication1.DTOs;

public class IssuePrescriptionCommand
{
    public PatientPostDTO patient { get; set; }
    public ICollection<MedicamentsPostDTO> medicaments { get; set; } = new List<MedicamentsPostDTO>();
    public DateTime Date { get; set; }
    public DateTime DueDate { get; set; }
    public int IdDoctor { get; set; }
    
}