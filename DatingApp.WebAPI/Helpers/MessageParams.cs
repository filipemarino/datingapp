namespace DatingApp.WebAPI.Helpers
{
    public class MessageParams
    {
        private const int _maxPageSize = 50;
        
        public int PageNumber { get; set; } = 1;
        private int pageSize = 10;
        public int PageSize
        {
            get { return pageSize; }
            set { pageSize = (value > _maxPageSize) ? _maxPageSize : value; }
        } 
        public int UserId { get; set; }
        public string MessageContainer { get; set; } = "Unread";
    }
}