using AutoMapper;
using Comercio.Api.DTOs.User;
using Comercio.Api.Models;
using Comercio.Api.Repository;
using Comercio.Api.Service.Results;
using Comercio.Api.Service.Token;
using Microsoft.EntityFrameworkCore;

namespace Comercio.Api.Service
{
    public class AuthService:IAuthService
    {
        private readonly IAuthRepository _repository;
        private readonly IJwtTokenGenerator _tokenGenerator;
        private readonly IMapper _mapper;


        public AuthService(IAuthRepository repository, IJwtTokenGenerator tokenGenerator, IMapper mapper)
        {
            _repository = repository;
            _tokenGenerator = tokenGenerator;
            _mapper = mapper;
        }

        public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto)
        {
            var user = await _repository.GetUserByEmailAsync(dto.Email);

            if(user == null)
            {
                return Result<AuthResponseDto>.Failure("Verifique sus datos de acceso e intente nuevamente.", ErrorType.InvalidCredentials);
            }

            var passwordValida = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

            if(!passwordValida)
            {
                return Result<AuthResponseDto>.Failure("Verifique sus datos de acceso e intente nuevamente.", ErrorType.InvalidCredentials);
            }

            var token = _tokenGenerator.GenerateToken(user);

            return Result<AuthResponseDto>.Success(new AuthResponseDto { Email = user.Email, Rol = user.Rol.ToString(), Token = token });
        }

        public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto dto)
        {
            var result = await _repository.UserExistsAsync(dto.Email);

            if(result)
            {
                return Result<AuthResponseDto>.Failure("Ya existe un usuario con ese email.", ErrorType.Conflict);
            }

            var user = _mapper.Map<User>(dto);

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            try
            {
                await _repository.CreateUserAsync(user);
            }
            catch (DbUpdateException)
            {

                return Result<AuthResponseDto>.Failure("Ya existe un usuario con ese email.", ErrorType.Conflict);
            }

            var token = _tokenGenerator.GenerateToken(user);

            return Result<AuthResponseDto>.Success(new AuthResponseDto { Email = user.Email, Rol = user.Rol.ToString(), Token = token });
        }
    }
}
