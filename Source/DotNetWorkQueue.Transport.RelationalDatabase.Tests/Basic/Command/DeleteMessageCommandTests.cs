﻿using DotNetWorkQueue.Transport.RelationalDatabase.Basic.Command;
using DotNetWorkQueue.Transport.Shared.Basic.Command;
using Xunit;

namespace DotNetWorkQueue.Transport.RelationalDatabase.Tests.Basic.Command
{
    public class DeleteMessageCommandTests
    {
        [Fact]
        public void Create_Default()
        {
            const int id = 19334;
            var test = new DeleteMessageCommand<long>(id);
            Assert.Equal(id, test.QueueId);
        }
    }
}
