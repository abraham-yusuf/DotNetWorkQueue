﻿using DotNetWorkQueue.IntegrationTests.Shared;
using DotNetWorkQueue.IntegrationTests.Shared.JobScheduler;
using DotNetWorkQueue.Transport.SQLite.Basic;
using DotNetWorkQueue.Transport.SQLite.Integration.Tests;
using Xunit;

namespace DotNetWorkQueue.Transport.SQLite.Linq.Integration.Tests.JobScheduler
{
    [Collection("JobScheduler")]
    public class JobSchedulerMultipleTests
    {
        [Theory]
        [InlineData(2, false),
         InlineData(2, true)]
        public void Run(
            int producerCount,
            bool inMemoryDb)
        {
            using (var connectionInfo = new IntegrationConnectionInfo(inMemoryDb))
            {
                var queueName = GenerateQueueName.Create();
                using (var queueContainer = new QueueContainer<SqLiteMessageQueueInit>(x => {
                }))
                {
                    var queueConnection = new DotNetWorkQueue.Configuration.QueueConnection(queueName, connectionInfo.ConnectionString);
                    try
                    {
                        var tests = new JobSchedulerTestsShared();
                        tests.RunTestMultipleProducers<SqLiteMessageQueueInit, SqliteJobQueueCreation>(queueConnection, true, producerCount, queueContainer.CreateTimeSync(connectionInfo.ConnectionString), LoggerShared.Create(queueName, GetType().Name));
                    }
                    finally
                    {

                        using (var queueCreator =
                            new QueueCreationContainer<SqLiteMessageQueueInit>())
                        {
                            using (
                                var oCreation =
                                    queueCreator.GetQueueCreation<SqLiteMessageQueueCreation>(queueConnection)
                                )
                            {
                                oCreation.RemoveQueue();
                            }
                        }
                    }
                }
            }
        }
    }
}
