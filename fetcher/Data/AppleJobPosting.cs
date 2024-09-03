namespace BetterAppleJobSearch.Fetcher.Data;

public class AppleJobPosting
{
    public int Id { get; set; }

    public required string ReqId { get; set; }

    public required string PositionId { get; set; }

    public required DateTime PostDate { get; set; }

    public required string Title { get; set; }

    public required string Summary { get; set; }

    public string? TeamName { get; set; }

    public List<string> Locations { get; set; } = [];

    public required bool IsRemote { get; set; }
}

/*
 * CREATE TABLE apple(
       id INTEGER PRIMARY KEY,
       req_id TEXT NOT NULL,
       position_id TEXT,
       post_date TEXT,
       title TEXT,
       summary TEXT,
       team TEXT,
       locations TEXT,
       is_remote INTEGER
   );
*/
