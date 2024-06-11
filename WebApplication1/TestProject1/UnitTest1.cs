using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using WebApplication1.Context;
using WebApplication1.DTOs;
using WebApplication1.Models;
using WebApplication1.Services;
using Xunit;
namespace TestProject1;

public class UnitTest1
{

    private readonly IDbService _dbService;

    public UnitTest1()
    {
        _dbService = new FakeDbService();
    }

    [Fact]
    public async Task AssignPrescriptionAsync_ShouldThrowException_WhenMedicamentDoesNotExist()
    {
        // Arrange
        var command = new IssuePrescriptionCommand
        {
            patient = new PatientPostDTO
                { IdPatient = 1, FirstName = "John", LastName = "Doe", Birthdate = DateTime.Now.AddYears(-30) },
            medicaments = new List<MedicamentsPostDTO>
                { new MedicamentsPostDTO { IdMedicament = 99 } }, // Non-existing medicament
            Date = DateTime.Now,
            DueDate = DateTime.Now.AddDays(10),
            IdDoctor = 1
        };

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _dbService.AssignPrescriptionAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task AssignPrescriptionAsync_ShouldThrowException_WhenDueDateIsInvalid()
    {
        // Arrange
        var command = new IssuePrescriptionCommand
        {
            patient = new PatientPostDTO
                { IdPatient = 1, FirstName = "John", LastName = "Doe", Birthdate = DateTime.Now.AddYears(-30) },
            medicaments = new List<MedicamentsPostDTO> { new MedicamentsPostDTO { IdMedicament = 1 } },
            Date = DateTime.Now,
            DueDate = DateTime.Now.AddDays(-1), // Invalid DueDate
            IdDoctor = 1
        };

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _dbService.AssignPrescriptionAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task AssignPrescriptionAsync_ShouldThrowException_WhenMedicamentListExceedsLimit()
    {
        // Arrange
        var command = new IssuePrescriptionCommand
        {
            patient = new PatientPostDTO
                { IdPatient = 1, FirstName = "John", LastName = "Doe", Birthdate = DateTime.Now.AddYears(-30) },
            medicaments = new List<MedicamentsPostDTO>(),
            Date = DateTime.Now,
            DueDate = DateTime.Now.AddDays(10),
            IdDoctor = 1
        };

        for (int i = 0; i < 11; i++)
        {
            command.medicaments.Add(new MedicamentsPostDTO { IdMedicament = i + 1 });
        }

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _dbService.AssignPrescriptionAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task AssignPrescriptionAsync_ShouldCreatePatient_WhenPatientDoesNotExist()
    {
        // Arrange
        var command = new IssuePrescriptionCommand
        {
            patient = new PatientPostDTO
                { IdPatient = 0, FirstName = "New", LastName = "Patient", Birthdate = DateTime.Now.AddYears(-30) },
            medicaments = new List<MedicamentsPostDTO> { new MedicamentsPostDTO { IdMedicament = 1 } },
            Date = DateTime.Now,
            DueDate = DateTime.Now.AddDays(10),
            IdDoctor = 1
        };

        // Act
        int prescriptionId = await _dbService.AssignPrescriptionAsync(command, CancellationToken.None);

        // Assert
        Assert.True(prescriptionId > 0);
    }
    [Fact]
    public async Task AssignPrescriptionAsync_ShouldThrowException_WhenDoctorDoesNotExist()
    {
        // Arrange
        var command = new IssuePrescriptionCommand
        {
            patient = new PatientPostDTO { IdPatient = 1, FirstName = "John", LastName = "Doe", Birthdate = DateTime.Now.AddYears(-30) },
            medicaments = new List<MedicamentsPostDTO> { new MedicamentsPostDTO { IdMedicament = 1 } },
            Date = DateTime.Now,
            DueDate = DateTime.Now.AddDays(10),
            IdDoctor = 99 // Non-existing doctor
        };

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _dbService.AssignPrescriptionAsync(command, CancellationToken.None));
    }
    [Fact]
    public async Task AssignPrescriptionAsync_ShouldWorkCorrectly_WhenAllDataIsValid()
    {
        // Arrange
        var command = new IssuePrescriptionCommand
        {
            patient = new PatientPostDTO { IdPatient = 1, FirstName = "John", LastName = "Doe", Birthdate = DateTime.Now.AddYears(-30) },
            medicaments = new List<MedicamentsPostDTO> 
            { 
                new MedicamentsPostDTO { IdMedicament = 1, Dose = 1, Details = "Take one daily" } 
            },
            Date = DateTime.Now,
            DueDate = DateTime.Now.AddDays(10),
            IdDoctor = 1 // Existing doctor
        };

        // Act
        int prescriptionId = await _dbService.AssignPrescriptionAsync(command, CancellationToken.None);

        // Assert
        Assert.True(prescriptionId > 0);
    }
}
    