using Billing.Application.Services;
using Billing.Contracts.Charges;
using Billing.Domain.Entities;
using Billing.Domain.Enums;
using Billing.Tests.Unit.Fakes;
using Xunit;

namespace Billing.Tests.Unit;

public class ChargesEngineTests
{
    private readonly FakeChargeRepository _repository;
    private readonly ChargeSettingService _settingService;
    private readonly ChargeCalculationService _calculationService;

    public ChargesEngineTests()
    {
        _repository = new FakeChargeRepository();
        _settingService = new ChargeSettingService(_repository);
        _calculationService = new ChargeCalculationService(_repository);
    }

    [Fact]
    public async Task CreateCharge_ValidRequest_CreatesSuccessfully()
    {
        var request = new CreateChargeRequest
        {
            Name = "Express Shipping",
            Code = "SHIP-EXP",
            ChargeType = "Shipping",
            CalculationType = "Fixed",
            Amount = 150.00m,
            IsTaxable = true,
            Status = "Active"
        };

        var result = await _settingService.CreateChargeAsync(request, tenantId: 1);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("SHIP-EXP", result.Data.Code);
        Assert.Equal(150.00m, result.Data.Amount);
        Assert.True(result.Data.IsTaxable);
    }

    [Fact]
    public async Task CreateCharge_DuplicateCode_ReturnsConflict()
    {
        var request = new CreateChargeRequest
        {
            Name = "Express Shipping",
            Code = "SHIP-DUP",
            Amount = 100m
        };

        await _settingService.CreateChargeAsync(request, tenantId: 1);
        var second = await _settingService.CreateChargeAsync(request, tenantId: 1);

        Assert.False(second.Success);
        Assert.Contains("already exists", second.Message);
    }

    [Fact]
    public async Task CalculateCharges_FixedAndPercentage_ComputesAccurately()
    {
        // 1. Fixed shipping charge
        await _repository.AddAsync(new ChargeConfiguration
        {
            TenantId = 1,
            Name = "Standard Shipping",
            Code = "SHIP-STD",
            ChargeType = ChargeType.Shipping,
            CalculationType = ChargeCalculationType.Fixed,
            Amount = 50.00m,
            IsTaxable = true,
            Status = "Active"
        });

        // 2. 2% convenience fee with max cap of 40
        await _repository.AddAsync(new ChargeConfiguration
        {
            TenantId = 1,
            Name = "Convenience Fee",
            Code = "CONV-2PCT",
            ChargeType = ChargeType.ConvenienceFee,
            CalculationType = ChargeCalculationType.Percentage,
            Amount = 2.00m,
            MaxChargeAmount = 40.00m,
            IsTaxable = true,
            Status = "Active"
        });

        // 3. Late fee non-taxable
        await _repository.AddAsync(new ChargeConfiguration
        {
            TenantId = 1,
            Name = "Late Fee",
            Code = "LATE-FEE",
            ChargeType = ChargeType.LateFee,
            CalculationType = ChargeCalculationType.Fixed,
            Amount = 20.00m,
            IsTaxable = false,
            Status = "Active"
        });

        var request = new CalculateChargesRequest
        {
            Subtotal = 1000.00m
        };

        var result = await _calculationService.CalculateChargesAsync(request, tenantId: 1);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);

        // Shipping = 50, Convenience = 2% of 1000 = 20 (below cap 40), LateFee = 20
        // Total = 50 + 20 + 20 = 90
        Assert.Equal(90.00m, result.Data.TotalCharges);
        Assert.Equal(70.00m, result.Data.TotalTaxableCharges); // 50 + 20
        Assert.Equal(20.00m, result.Data.TotalNonTaxableCharges); // 20
        Assert.Equal(3, result.Data.AppliedCharges.Count);
    }

    [Fact]
    public async Task CalculateCharges_RespectsMinInvoiceAmountThreshold()
    {
        await _repository.AddAsync(new ChargeConfiguration
        {
            TenantId = 1,
            Name = "High Order Processing",
            Code = "HIGH-ORDER",
            ChargeType = ChargeType.Handling,
            CalculationType = ChargeCalculationType.Fixed,
            Amount = 200.00m,
            MinInvoiceAmount = 5000.00m, // Only applies if order >= 5000
            Status = "Active"
        });

        var belowThreshold = await _calculationService.CalculateChargesAsync(new CalculateChargesRequest { Subtotal = 3000.00m }, tenantId: 1);
        Assert.Equal(0.00m, belowThreshold.Data!.TotalCharges);

        var aboveThreshold = await _calculationService.CalculateChargesAsync(new CalculateChargesRequest { Subtotal = 6000.00m }, tenantId: 1);
        Assert.Equal(200.00m, aboveThreshold.Data!.TotalCharges);
    }

    [Fact]
    public async Task CalculateCharges_PercentageCappedByMaxChargeAmount()
    {
        await _repository.AddAsync(new ChargeConfiguration
        {
            TenantId = 1,
            Name = "Capped Handling",
            Code = "CAP-HANDLING",
            ChargeType = ChargeType.Handling,
            CalculationType = ChargeCalculationType.Percentage,
            Amount = 10.00m, // 10%
            MaxChargeAmount = 50.00m, // Capped at 50
            Status = "Active"
        });

        // 10% of 1000 = 100, but cap is 50
        var result = await _calculationService.CalculateChargesAsync(new CalculateChargesRequest { Subtotal = 1000.00m }, tenantId: 1);
        Assert.Equal(50.00m, result.Data!.TotalCharges);
    }
}
