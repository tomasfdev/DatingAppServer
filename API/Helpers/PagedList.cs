using Microsoft.EntityFrameworkCore;

namespace API.Helpers
{
    public class PagedList<T> : List<T>   //PagedList é uma generic type class que deriva de uma List também generic
    {
        public PagedList(IEnumerable<T> items, int count, int pageNumber, int pageSize)
        {
            CurrentPage = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            PageSize = pageSize;
            TotalCount = count;
            AddRange(items);    //acrescenta items à lista
        }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }

        public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = await source.CountAsync();  //analisa a query(IQueryable<T>) e vai contar quantos Models(<T>) existem na DB
            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(); //vai pegar todos os items existentes na DB

            return new PagedList<T>(items, count, pageNumber, pageSize);
        }
    }
}
