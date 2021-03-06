﻿using System;
using System.Threading.Tasks;
using Sample.Infrastructure.Remoting.Contracts;

namespace Sample.Infrastructure.Remoting.Communication
{
    internal class ResponseAwaiter<TResponse>
        where TResponse : IRemoteMessage
    {
        private readonly TaskCompletionSource<TResponse> _completionSource;

        public ResponseAwaiter()
        {
            CorrelationId = Guid.NewGuid().ToString();
            _completionSource = new TaskCompletionSource<TResponse>();
        }

        public string CorrelationId { get; }

        public Task<TResponse> Result => _completionSource.Task;

        public void SetResult(TResponse result)
        {
            _completionSource.SetResult(result);
        }

        public void SetException(Exception ex)
        {
            _completionSource.SetException(ex);
        }
    }
}