﻿using DotNetWorkQueue.Configuration;
using DotNetWorkQueue.IntegrationTests.Shared;
using DotNetWorkQueue.IntegrationTests.Shared.Producer;
using DotNetWorkQueue.Transport.PostgreSQL.Basic;
using DotNetWorkQueue.Transport.PostgreSQL.Schema;
using Xunit;

namespace DotNetWorkQueue.Transport.PostgreSQL.Integration.Tests.Producer
{
    [Collection("producer")]
    public class SimpleProducerAsync
    {
        [Theory]
        [InlineData(100, true, true, true, false, false, false, true, false, false, false),
         InlineData(100, false, true, true, false, false, false, true, false, false, false),
         InlineData(100, false, false, false, false, false, false, false, false, false, false),
         InlineData(100, true, false, false, false, false, false, false, false, false, false),
         InlineData(100, false, false, false, false, false, false, false, true, false, false),
         InlineData(100, false, false, false, false, false, false, true, true, false, false),
         InlineData(100, false, true, false, true, true, true, false, true, false, false),
         InlineData(100, false, true, true, false, true, true, true, true, false, false),
         InlineData(100, true, true, true, false, false, false, true, false, true, false),

         InlineData(10, true, true, true, false, false, false, true, false, false, true),
         InlineData(10, false, true, true, false, false, false, true, false, false, true),
         InlineData(10, false, false, false, false, false, false, false, false, false, true),
         InlineData(10, true, false, false, false, false, false, false, false, false, true),
         InlineData(10, false, false, false, false, false, false, false, true, false, true),
         InlineData(10, false, false, false, false, false, false, true, true, false, true),
         InlineData(10, false, true, false, true, true, true, false, true, false, true),
         InlineData(10, false, true, true, false, true, true, true, true, false, true),
         InlineData(10, true, true, true, false, false, false, true, false, true, true)]
        public async void Run(
            int messageCount,
            bool interceptors,
            bool enableDelayedProcessing,
            bool enableHeartBeat,
            bool enableHoldTransactionUntilMessageCommitted,
            bool enableMessageExpiration,
            bool enablePriority,
            bool enableStatus,
            bool enableStatusTable,
            bool additionalColumn, 
            bool enableChaos)
        {

            var queueName = GenerateQueueName.Create();
            var logProvider = LoggerShared.Create(queueName, GetType().Name);
            using (
                var queueCreator =
                    new QueueCreationContainer<PostgreSqlMessageQueueInit>(
                        serviceRegister => serviceRegister.Register(() => logProvider, LifeStyles.Singleton)))
            {
                var queueConnection = new QueueConnection(queueName, ConnectionInfo.ConnectionString);
                ICreationScope scope = null;
                var oCreation = queueCreator.GetQueueCreation<PostgreSqlMessageQueueCreation>(queueConnection);
                try
                {
                   
                        oCreation.Options.EnableDelayedProcessing = enableDelayedProcessing;
                        oCreation.Options.EnableHeartBeat = enableHeartBeat;
                        oCreation.Options.EnableMessageExpiration = enableMessageExpiration;
                        oCreation.Options.EnableHoldTransactionUntilMessageCommitted =
                            enableHoldTransactionUntilMessageCommitted;
                        oCreation.Options.EnablePriority = enablePriority;
                        oCreation.Options.EnableStatus = enableStatus;
                        oCreation.Options.EnableStatusTable = enableStatusTable;

                        if (additionalColumn)
                        {
                            oCreation.Options.AdditionalColumns.Add(new Column("OrderID", ColumnTypes.Integer, false));
                        }

                        var result = oCreation.CreateQueue();
                        Assert.True(result.Success, result.ErrorMessage);
                        scope = oCreation.Scope;

                    var producer = new ProducerAsyncShared();
                        await producer.RunTestAsync<PostgreSqlMessageQueueInit, FakeMessage>(queueConnection, interceptors, messageCount, logProvider,
                            Helpers.GenerateData,
                            Helpers.Verify, false, oCreation.Scope, enableChaos).ConfigureAwait(false);
                    
                }
                finally
                {
                    oCreation.RemoveQueue();
                    oCreation.Dispose();
                    scope?.Dispose();

                }
            }
        }
    }
}
