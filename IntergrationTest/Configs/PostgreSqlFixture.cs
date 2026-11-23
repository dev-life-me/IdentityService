using System;
using System.Collections.Generic;
using System.Text;
using Testcontainers.PostgreSql;
using Xunit;

namespace IntergrationTest.Configs
{
    public sealed class PostgreSqlFixture : IAsyncLifetime
    {
        public PostgreSqlContainer PostgreSqlContainer { get; }
            = new PostgreSqlBuilder("postgres:17-alpine")
                .WithDatabase("identity_test")
                .WithUsername("test")
                .WithPassword("test")
                .Build();

        ValueTask IAsyncLifetime.InitializeAsync()
        {
            return new ValueTask(PostgreSqlContainer.StartAsync());
        }
       
        ValueTask IAsyncDisposable.DisposeAsync()
        {
            return new ValueTask(PostgreSqlContainer.DisposeAsync().AsTask());
        }
    }
}

