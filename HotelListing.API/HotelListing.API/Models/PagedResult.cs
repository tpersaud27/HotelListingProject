namespace HotelListing.API.Models
{
    public class PagedResult<T> 
    {
        // This indicates how many records are available
        public int TotalCount { get; set; }
        // The page number of the record
        public int PageNumber { get; set; }
        // Number of records we are getting back
        public int RecordNumber { get; set; }
        public List<T> Items { get; set; }

    }
}
