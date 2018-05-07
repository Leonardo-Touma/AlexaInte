using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AwsBilly
{
    public class Function
    {
        SkillResponse response;
        ILambdaLogger log;
        IOutputSpeech innerResponse;
        public Function()
        {
            response = new SkillResponse();
        }
        public SkillResponse FunctionHandler(SkillRequest input, ILambdaContext context)
        {
            response.Response = new ResponseBody();
            response.Response.ShouldEndSession = false;
            innerResponse = null;
            log = context.Logger;
            log.LogLine("Skill Request Object");
            log.LogLine(JsonConvert.SerializeObject(input));

            if (input.GetRequestType() == typeof(LaunchRequest))
            {
                log.LogLine("Default LaunchRequest made");
                innerResponse = new PlainTextOutputSpeech();
                (innerResponse as PlainTextOutputSpeech).Text = "Launch Request. Calculator opened";
            }
            else if (input.GetRequestType() == typeof(IntentRequest))
            {
                var IntentRequest = (IntentRequest)input.Request;
                switch (IntentRequest.Intent.Name)
                {
                    case "Amazon.CancelIntent":
                        log.LogLine($"Amazon.CancelIntent: send StopMessage");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = "Cancelled";
                        response.Response.ShouldEndSession = true;
                        break;
                    case "Amazon.StopIntent":
                        log.LogLine($"Amazon.StopIntent: send StopMessage");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = "Stopping";
                        response.Response.ShouldEndSession = true;
                        break;
                    case "Amazon.HelpIntent":
                        log.LogLine($"Amazon.HelpIntent: send StopMessage");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = "Help me";
                        response.Response.ShouldEndSession = true;
                        break;
                    case "billy":
                        BillyIntent();
                        break;
                    default:
                        log.LogLine("Unknown intent: " + IntentRequest.Intent.Name);
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = "Unknown intent";
                        response.Response.ShouldEndSession = true;
                        break;
                }
            }
            response.Response.OutputSpeech = innerResponse;
            response.Version = "1.0";
            log.LogLine("Skill Response object...");
            log.LogLine(JsonConvert.SerializeObject(response));
            response.Response.ShouldEndSession = true;
            return response;
        }
        public void BillyIntent()
        {
            log.LogLine($"Billy called");
            innerResponse = new PlainTextOutputSpeech();
            (innerResponse as PlainTextOutputSpeech).Text = "Hello from VS.";
        }
    }
}
