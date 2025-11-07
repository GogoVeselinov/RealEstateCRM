using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealEstateCRM.Data
{
    public interface IDbInitializer
    {
        Task InitializeAsync(CancellationToken ct = default);
    }
}