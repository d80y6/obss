using Obss.SharedKernel.Application.Contracts;

namespace Obss.Rating.Application.Services;

public interface IZteCdrParser
{
    Result<NormalizedCdrData> Parse(string rawPayload);
}
