﻿using System;
using DotNetWorkQueue.IntegrationTests.Shared;
using DotNetWorkQueue.IntegrationTests.Shared.Consumer;
using DotNetWorkQueue.IntegrationTests.Shared.Producer;
using DotNetWorkQueue.Transport.LiteDb.Basic;
using Xunit;

namespace DotNetWorkQueue.Transport.LiteDb.IntegrationTests.Consumer
{
    [Collection("Consumer")]
    public class ConsumerExpiredMessage
    {
        [Theory]
        [InlineData(50, 0, 60, 1, false, IntegrationConnectionInfo.ConnectionTypes.Direct),
         InlineData(10, 0, 60, 1,  true, IntegrationConnectionInfo.ConnectionTypes.Memory)]
        public void Run(int messageCount, int runtime, int timeOut, int workerCount, bool enableChaos, IntegrationConnectionInfo.ConnectionTypes connectionType)
        {
            using (var connectionInfo = new IntegrationConnectionInfo(connectionType))
            {
                var queueName = GenerateQueueName.Create();
                var logProvider = LoggerShared.Create(queueName, GetType().Name);
                using (
                    var queueCreator =
                        new QueueCreationContainer<LiteDbMessageQueueInit>(
                            serviceRegister => serviceRegister.Register(() => logProvider, LifeStyles.Singleton)))
                {
                    var queueConnection = new DotNetWorkQueue.Configuration.QueueConnection(queueName, connectionInfo.ConnectionString);
                    ICreationScope scope = null;
                    var oCreation = queueCreator.GetQueueCreation<LiteDbMessageQueueCreation>(queueConnection);
                    try
                    {


                        oCreation.Options.EnableStatusTable = true;
                        oCreation.Options.EnableMessageExpiration = true;
                        oCreation.Options.EnableDelayedProcessing = true;

                        var result = oCreation.CreateQueue();
                        Assert.True(result.Success, result.ErrorMessage);
                        scope = oCreation.Scope;

                        var producer = new ProducerShared();
                        producer.RunTest<LiteDbMessageQueueInit, FakeMessage>(queueConnection, false, messageCount,
                            logProvider, Helpers.GenerateData,
                            Helpers.Verify, false, oCreation.Scope, false);

                        var consumer = new ConsumerExpiredMessageShared<FakeMessage>();
                        consumer.RunConsumer<LiteDbMessageQueueInit>(queueConnection,
                            false,
                            logProvider,
                            runtime, messageCount,
                            workerCount, timeOut, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(35), "second(*%10)",
                            null, enableChaos, scope);

                        new VerifyQueueRecordCount(queueName, connectionInfo.ConnectionString, oCreation.Options, scope)
                            .Verify(0, false, false);

                    }
                    finally
                    {
                        oCreation?.RemoveQueue();
                        oCreation?.Dispose();
                        scope?.Dispose();
                    }
                }
            }
        }
    }
}
