using Microsoft.Extensions.Caching.Memory;

namespace WorkerServiceCaching;

public sealed class PhotoService(
    IMemoryCache cache,
    CacheSignal<Photo> cacheSignal,
    ILogger<PhotoService> logger
)
{

    public async IAsyncEnumerable<Photo> GetPhotosAsync(Func<Photo, bool>? filter)
    {
        try 
        {
            logger.LogWarning("PhotoService: Waiting for photos");
            await cacheSignal.WaitAsync();
            logger.LogWarning("PhotoService: Finished waiting for photos");
            Photo[] photos = (await cache.GetOrCreateAsync("Photos", _ => 
            {
                logger.LogWarning("PhotoService: This should never happen!");
                return Task.FromResult(Array.Empty<Photo>());
            }))!;

            // if no filter is provided, use a pass-thru
            filter ??=  _ => true;
            foreach(Photo photo in photos)
            {
                if(!default(Photo).Equals(photo) && filter(photo))
                {
                    yield return photo;
                }
            } 
        }
        finally
        {
            cacheSignal.Release();
        }
    }

}