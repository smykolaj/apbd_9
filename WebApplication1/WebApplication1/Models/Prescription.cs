using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models;

[Table("prescriptions")]
public class Prescription
{
    [Key]
    public int IdPrescription { get; set; }
    public DateTime Date { get; set; }
    public DateTime DueDate { get; set; }
    public int IdPatient { get; set; } 
    public int IdDoctor { get; set; } 

    [ForeignKey(nameof(IdPatient))]
    public Patient Patient { get; set; }= null!;
    
    [ForeignKey(nameof(IdDoctor))]
    public Doctor Doctor { get; set; } = null!;

    public ICollection<PrescriptionMedicament> PrescriptionMedicaments { get; set; } =
        new HashSet<PrescriptionMedicament>();

}