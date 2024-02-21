using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Data.Utils.Paging;

namespace Data.Common.PaginationModel;
public class PagingParam<TKey> where TKey : System.Enum
{
    private int _page = PagingConstants.DefaultPage;

    /// <summary>
    /// Gets or sets current page number.
    /// </summary>
    public int PageIndex
    {
        get => _page;
        set => _page = (value);
    }

    /// <summary>
    /// Gets or sets size of current page.
    /// </summary>
    [DefaultValue(PagingConstants.DefaultPageSize)]
    public int PageSize { get; set; } = PagingConstants.DefaultPageSize;


    [Description("Parameter use for sorting result. Value: {propertyName}")]
    public TKey SortKey { get; set; } = default(TKey);

    /// <summary>
    /// Gets or sets ordering criteria.
    /// </summary>
    [EnumDataType(typeof(PagingConstant.OrderCriteria))]
    [JsonConverter(typeof(PagingConstant.OrderCriteria))]
    public PagingConstant.OrderCriteria SortOrder { get; set; }
}