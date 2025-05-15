using Antifraud.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antifraud.Service.Implementation
{
    public class AntifraudService : IAntifraudService
    {
        public Task<bool> VerifyTransaction(Guid eventId)
        {
            throw new NotImplementedException();
        }
    }
}
