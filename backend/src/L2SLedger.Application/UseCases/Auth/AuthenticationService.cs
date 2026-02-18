using AutoMapper;
using L2SLedger.Application.DTOs.Auth;
using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Constants;
using L2SLedger.Domain.Entities;
using L2SLedger.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace L2SLedger.Application.UseCases.Auth;

/// <summary>
/// Implementação do serviço de autenticação.
/// Conforme ADR-001, ADR-002 e ADR-020 (Clean Architecture).
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IFirebaseAuthService _firebaseAuthService;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        IFirebaseAuthService firebaseAuthService,
        IUserRepository userRepository,
        IMapper mapper,
        ILogger<AuthenticationService> logger)
    {
        _firebaseAuthService = firebaseAuthService;
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando processo de login");

        // Validar Firebase ID Token
        var firebaseUser = await _firebaseAuthService.ValidateTokenAsync(
            request.FirebaseIdToken,
            cancellationToken);

        _logger.LogInformation("Token Firebase validado para usuário {FirebaseUid}", firebaseUser.Uid);

        // Verificar se email está verificado (ADR-002)
        if (!firebaseUser.EmailVerified)
        {
            _logger.LogWarning("Tentativa de login com email não verificado: {Email}", firebaseUser.Email);
            throw new AuthenticationException(
                ErrorCodes.AUTH_EMAIL_NOT_VERIFIED,
                "Email não verificado. Verifique seu email antes de fazer login.");
        }

        // Buscar ou criar usuário interno
        var user = await _userRepository.GetByFirebaseUidAsync(firebaseUser.Uid, cancellationToken);

        if (user == null)
        {
            _logger.LogInformation("Criando novo usuário interno para {FirebaseUid}", firebaseUser.Uid);

            user = new User(
                firebaseUser.Uid,
                firebaseUser.Email,
                firebaseUser.DisplayName ?? firebaseUser.Email,
                firebaseUser.EmailVerified);

            user = await _userRepository.AddAsync(user, cancellationToken);

            _logger.LogInformation("Usuário criado com ID {UserId}", user.Id);
        }
        else
        {
            _logger.LogInformation("Usuário existente encontrado: {UserId}", user.Id);

            // Atualizar verificação de email se necessário
            if (firebaseUser.EmailVerified && !user.EmailVerified)
            {
                user.VerifyEmail();
                await _userRepository.UpdateAsync(user, cancellationToken);
            }
        }

        // Verificar status do usuário (user-status-plan.md)
        if (user.Status != UserStatus.Active)
        {
            _logger.LogWarning(
                "Tentativa de login com usuário inativo: {UserId}, Status: {Status}",
                user.Id,
                user.Status);

            var errorCode = user.Status switch
            {
                UserStatus.Pending => ErrorCodes.AUTH_USER_PENDING,
                UserStatus.Suspended => ErrorCodes.AUTH_USER_SUSPENDED,
                UserStatus.Rejected => ErrorCodes.AUTH_USER_REJECTED,
                _ => ErrorCodes.AUTH_USER_INACTIVE
            };

            var message = user.Status switch
            {
                UserStatus.Pending => "Seu cadastro está aguardando aprovação do administrador.",
                UserStatus.Suspended => "Sua conta foi suspensa. Entre em contato com o administrador.",
                UserStatus.Rejected => "Seu cadastro foi rejeitado. Entre em contato com o administrador.",
                _ => "Sua conta está inativa."
            };

            throw new AuthenticationException(errorCode, message);
        }

        var userDto = _mapper.Map<UserDto>(user);

        return new LoginResponse
        {
            User = userDto
        };
    }

    public async Task<CurrentUserResponse> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user == null)
        {
            throw new AuthenticationException(
                ErrorCodes.AUTH_USER_NOT_FOUND,
                "Usuário não encontrado");
        }

        var userDto = _mapper.Map<UserDto>(user);

        return new CurrentUserResponse
        {
            User = userDto
        };
    }
}
