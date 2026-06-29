using Mapster;
using MediatR;
using Obss.IAM.Application.Abstractions;
using Obss.IAM.Application.DTOs;
using Obss.IAM.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.IAM.Application.Commands.CreateUser;

public sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var tenantId = TenantId.Create(request.TenantId);
        var email = Email.Create(request.Email);

        var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (existingUser is not null)
            return Result.Failure<UserDto>(Error.Conflict($"Email '{request.Email}' is already registered."));

        var existingUsername = await _userRepository.GetByUsernameAsync(request.Username, request.TenantId, cancellationToken);

        if (existingUsername is not null)
            return Result.Failure<UserDto>(Error.Conflict($"Username '{request.Username}' is already taken."));

        PhoneNumber? phoneNumber = null;
        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            phoneNumber = PhoneNumber.Create(request.PhoneNumber, request.CountryCode ?? "+967");
        }

        var user = User.Create(
            tenantId,
            request.Username,
            email,
            request.FirstName,
            request.LastName,
            phoneNumber,
            request.ExternalId);

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(user.Adapt<UserDto>());
    }
}
