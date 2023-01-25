namespace HotelListing.API.Middleware
{
    public class ErrorDetails 
    {
        // This is the type of error (not found, null, etc)
        public string ErrorType { get; set; }
        // This is the message associated to the error
        public string ErrorMessage { get; set; } 

    }

}
