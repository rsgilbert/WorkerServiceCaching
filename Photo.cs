namespace WorkerServiceCaching;

public readonly record struct Photo ( 
    int AlbumId,
    int Id,
    string Title,
    string Url,
    string ThumbnailUrl
);

