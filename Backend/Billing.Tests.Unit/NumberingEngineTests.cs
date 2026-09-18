using Billing.Application.Services;
using Billing.Contracts.Numbering;
using Billing.Domain.Entities;
using Billing.Domain.Enums;
using Billing.Tests.Unit.Fakes;
using Xunit;

namespace Billing.Tests.Unit;

public class NumberingEngineTests
{
    private readonly FakeNumberingRepository _repository;
    private readonly NumberGenerationService _generationService;
    private readonly NumberingSettingService _settingService;

    public NumberingEngineTests()
    {
        _repository = new FakeNumberingRepository();
        _generationService = new NumberGenerationService(_repository);
        _settingService = new NumberingSettingService(_repository, _generationService);
    }

    [Fact]
    public void ReplaceTokens_StandardTokens_ReplacesCorrectly()
    {
        var testDate = new DateTime(2026, 9, 18, 12, 0, 0, DateTimeKind.Utc);

        var template = "{YEAR}-{MONTH}-{MM}-{DD}-Q{QUARTER}";
        var result = _generationService.ReplaceTokens(template, testDate);

        Assert.Equal("2026-Sep-09-18-QQ3", result);
    }

    [Fact]
    public void ReplaceTokens_IndianFinancialYear_ReplacesCorrectly()
    {
        // Sep 2026 is in FY 2026-2027 => "26-27"
        var testDate = new DateTime(2026, 9, 18, 0, 0, 0, DateTimeKind.Utc);
        var result = _generationService.ReplaceTokens("FY-{FY}-", testDate);
        Assert.Equal("FY-26-27-", result);

        // Feb 2027 is in FY 2026-2027 => "26-27"
        var febDate = new DateTime(2027, 2, 10, 0, 0, 0, DateTimeKind.Utc);
        var febResult = _generationService.ReplaceTokens("FY-{FY}-", febDate);
        Assert.Equal("FY-26-27-", febResult);
    }

    [Fact]
    public void FormatNumber_PrefixTokensSequenceSuffix_FormatsProperly()
    {
        var testDate = new DateTime(2026, 1, 15);
        var formatted = _generationService.FormatNumber("INV/", "{YYYY}/", 42, 5, "/FIN", testDate);

        // INV/2026/00042/FIN
        Assert.Equal("INV/2026/00042/FIN", formatted);
    }

    [Fact]
    public async Task GenerateNextNumberAsync_IncrementsSequenceAtomically()
    {
        var request = new GenerateNumberRequest
        {
            DocumentType = "Invoice",
            TransactionDate = new DateTime(2026, 5, 1)
        };

        var first = await _generationService.GenerateNextNumberAsync(request, tenantId: 1);
        Assert.True(first.Success);
        Assert.Equal(1, first.Data!.SequenceNumber);
        Assert.Equal("INV-2026-0001", first.Data.GeneratedNumber);

        var second = await _generationService.GenerateNextNumberAsync(request, tenantId: 1);
        Assert.True(second.Success);
        Assert.Equal(2, second.Data!.SequenceNumber);
        Assert.Equal("INV-2026-0002", second.Data.GeneratedNumber);
    }

    [Fact]
    public async Task UpdateSettingAsync_ModifiesFormatAndReflectsInPreview()
    {
        var updateReq = new UpdateNumberingSettingRequest
        {
            DocumentType = "Invoice",
            Prefix = "ACME-",
            Tokens = "{YY}{MM}-",
            SequenceLength = 6,
            NextNumber = 100,
            ResetPolicy = "Yearly"
        };

        var result = await _settingService.UpdateSettingAsync(updateReq, tenantId: 1);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("ACME-", result.Data.Prefix);
        Assert.Equal(6, result.Data.SequenceLength);

        // Preview should have 6 digits padded
        Assert.Contains("000100", result.Data.Preview);
    }

    [Fact]
    public void Preview_WithoutSaving_ReturnsCorrectStructure()
    {
        var preview = _settingService.Preview(new NumberPreviewRequest
        {
            DocumentType = "Credit Note",
            Prefix = "CN-",
            Tokens = "{YEAR}-",
            SequenceLength = 4,
            NextNumber = 7,
            Date = new DateTime(2026, 10, 1)
        });

        Assert.Equal("CN-2026-0007", preview.FullPreview);
        Assert.Equal("CN-", preview.Parts.Prefix);
        Assert.Equal("2026-", preview.Parts.Tokens);
        Assert.Equal("0007", preview.Parts.Sequence);
    }
}
