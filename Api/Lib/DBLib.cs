#nullable enable
using System;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Serilog;
using Serilog.Core;
using SqlKata.Compilers;
using SqlKata.Execution;

namespace Api.Lib
{
    public class DBLib
    {
        public static QueryFactory GetQueryBuilder()
        {
            QueryFactory qf = new QueryFactory(GetConnection(), new MySqlCompiler(), 60);
            return qf;
        }

        private static MySqlConnection GetConnection() //MySql 커넥션 가져오기 (Open된 상태로 리턴), 오류발생시 에러 로그후 다시 throw
        {
            try
            {
                MySqlConnection connection =  new MySqlConnection(Startup.StaticConfiguration.GetConnectionString("mysql"));
                connection.Open();
                return connection;
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                throw;
            }
        }
    }
}
