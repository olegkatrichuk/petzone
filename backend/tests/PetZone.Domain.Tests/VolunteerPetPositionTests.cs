using System;
using System.Linq;
using PetZone.Volunteers.Domain.Models;
using Xunit;

namespace PetZone.Domain.Tests;

public class VolunteerPetPositionTests
{
    // --- ХЕЛПЕРЫ ---

    private static Volunteer CreateVolunteer()
    {
        var name = FullName.Create("John", "Doe").Value;
        var email = Email.Create("john@test.com").Value;
        var experience = Experience.Create(3).Value;
        var phone = PhoneNumber.Create("+380991234567").Value;

        return Volunteer.Create(Guid.NewGuid(), Guid.NewGuid(), name, email,
            "Test volunteer description", experience, phone).Value;
    }

    private static Pet CreatePet(string nickname = "Buddy")
    {
        var health = HealthInfo.Create("Healthy").Value;
        var address = Address.Create("Kyiv", "Main Street").Value;
        var weight = Weight.Create(5).Value;
        var height = Height.Create(30).Value;
        var phone = PhoneNumber.Create("+380991234567").Value;
        var speciesBreed = SpeciesBreed.Create(Guid.NewGuid(), Guid.NewGuid()).Value;

        return Pet.Create(
            Guid.NewGuid(), nickname, "Test description", "Brown",
            health, address, weight, height, phone,
            false, DateTime.UtcNow.AddYears(-1), false,
            HelpStatus.LookingForHome, null, null, null, speciesBreed).Value;
    }

    // --- ТЕСТЫ ДОБАВЛЕНИЯ ---

    [Fact]
    public void AddPet_FirstPet_ShouldHavePosition1()
    {
        var volunteer = CreateVolunteer();
        var pet = CreatePet();

        volunteer.AddPet(pet);

        Assert.Equal(1, pet.Position);
    }

    [Fact]
    public void AddPet_ThreePets_ShouldHaveSequentialPositions()
    {
        var volunteer = CreateVolunteer();
        var pet1 = CreatePet("Pet1");
        var pet2 = CreatePet("Pet2");
        var pet3 = CreatePet("Pet3");

        volunteer.AddPet(pet1);
        volunteer.AddPet(pet2);
        volunteer.AddPet(pet3);

        Assert.Equal(1, pet1.Position);
        Assert.Equal(2, pet2.Position);
        Assert.Equal(3, pet3.Position);
    }

    [Fact]
    public void AddPet_Duplicate_ShouldReturnError()
    {
        var volunteer = CreateVolunteer();
        var pet = CreatePet();

        volunteer.AddPet(pet);
        var result = volunteer.AddPet(pet);

        Assert.True(result.IsFailure);
        Assert.Equal("volunteer.pet_already_exists", result.Error.Code);
    }

    // --- ТЕСТЫ УДАЛЕНИЯ ---

    [Fact]
    public void RemovePet_Middle_ShouldRecalculatePositions()
    {
        var volunteer = CreateVolunteer();
        var pet1 = CreatePet("Pet1");
        var pet2 = CreatePet("Pet2");
        var pet3 = CreatePet("Pet3");

        volunteer.AddPet(pet1);
        volunteer.AddPet(pet2);
        volunteer.AddPet(pet3);

        volunteer.RemovePet(pet2);

        Assert.Equal(1, pet1.Position);
        Assert.Equal(2, pet3.Position); // сдвинулся с 3 на 2
        Assert.Equal(2, volunteer.Pets.Count);
    }

    [Fact]
    public void RemovePet_NotExisting_ShouldReturnError()
    {
        var volunteer = CreateVolunteer();
        var pet = CreatePet();

        var result = volunteer.RemovePet(pet);

        Assert.True(result.IsFailure);
        Assert.Equal("volunteer.pet_not_found", result.Error.Code);
    }

    [Fact]
    public void RemovePet_First_ShouldRecalculatePositions()
    {
        var volunteer = CreateVolunteer();
        var pet1 = CreatePet("Pet1");
        var pet2 = CreatePet("Pet2");
        var pet3 = CreatePet("Pet3");

        volunteer.AddPet(pet1);
        volunteer.AddPet(pet2);
        volunteer.AddPet(pet3);

        volunteer.RemovePet(pet1);

        Assert.Equal(1, pet2.Position);
        Assert.Equal(2, pet3.Position);
    }

