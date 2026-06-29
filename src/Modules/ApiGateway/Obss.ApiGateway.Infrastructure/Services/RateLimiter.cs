using System.Collections.Concurrent;
using Obss.ApiGateway.Domain.Services;
using Obss.ApiGateway.Domain.ValueObjects;

namespace Obss.ApiGateway.Infrastructure.Services;

public sealed class RateLimiter : IRateLimiter, IDisposable
{
    private readonly ConcurrentDictionary<string, SlidingWindowCounter> _counters = new();
    private readonly TimeSpan _windowSize = TimeSpan.FromMinutes(1);
    private readonly Timer _cleanupTimer;

    public RateLimiter()
    {
        _cleanupTimer = new Timer(Cleanup, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }

    public RateLimitResult CheckRateLimit(string apiKey, string path)
    {
        var key = $"{apiKey}:{path}";
        var counter = _counters.GetOrAdd(key, _ => new SlidingWindowCounter(_windowSize));

        lock (counter)
        {
            counter.RemoveExpiredEntries();
            return counter.TryConsume();
        }
    }

    public void SetRateLimit(string apiKey, string path, int limit)
    {
        var key = $"{apiKey}:{path}";
        var counter = _counters.GetOrAdd(key, _ => new SlidingWindowCounter(_windowSize, limit));

        lock (counter)
        {
            counter.SetLimit(limit);
        }
    }

    private void Cleanup(object? state)
    {
        foreach (var kvp in _counters)
        {
            lock (kvp.Value)
            {
                kvp.Value.RemoveExpiredEntries();
                if (kvp.Value.IsEmpty)
                {
                    _counters.TryRemove(kvp.Key, out _);
                }
            }
        }
    }

    public void Dispose()
    {
        _cleanupTimer.Dispose();
    }

    private sealed class SlidingWindowCounter
    {
        private readonly TimeSpan _windowSize;
        private readonly LinkedList<DateTime> _timestamps = [];
        private int _limit;

        public bool IsEmpty => _timestamps.Count == 0;

        public SlidingWindowCounter(TimeSpan windowSize, int limit = 60)
        {
            _windowSize = windowSize;
            _limit = limit;
        }

        public void RemoveExpiredEntries()
        {
            var cutoff = DateTime.UtcNow - _windowSize;
            while (_timestamps.First?.Value < cutoff)
            {
                _timestamps.RemoveFirst();
            }
        }

        public RateLimitResult TryConsume()
        {
            var now = DateTime.UtcNow;
            _timestamps.AddLast(now);

            var count = _timestamps.Count;
            var isAllowed = count <= _limit;
            var remaining = Math.Max(0, _limit - count);

            TimeSpan? retryAfter = null;
            if (!isAllowed && _timestamps.First is not null)
            {
                retryAfter = _windowSize - (now - _timestamps.First.Value);
                if (retryAfter < TimeSpan.Zero)
                    retryAfter = TimeSpan.Zero;
            }

            return new RateLimitResult(isAllowed, remaining, _limit, retryAfter);
        }

        public void SetLimit(int limit)
        {
            _limit = limit;
        }
    }
}
