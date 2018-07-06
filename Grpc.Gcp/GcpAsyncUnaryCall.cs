﻿#region Copyright notice and license

// Copyright 2015 gRPC authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Grpc.Core;

namespace Grpc.Gcp
{
    /// <summary>
    /// Return type for single request - single response call.
    /// </summary>
    /// <typeparam name="TResponse">Response message type for this call.</typeparam>
    public sealed class GcpAsyncUnaryCall<TResponse> : IDisposable
    {
        readonly Task<TResponse> responseAsync;
        readonly Task<Metadata> responseHeadersAsync;
        readonly Func<Status> getStatusFunc;
        readonly Func<Metadata> getTrailersFunc;
        readonly Action disposeAction;
        readonly AsyncUnaryCall<TResponse> originalCall;


        /// <summary>
        /// Creates a new AsyncUnaryCall object with the specified properties.
        /// </summary>
        /// <param name="responseAsync">The response of the asynchronous call.</param>
        /// <param name="responseHeadersAsync">Response headers of the asynchronous call.</param>
        /// <param name="getStatusFunc">Delegate returning the status of the call.</param>
        /// <param name="getTrailersFunc">Delegate returning the trailing metadata of the call.</param>
        /// <param name="disposeAction">Delegate to invoke when Dispose is called on the call object.</param>
        public GcpAsyncUnaryCall(Task<TResponse> responseAsync,
                              Task<Metadata> responseHeadersAsync,
                              Func<Status> getStatusFunc,
                              Func<Metadata> getTrailersFunc,
                              Action disposeAction)
        {
            this.responseHeadersAsync = responseHeadersAsync;
            this.getStatusFunc = getStatusFunc;
            this.getTrailersFunc = getTrailersFunc;
            this.disposeAction = disposeAction;
        }

        //public GcpAsyncUnaryCall(AsyncUnaryCall<TResponse> originalCall,
        //                      Action<TResponse> callback)
        //{
        //    this.originalCall = originalCall;
        //    this.responseAsync = this.originalCall.ResponseAsync.ContinueWith(antecendent => callback(antecendent.Result));
        //    this.responseHeadersAsync = this.originalCall.ResponseHeadersAsync;
        //}

        /// <summary>
        /// Asynchronous call result.
        /// </summary>
        public Task<TResponse> ResponseAsync
        {
            get
            {
                return this.responseAsync;
            }
        }

        /// <summary>
        /// Asynchronous access to response headers.
        /// </summary>
        public Task<Metadata> ResponseHeadersAsync
        {
            get
            {
                return this.responseHeadersAsync;
            }
        }

        /// <summary>
        /// Allows awaiting this object directly.
        /// </summary>
        public TaskAwaiter<TResponse> GetAwaiter()
        {
            return responseAsync.GetAwaiter();
        }

        /// <summary>
        /// Gets the call status if the call has already finished.
        /// Throws InvalidOperationException otherwise.
        /// </summary>
        public Status GetStatus()
        {
            return this.originalCall.GetStatus();
        }

        /// <summary>
        /// Gets the call trailing metadata if the call has already finished.
        /// Throws InvalidOperationException otherwise.
        /// </summary>
        public Metadata GetTrailers()
        {
            return this.originalCall.GetTrailers();
        }

        /// <summary>
        /// Provides means to cleanup after the call.
        /// If the call has already finished normally (request stream has been completed and call result has been received), doesn't do anything.
        /// Otherwise, requests cancellation of the call which should terminate all pending async operations associated with the call.
        /// As a result, all resources being used by the call should be released eventually.
        /// </summary>
        /// <remarks>
        /// Normally, there is no need for you to dispose the call unless you want to utilize the
        /// "Cancel" semantics of invoking <c>Dispose</c>.
        /// </remarks>
        public void Dispose()
        {
            this.originalCall.Dispose();
        }
    }
}
