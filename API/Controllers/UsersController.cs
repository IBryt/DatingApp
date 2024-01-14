using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UsersController(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
    {
        var members = await _userRepository.GetMembersAsync();
        return Ok(members);
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<MemberDto>> GetUsers(string username)
    {
        var member = await _userRepository.GetMemberAsync(username);
        return Ok(member);
    }
}
