using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealEstateCRM.Models.Common
{
    /// <summary>
    /// Типове имоти в системата.
    /// </summary>
    public enum PropertyType
    {
        Apartment = 0,
        House = 1,
        Office = 2,
        Land = 3,
        Other = 9
    }

    /// <summary>
    /// Статус на дадена обява.
    /// </summary>
    public enum ListingStatus
    {
        Draft = 0,        // още не е публикувана
        Active = 1,       // активно обявена
        UnderOffer = 2,   // има предложен купувач
        Sold = 3,         // продадена
        Withdrawn = 4     // изтеглена от пазара
    }

    /// <summary>
    /// Тип на клиента (купувач/продавач/наемодател и т.н.).
    /// </summary>
    public enum ClientType
    {
        Buyer = 0,
        Seller = 1,
        Landlord = 2,
        Tenant = 3
    }

    public enum VisitStatus
    {
        Scheduled = 0,
        Confirmed = 1,
        Completed = 2,
        Cancelled = 3,
        NoShow = 4
    }


}