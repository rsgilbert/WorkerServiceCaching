using Microsoft.Extensions.Caching.Memory;
using WorkerServiceCaching;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<CacheWorker>();
builder.Services.AddHostedService<CacheWorker>();
builder.Services.AddScoped<PhotoService>();
// builder.Services.AddSingleton<CacheSignal<Photo>>();
builder.Services.AddSingleton(typeof(CacheSignal<>));


using IHost host = builder.Build();


await host.StartAsync();

PhotoService photoService = host.Services.GetRequiredService<PhotoService>();
var photos =  photoService.GetPhotosAsync(null);
int count = photos.ToBlockingEnumerable().Count();
Console.WriteLine($"Got {count} photos");
Console.WriteLine("Program: 2s delay started");
await Task.Delay(8_000);
Console.WriteLine("Program: 2s delay complete");
photos =  photoService.GetPhotosAsync(null);
Console.WriteLine("Program: Got photos again");
count = photos.ToBlockingEnumerable().Count();
Console.WriteLine($"Now Got {count} photos");

// await foreach(Photo photo in photos)
// {
//     Console.WriteLine($"Photo {photo}");
// }
