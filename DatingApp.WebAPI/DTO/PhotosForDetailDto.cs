using System;

namespace DatingApp.WebAPI.DTO
{
    public class PhotosForDetailDto
    {
        public int Id { get; set; }
        public string UrlPhoto { get; set; }
        public string Description { get; set; }
        public DateTime DateAdded { get; set; }
        public bool IsMain { get; set; }
    }
}