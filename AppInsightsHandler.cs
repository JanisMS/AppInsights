using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
//using Microsoft.ApplicationInsights.DependencyCollector;
//using Microsoft.ApplicationInsights.Web;

namespace AppInsightsHandler
{
    public class AppInsightsHandler
    {

        private TelemetryClient Client;

        private RequestTelemetry requestTelemetry;
        private ExceptionTelemetry exceptionTelemetry;
        private EventTelemetry eventTelemetry;

        string objectType;

        List<KeyValuePair<string, string>> properties = new List<KeyValuePair<string, string>>();
        List<KeyValuePair<string, string>> ContextGlobalProperties = new List<KeyValuePair<string, string>>();
        List<KeyValuePair<string, Double>> metrics = new List<KeyValuePair<string, Double>>();

        public void instantiateClient(string _instrumentationKey, string _objectType)
        {

            if (string.IsNullOrEmpty(_instrumentationKey))
            {
                throw new ArgumentNullException(nameof(_instrumentationKey), "Instrumentation key is required.");
            }
            if (string.IsNullOrEmpty(_objectType))
            {
                throw new ArgumentNullException(nameof(_objectType), "Object type is required.");
            }
            objectType = _objectType;
            TelemetryConfiguration config = new TelemetryConfiguration(_instrumentationKey);
            Client = new TelemetryClient(config);
            switch (objectType)
            {
                case "ExceptionTelemetry":
                    exceptionTelemetry = new ExceptionTelemetry();
                    break;
                case "RequestTelemetry":
                    requestTelemetry = new RequestTelemetry();
                    break;
                case "EventTelemetry":
                    eventTelemetry = new EventTelemetry();
                    break;
            }

        }

        public void addProperty(string _key, string _value)
        {
            KeyValuePair<string, string> property = new KeyValuePair<string, string>(_key, _value);
            if (!properties.Exists(item => item.Key == _key))
            {
                properties.Add(property);
            }
        }

        public void addContextGlobalProperty(string _key, string _value)
        {
            KeyValuePair<string, string> property = new KeyValuePair<string, string>(_key, _value);
            if (!ContextGlobalProperties.Exists(item => item.Key == _key))
            {
                ContextGlobalProperties.Add(property);
            }
        }
        public void addMetrics(string _key, Double _value)
        {
            KeyValuePair<string, Double> metric = new KeyValuePair<string, Double>(_key, _value);
            if (!metrics.Exists(item => item.Key == _key))
            {
                metrics.Add(metric);
            }
        }

        public void setContextUserInfo(string _accountId, string _authenticatedUserId, string _userId = "", string _userAgent = "")
        {
            switch (objectType)
            {
                case "ExceptionTelemetry":
                    exceptionTelemetry.Context.User.AccountId = _accountId;
                    exceptionTelemetry.Context.User.AuthenticatedUserId = _authenticatedUserId;
                    exceptionTelemetry.Context.User.Id = _userId;
                    exceptionTelemetry.Context.User.UserAgent = _userAgent;
                    break;
                case "RequestTelemetry":
                    requestTelemetry.Context.User.AccountId = _accountId;
                    requestTelemetry.Context.User.AuthenticatedUserId = _authenticatedUserId;
                    requestTelemetry.Context.User.Id = _userId;
                    requestTelemetry.Context.User.UserAgent = _userAgent;
                    break;
                case "EventTelemetry":
                    eventTelemetry.Context.User.AccountId = _accountId;
                    eventTelemetry.Context.User.AuthenticatedUserId = _authenticatedUserId;
                    eventTelemetry.Context.User.Id = _userId;
                    eventTelemetry.Context.User.UserAgent = _userAgent;
                    break;
            }
        }
        public void setContextOperation(int _operationId, string _operationName, string _parentId = "")
        {
            switch (objectType)
            {
                case "ExceptionTelemetry":
                    exceptionTelemetry.Context.Operation.Id = String.Format("{0}",_operationId);
                    exceptionTelemetry.Context.Operation.Name = _operationName;
                    exceptionTelemetry.Context.Operation.ParentId = _parentId;
                    break;
                case "RequestTelemetry":
                    requestTelemetry.Context.Operation.Id = String.Format("{0}", _operationId);
                    requestTelemetry.Context.Operation.Name = _operationName;
                    requestTelemetry.Context.Operation.ParentId = _parentId;
                    break;
                case "EventTelemetry":
                    eventTelemetry.Context.Operation.Id = String.Format("{0}", _operationId);
                    eventTelemetry.Context.Operation.Name = _operationName;
                    eventTelemetry.Context.Operation.ParentId = _parentId;
                    break;
            }
        }
        public void setContextComponentVersion(string _version)
        {
            switch (objectType)
            {
                case "ExceptionTelemetry":
                    exceptionTelemetry.Context.Component.Version = _version;
                    break;
                case "RequestTelemetry":
                    requestTelemetry.Context.Component.Version = _version;
                    break;
                case "EventTelemetry":
                    eventTelemetry.Context.Component.Version = _version;
                    break;
            }
        }

