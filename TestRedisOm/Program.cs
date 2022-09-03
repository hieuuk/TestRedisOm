﻿using Redis.OM;
using Redis.OM.Searching;
using Redis.OM.Aggregation;
using Redis.OM.Modeling;


var actor1 = new Actor
{
    Id = "1",
    Name = "Brad Pitt"
};
var actor2 = new Actor
{
    Id = "2",
    Name = "Tom Cruise"
};

var movie1 = new Movie
{
    Id = "1",
    Title = "Mission: Impossible - Rogue Nation",
    SingleActor = actor2,
    Actors = new List<Actor>()
    {
        actor2
    }
};

var movie2 = new Movie
{
    Id = "2",
    Title = "Try random name",
    SingleActor = actor1,
    Actors = new List<Actor>
    {
        actor1,
        actor2
    }
};

var provider = new RedisConnectionProvider("http://localhost:6379");
await provider.Connection.DropIndexAsync(typeof(Movie));
await provider.Connection.CreateIndexAsync(typeof(Movie));
var midx = (RedisCollection<Movie>)provider.RedisCollection<Movie>();

await midx.InsertAsync(movie1);
await midx.InsertAsync(movie2);

// WORKING
//var result = await midx
//     .Where(m => m.SingleActor.Name == "Cruise")
//     .ToListAsync();

// RETURN NOTHING
//var result = await midx
//     .Where(m => m.SingleActor.Name.Contains("om"))
//     .ToListAsync();
// THIS WORKING
//var aggregations = new RedisAggregationSet<Movie>(provider.Connection);
//var result = (await aggregations.Load(x => x.RecordShell.SingleActor.Name).Apply(x => x.RecordShell.SingleActor.Name.Contains("om"), "doesContain")
//    .Filter(x => x["doesContain"] == 1).LoadAll().ToListAsync()).Select(x => x.Hydrate()).ToList();

// RETURN NOTHING
//var result = await midx
//     .Where(m => m.Title == "Interview")
//     .ToListAsync();
//var aggregations = new RedisAggregationSet<Movie>(provider.Connection);
//var result = (await aggregations.Load(x => x.RecordShell.Title).Apply(x => x.RecordShell.Title.Contains("possib"), "doesContain")
//    .Filter(x => x["doesContain"] == 1).LoadAll().ToListAsync()).Select(x => x.Hydrate()).ToList();

// ERROR: Syntax error at offset 19 near With
//var result = await midx
//     .Where(m => m.Title == "Interview With The Vampire")
//     .ToListAsync();

// RETURN NOTHING
//var result = await midx
//     .Where(m => m.Title == "Interview")
//     .ToListAsync();

// WORKING
var result = await midx
     .Where(m => m.Actors.Any(a => a.Id == "2"))
     .ToListAsync();

// RETURN ONLY MOVIE 1
//var result = await midx
//     .Where(m => m.Actors.Any(a => a.Name == "Tom Cruise"))
//     .ToListAsync();

// RETURN NOTHING
//var result = await midx
//     .Where(m => m.Title.Contains("pir"))
//     .ToListAsync();

// RETURN NOTHING
//var result = await midx
//     .Where(m => m.Actors.Any(a => a.Name == "Tom"))
//     .ToListAsync();

foreach (var item in result)
{
    Console.WriteLine($"{item.Id} - {item.Title}");
    foreach (var ma in item.Actors)
    {
        Console.WriteLine($"Actor {ma.Id} - {ma.Name}");
    }
}

Console.WriteLine("Completed");

[Document(StorageType = StorageType.Json, Prefixes = new string[] { "mov" }, IndexName = "midx")]
public class Movie
{
    [RedisIdField]
    [Indexed]
    public string Id { get; set; }
    [Searchable(Aggregatable = true)]
    public string Title { get; set; }
    [Indexed(CascadeDepth = 1)]
    public Actor SingleActor { get; set; }
    [Indexed(JsonPath = "$.Id")]
    [Searchable(JsonPath = "$.Name")]
    public List<Actor> Actors { get; set; } 
}

public class Actor
{
    [RedisIdField]
    [Indexed]
    public string Id { get; set; }
    [Searchable(Aggregatable = true)]
    public string Name { get; set; }
}
