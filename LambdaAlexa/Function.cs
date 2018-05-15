using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace LambdaAlexa
{
    public class Function
    {
        private static HttpClient _httpClient;
        public Function()
        {
            _httpClient = new HttpClient();
        }
        public async Task<SkillResponse> FunctionHandler(SkillRequest input, ILambdaContext context)
        {
            var requestType = input.GetRequestType();
            if (requestType == typeof(IntentRequest))
            {
                var intentRequest = input.Request as IntentRequest;
                var countryRequested = intentRequest?.Intent?.Slots["Country"].Value;
                if (countryRequested == null)
                {
                    context.Logger.LogLine($"The country was not understood.");
                    return MakeSkillResponse("I'm sorry, but I didn't understand the country you were asking for. Please ask again.", false);
                }
                var countryInfo = await GetCountryInfo(countryRequested, context);
                var outputText = $"About {countryInfo.name}. The capitol is {countryInfo.capital} and the population is {countryInfo.population}.";
                return MakeSkillResponse(outputText, true);
            }
            else
            {
                return MakeSkillResponse(
                        $"I don't know how to handle this intent. Please say something like Alexa, ask Country Info about France.",
                        true);
            }
        }
        private SkillResponse MakeSkillResponse(string outputSpeech,
            bool shouldEndSession,
            string repromptText = "Just say, tell me about France to learn more. To exit, say, exit.")
        {
            var response = new ResponseBody
            {
                ShouldEndSession = shouldEndSession,
                OutputSpeech = new PlainTextOutputSpeech { Text = outputSpeech }
            };
            if (repromptText != null)
            {
                response.Reprompt = new Reprompt() { OutputSpeech = new PlainTextOutputSpeech() { Text = repromptText } };
            }
            var skillResponse = new SkillResponse
            {
                Response = response,
                Version = "1.0"
            };
            return skillResponse;
        }
        private async Task<Country> GetCountryInfo(string countryName, ILambdaContext context)
        {
            countryName = countryName.ToLowerInvariant();
            var countries = new List<Country>();
            var countryPartNames = countryName.Split(' ');
            if (countryPartNames.Length > 1)
            {
                foreach (var searchPart in countryPartNames)
                {
                    if (searchPart != "the" || searchPart != "of")
                    {
                        countries.AddRange(await GetResultsForCountrySearch(searchPart, context));
                    }
                }
            }
            else
            {
                countries.AddRange(await GetResultsForCountrySearch(countryName, context));
            }
            var bestMatch = (from c in countries
                             where c.name.ToLowerInvariant() == countryName
                             orderby c.population descending
                             select c).FirstOrDefault();
            var match = bestMatch ?? (from c in countries
                                      where c.name.ToLowerInvariant().IndexOf(countryName) > 0
                                      orderby c.population descending
                                      select c).FirstOrDefault();
            return match;
        }
        private async Task<List<Country>> GetResultsForCountrySearch(string countryName, ILambdaContext context)
        {
            List<Country> countries = new List<Country>();
            var uri = new Uri($"https://restcountries.eu/rest/v2/name/{countryName}");
            context.Logger.LogLine($"Attempting to fetch data from {uri.AbsoluteUri}");
            try
            {
                var response = await _httpClient.GetStringAsync(uri);
                countries = JsonConvert.DeserializeObject<List<Country>>(response);
            }
            catch (Exception ex)
            {
                context.Logger.LogLine($"\nException: {ex.Message}");
                context.Logger.LogLine($"\nStack Trace: {ex.StackTrace}");
            }
            return countries;
        }
    }

    public class Country
    {
        public string name { get; set; }
        public string capital { get; set; }
        public int population { get; set; }
    }
}