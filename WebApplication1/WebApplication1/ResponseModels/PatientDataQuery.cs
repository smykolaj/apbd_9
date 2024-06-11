using WebApplication1.DTOs;

namespace WebApplication1.ResponseModels;

public class PatientDataQuery
{
    public int IdPatient { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime Birthdate { get; set; }
    public ICollection<PrescriptionGetDTO> Prescriptions { get; set; } = new List<PrescriptionGetDTO>();

}