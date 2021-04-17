﻿using System.Threading.Tasks;
using DotNetWorkQueue.Configuration;
using DotNetWorkQueue.Transport.Redis.Basic;
using Xunit;

namespace DotNetWorkQueue.Transport.Redis.IntegrationTests.ConsumerAsync
{
    [Collection("ConsumerAsync")]
    public class MultiConsumerAsync
    {
        [Theory]
        [InlineData(250, 1, 400, 10, 5, 5, ConnectionInfoTypes.Linux),
         InlineData(35, 5, 200, 10, 1, 2, ConnectionInfoTypes.Linux)]
        public async Task Run(int messageCount, int runtime, int timeOut, int workerCount, int readerCount, int queueSize, ConnectionInfoTypes type)
        {
            var queueName = GenerateQueueName.Create();
            var connectionString = new ConnectionInfo(type).ConnectionString;
            var consumer =
                new DotNetWorkQueue.IntegrationTests.Shared.ConsumerAsync.Implementation.MultiConsumerAsync();

            await consumer.Run<RedisQueueInit, RedisQueueCreation>(new QueueConnection(queueName, connectionString),
                messageCount, runtime, timeOut, workerCount, readerCount, queueSize, false, x => { },
                Helpers.GenerateData, Helpers.Verify, VerifyQueueCount).ConfigureAwait(false);
        }

        private void VerifyQueueCount(QueueConnection queueConnection, IBaseTransportOptions arg3, ICreationScope arg4, int arg5, bool arg6, bool arg7)
        {
            //noop
        }
    }
}