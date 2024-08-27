using Newtonsoft.Json.Linq;
using System;


public class QuoteFetch {
  private readonly HttpClient _httpClientQuote;
  private readonly HttpClient _httpClientStarWar;


  public QuoteFetch() {
    _httpClientQuote = new HttpClient();
    _httpClientQuote.DefaultRequestHeaders.Add("User-Agent", "PitchrApp/1.0");

    _httpClientStarWar = new HttpClient();
    _httpClientStarWar.DefaultRequestHeaders.Add("User-Agent", "PitchrApp/1.0");
  }

  public async Task<string> GetRandomQuoteAsync() {
    try {
      string url = "https://dummyjson.com/quotes/random";
      var response = await _httpClientQuote.GetAsync(url);

      //verify request was successful
      response.EnsureSuccessStatusCode();
      //grab random swapi name
      string characterName = await GetRandomCharacterNameAsync();
      var responseBody = await response.Content.ReadAsStringAsync();
      var json = JObject.Parse(responseBody);
      int x = 0;
      string quote = json["quote"].ToString();
      return $"\"{quote}\" \n\t\t-{characterName}";
    }
    catch (HttpRequestException ex) {
      Console.WriteLine($"HTTP Request error: {ex.Message}");
      return "Unable to fetch quote. Please try again later.";
    }
    catch (Exception ex) {
      Console.WriteLine($"Error fetching quote: {ex.Message}");
      return "Error fetching quote.";
    }
  }

  public async Task<string> GetRandomCharacterNameAsync() {
    // SWAPI currently has 83 characters (as of the last check)
    Random random = new Random();
    int randomId = random.Next(1, 84);

    string apiUrl = $"https://swapi.dev/api/people/{randomId}/";

    try {
      HttpResponseMessage response = await _httpClientStarWar.GetAsync(apiUrl);
      response.EnsureSuccessStatusCode();

      string responseBody = await response.Content.ReadAsStringAsync();
      JObject characterData = JObject.Parse(responseBody);

      string characterName = characterData["name"].ToString();
      return characterName;
    }
    catch (HttpRequestException e) {
      // Handle the error as needed
      Console.WriteLine($"Request error: {e.Message}");
      return "Unknown Character";
    }
  }

}
