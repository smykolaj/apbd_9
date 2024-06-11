using WebApplication1.DTOs;
using WebApplication1.ResponseModels;

namespace WebApplication1.Services;

public interface IDbService
{

    Task<bool> PatientExistsById(int patientID);

    Task<bool> DoctorExistsById(IssuePrescriptionCommand command);
    Task<bool> PatientExistsByIdAndFullNameAsync(PatientPostDTO patientDto, CancellationToken cancellationToken);
    Task<int> PatientExistsByFullNameAsync(PatientPostDTO patientDto, CancellationToken cancellationToken);
    
    Task<int> CreatePatientAsync(PatientPostDTO patientDto, CancellationToken cancellationToken);
    Task<bool> MedicamentExistsAsync(MedicamentsPostDTO medicamentDto, CancellationToken cancellationToken);
    bool MedicamentListSizeSmallerThanEleven(IssuePrescriptionCommand command);
    bool DueDateIsBigger(IssuePrescriptionCommand command);
    Task<int> AssignPrescriptionAsync(IssuePrescriptionCommand command, CancellationToken cancellationToken);
    Task<int> CreatePrescriptionAsync(IssuePrescriptionCommand command, int patientId, CancellationToken cancellationToken);
    Task<int> CreatePrescriptionMedicamentsAsync(IssuePrescriptionCommand command, int idPrescription, CancellationToken cancellationToken);

    Task<PatientDataQuery> GetPatientInfo(int patientId, CancellationToken cancellationToken);
}