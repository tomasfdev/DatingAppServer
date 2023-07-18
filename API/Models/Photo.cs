using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("Photos")]
    public class Photo
    {
        public int Id { get; set; }
        public string? Url { get; set; }
        public bool IsMain { get; set; }
        public string? PublicId { get; set; }
        #region AppUser M-1 Photo
        public int AppUserId { get; set; }
        public virtual AppUser AppUser { get; set; }
        #endregion
    }
}