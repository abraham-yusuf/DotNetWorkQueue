﻿using System;
using DotNetWorkQueue.IntegrationTests.Shared;
using DotNetWorkQueue.IntegrationTests.Shared.ConsumerMethod;
using DotNetWorkQueue.IntegrationTests.Shared.ProducerMethod;
using DotNetWorkQueue.Queue;
using DotNetWorkQueue.Transport.SQLite.Basic;
using DotNetWorkQueue.Transport.SQLite.Integration.Tests;
using Xunit;

namespace DotNetWorkQueue.Transport.SQLite.Linq.Integration.Tests.ConsumerMethod
{
    [Collection("Consumer")]
    public class ConsumerMethodCancelWork
    {
        [Theory]
        [InlineData(2, 45, 90, 3, false, LinqMethodTypes.Dynamic, false),
        InlineData(2, 45, 90, 3, true, LinqMethodTypes.Dynamic, false),
        InlineData(2, 45, 90, 3, false, LinqMethodTypes.Compiled, false),
        InlineData(2, 45, 90, 3, true, LinqMethodTypes.Compiled, false)]
        public void Run(int messageCount, int runtime, 
            int timeOut, int workerCount, bool inMemoryDb, LinqMethodTypes linqMethodTypes, bool enableChaos)
        {
            using (var connectionInfo = new IntegrationConnectionInfo(inMemoryDb))
            {
                var queueName = GenerateQueueName.Create();
                var logProvider = LoggerShared.Create(queueName, GetType().Name);
                using (
                    var queueCreator =
                        new QueueCreationContainer<SqLiteMessageQueueInit>(
                            serviceRegister => serviceRegister.Register(() => logProvider, LifeStyles.Singleton)))
                {
                    var queueConnection = new DotNetWorkQueue.Configuration.QueueConnection(queueName, connectionInfo.ConnectionString);
                    try
                    {
                        using (
                            var oCreation =
                                queueCreator.GetQueueCreation<SqLiteMessageQueueCreation>(queueConnection)
                            )
                        {
                            oCreation.Options.EnableDelayedProcessing = true;
                            oCreation.Options.EnableHeartBeat = true;
                            oCreation.Options.EnableStatus = true;
                            oCreation.Options.EnableStatusTable = true;

                            var result = oCreation.CreateQueue();
                            Assert.True(result.Success, result.ErrorMessage);

                            var producer = new ProducerMethodShared();
                            var id = Guid.NewGuid();
                            if (linqMethodTypes == LinqMethodTypes.Compiled)
                            {
                                producer.RunTestCompiled<SqLiteMessageQueueInit>(queueConnection, false, messageCount, logProvider, Helpers.GenerateData,
                               Helpers.Verify, false, id, GenerateMethod.CreateCancelCompiled, runtime, oCreation.Scope, false);
                            }
                            else
                            {
                                producer.RunTestDynamic<SqLiteMessageQueueInit>(queueConnection, false, messageCount, logProvider, Helpers.GenerateData,
                                  Helpers.Verify, false, id, GenerateMethod.CreateCancelDynamic, runtime, oCreation.Scope, false);

                            }

                            var consumer = new ConsumerMethodCancelWorkShared<SqLiteMessageQueueInit>();
                            consumer.RunConsumer(queueConnection, false, logProvider,
                                runtime, messageCount,
                                workerCount, timeOut,
                                serviceRegister => serviceRegister.Register<IMessageMethodHandling>(() => new MethodMessageProcessingCancel(id), LifeStyles.Singleton), 
                                TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(35), "second(*%10)", id, enableChaos, new CreationScopeNoOp());

                            new VerifyQueueRecordCount(queueName, connectionInfo.ConnectionString, oCreation.Options).Verify(0, false, false);
                            GenerateMethod.ClearCancel(id);
                        }
                    }
                    finally
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
