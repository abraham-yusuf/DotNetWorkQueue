﻿// ---------------------------------------------------------------------
//This file is part of DotNetWorkQueue
//Copyright © 2015-2021 Brian Lehnen
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
using System.Collections.Generic;

namespace DotNetWorkQueue.Transport.Shared.Basic.Query
{
    /// <summary>
    /// Returns the type and count of errors from the errors table
    /// </summary>
    public class GetMessageErrorsQuery<T> : IQuery<Dictionary<string, int>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetMessageErrorsQuery{T}"/> class.
        /// </summary>
        /// <param name="queueId">The queue identifier.</param>
        public GetMessageErrorsQuery(T queueId)
        {
            QueueId = queueId;
        }
        /// <summary>
        /// Gets the queue identifier.
        /// </summary>
        /// <value>
        /// The queue identifier.
        /// </value>
        public T QueueId { get; }
    }
}