        public void setContextGlobalProperties()
        {
            foreach (KeyValuePair<string, string> element in ContextGlobalProperties)
            {

                switch (objectType)
                {
                    case "ExceptionTelemetry":
                        exceptionTelemetry.Context.GlobalProperties.Add(element);
                        break;
                    case "RequestTelemetry":
                        requestTelemetry.Context.GlobalProperties.Add(element);
                        break;
                    case "EventTelemetry":
                        eventTelemetry.Context.GlobalProperties.Add(element);
                        break;
                }
            }
        }

        public void setsysExceptionTelemetryValues(Exception _exception, string _message, string _problemId, int _severityLevel)
        {
            if (exceptionTelemetry != null)
            {

                exceptionTelemetry.Exception = _exception;
                exceptionTelemetry.Message = _message;
                exceptionTelemetry.ProblemId = _problemId;
                switch (_severityLevel)
                {
                    case 4:
                        exceptionTelemetry.SeverityLevel = SeverityLevel.Critical;
                        break;
                    default:
                        exceptionTelemetry.SeverityLevel = SeverityLevel.Information;
                        break;
                }
                exceptionTelemetry.Timestamp = DateTimeOffset.UtcNow;
                foreach (KeyValuePair<string, string> element in properties)
                {
                    exceptionTelemetry.Properties.Add(element);
                }
                foreach (KeyValuePair<string, Double> element in metrics)
                {
                    exceptionTelemetry.Metrics.Add(element);
                }

            }
        }
        public void TrackTelemetry()
        {
            if (Client != null && objectType != "")
            {
                switch (objectType)
                {
                    case "ExceptionTelemetry":
                        Client.TrackException(exceptionTelemetry);
                        break;
                    case "RequestTelemetry":
                        Client.TrackRequest(requestTelemetry);
                        break;
                    case "EventTelemetry":
                        Client.TrackEvent(eventTelemetry);
                        break;
                }
                Client.Flush();
            }
        }
        public void setExceptionTelemetryValues(string _exceptionMessage, string _exceptionSource, string _message, string _problemId, int _severityLevel)
        {
            if (exceptionTelemetry != null)
            {
                Exception e = new Exception(_exceptionMessage);
                e.Source = _exceptionSource;
                exceptionTelemetry.Exception = e;
                exceptionTelemetry.Message = _message;
                exceptionTelemetry.ProblemId = _problemId;
                switch (_severityLevel)
                {
                    case 0:
                        exceptionTelemetry.SeverityLevel = SeverityLevel.Verbose;
                        break;
                    case 1:
                        exceptionTelemetry.SeverityLevel = SeverityLevel.Information;
                        break;
                    case 2:
                        exceptionTelemetry.SeverityLevel = SeverityLevel.Warning;
                        break;
                    case 3:
                        exceptionTelemetry.SeverityLevel = SeverityLevel.Error;
                        break;
                    case 4:
                        exceptionTelemetry.SeverityLevel = SeverityLevel.Critical;
                        break;
                    default:
                        exceptionTelemetry.SeverityLevel = SeverityLevel.Information;
                        break;
                }
                exceptionTelemetry.Timestamp = DateTimeOffset.UtcNow;
                foreach (KeyValuePair<string, string> element in properties)
                {
                    exceptionTelemetry.Properties.Add(element);
                }
                foreach (KeyValuePair<string, Double> element in metrics)
                {
                    exceptionTelemetry.Metrics.Add(element);
                }

            }
        }

        public void setRequestTelemetryValues(string _requestName, string _requestId, Boolean _success, string _responseCode, string _source, string _uriString, Double _duration)
        {
            if (requestTelemetry != null)
            {
                requestTelemetry.Name = _requestName;
                requestTelemetry.Timestamp = DateTimeOffset.UtcNow;
                requestTelemetry.Success = _success;
                requestTelemetry.Id = _requestId;
                requestTelemetry.ResponseCode = _responseCode;
                requestTelemetry.Source = _source;
                requestTelemetry.Duration = TimeSpan.FromSeconds(_duration);
                if (_uriString != "")
                {
                    Uri uri = new Uri(_uriString);
                    requestTelemetry.Url = uri;
                }

                foreach (KeyValuePair<string, string> element in properties)
                {
                    requestTelemetry.Properties.Add(element);
                }
                foreach (KeyValuePair<string, Double> element in metrics)
                {
                    requestTelemetry.Metrics.Add(element);
                }
            }
        }

        public void setEventTelemetryValues(string _requestName)
        {
            if (eventTelemetry != null)
            {
                eventTelemetry.Name = _requestName;
                eventTelemetry.Timestamp = DateTimeOffset.UtcNow;

                foreach (KeyValuePair<string, string> element in properties)
                {
                    eventTelemetry.Properties.Add(element);
                }
                foreach (KeyValuePair<string, Double> element in metrics)
                {
                    eventTelemetry.Metrics.Add(element);
                }
            }
        }

    }
}
