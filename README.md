# ssl-expiry-check
check SSL certificate  expiry using .NET C#

## Usage

```C#
string hostname = "www.google.com";
Dictionary<string, int> data = await SSLChecker.GetRemainingDays(hostname);

// result
// hostname, days remaining
// www.google.com,  90


string hostname = "google.com"
Dictionary<string, int> data = await SSLChecker.GetRemainingDays(hostname);

// result (http 301)
// hostname, days remaining
// google.com,  90
// www.google.com,  90

if (data.Values.First() < 0)
    Console.Write($"{hostname} has expired")
```

