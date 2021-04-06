﻿using DotNetWorkQueue.Transport.RelationalDatabase.Basic.Command;
using DotNetWorkQueue.Transport.Shared.Basic.Command;
using Xunit;

namespace DotNetWorkQueue.Transport.RelationalDatabase.Tests.Basic.Command
{
    public class SendHeartBeatCommandTests
    {
        [Fact]
        public void Create_Default()
        {
            const int id = 19334;
            var test = new SendHeartBeatCommand<long>(id);
            Assert.Equal(id, test.QueueId);
        }
    }
}
