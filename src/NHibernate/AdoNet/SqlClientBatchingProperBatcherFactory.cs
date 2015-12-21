using NHibernate.Engine;

namespace NHibernate.AdoNet
{
    public class SqlClientBatchingProperBatcherFactory : SqlClientBatchingBatcherFactory
	{
		public override IBatcher CreateBatcher(ConnectionManager connectionManager, IInterceptor interceptor)
		{
            return new SqlClientBatchingProperBatcher(connectionManager, interceptor);
		}
	}
}