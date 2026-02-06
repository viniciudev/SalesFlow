using Model.Registrations;
using System;
using System.Collections.Generic;

namespace Model
{
    public class User : BaseEntity
    {
        [Uppercase]
        public string Name { get; set; }
        public string CellPhone { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Role { get; set; }
        public Company Company { get; set; }
        public int IdCompany { get; set; }
        public TypeUser TypeUser { get; set; }
        public bool VerifiedEmail { get; set; } = false;
        public string TokenVerify { get; set; }
        public ICollection<UserPermission> UserPermissions { get; set; }
    }
    public enum TypeUser
    {
        client,
        provider,
        manager
    }
}
