namespace HurryUpHaul.Contracts.Models
{
    public class Paging
    {
        public int PageSize { get; init; }
        public int PageNumber { get; init; }

        public Paging(int pageSize, int pageNumber)
        {
            PageSize = pageSize;
            PageNumber = pageNumber;
        }
    }
}