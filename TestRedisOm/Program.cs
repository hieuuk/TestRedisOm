using Redis.OM;
using Redis.OM.Searching;
using Redis.OM.Modeling;


var actor1 = new Actor
{
    Id = 1,
    Name = "Brad Pitt"
};
var actor2 = new Actor
{
    Id = 2,
    Name = "Tom Cruise"
};

var movie1 = new Movie
{
    Id = 1,
    Title = "MI Roque",
    SingleActor = actor2,
    Actors = new List<Actor>()
    {
        actor2
    }
};

var movie2 = new Movie
{
    Id = 2,
    Title = "Vampire",
    SingleActor = actor1,
    Actors = new List<Actor>
    {
        actor1,
        actor2
    }
};

var provider = new RedisConnectionProvider("http://localhost:6379");
await provider.Connection.CreateIndexAsync(typeof(Movie));
var midx = (RedisCollection<Movie>)provider.RedisCollection<Movie>();

//await midx.InsertAsync(movie1);
//await midx.InsertAsync(movie2);

//var result = await midx
//     .Where(m => m.SingleActor.Name == "Tom")
//     .ToListAsync();

var result = await midx
     .Where(m => m.Title.Contains("pir"))
     .ToListAsync();

foreach (var item in result)
{
    Console.WriteLine($"{item.Id} - {item.Title}");
    foreach (var ma in item.Actors)
    {
        Console.WriteLine($"actress {ma.Id} - {ma.Name}");
    }
}

Console.WriteLine("Completed");
Console.ReadLine();

[Document(StorageType = StorageType.Json, Prefixes = new string[] { "mov" }, IndexName = "midx")]
public class Movie
{
    [RedisIdField]
    public int Id { get; set; }
    [Searchable]
    public string Title { get; set; }
    [Indexed(CascadeDepth = 1)]
    public Actor SingleActor { get; set; }
    //[Indexed(JsonPath = "$.Id")]
    //[Searchable(JsonPath = "$.Name")]
    [Indexed(CascadeDepth = 1)]
    public List<Actor> Actors { get; set; } 
}

public class Actor
{
    [RedisIdField]
    public int Id { get; set; }
    [Searchable]
    public string Name { get; set; }
}
