using System;
namespace Data.Utils.Paging;

/// <summary>
/// Contains constant classes related to Pagination.
/// </summary>
public static class PagingConstant
{
    /// <summary>
    /// Contains property length related to Paging
    /// </summary>
    public static class FixedPagingConstant
    {
        /// <summary>
        /// LimitationPageSize
        /// </summary>
        public const int MaxPageSize = 500;
    }

    public enum OrderCriteria
    {
        /// <summary>
        /// descendant
        /// </summary>
        DESC,

        /// <summary>
        /// ascendant
        /// </summary>
        ASC,
    }
}

