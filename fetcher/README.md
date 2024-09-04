# Apple Job Fetcher

`POST https://jobs.apple.com/en-us/search`

## Locations

`GET https://jobs.apple.com/api/v1/refData/postlocation?input={location}`

To query locations, it's a simple GET request to this API. It seems intended to be an autocomplete
thing so input is just a string filter. I had to remove a bunch of incorrect matches from the
results when getting the locations together.

There's also some additional weirdness with how the location and search APIs work together:

- You cannot search without a location attached, meaning that there's no in-built way to look for
  jobs across all locations.
- It's weirdly finnicky. For example, one of the corporate locations is Bankok. If you query Bangkok
  on their `postlocation` API, you get back three relevant possible results: one for the city, one
  for what seems to be the state, and one for the country of Thailand. However, if you pass all
  three of these locations to the job search API, only the first two return any actual results.

Looking at the above further, it actually seems inconsistent there too. Like if you search China,
you get some results. I think what I will try doing is sending the "state" entry for each item as well as the country.

### Process

1. Downloaded a list of corporate locations from https://www.apple.com/careers/us/work-at-apple.html
2. Queried the `postlocation` API for each location
3. Cleaned up the data to get state + country for each location
4. Saved the results in [locations.json](./locations-2024-09-01.json)
