using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MagniseTest.Domain.Entities;

namespace MagniseTest.Application.Interfaces
{
    public interface IFintachartsInstrumentService
    {
        Task<List<Asset>> GetInstrumentsAsync();
    }
}
