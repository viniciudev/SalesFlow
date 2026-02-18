// Controllers/UserPermissionsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Model;
using Model.Enums;
using Model.Registrations;
using Repository;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserPermissionsController : ControllerBase
{
    private readonly IUserPermissionRepository _userPermissionRepository;

    public UserPermissionsController(IUserPermissionRepository userPermissionRepository)
    {
        _userPermissionRepository = userPermissionRepository;
    }

    // GET: api/userpermissions/{userId}
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserPermissions(int userId)
    {
        var permissions = await _userPermissionRepository.GetCodePermissions(userId);
            

        return Ok(permissions);
    }

    // GET: api/userpermissions/all
    [HttpGet("all")]

    public async Task<IActionResult> GetAllPermissions()
    {
        var permissions = await _userPermissionRepository.GetAll();
        return Ok(permissions);
    }

    // POST: api/userpermissions/{userId}
    [HttpPost("{userId}")]
 
    public async Task<IActionResult> UpdateUserPermissions(int userId, [FromBody] UpdatePermissionsDto dto)
    {
        // Remove todas as permissões atuais
        var userPermissions = await _userPermissionRepository.UserPermissions(userId);
        foreach (var item in userPermissions)
        {
            await _userPermissionRepository.DeleteAsync(item.Id);
        }
        //// Adiciona as novas permissões
        try
        {

      
        foreach (var permissionCode in dto.Permissions)
        {
                await _userPermissionRepository.CreateAsync(new UserPermission
                {
                    UserId = userId,
                    PermissionId = permissionCode
                });
        }
        }
        catch (Exception ex)
        {

            throw;
        }
        return Ok();
    }

    // GET: api/userpermissions/my-permissions
    [HttpGet("my-permissions")]
    public async Task<IActionResult> GetMyPermissions()

    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value);

        List<PermissionEnum> permissions = await _userPermissionRepository.GetCodePermissions(userId);
           
        return Ok(permissions);
    }
}

public class UpdatePermissionsDto
{
    public List<int> Permissions { get; set; } = new();
}