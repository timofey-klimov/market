using Auth.Domain.Storage.Transaction;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace Auth.Storage
{
    public class TransactionProvider(AuthContext context) : ITransactionProvider
    {
        public IDbTransaction BeginTransaction()
        {
            return context.Database.BeginTransaction().GetDbTransaction();
        }
    }
}
