﻿using API.Extensions;

namespace API.Models
{
    public class AppUser
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? KnownAs { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime LastActive { get; set; } = DateTime.Now;
        public string? Gender { get; set; }
        public string? Introduction { get; set; }
        public string? LookingFor { get; set; }
        public string? Interests { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }

        #region Relação AppUser 1-M Photo
        public virtual ICollection<Photo> Photos { get; set; }
        #endregion

        #region Relação AppUser M-M UserLike
        public virtual ICollection<UserLike>? LikedUsers { get; set; }   //users que gostou
        public virtual ICollection<UserLike>? LikedByUsers { get; set; } //users que gostaram
        #endregion

        #region Relação AppUser M-M Message
        public virtual ICollection<Message> MessagesSent { get; set; }
        public virtual ICollection<Message> MessagesReceived { get; set; }
        #endregion
    }
}
