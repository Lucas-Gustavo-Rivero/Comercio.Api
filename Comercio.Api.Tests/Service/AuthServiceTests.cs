using AutoMapper;
using Comercio.Api.DTOs.User;
using Comercio.Api.Models;
using Comercio.Api.Repository;
using Comercio.Api.Service;
using Comercio.Api.Service.Results;
using Comercio.Api.Service.Token;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Comercio.Api.Tests.Service
{
    public class AuthServiceTests
    {
        private readonly Mock<IAuthRepository> _repositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IJwtTokenGenerator> _tokenGeneratorMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _repositoryMock = new Mock<IAuthRepository>();
            _mapperMock = new Mock<IMapper>();
            _tokenGeneratorMock = new Mock<IJwtTokenGenerator>();
            _authService = new AuthService(_repositoryMock.Object, _tokenGeneratorMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task RegisterAsync_UsuarioExiste_RetornaFailureConflict()
        {
            //ARRANGE
            var registerDto = new RegisterDto { Email = "test@hotmail.com" };
            _repositoryMock.Setup(repo => repo.UserExistsAsync(registerDto.Email)).ReturnsAsync(true);
            
            //ACT

            var result = await _authService.RegisterAsync(registerDto);

            //ASSERT
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Conflict, result.ErrorType);
            Assert.Equal("Ya existe un usuario con ese email.", result.ErrorMessage);
            _mapperMock.Verify(m => m.Map<User>(registerDto), Times.Never);
            _repositoryMock.Verify(repo => repo.CreateUserAsync(It.IsAny<User>()), Times.Never);
            _tokenGeneratorMock.Verify(t => t.GenerateToken(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_UsuarioNoExiste_RetornaSuccess()
        {
            //ARRANGE
            var registerDto = new RegisterDto { Email = "test@hotmail.com", Password = "test123456789" };
            var user = new User { Id = 1, Email = "test@hotmail.com", Rol = Roles.Usuario };
            string tokenGenerado = "token-falso-123";

            _repositoryMock.Setup(repo => repo.UserExistsAsync(registerDto.Email)).ReturnsAsync(false);
            _mapperMock.Setup(m => m.Map<User>(registerDto)).Returns(user);
            _tokenGeneratorMock.Setup(t => t.GenerateToken(user)).Returns(tokenGenerado);

            //ACT

            var result = await _authService.RegisterAsync(registerDto);

            //ASSERT

            Assert.True(result.IsSuccess);
            Assert.Equal(ErrorType.Ninguno, result.ErrorType);
            Assert.NotNull(result.Data);

            Assert.Equal(registerDto.Email, result.Data.Email);
            Assert.Equal("Usuario", result.Data.Rol);
            Assert.Equal(tokenGenerado, result.Data.Token);

            _repositoryMock.Verify(repo => repo.CreateUserAsync(user), Times.Once);
            _mapperMock.Verify(m => m.Map<User>(registerDto), Times.Once);
            _tokenGeneratorMock.Verify(t => t.GenerateToken(user), Times.Once);
            _repositoryMock.Verify(repo => repo.CreateUserAsync(It.Is<User>(u => !string.IsNullOrEmpty(u.PasswordHash) && u.PasswordHash != registerDto.Password)), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_DbUpdateException_RetornaFailureConflict()
        {
            //ARRANGE
            var registerDto = new RegisterDto { Email = "test@hotmail.com", Password = "test123456789" };
            var user = new User { Id = 1, Email = registerDto.Email };
            _repositoryMock.Setup(repo => repo.UserExistsAsync(registerDto.Email)).ReturnsAsync(false);
            _mapperMock.Setup(m => m.Map<User>(registerDto)).Returns(user);
            _repositoryMock.Setup(repo => repo.CreateUserAsync(user)).ThrowsAsync(new DbUpdateException());

            //ACT

            var result = await _authService.RegisterAsync(registerDto);

            //ASSERT

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Conflict, result.ErrorType);
            Assert.Equal("Ya existe un usuario con ese email.", result.ErrorMessage);

            _mapperMock.Verify(m => m.Map<User>(registerDto), Times.Once);
            _repositoryMock.Verify(repo => repo.CreateUserAsync(user), Times.Once);
            _tokenGeneratorMock.Verify(t => t.GenerateToken(user), Times.Never);

        }

        [Fact]
        public async Task LoginAsync_UsuarioNoExiste_RetornaFailureInvalidCredentials()
        {
            //ARRANGE
            var loginDto = new LoginDto { Email = "test@hotmail.com" };
            _repositoryMock.Setup(repo => repo.GetUserByEmailAsync(loginDto.Email)).ReturnsAsync((User?)null);

            //ACT

            var result = await _authService.LoginAsync(loginDto);

            //ASSERT

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.InvalidCredentials, result.ErrorType);
            Assert.Equal("Verifique sus datos de acceso e intente nuevamente.", result.ErrorMessage);

            _tokenGeneratorMock.Verify(t => t.GenerateToken(It.IsAny<User>()), Times.Never);

        }

        [Fact]
        public async Task LoginAsync_UsuarioExisteContraseñaInvalida_RetornaFailureInvalidCredentials()
        {
            //ARRANGE
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("PasswordCorrecto");
            var loginDto = new LoginDto {Email = "test@hotmail.com", Password = "PasswordIncorrecto" };
            var user = new User {Id = 1,Email = loginDto.Email ,PasswordHash = passwordHash };

            _repositoryMock.Setup(repo => repo.GetUserByEmailAsync(loginDto.Email)).ReturnsAsync(user);

            //ACT

            var result = await _authService.LoginAsync(loginDto);

            //ASSERT

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.InvalidCredentials, result.ErrorType);
            Assert.Equal("Verifique sus datos de acceso e intente nuevamente.", result.ErrorMessage);

            _tokenGeneratorMock.Verify(t => t.GenerateToken(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_UsuarioExisteContraseñaValida_RetornaSuccess()
        {
            //ARRANGE
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("PasswordCorrecto");
            var loginDto = new LoginDto { Email = "test@hotmail.com", Password = "PasswordCorrecto" };
            var user = new User { Id = 1, Email = loginDto.Email, PasswordHash = passwordHash , Rol = Roles.Usuario};
            string token = "token-falso-123";

            _repositoryMock.Setup(repo => repo.GetUserByEmailAsync(loginDto.Email)).ReturnsAsync(user);
            _tokenGeneratorMock.Setup(t => t.GenerateToken(user)).Returns(token);

            //ACT

            var result = await _authService.LoginAsync(loginDto);

            //ASSERT

            Assert.True(result.IsSuccess);
            Assert.Equal(ErrorType.Ninguno, result.ErrorType);

            Assert.NotNull(result.Data);
            Assert.Equal(user.Email, result.Data.Email);
            Assert.Equal("Usuario", result.Data.Rol);
            Assert.Equal(token, result.Data.Token);

            _tokenGeneratorMock.Verify(t => t.GenerateToken(user), Times.Once);
        }


    }
}
