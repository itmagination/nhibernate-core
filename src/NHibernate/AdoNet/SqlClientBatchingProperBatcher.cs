using System;
using System.Data;
using System.Data.Common;
using System.Text;
using NHibernate.AdoNet.Util;
using NHibernate.Exceptions;
using NHibernate.SqlCommand;
using NHibernate.Util;
using Environment = NHibernate.Cfg.Environment;

namespace NHibernate.AdoNet
{
    public class SqlClientBatchingProperBatcher : AbstractBatcher
	{
        private readonly int _defaultTimeout;
        private int _batchSize;
        private SqlClientSqlProperBatchingCommandSet _currentBatch;
        private StringBuilder _currentBatchCommandsLog;
        private int _totalExpectedRowsAffected;

        public SqlClientBatchingProperBatcher(ConnectionManager connectionManager, IInterceptor interceptor)
            : base(connectionManager, interceptor)
        {
            _batchSize = Factory.Settings.AdoBatchSize;
            _defaultTimeout = PropertiesHelper.GetInt32(Environment.CommandTimeout, Environment.Properties, -1);

            _currentBatch = CreateConfiguredBatch();
        }

        public override int BatchSize
        {
            get { return _batchSize; }
            set { _batchSize = value; }
        }

        protected override int CountOfStatementsInCurrentBatch
        {
            get { return _currentBatch.CountOfCommands; }
        }

        public override void AddToBatch(IExpectation expectation)
        {
            _totalExpectedRowsAffected += expectation.ExpectedRowCount;
            IDbCommand batchUpdate = CurrentCommand;
            Driver.AdjustCommand(batchUpdate);

            //Always append batch command with parameters to the current batch log - will be used in case of a db exception
            string lineWithParameters = GetFormattedCommandLineWithParameters(batchUpdate);
            _currentBatchCommandsLog.AppendFormat("Command {0}:", _currentBatch.CountOfCommands)
                                    .AppendLine(lineWithParameters);

            if (Log.IsDebugEnabled)
            {
                Log.Debug("Adding to batch:" + lineWithParameters);
            }
            _currentBatch.Append((System.Data.SqlClient.SqlCommand) batchUpdate);

            if (_currentBatch.CountOfCommands >= _batchSize)
            {
                ExecuteBatchWithTiming(batchUpdate);
            }
        }

        protected virtual string GetFormattedCommandLineWithParameters(IDbCommand batchUpdate)
        {
            SqlStatementLogger sqlStatementLogger = Factory.Settings.SqlStatementLogger;
            string lineWithParameters = sqlStatementLogger.GetCommandLineWithParameters(batchUpdate);

            FormatStyle formatStyle = sqlStatementLogger.DetermineActualStyle(FormatStyle.Basic);
            lineWithParameters = formatStyle.Formatter.Format(lineWithParameters);

            return lineWithParameters;
        }

        protected override void DoExecuteBatch(IDbCommand ps)
        {
            Log.DebugFormat("Executing batch");
            CheckReaders();
            Prepare(_currentBatch.BatchCommand);
            string currentBatchCommandsLog = _currentBatchCommandsLog.ToString();
            if (Factory.Settings.SqlStatementLogger.IsDebugEnabled)
            {
                Factory.Settings.SqlStatementLogger.LogBatchCommand(currentBatchCommandsLog);
            }

            int rowsAffected;
            try
            {
                rowsAffected = _currentBatch.ExecuteNonQuery();
            }
            catch (DbException e)
            {
                throw ADOExceptionHelper.Convert(Factory.SQLExceptionConverter, e, "could not execute batch command.",
                                                 new SqlString(currentBatchCommandsLog));
            }

            Expectations.VerifyOutcomeBatched(_totalExpectedRowsAffected, rowsAffected);

            _currentBatch.Dispose();
            _totalExpectedRowsAffected = 0;
            _currentBatch = CreateConfiguredBatch();
        }

        private SqlClientSqlProperBatchingCommandSet CreateConfiguredBatch()
        {
            var result = new SqlClientSqlProperBatchingCommandSet();
            if (_defaultTimeout > 0)
            {
                try
                {
                    result.CommandTimeout = _defaultTimeout;
                }
                catch (Exception e)
                {
                    if (Log.IsWarnEnabled)
                    {
                        Log.Warn(e.ToString());
                    }
                }
            }

            //we always create this, because we need to deal with a scenario in which
            //the user change the logging configuration at runtime. Trying to put this
            //behind an if(log.IsDebugEnabled) will cause a null reference exception 
            //at that point.
            _currentBatchCommandsLog = new StringBuilder().AppendLine("Batch commands:");

            return result;
        }
	}
}