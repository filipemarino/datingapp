using System;

namespace DatingApp.WebAPI.Models
{
    public class Photo
    {
        public int Id { get; set; }
        public string UrlPhoto { get; set; }
        public string Description { get; set; }
        public DateTime DateAdded { get; set; }
        public string PublicId { get; set; }
        public bool IsMain { get; set; }
        public int UserId { get; set; }
    }
}