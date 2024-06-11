using WebApplication1.ResponseModels;

namespace TestProject1;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApplication1.DTOs;
using WebApplication1.Models;
using WebApplication1.Services;

public class FakeDbService : IDbService
{
    private ICollection<Patient> patients;
    private ICollection<Medicament> medicaments;
    private ICollection<Doctor> doctors;
    private ICollection<Prescription> prescriptions;
    private ICollection<PrescriptionMedicament> prescriptionMedicaments;

    public FakeDbService()
    {
        patients = new List<Patient>
        {
            new Patient { IdPatient = 1, FirstName = "John", LastName = "Doe", Birthdate = new DateTime(1990, 1, 1) }
            // Add more patients as needed
        };

        medicaments = new List<Medicament>
        {
            new Medicament { IdMedicament = 1, Name = "Medicament1" }
            // Add more medicaments as needed
        };

        doctors = new List<Doctor>
        {
            new Doctor { IdDoctor = 1, FirstName = "Jane", LastName = "Smith" }
            // Add more doctors as needed
        };

        prescriptions = new List<Prescription>();

        prescriptionMedicaments = new List<PrescriptionMedicament>();
    }

    public Task<bool> PatientExistsById(int patientID)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DoctorExistsById(IssuePrescriptionCommand command)
    {
        return Task.FromResult(doctors.Any(d => d.IdDoctor == command.IdDoctor));
    }

    public Task<bool> PatientExistsByIdAndFullNameAsync(PatientPostDTO patientDTO, CancellationToken cancellationToken)
    {
        return Task.FromResult(patients.Any(p => p.IdPatient == patientDTO.IdPatient && p.FirstName == patientDTO.FirstName && p.LastName == patientDTO.LastName));
    }

    public Task<int> PatientExistsByFullNameAsync(PatientPostDTO patientDto, CancellationToken cancellationToken)
    {
        var patient = patients.FirstOrDefault(p => p.FirstName == patientDto.FirstName && p.LastName == patientDto.LastName && p.Birthdate == patientDto.Birthdate);
        return Task.FromResult(patient?.IdPatient ?? 0);
    }

    public Task<int> CreatePatientAsync(PatientPostDTO patientDto, CancellationToken cancellationToken)
    {
        var newPatient = new Patient
        {
            IdPatient = patients.Any() ? patients.Max(p => p.IdPatient) + 1 : 1,
            FirstName = patientDto.FirstName,
            LastName = patientDto.LastName,
            Birthdate = patientDto.Birthdate
        };
        patients.Add(newPatient);
        return Task.FromResult(newPatient.IdPatient);
    }

    public Task<bool> MedicamentExistsAsync(MedicamentsPostDTO medicamentDto, CancellationToken cancellationToken)
    {
        return Task.FromResult(medicaments.Any(m => m.IdMedicament == medicamentDto.IdMedicament));
    }

    public bool MedicamentListSizeSmallerThanEleven(IssuePrescriptionCommand command)
    {
        return command.medicaments.Count < 11;
    }

    public bool DueDateIsBigger(IssuePrescriptionCommand command)
    {
        return command.DueDate >= command.Date;
    }

    public async Task<int> AssignPrescriptionAsync(IssuePrescriptionCommand command, CancellationToken cancellationToken)
    {
        int patientId = command.patient.IdPatient;
        if (!await PatientExistsByIdAndFullNameAsync(command.patient, cancellationToken))
        {
            try
            {
                patientId = await PatientExistsByFullNameAsync(command.patient, cancellationToken);
            }
            catch (Exception)
            {
                patientId = await CreatePatientAsync(command.patient, cancellationToken);
            }
        }

        foreach (var medic in command.medicaments)
        {
            if (!await MedicamentExistsAsync(medic, cancellationToken))
                throw new Exception($"Medicament with id {medic.IdMedicament} doesn't exist");
        }

        if (!MedicamentListSizeSmallerThanEleven(command))
            throw new Exception($"Prescription can have up to 10 medicaments. The input has {command.medicaments.Count}");

        if (!DueDateIsBigger(command))
            throw new Exception("The DueDate has to be later than the Date");

        if (!await DoctorExistsById(command))
            throw new Exception("The Doctor doesn't exist");

        int idPrescription = await CreatePrescriptionAsync(command, patientId, cancellationToken);
        await CreatePrescriptionMedicamentsAsync(command, idPrescription, cancellationToken);
        return idPrescription;
    }

    public Task<int> CreatePrescriptionAsync(IssuePrescriptionCommand command, int patientId, CancellationToken cancellationToken)
    {
        var newPrescription = new Prescription
        {
            IdPrescription = prescriptions.Any() ? prescriptions.Max(p => p.IdPrescription) + 1 : 1,
            Date = command.Date,
            DueDate = command.DueDate,
            IdPatient = patientId,
            IdDoctor = command.IdDoctor
        };
        prescriptions.Add(newPrescription);
        return Task.FromResult(newPrescription.IdPrescription);
    }

    public Task<int> CreatePrescriptionMedicamentsAsync(IssuePrescriptionCommand command, int idPrescription, CancellationToken cancellationToken)
    {
        int added = 0;
        foreach (var medicament in command.medicaments)
        {
            var newPrescriptionMedicament = new PrescriptionMedicament
            {
                IdMedicament = medicament.IdMedicament,
                IdPrescription = idPrescription,
                Dose = medicament.Dose,
                Details = medicament.Details
            };
            prescriptionMedicaments.Add(newPrescriptionMedicament);
            added++;
        }
        return Task.FromResult(added);
    }

    public Task<PatientDataQuery> GetPatientInfo(int patientId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
