namespace API.Models
{
    public class UserLike
    {
        #region Relação AppUser M-M UserLike
        public int SourceUserId { get; set; }
        public virtual AppUser? SourceUser { get; set; } //Quem deu like, O user que deu like
        #endregion
        #region Relação AppUser M-M UserLike
        public int TargetUserId { get; set; }
        public virtual AppUser? TargetUser { get; set; } //Quem levou like, O user que o "current user" gostou
        #endregion
    }
}
