using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace NHibernate.AdoNet
{
	using Action = System.Action;
	using SqlCommand = System.Data.SqlClient.SqlCommand;

    public class SqlClientSqlProperBatchingCommandSet : IDisposable
	{
		private static readonly System.Type sqlCmdSetType;
		private readonly object instance;
		private readonly Action doDispose;
		private int countOfCommands;
        private StringBuilder _sb= new StringBuilder();
	    private SqlCommand _command;
        private SqlCommand _firstCommand;
        private bool _shouldClearCmd = false;

		static SqlClientSqlProperBatchingCommandSet()
		{
			var sysData = Assembly.Load("System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
			sqlCmdSetType = sysData.GetType("System.Data.SqlClient.SqlCommandSet");
			Debug.Assert(sqlCmdSetType != null, "Could not find SqlCommandSet!");
		}

        public SqlClientSqlProperBatchingCommandSet()
		{
			instance = Activator.CreateInstance(sqlCmdSetType, true);
            doDispose = (Action) Delegate.CreateDelegate(typeof (Action), instance, "Dispose");
		}

		/// <summary>
		/// Append a command to the batch
		/// </summary>
		/// <param name="command"></param>
		public void Append(SqlCommand command)
		{
		    if (_shouldClearCmd)
		    {
		        _command = null;
		        _shouldClearCmd = false;
		    }
		    AssertHasParameters(command);
			//doAppend(command);
            _sb.AppendLine(SqlCommandToStringConverter.Convert(command, _sb.Length == 0));
			countOfCommands++;

            if (countOfCommands == 1)
                _firstCommand = command;
		}

		/// <summary>
		/// This is required because SqlClient.SqlCommandSet will throw if 
		/// the command has no parameters.
		/// </summary>
		/// <param name="command"></param>
		private static void AssertHasParameters(SqlCommand command)
		{
			if (command.Parameters.Count == 0)
			{
				throw new ArgumentException("A command in SqlCommandSet must have parameters. You can't pass hardcoded sql strings.");
			}
		}


		/// <summary>
		/// Return the batch command to be executed
		/// </summary>
		public SqlCommand BatchCommand
		{
			get
			{
			    return _command ?? (_command = new SqlCommand(_sb.ToString()));
			}
		}

		/// <summary>
		/// The number of commands batched in this instance
		/// </summary>
		public int CountOfCommands
		{
			get { return countOfCommands; }
		}

		/// <summary>
		/// Executes the batch
		/// </summary>
		/// <returns>
		/// This seems to be returning the total number of affected rows in all queries
		/// </returns>
		public int ExecuteNonQuery()
		{
			if (Connection == null)
				throw new ArgumentNullException(
					"Connection was not set! You must set the connection property before calling ExecuteNonQuery()");

			if (CountOfCommands == 0)
				return 0;

		    PrepareBatchCommandTextAndParameteres();
		    var ret= BatchCommand.ExecuteNonQuery();
		    _sb.Clear();
		    _shouldClearCmd = true;
		    return ret;
		}

        private void PrepareBatchCommandTextAndParameteres()
        {
            if (CountOfCommands == 1)
            {
                //we're doing that because our batched INSERT looks like 'INSERT ... VALUES (...)'
                //but if we're running batch that has only one command we want to use sp_executesql with parameters.
                BatchCommand.CommandText = _firstCommand.CommandText;
                ReplaceBatchCommandParametersWithFirstCommandText();
            }
            else
            {
                BatchCommand.CommandText = _sb.ToString();
            }
        }

        private void ReplaceBatchCommandParametersWithFirstCommandText()
        {
            BatchCommand.Parameters.Clear();
            for (var index = 0; index < _firstCommand.Parameters.Count; index++)
                BatchCommand.Parameters.Add(((ICloneable)_firstCommand.Parameters[index]).Clone());
        }

		public SqlConnection Connection
		{
            get { return BatchCommand.Connection; }
            set { BatchCommand.Connection = value; }
		}

		public SqlTransaction Transaction
		{
			set
			{
                BatchCommand.Transaction = value;
			}
		}

		public int CommandTimeout
		{
			set
			{
                BatchCommand.CommandTimeout = value;
			}
		}

		///<summary>
		///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		///</summary>
		///<filterpriority>2</filterpriority>
		public void Dispose()
		{
			doDispose();
		}
	}
}
