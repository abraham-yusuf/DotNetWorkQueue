﻿using System;
using DotNetWorkQueue.Configuration;
using DotNetWorkQueue.IntegrationTests.Shared;
using DotNetWorkQueue.IntegrationTests.Shared.ConsumerAsync;
using DotNetWorkQueue.IntegrationTests.Shared.Producer;
using DotNetWorkQueue.Queue;
using DotNetWorkQueue.Transport.Redis.Basic;
using Xunit;

namespace DotNetWorkQueue.Transport.Redis.IntegrationTests.ConsumerAsync
{
    [Collection("ConsumerAsync")]
    public class ConsumerAsyncPoisonMessage
    {
        [Theory]
        [InlineData(1, 60, 1, 1, 0, ConnectionInfoTypes.Linux, false),
        InlineData(10, 60, 5, 1, 0, ConnectionInfoTypes.Linux, true)]
        public void Run(int messageCount, int timeOut, int workerCount, 
            int readerCount, int queueSize, ConnectionInfoTypes type, bool route)
        {
            var queueName = GenerateQueueName.Create();
            var logProvider = LoggerShared.Create(queueName, GetType().Name);
            var connectionString = new ConnectionInfo(type).ConnectionString;
            using (
                var queueCreator =
                    new QueueCreationContainer<RedisQueueInit>(
                        serviceRegister => serviceRegister.Register(() => logProvider, LifeStyles.Singleton)))
            {
                var queueConnection = new QueueConnection(queueName, connectionString);
                try
                {
                    //create data
                    if (route)
                    {
                        var producer = new ProducerShared();
                        producer.RunTest<RedisQueueInit, FakeMessage>(queueConnection, false, messageCount, logProvider, Helpers.GenerateRouteData,
                            Helpers.Verify, false, null, false);
                    }
                    else
                    {
                        var producer = new ProducerShared();
                        producer.RunTest<RedisQueueInit, FakeMessage>(queueConnection, false, messageCount, logProvider, Helpers.GenerateData,
                            Helpers.Verify, false, null, false);
                    }

                    //process data
                    var defaultRoute = route ? Helpers.DefaultRoute : null;
                    var consumer = new ConsumerAsyncPoisonMessageShared<FakeMessage>();
                    consumer.RunConsumer<RedisQueueInit>(queueConnection, false,
                        workerCount, logProvider,
                        timeOut, readerCount, queueSize, messageCount, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(12), "second(*%3)", defaultRoute, false, new CreationScopeNoOp());

                    ValidateErrorCounts(queueName, connectionString, messageCount);
                    using (
                        var count = new VerifyQueueRecordCount(queueName, connectionString))
                    {
                        count.Verify(messageCount, true, 2);
                    }
                }
                finally
                {
                    using (
                        var oCreation =
                            queueCreator.GetQueueCreation<RedisQueueCreation>(queueConnection)
                        )
                    {
                        oCreation.RemoveQueue();
                    }
                }
            }
        }
        private void ValidateErrorCounts(string queueName, string connectionString, long messageCount)
        {
            //poison messages are moved to the error queue right away
            //they don't update the tracking table, so specify 0 for the error count.
            //They still update the error table itself
            using (var error = new VerifyErrorCounts(queueName, connectionString))
            {
                error.Verify(messageCount, 0);
            }
        }
    }
}
