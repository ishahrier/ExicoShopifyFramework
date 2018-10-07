using System;
using System.Collections.Generic;
using System.Text;

namespace Exico.Shopify.Data.Framework
{
    public class PagedResult<T> where T : class
    {
        public const int DEFAULT_ITEMS_PER_PAGE = 50;
        public int PageNum { get; protected set; }
        public int ItemsPerPage { get; protected set; }
        public int TotalCount { get; protected set; }
        public List<T> ResultData { get; protected set; }
        public int ResultCount => this.ResultData.Count;

        private PagedResult() { }
        public PagedResult(List<T> resultData, int totalCount, int? page, int? itemsPerpage) : this()
        {
            this.ResultData = resultData;
            this.PageNum = page ?? 1;
            this.ItemsPerPage = itemsPerpage ?? DEFAULT_ITEMS_PER_PAGE;
            this.TotalCount = totalCount;
        }
        public int TotalPagesCount
        {
            get
            {
                if (TotalCount > 0)
                {
                    if (TotalCount <= ItemsPerPage)
                    {
                        return 1;
                    }
                    else
                    {
                        return (TotalCount / ItemsPerPage) + (TotalCount % ItemsPerPage > 0 ? 1 : 0);
                    }
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}
