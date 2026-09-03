using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Domain.Entities.User;
using TraceFlow.Api.Infrastructure.Persistence;
using TraceFlow.Api.Application.Common.Security;
using TraceFlow.api.Application.Auth.Register;

namespace TraceFlow.Api.Application.Users.Commands.CreateUser;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly PasswordHasher _passwordHasher;
    public RegisterCommandHandler(AppDbContext dbContext, PasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }
    public async Task<RegisterResponse> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        var emailExists = await _dbContext.Users
            .AnyAsync(
                user => user.Email == request.Email,
                cancellationToken);

        if (emailExists)
        {
            throw new InvalidOperationException(
                "A user with this email already exists.");
        }

        var passwordHash = _passwordHasher.Hash(request.Password);

        var user = new User(
            request.Email,
            request.UserName,
            request.FirstName,
            request.LastName,
            passwordHash
            );

        _dbContext.Users.Add(user);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        var reponse = new RegisterResponse(
            request.Email,
            request.UserName,
            request.FirstName,
            request.LastName
        );

        return reponse;
    }
}