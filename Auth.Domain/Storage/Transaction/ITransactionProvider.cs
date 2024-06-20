using System.Data;

namespace Auth.Domain.Storage.Transaction
{
    public interface ITransactionProvider
    {
        IDbTransaction BeginTransaction();
    }
}
