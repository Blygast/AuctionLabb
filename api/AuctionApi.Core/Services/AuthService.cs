using AuctionApi.Core.Common.Exceptions;
using AuctionApi.Core.Entities;
using AuctionApi.Core.Interfaces;

namespace AuctionApi.Core.Services;

public class AuthService
{
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenService _tokens;

    public AuthService(IUnitOfWork uow, IPasswordHasher hasher, IJwtTokenService tokens)
    {
        _uow = uow;
        _hasher = hasher;
        _tokens = tokens;
    }

    public async Task<(User user, string token)> RegisterAsync(string name, string email, string password, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ValidationException("Name is required.");
        if (string.IsNullOrWhiteSpace(email)) throw new ValidationException("Email is required.");
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            throw new ValidationException("Password must be at least 6 characters.");

        var normalizedEmail = email.Trim().ToLowerInvariant();
        if (await _uow.Users.EmailExistsAsync(normalizedEmail))
            throw new ConflictException("A user with this email already exists.");

        var user = new User
        {
            Name = name.Trim(),
            Email = normalizedEmail,
            PasswordHash = _hasher.Hash(password),
            Role = "User",
            IsActive = true,
        };

        await _uow.Users.AddAsync(user, ct);
        await _uow.SaveChangesAsync(ct);

        return (user, _tokens.GenerateToken(user.Id, user.Name, user.Email, user.Role));
    }

    public async Task<(User user, string token)> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            throw new ValidationException("Email and password are required.");

        var user = await _uow.Users.GetByEmailAsync(email.Trim().ToLowerInvariant());
        if (user == null || !_hasher.Verify(password, user.PasswordHash))
            throw new ValidationException("Invalid email or password.");

        if (!user.IsActive)
            throw new ForbiddenException("This account has been deactivated.");

        return (user, _tokens.GenerateToken(user.Id, user.Name, user.Email, user.Role));
    }

    public async Task<User> GetMeAsync(int userId, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(userId, ct);
        if (user == null) throw new ForbiddenException("User no longer exists.");
        return user;
    }

    public async Task UpdatePasswordAsync(int userId, string currentPassword, string newPassword, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(currentPassword))
            throw new ValidationException("Current password is required.");
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            throw new ValidationException("New password must be at least 6 characters.");

        var user = await _uow.Users.GetByIdAsync(userId, ct);
        if (user == null) throw new ForbiddenException("User no longer exists.");

        if (!_hasher.Verify(currentPassword, user.PasswordHash))
            throw new ValidationException("Current password is incorrect.");

        user.PasswordHash = _hasher.Hash(newPassword);
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(ct);
    }
}
