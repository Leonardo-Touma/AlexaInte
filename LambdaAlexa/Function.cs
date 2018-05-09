using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace LambdaAlexa
{
    public class Function
    {
        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private static HttpClient _httpClient;
        public const string INVOCATION_NAME = "billy Info";
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
                var billyRequested = intentRequest?.Intent?.Slots["billy"].Value;

                if (billyRequested == null)
                {
                    context.Logger.LogLine($"The billy was not understood.");
                    return MakeSkillResponse("I'm sorry, but I didn't understand the billy you were asking for. Please ask again.", false);
                }

                var billyinfo = await GetBillyInfo(billyRequested, context);
                var outputText = $"About {billyinfo.name}. The name is {billyinfo.name} and the type is {billyinfo.type}.";
                return MakeSkillResponse(
                    outputText
                    , true);
            }
            else
            {
                return MakeSkillResponse(
                        $"I don't know how to handle this intent. Please say something like Alexa, ask {INVOCATION_NAME} about Canada.",
                        true);
            }
        }


        private SkillResponse MakeSkillResponse(string outputSpeech,
            bool shouldEndSession,
            string repromptText = "Just say, tell me about Canada to learn more. To exit, say, exit.")
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


        private async Task<Billy> GetBillyInfo(string Name, ILambdaContext context)
        {
            Name = Name.ToLowerInvariant();
            var billys = new List<Billy>();

            // search by "North Korea" or "Vatican City" gives us poor results
            // instead search by both "North" and "Korea" to get better results
            var Billynames = Name.Split(' ');
            if (Billynames.Length > 1)
            {
                foreach (var searchPart in Billynames)
                {
                    // The United States of America results in too many search requests.
                    if (searchPart != "the" || searchPart != "of")
                    {

                        return new Billy{ name = "billy", type = "Bokhylla" };
                        //billys.AddRange(await billySearch(searchPart, context));
                    }
                }
            }
            else
            {
                return null;
                //billys.AddRange(await billySearch(Name, context));
            }

            // try to find a match on the name "korea" could return both north korea and south korea
            var bestMatch = (from c in billys
                             where c.name.ToLowerInvariant() == Name ||
                             c.demonym.ToLowerInvariant() == $"{Name}n"   // north korea hack (name is not North Korea, by demonym is North Korean)
                             //orderby c.population descending
                             select c).FirstOrDefault();

            var match = bestMatch ?? (from c in billys
                                      where c.name.ToLowerInvariant().IndexOf(Name) > 0
                                      || c.demonym.ToLowerInvariant().IndexOf(Name) > 0
                                      //orderby c.population descending
                                      select c).FirstOrDefault();

            if (match == null && billys.Count > 0)
            {
                match = billys.FirstOrDefault();
            }

            return match;
        }

        //private async Task<List<Billy>> billySearch(string billy, ILambdaContext context)
        //{
        //    List<Billy> billys = new List<Billy>();
        //    var uri = new Uri($"https://restcountries.eu/rest/v2/name/{billy}");
        //    context.Logger.LogLine($"Attempting to fetch data from {uri.AbsoluteUri}");
        //    try
        //    {
        //        var response = await _httpClient.GetStringAsync(uri);
        //        context.Logger.LogLine($"Response from URL:\n{response}");
        //        // TODO: (PMO) Handle bad requests
        //        billys = JsonConvert.DeserializeObject<List<Billy>>(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        context.Logger.LogLine($"\nException: {ex.Message}");
        //        context.Logger.LogLine($"\nStack Trace: {ex.StackTrace}");
        //    }
        //    return billys;
        //}
    }

    public class Billy
    {
        public string name { get; set; }
        public string type { get; set; }
        //
        //
        //------------------------------------------------
        //
        //
        //public string name { get; set; }
        //public string[] topLevelDomain { get; set; }
        //public string alpha2Code { get; set; }
        //public string alpha3Code { get; set; }
        //public string[] callingCodes { get; set; }
        //public string capital { get; set; }
        //public string[] altSpellings { get; set; }
        //public string region { get; set; }
        //public int population { get; set; }
        //public float[] latlng { get; set; }
        public string demonym { get; set; }
        //public float area { get; set; }
        //public float? gini { get; set; }
        //public string[] timezones { get; set; }
        //public string[] borders { get; set; }
        //public string nativeName { get; set; }
        //public string numericCode { get; set; }
        //public Currency[] currencies { get; set; }
        //public Language[] languages { get; set; }
        //public Translations translations { get; set; }
    }

    public class Translations
    {
        public string de { get; set; }
        public string es { get; set; }
        public string fr { get; set; }
        public string ja { get; set; }
        public string it { get; set; }
        public string br { get; set; }
        public string pt { get; set; }
    }

    public class Currency
    {
        public string code { get; set; }
        public string name { get; set; }
        public string symbol { get; set; }
    }

    public class Language
    {
        public string iso639_1 { get; set; }
        public string iso639_2 { get; set; }
        public string name { get; set; }
        public string nativeName { get; set; }
    }
}