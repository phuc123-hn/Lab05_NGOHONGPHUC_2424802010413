using FluentValidation;
using MediatR;
using PricingService.Api.Commands;
using PricingService.Api.Commands.Dto;
using PricingService.Domain;

namespace PricingService.Commands;

public class CalculatePriceHandler : IRequestHandler<CalculatePriceCommand, CalculatePriceResult>
{
    private readonly IDataStore dataStore;

    public CalculatePriceHandler(IDataStore dataStore)
    {
        this.dataStore = dataStore;
    }

    public async Task<CalculatePriceResult> Handle(CalculatePriceCommand request, CancellationToken cancellationToken)
    {
        await new CalculatePriceCommandValidator().ValidateAndThrowAsync(request, cancellationToken);

        var tariff = await dataStore.Tariffs[request.ProductCode];

        var subject = request.Answers.ToDictionary(answer => answer.QuestionCode, GetAnswerValue);

        var calculation = new Calculation(
            request.ProductCode,
            request.PolicyFrom,
            request.PolicyTo,
            request.SelectedCovers,
            subject);

        var price = tariff.CalculatePrice(calculation);

        return new CalculatePriceResult
        {
            TotalPrice = price.TotalPremium,
            CoverPrices = price.Covers.ToDictionary(cover => cover.Key, cover => cover.Value.Price)
        };
    }

    // NumericQuestionAnswer.Answer là số, TextQuestionAnswer.Answer là chuỗi
    private static object GetAnswerValue(QuestionAnswer answer)
    {
        dynamic dynamicAnswer = answer;
        return (object)dynamicAnswer.Answer;
    }
}
