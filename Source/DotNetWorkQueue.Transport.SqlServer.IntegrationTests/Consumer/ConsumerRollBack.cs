﻿using System;
using DotNetWorkQueue.IntegrationTests.Shared;
using DotNetWorkQueue.IntegrationTests.Shared.Consumer;
using DotNetWorkQueue.IntegrationTests.Shared.Producer;
using DotNetWorkQueue.Queue;
using DotNetWorkQueue.Transport.SqlServer.Basic;
using Xunit;

namespace DotNetWorkQueue.Transport.SqlServer.IntegrationTests.Consumer
{
    [Collection("Consumer")]
    public class ConsumerRollBack
    {

        [Theory]
        [InlineData(500, 0, 240, 5, false, false),
        InlineData(10, 15, 180, 7, true, true)]
        public void Run(int messageCount, int runtime, int timeOut, int workerCount, bool useTransactions, bool enableChaos)
        {
            var queueName = GenerateQueueName.Create();
            var logProvider = LoggerShared.Create(queueName, GetType().Name);
            using (
                var queueCreator =
                    new QueueCreationContainer<SqlServerMessageQueueInit>(
                        serviceRegister => serviceRegister.Register(() => logProvider, LifeStyles.Singleton)))
            {
                var queueConnection = new DotNetWorkQueue.Configuration.QueueConnection(queueName, ConnectionInfo.ConnectionString);
                try
                {

                    using (
                        var oCreation =
                            queueCreator.GetQueueCreation<SqlServerMessageQueueCreation>(queueConnection)
                        )
                    {
                        oCreation.Options.EnableDelayedProcessing = true;
                        oCreation.Options.EnableHeartBeat = !useTransactions;
                        oCreation.Options.EnableHoldTransactionUntilMessageCommitted = useTransactions;
                        oCreation.Options.EnableStatus = !useTransactions;
                        oCreation.Options.EnableStatusTable = true;

                        var result = oCreation.CreateQueue();
                        Assert.True(result.Success, result.ErrorMessage);

                        //create data
                        var producer = new ProducerShared();
                        producer.RunTest<SqlServerMessageQueueInit, FakeMessage>(queueConnection, false, messageCount, logProvider, Helpers.GenerateData,
                            Helpers.Verify, false, oCreation.Scope, false);

                        //process data
                        var consumer = new ConsumerRollBackShared<FakeMessage>();
                        consumer.RunConsumer<SqlServerMessageQueueInit>(queueConnection,
                            false,
                            workerCount, logProvider, timeOut, runtime, messageCount, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(12), "second(*%3)", null, enableChaos, new CreationScopeNoOp());

                        new VerifyQueueRecordCount(queueConnection, oCreation.Options).Verify(0, false, false);
                    }
                }
                finally
                {
                    using (
                        var oCreation =
                            queueCreator.GetQueueCreation<SqlServerMessageQueueCreation>(queueConnection)
                        )
                    {
                        oCreation.RemoveQueue();
                    }
                }
            }
        }
    }
}
