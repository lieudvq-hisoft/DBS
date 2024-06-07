using System;
namespace Data.Models
{
    public class PagingModel
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalPage { get; set; }
        public int TotalSize { get; set; }
        public int PageSkip { get; set; }
        public object Data { get; set; }

        public PagingModel()
        {

        }

        public PagingModel(int pageIndex, int pageSize, int totalSize)
        {
            PageIndex = pageIndex <= 0 ? 1 : pageIndex;
            PageSize = pageSize <= 0 ? 5 : pageSize;
            TotalSize = totalSize;
            TotalPage = (int)Math.Ceiling(TotalSize / (double)PageSize);
            PageSkip = (PageIndex - 1) * PageSize;
        }
    }
}