    // --- ТЕСТЫ ПЕРЕМЕЩЕНИЯ ---

    [Fact]
    public void MovePet_Forward_ShouldShiftOthersDown()
    {
        // Arrange: 1,2,3,4,5
        var volunteer = CreateVolunteer();
        var pets = Enumerable.Range(1, 5)
            .Select(i => CreatePet($"Pet{i}"))
            .ToList();

        foreach (var p in pets) volunteer.AddPet(p);

        // Act: двигаем Pet5 (позиция 5) → позиция 2
        volunteer.MovePet(pets[4], 2);

        // Assert
        Assert.Equal(1, pets[0].Position); // Pet1 остался
        Assert.Equal(2, pets[4].Position); // Pet5 теперь на 2
        Assert.Equal(3, pets[1].Position); // Pet2 сдвинулся на 3
        Assert.Equal(4, pets[2].Position); // Pet3 сдвинулся на 4
        Assert.Equal(5, pets[3].Position); // Pet4 сдвинулся на 5
    }

    [Fact]
    public void MovePet_Backward_ShouldShiftOthersUp()
    {
        // Arrange: 1,2,3,4,5
        var volunteer = CreateVolunteer();
        var pets = Enumerable.Range(1, 5)
            .Select(i => CreatePet($"Pet{i}"))
            .ToList();

        foreach (var p in pets) volunteer.AddPet(p);

        // Act: двигаем Pet2 (позиция 2) → позиция 4
        volunteer.MovePet(pets[1], 4);

        // Assert
        Assert.Equal(1, pets[0].Position); // Pet1 остался
        Assert.Equal(2, pets[2].Position); // Pet3 сдвинулся на 2
        Assert.Equal(3, pets[3].Position); // Pet4 сдвинулся на 3
        Assert.Equal(4, pets[1].Position); // Pet2 теперь на 4
        Assert.Equal(5, pets[4].Position); // Pet5 остался
    }

    [Fact]
    public void MovePetToFirst_ShouldBecomePosition1()
    {
        var volunteer = CreateVolunteer();
        var pet1 = CreatePet("Pet1");
        var pet2 = CreatePet("Pet2");
        var pet3 = CreatePet("Pet3");

        volunteer.AddPet(pet1);
        volunteer.AddPet(pet2);
        volunteer.AddPet(pet3);

        volunteer.MovePetToFirst(pet3);

        Assert.Equal(1, pet3.Position);
        Assert.Equal(2, pet1.Position);
        Assert.Equal(3, pet2.Position);
    }

    [Fact]
    public void MovePetToLast_ShouldBecomeLastPosition()
    {
        var volunteer = CreateVolunteer();
        var pet1 = CreatePet("Pet1");
        var pet2 = CreatePet("Pet2");
        var pet3 = CreatePet("Pet3");

        volunteer.AddPet(pet1);
        volunteer.AddPet(pet2);
        volunteer.AddPet(pet3);

        volunteer.MovePetToLast(pet1);

        Assert.Equal(1, pet2.Position);
        Assert.Equal(2, pet3.Position);
        Assert.Equal(3, pet1.Position);
    }

    [Fact]
    public void MovePet_SamePosition_ShouldNotChangeAnything()
    {
        var volunteer = CreateVolunteer();
        var pet1 = CreatePet("Pet1");
        var pet2 = CreatePet("Pet2");

        volunteer.AddPet(pet1);
        volunteer.AddPet(pet2);

        var result = volunteer.MovePet(pet1, 1);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, pet1.Position);
        Assert.Equal(2, pet2.Position);
    }

    [Fact]
    public void MovePet_InvalidPosition_ShouldReturnError()
    {
        var volunteer = CreateVolunteer();
        var pet = CreatePet();

        volunteer.AddPet(pet);

        var result = volunteer.MovePet(pet, 99);

        Assert.True(result.IsFailure);
        Assert.Equal("volunteer.invalid_position", result.Error.Code);
    }

    [Fact]
    public void MovePet_PositionZero_ShouldReturnError()
    {
        var volunteer = CreateVolunteer();
        var pet = CreatePet();

        volunteer.AddPet(pet);

        var result = volunteer.MovePet(pet, 0);

        Assert.True(result.IsFailure);
        Assert.Equal("volunteer.invalid_position", result.Error.Code);
    }
}