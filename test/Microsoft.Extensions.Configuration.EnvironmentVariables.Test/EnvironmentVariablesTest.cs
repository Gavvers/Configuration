// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using Microsoft.Extensions.Configuration.Test;
using Xunit;

namespace Microsoft.Extensions.Configuration.EnvironmentVariables.Test
{
    public class EnvironmentVariablesTest
    {
        [Fact]
        public void LoadKeyValuePairsFromEnvironmentDictionary()
        {
            var dict = new Hashtable()
                {
                    {"DefaultConnection:ConnectionString", "TestConnectionString"},
                    {"DefaultConnection:Provider", "SqlClient"},
                    {"Inventory:ConnectionString", "AnotherTestConnectionString"},
                    {"Inventory:Provider", "MySql"}
                };
            var envConfigSrc = new EnvironmentVariablesConfigurationProvider();

            envConfigSrc.Load(dict);

            Assert.Equal("TestConnectionString", envConfigSrc.Get("defaultconnection:ConnectionString"));
            Assert.Equal("SqlClient", envConfigSrc.Get("DEFAULTCONNECTION:PROVIDER"));
            Assert.Equal("AnotherTestConnectionString", envConfigSrc.Get("Inventory:CONNECTIONSTRING"));
            Assert.Equal("MySql", envConfigSrc.Get("Inventory:Provider"));
        }

        [Fact]
        public void LoadKeyValuePairsFromEnvironmentDictionaryWithPrefix()
        {
            var dict = new Hashtable()
                {
                    {"DefaultConnection:ConnectionString", "TestConnectionString"},
                    {"DefaultConnection:Provider", "SqlClient"},
                    {"Inventory:ConnectionString", "AnotherTestConnectionString"},
                    {"Inventory:Provider", "MySql"}
                };
            var envConfigSrc = new EnvironmentVariablesConfigurationProvider("DefaultConnection:");

            envConfigSrc.Load(dict);

            Assert.Equal("TestConnectionString", envConfigSrc.Get("ConnectionString"));
            Assert.Equal("SqlClient", envConfigSrc.Get("Provider"));
        }

        [Fact]
        public void LoadKeyValuePairsFromAzureEnvironment()
        {
            var dict = new Hashtable()
                {
                    {"APPSETTING_AppName", "TestAppName"},
                    {"CUSTOMCONNSTR_db1", "CustomConnStr"},
                    {"SQLCONNSTR_db2", "SQLConnStr"},
                    {"MYSQLCONNSTR_db3", "MySQLConnStr"},
                    {"SQLAZURECONNSTR_db4", "SQLAzureConnStr"},
                    {"CommonEnv", "CommonEnvValue"},
                };
            var envConfigSrc = new EnvironmentVariablesConfigurationProvider();

            envConfigSrc.Load(dict);

            string value;
            Assert.Equal("TestAppName", envConfigSrc.Get("APPSETTING_AppName"));
            Assert.False(envConfigSrc.TryGet("AppName", out value));
            Assert.Equal("CustomConnStr", envConfigSrc.Get("db1"));
            Assert.Equal("SQLConnStr", envConfigSrc.Get("db2"));
            Assert.Equal("System.Data.SqlClient", envConfigSrc.Get("db2:ProviderName"));
            Assert.Equal("MySQLConnStr", envConfigSrc.Get("db3"));
            Assert.Equal("MySql.Data.MySqlClient", envConfigSrc.Get("db3:ProviderName"));
            Assert.Equal("SQLAzureConnStr", envConfigSrc.Get("db4"));
            Assert.Equal("System.Data.SqlClient", envConfigSrc.Get("db4:ProviderName"));
            Assert.Equal("CommonEnvValue", envConfigSrc.Get("CommonEnv"));
        }

        [Fact]
        public void LoadKeyValuePairsFromAzureEnvironmentWithPrefix()
        {
            var dict = new Hashtable()
                {
                    {"CUSTOMCONNSTR_Prefix:db1", "CustomConnStr"},
                    {"SQLCONNSTR_Prefix:db2", "SQLConnStr"},
                    {"MYSQLCONNSTR_Prefix:db3", "MySQLConnStr"},
                    {"SQLAZURECONNSTR_Prefix:db4", "SQLAzureConnStr"},
                    {"CommonEnv", "CommonEnvValue"},
                };
            var envConfigSrc = new EnvironmentVariablesConfigurationProvider("Data:");

            envConfigSrc.Load(dict);

            Assert.Equal("CustomConnStr", envConfigSrc.Get("Prefix:db1"));
            Assert.Equal("SQLConnStr", envConfigSrc.Get("Prefix:db2"));
            Assert.Equal("System.Data.SqlClient", envConfigSrc.Get("Prefix:db2:ProviderName"));
            Assert.Equal("MySQLConnStr", envConfigSrc.Get("Prefix:db3"));
            Assert.Equal("MySql.Data.MySqlClient", envConfigSrc.Get("Prefix:db3:ProviderName"));
            Assert.Equal("SQLAzureConnStr", envConfigSrc.Get("Prefix:db4"));
            Assert.Equal("System.Data.SqlClient", envConfigSrc.Get("Prefix:db4:ProviderName"));
        }

        [Fact]
        public void LastVariableAddedWhenKeyIsDuplicatedInAzureEnvironment()
        {
            var dict = new Hashtable()
                {
                    {"db2", "CommonEnvValue"},
                    {"SQLCONNSTR_db2", "SQLConnStr"},
                };
            var envConfigSrc = new EnvironmentVariablesConfigurationProvider();

            envConfigSrc.Load(dict);

            Assert.True(!string.IsNullOrEmpty(envConfigSrc.Get("db2")));
            Assert.Equal("System.Data.SqlClient", envConfigSrc.Get("db2:ProviderName"));
        }

        [Fact]
        public void LastVariableAddedWhenMultipleEnvironmentVariablesWithSameNameButDifferentCaseExist()
        {
            var dict = new Hashtable()
                {
                    {"CommonEnv", "CommonEnvValue1"},
                    {"commonenv", "commonenvValue2"},
                    {"cOMMonEnv", "commonenvValue3"},
                };
            var envConfigSrc = new EnvironmentVariablesConfigurationProvider();

            envConfigSrc.Load(dict);

            Assert.True(!string.IsNullOrEmpty(envConfigSrc.Get("cOMMonEnv")));
            Assert.True(!string.IsNullOrEmpty(envConfigSrc.Get("CommonEnv")));
        }

        [Fact]
        public void ReplaceDoubleUnderscoreInEnvironmentVariables()
        {
            var dict = new Hashtable()
                {
                    {"data__ConnectionString", "connection"},
                    {"SQLCONNSTR__db1", "connStr"}
                };
            var envConfigSrc = new EnvironmentVariablesConfigurationProvider();

            envConfigSrc.Load(dict);

            Assert.Equal("connection", envConfigSrc.Get("data:ConnectionString"));
            Assert.Equal("System.Data.SqlClient", envConfigSrc.Get("_db1:ProviderName"));
        }
    }
}
