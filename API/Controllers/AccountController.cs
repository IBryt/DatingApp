using API.Data;
using API.DTOs;
using API.Entities;
using API.interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController : BaseApiController
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

        user.UserName = registerDto.Username;
     
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
            Gender = user.Gender,
        };
    }

    private async Task<bool> UserExists(string username)
    {
        return await _context.Users.AnyAsync(u => u.UserName == username.ToLower());
    }
}
