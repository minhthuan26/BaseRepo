using Microsoft.EntityFrameworkCore;

namespace CommonLibs.Utils
{
    public class Pagination<T> : List<T>
    {
        public int PageIndex { get; set; }
        public int TotalPage { get; set; }

        public Pagination(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPage = (int)Math.Ceiling(count / (double)pageSize);
            AddRange(items);
        }

        public static Pagination<T> Create(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = source.Count();
            var items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

            return new Pagination<T>(items, count, pageIndex, pageSize);
        }

        public static async Task<Pagination<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize, CancellationToken cancellationToken = default)
        {
            var count = await source.CountAsync(cancellationToken);
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

            return new Pagination<T>(items, count, pageIndex, pageSize);
        }
    }

    public class FilterResponse<T>
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalPage { get; set; }
        public int TotalCount { get; set; }
        public List<T> Items { get; set; } = new List<T>();

        public FilterResponse(Pagination<T> pagination, int pageSize, int totalCount)
        {
            PageIndex = pagination.PageIndex;
            PageSize = pageSize;
            TotalPage = pagination.TotalPage;
            TotalCount = totalCount;
            Items = pagination;
        }
    }
}
