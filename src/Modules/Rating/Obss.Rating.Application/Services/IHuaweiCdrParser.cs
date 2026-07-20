using Obss.SharedKernel.Application.Contracts;

namespace Obss.Rating.Application.Services;

public interface IHuaweiCdrParser
{
    Result<NormalizedCdrData> Parse(string rawPayload);
}
