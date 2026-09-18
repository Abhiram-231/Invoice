using Billing.Application.Interfaces;
using Billing.Contracts;
using Billing.Contracts.Charges;
using Billing.Domain.Entities;
using Billing.Domain.Enums;

namespace Billing.Application.Services;

public class ChargeCalculationService : IChargeCalculationService
{
    private readonly IChargeRepository _chargeRepository;

    public ChargeCalculationService(IChargeRepository chargeRepository)
    {
        _chargeRepository = chargeRepository;
    }

    public async Task<ApiResponse<ChargeCalculationResultDto>> CalculateChargesAsync(CalculateChargesRequest request, int tenantId)
    {
        if (tenantId <= 0)
        {
            return ApiResponse<ChargeCalculationResultDto>.Fail("Invalid tenant identifier", "Tenant ID must be greater than 0.");
        }

        if (request == null || request.Subtotal < 0)
        {
            return ApiResponse<ChargeCalculationResultDto>.Fail("Validation failed", "Subtotal must be non-negative.");
        }

        var allCharges = await _chargeRepository.GetAllAsync(tenantId, activeOnly: true);

        IEnumerable<ChargeConfiguration> targetCharges = allCharges;

        if (request.SelectedChargeIds != null && request.SelectedChargeIds.Any())
        {
            targetCharges = targetCharges.Where(c => request.SelectedChargeIds.Contains(c.Id));
        }
        else if (request.SelectedChargeCodes != null && request.SelectedChargeCodes.Any())
        {
            var codes = request.SelectedChargeCodes.Select(c => c.Trim().ToUpperInvariant()).ToHashSet();
            targetCharges = targetCharges.Where(c => codes.Contains(c.Code.ToUpperInvariant()));
        }

        var result = Calculate(request.Subtotal, targetCharges);
        return ApiResponse<ChargeCalculationResultDto>.Ok(result, "Charges calculated successfully.");
    }

    public ChargeCalculationResultDto Calculate(decimal subtotal, IEnumerable<ChargeConfiguration> charges)
    {
        var result = new ChargeCalculationResultDto
        {
            Subtotal = subtotal
        };

        if (charges == null) return result;

        foreach (var charge in charges.Where(c => c.IsActive))
        {
            // If minimum invoice threshold is configured, check if subtotal satisfies it
            if (charge.MinInvoiceAmount.HasValue && subtotal < charge.MinInvoiceAmount.Value)
            {
                continue;
            }

            decimal calculated = 0m;
            if (charge.CalculationType == ChargeCalculationType.Fixed)
            {
                calculated = charge.Amount;
            }
            else if (charge.CalculationType == ChargeCalculationType.Percentage)
            {
                calculated = Math.Round(subtotal * (charge.Amount / 100.00m), 2, MidpointRounding.AwayFromZero);

                // Check max charge cap
                if (charge.MaxChargeAmount.HasValue && calculated > charge.MaxChargeAmount.Value)
                {
                    calculated = charge.MaxChargeAmount.Value;
                }
            }

            if (calculated <= 0) continue;

            result.AppliedCharges.Add(new AppliedChargeDto
            {
                Id = charge.Id,
                Name = charge.Name,
                Code = charge.Code,
                ChargeType = charge.ChargeType.ToString(),
                CalculationType = charge.CalculationType.ToString(),
                Rate = charge.Amount,
                CalculatedAmount = calculated,
                IsTaxable = charge.IsTaxable,
                TaxCategory = charge.TaxCategory
            });

            result.TotalCharges += calculated;
            if (charge.IsTaxable)
            {
                result.TotalTaxableCharges += calculated;
            }
            else
            {
                result.TotalNonTaxableCharges += calculated;
            }
        }

        result.TotalCharges = Math.Round(result.TotalCharges, 2, MidpointRounding.AwayFromZero);
        result.TotalTaxableCharges = Math.Round(result.TotalTaxableCharges, 2, MidpointRounding.AwayFromZero);
        result.TotalNonTaxableCharges = Math.Round(result.TotalNonTaxableCharges, 2, MidpointRounding.AwayFromZero);

        return result;
    }
}
