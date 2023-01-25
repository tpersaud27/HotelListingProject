namespace HotelListing.API.Models
{
    public class QueryParameters
    {

        private int _pageSize = 15;

        public int StartIndex { get; set; }
        public int PageNumber { get; set; }

        // This is for when the user wants to set the amount of records per page
        // They can send it over in the query parameters
        public int PageSize
        {
            get { return _pageSize; }
            set { _pageSize = value; }
        }

    }
}
