using MediatR;

namespace Obss.SharedKernel.Application.Authorization;

public interface IRequirePermission
{
    string RequiredPermission { get; }
}
