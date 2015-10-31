﻿// ---------------------------------------------------------------------
//This file is part of DotNetWorkQueue
//Copyright © 2015 Brian Lehnen
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

using System;
using DotNetWorkQueue.Exceptions;
namespace DotNetWorkQueue.Queue
{
    /// <summary>
    /// Commits a message by asking the context to perform a commit
    /// </summary>
    internal class CommitMessage: ICommitMessage
    {
        /// <summary>
        /// Commits the specified message via the context
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// <exception cref="CommitException">An error has occured Committing a processed message</exception>
        public bool Commit(IMessageContext context)
        {
            Guard.NotNull(() => context, context);
            try
            {
                context.RaiseCommit();
                return true;
            }
            catch (Exception commitException)
            {
                throw new CommitException(
                    "An error has occured committing a processed message",
                    commitException);
            }
        }
    }
}
