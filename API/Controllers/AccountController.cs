using API.Data;
using API.DTOs;
using API.Entities;
using API.interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly DataContext _context;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    public AccountController(DataContext context, ITokenService tokenService, IMapper mapper)
    {
        _context = context;
        _tokenService = tokenService;
        _mapper = mapper;
    }

    [HttpPost("Register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        if (await UserExists(registerDto.Username))
        {
            return BadRequest("Username is taken");
        }

        var user = _mapper.Map<AppUser>(registerDto);

        using var hmac = new HMACSHA512();

        user.UserName = registerDto.Username;
        user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
        user.PasswordSalt = hmac.Key;

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        var token = _tokenService.CreateToken(user);

        var userDto = GetUserDto(user, token);

        return userDto;
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await _context.Users
            .Include(x => x.Photos)
            .SingleOrDefaultAsync(x => x.UserName == loginDto.Username);

        if (user == null)
        {
            return Unauthorized("Invalid username");
        }

        using var hmac = new HMACSHA512(user.PasswordSalt);

        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        for (int i = 0; i < computedHash.Length; i++)
        {
            if (computedHash[i] != user.PasswordHash[i])
            {
                return Unauthorized("Invalid password");
            }
        }

        var token = _tokenService.CreateToken(user);

        var userDto = GetUserDto(user, token);

        return userDto;
    }

    private static UserDto GetUserDto(AppUser user, string token)
    {
        return new UserDto
        {
            Username = user.UserName,
            Token = token,
            PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url ?? "",
            KnownAs = user.KnownAs,
        };
    }

    private async Task<bool> UserExists(string username)
    {
        return await _context.Users.AnyAsync(u => u.UserName == username.ToLower());
    }
}
