using SabakaBank.Backend.Domain.Entities;

namespace SabakaBank.Backend.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}