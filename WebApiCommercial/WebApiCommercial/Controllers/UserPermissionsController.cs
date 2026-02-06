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
    [RequirePermission(PermissionEnum.USUARIO_VIEW)]
    public async Task<IActionResult> GetUserPermissions(int userId)
    {
        var permissions = await _userPermissionRepository.UserPermissions(userId);
            

        return Ok(permissions);
    }

    // GET: api/userpermissions/all
    [HttpGet("all")]
    [RequirePermission(PermissionEnum.USUARIO_PERMISSION_MANAGER)]
    public async Task<IActionResult> GetAllPermissions()
    {
        var permissions = await _userPermissionRepository.GetAll();
        return Ok(permissions);
    }

    // POST: api/userpermissions/{userId}
    [HttpPost("{userId}")]
    [RequirePermission(PermissionEnum.USUARIO_PERMISSION_MANAGER)]
    public async Task<IActionResult> UpdateUserPermissions(int userId, [FromBody] UpdatePermissionsDto dto)
    {
        // Remove todas as permissões atuais
        //var currentPermissions = await _context.UserPermissions
        //    .Where(up => up.UserId == userId)
        //    .ToListAsync();

        //_context.UserPermissions.RemoveRange(currentPermissions);

        //// Adiciona as novas permissões
        //foreach (var permissionCode in dto.Permissions)
        //{
        //    if (Enum.TryParse<PermissionEnum>(permissionCode, out var permissionEnum))
        //    {
        //        _context.UserPermissions.Add(new UserPermission
        //        {
        //            UserId = userId,
        //            PermissionId = permissionEnum
        //        });
        //    }
        //}

        //await _context.SaveChangesAsync();
        return Ok();
    }

    // GET: api/userpermissions/my-permissions
    [HttpGet("my-permissions")]
    public async Task<IActionResult> GetMyPermissions()
    {
        //var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        //var permissions = await _context.UserPermissions
        //    .Where(up => up.UserId == userId)
        //    .Select(up => up.Permission.Id.ToString())
        //    .ToListAsync();

        return Ok();
    }
}

public class UpdatePermissionsDto
{
    public List<string> Permissions { get; set; } = new();
}