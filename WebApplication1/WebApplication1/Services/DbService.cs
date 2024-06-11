using Microsoft.EntityFrameworkCore;
using WebApplication1.Context;
using WebApplication1.DTOs;
using WebApplication1.Models;
using WebApplication1.ResponseModels;

namespace WebApplication1.Services;

public class DbService : IDbService
{
    private readonly ApplicationContext _context;

    public DbService(ApplicationContext context)
    {
        _context = context;
    }


    public async Task<bool> PatientExistsById(int patientID)
    {
        return await _context.Patients.AnyAsync(d => d.IdPatient.Equals(patientID));
    }

    public async Task<bool> DoctorExistsById(IssuePrescriptionCommand command)
    {
        return await _context.Doctors.AnyAsync(d => d.IdDoctor.Equals(command.IdDoctor));
    }

    public async Task<bool> PatientExistsByIdAndFullNameAsync(PatientPostDTO patientDTO, CancellationToken cancellationToken)
    {
        return await _context.Patients.AnyAsync(p => p.IdPatient.Equals(patientDTO.IdPatient)
                                                     && p.FirstName.Equals(patientDTO.FirstName)
                                                     && p.LastName.Equals(patientDTO.LastName), cancellationToken);
    }

    public async Task<int> PatientExistsByFullNameAsync(PatientPostDTO patientDto, CancellationToken cancellationToken)
    {
        return await _context.Patients.Where(p => p.FirstName.Equals(patientDto.FirstName)
                                                  && p.LastName.Equals(patientDto.LastName)
                                                  && p.Birthdate.Equals(patientDto.Birthdate)).Select(p => p.IdPatient)
            .SingleAsync(cancellationToken);
    }

    public async Task<int> CreatePatientAsync(PatientPostDTO patientDto, CancellationToken cancellationToken)
    {
        Patient patientToAdd = new Patient()
        {
            FirstName = patientDto.FirstName,
            LastName = patientDto.LastName,
            Birthdate = patientDto.Birthdate
        };
        await _context.Patients.AddAsync(patientToAdd, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return patientToAdd.IdPatient;
    }

    public async Task<bool> MedicamentExistsAsync(MedicamentsPostDTO medicamentDto, CancellationToken cancellationToken)
    {
        return await _context.Medicaments.AnyAsync(m => m.IdMedicament.Equals(medicamentDto.IdMedicament),
            cancellationToken);
    }

    public bool MedicamentListSizeSmallerThanEleven(IssuePrescriptionCommand command)
    {
        return command.medicaments.Count < 11;
    }

    public bool DueDateIsBigger(IssuePrescriptionCommand command)
    {
        return command.DueDate.CompareTo(command.Date) >= 0;
    }

    public async Task<int> AssignPrescriptionAsync(IssuePrescriptionCommand command,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            int patientId = command.patient.IdPatient;
            if (!await PatientExistsByIdAndFullNameAsync(command.patient, cancellationToken))
                try
                {
                    patientId = await PatientExistsByFullNameAsync(command.patient, cancellationToken);
                }
                catch (Exception e)
                {
                    patientId = await CreatePatientAsync(command.patient, cancellationToken);
                }


            foreach (var medic in command.medicaments)
            {
                if (!await MedicamentExistsAsync(medic, cancellationToken))
                    throw new Exception($"Medicament with id  {medic.IdMedicament} doesn't exist");
            }

            if (!MedicamentListSizeSmallerThanEleven(command))
                throw new Exception(
                    $"Prescription can have up to 10 medicaments. The input has {command.medicaments.Count}");

            if (!DueDateIsBigger(command))
                throw new Exception("The DueDate has to be later than the Date ");

            if (!await DoctorExistsById(command))
                throw new Exception("The Doctor doesnt exist");


            int idPrescription = await CreatePrescriptionAsync(command, patientId, cancellationToken);
            await CreatePrescriptionMedicamentsAsync(command, idPrescription, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return idPrescription;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<int> CreatePrescriptionAsync(IssuePrescriptionCommand command, int patientId,
        CancellationToken cancellationToken)
    {
        Prescription prescription = new Prescription
        {
            Date = command.Date,
            DueDate = command.DueDate,
            IdPatient = patientId,
            IdDoctor = command.IdDoctor
        };
        _context.Prescriptions.Add(prescription);
        await _context.SaveChangesAsync(cancellationToken);
        return prescription.IdPrescription;
    }

    public async Task<int> CreatePrescriptionMedicamentsAsync(IssuePrescriptionCommand command, int idPrescription,
        CancellationToken cancellationToken)
    {
        int added = 0;
        foreach (var medicament in command.medicaments)
        {
            await _context.PrescriptionMedicaments.AddAsync(new PrescriptionMedicament
            {
                IdMedicament = medicament.IdMedicament,
                IdPrescription = idPrescription,
                Dose = medicament.Dose,
                Details = medicament.Details
            });
            await _context.SaveChangesAsync(cancellationToken);
            added++;
        }

        return added;
    }

    public async Task<PatientDataQuery> GetPatientInfo(int patientId, CancellationToken cancellationToken)
    {
        if (! await PatientExistsById(patientId))
        {
            throw new Exception($"Patient with id {patientId} doesn't exist");

        }
        var patientData = await _context
            .Patients
            .Where(p => p.IdPatient.Equals(patientId))
            .Select(p => new PatientDataQuery()
            {
                IdPatient = p.IdPatient,
                FirstName = p.FirstName,
                LastName = p.LastName,
                Birthdate = p.Birthdate,
                Prescriptions = p.Prescriptions
                    .OrderBy(pre => pre.DueDate)
                    .Select(pre => new PrescriptionGetDTO()
                    {
                        IdPrescription = pre.IdPrescription,
                        Date =DateOnly.FromDateTime(pre.Date),
                        DueDate = DateOnly.FromDateTime(pre.DueDate),
                        Doctor = new DoctorGetDTO()
                        {
                            IdDoctor = pre.Doctor.IdDoctor,
                            FirstName = pre.Doctor.FirstName
                        },
                        Medicaments = pre.PrescriptionMedicaments
                            .Select(pm => new MedicamentGetDTO()
                            {
                                IdMedicament = pm.Medicament.IdMedicament,
                                Name = pm.Medicament.Name,
                                Dose = pm.Dose,
                                Details = pm.Details
                            }).ToList()
                    }).ToList()
            }).FirstOrDefaultAsync(cancellationToken);
        return patientData;
    }
}