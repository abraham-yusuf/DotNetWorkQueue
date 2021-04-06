﻿// ---------------------------------------------------------------------
//This file is part of DotNetWorkQueue
//Copyright © 2015-2020 Brian Lehnen
//
//This library is free software; you can redistribute it and/or
//modify it under the terms of the GNU Lesser General Public
//License as published by the Free Software Foundation; either
//version 2.1 of the License, or (at your option) any later version.
//
//This library is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//Lesser General Public License for more details.
//
//You should have received a copy of the GNU Lesser General Public
//License along with this library; if not, write to the Free Software
//Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
// ---------------------------------------------------------------------
using DotNetWorkQueue.Transport.Redis.Basic.Command;
using DotNetWorkQueue.Transport.Shared;
using DotNetWorkQueue.Transport.Shared.Basic.Command;

namespace DotNetWorkQueue.Transport.Redis.Basic
{
    /// <summary>
    /// Removes a message from storage
    /// </summary>
    public class RemoveMessage : IRemoveMessage
    {
        private readonly ICommandHandlerWithOutput<DeleteMessageCommand<string>, bool> _deleteMessage;

        /// <summary>Initializes a new instance of the <see cref="RemoveMessage"/> class.</summary>
        /// <param name="deleteMessage">The delete message.</param>
        public RemoveMessage(ICommandHandlerWithOutput<DeleteMessageCommand<string>, bool> deleteMessage)
        {
            _deleteMessage = deleteMessage;
        }
        /// <inheritdoc />
        public RemoveMessageStatus Remove(IMessageId id, RemoveMessageReason reason)
        {
            if (id == null || !id.HasValue)
                return RemoveMessageStatus.NotFound;

            var result =_deleteMessage.Handle(new DeleteMessageCommand<string>(id.Id.Value.ToString()));
            return result ? RemoveMessageStatus.Removed : RemoveMessageStatus.NotFound;
        }

        /// <inheritdoc />
        public RemoveMessageStatus Remove(IMessageContext context, RemoveMessageReason reason)
        {
            return Remove(context.MessageId, reason);
        }
    }
}
