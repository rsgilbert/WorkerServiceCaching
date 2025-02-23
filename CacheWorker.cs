using Microsoft.Extensions.Caching.Memory;

namespace WorkerServiceCaching;

public class CacheWorker(
    ILogger<CacheWorker> logger,
    HttpClient httpClient,
    CacheSignal<Photo> cacheSignal,
    IMemoryCache cache
) : BackgroundService
{
    private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(2);
    private bool _isCacheInitialized = false;
    private const string Url = "https://jsonplaceholder.typicode.com/photos";


    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("StartAsync");
        await cacheSignal.WaitAsync();
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("ExecuteAsync");
        while (!cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("Updating cache");

            try
            {
                Photo[]? photos = await httpClient.GetFromJsonAsync<Photo[]>(
                    Url,
                    cancellationToken
                );
                if (photos is { Length: > 0 })
                {
                    cache.Set("Photos", photos);
                    logger.LogInformation("Cache updated with {Count:#,#} photos", photos.Length);
                }
                else
                {
                    logger.LogWarning("Unable to fetch photos from update cache.");
                }
                // await Task.Delay(5_000, cancellationToken);

            }
            catch(Exception e)
            {
                logger.LogError(e.ToString());
            }
            finally
            {
                if (!_isCacheInitialized)
                {
                    cacheSignal.Release();
                    _isCacheInitialized = true;
                }
            }

            try
            {
                logger.LogInformation("Will attempt to update the cache in {Seconds} seconds from now", _updateInterval.Seconds);
                await Task.Delay(_updateInterval, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                logger.LogWarning("Cancellation acknowledged: shutting down");
                break;
            }
        }
    }



}