using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;

namespace Peppa
{
    public class RestApi
    {
        private RestApi(Method method)
        {
            _restClient = new RestClient();

            _request = new RestRequest((RestSharp.Method)method);
            _request.JsonSerializer = new JsonSerializer();
        }

        private String _url;
        private IRestClient _restClient;
        private IRestRequest _request;
        private IRestResponse _response;
        private Dictionary<string, object> _bodyDictionary;
        private Dictionary<string, object> _paramDictionary;

        public IList<RestResponseCookie> GetCookie()
        {
            return _response.Cookies;
        }

        public RestApi AddCookie(string key, string value)
        {
            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
            {
                this._request.AddCookie(key, value);
            }
            return this;
        }

        public RestApi AddCookie(IList<RestResponseCookie> items)
        {
            if (items != null)
            {
                foreach (var item in items)
                {
                    this._request.AddCookie(item.Name, item.Value);
                }
            }
            return this;
        }

        public static RestApi NewInstance(Method method)
        {
            return new RestApi(method);
        }

        public RestApi SetUrl(string url)
        {
            this._url = url;
            _restClient.BaseUrl = new Uri(this._url);
            return this;
        }

        public RestApi SetMethod()
        {
            return this;
        }

        public RestApi AddHeader(string contentType, string value)
        {
            _request.AddHeader(contentType, value);
            return this;
        }

        public RestApi AddUrlSegment(string key, string value)
        {
            _request.AddUrlSegment(key, value);
            return this;
        }

        public RestApi AddQueryParameter(string key, object value)
        {
            if (_paramDictionary == null)
            {
                _paramDictionary = new Dictionary<string, object>();
            }

            if (_paramDictionary.ContainsKey(key))
            {
                _paramDictionary.Remove(key);
            }
            _paramDictionary.Add(key, value?.ToString());
            return this;
        }

        public RestApi AddQueryParameter(object jsonObj)
        {
            String value = DynamicJson.SerializeObject(jsonObj);
            var dictionary = DynamicJson.DeserializeObject<Dictionary<string, object>>(value);
            if (_paramDictionary != null)
            {
                foreach (var key in dictionary.Keys)
                {
                    this._paramDictionary.Add(key, dictionary[key]);
                }
            }
            else
            {
                this._paramDictionary = dictionary;
            }

            return this;
        }

        public RestApi AddJsonBody(object jsonObj)
        {
            String value = DynamicJson.SerializeObject(jsonObj);
            var dictionary = DynamicJson.DeserializeObject<Dictionary<string, object>>(value);
            if (_bodyDictionary != null)
            {
                foreach (var key in dictionary.Keys)
                {
                    this._bodyDictionary.Add(key, dictionary[key]);
                }
            }
            else
            {
                this._bodyDictionary = dictionary;
            }

            return this;
        }

        public RestApi AddJsonBody(string key, object value)
        {
            if (_bodyDictionary == null)
            {
                _bodyDictionary = new Dictionary<string, object>();
            }

            if (_bodyDictionary.ContainsKey(key))
            {
                _bodyDictionary.Remove(key);
            }
            _bodyDictionary.Add(key, value);
            return this;
        }

        public RestApi ExecuteAsync(Action<bool, Exception, RestApi> callback)
        {
            PrepareRequest();
            PeppaUtils.Debug("Async:" + this._restClient.BuildUri(this._request).ToString());
            var obj = _restClient.ExecuteAsync(this._request, (response) =>
            {
                this._response = response;
                callback?.Invoke(true, null, this);
            });

            return this;
        }

        public RestApi Execute()
        {
            PrepareRequest();
            PeppaUtils.Debug(this._restClient.BuildUri(this._request).ToString());
            this._response = _restClient.Execute(this._request);
            return this;
        }

        private void PrepareRequest()
        {
            if (_bodyDictionary != null)
            {
                BuildJsonBody();
                _request.AddJsonBody(_bodyDictionary);
            }
            else
            {
                Dictionary<string, string> dicParams = new Dictionary<string, string>();
                _request.AddJsonBody(dicParams);
            }

            if (_paramDictionary != null)
            {
                foreach (var key in _paramDictionary.Keys)
                {
                    _request.AddQueryParameter(key,
                        _paramDictionary[key] == null ? null : _paramDictionary[key].ToString());
                }
            }
        }

        /// <summary>
        /// 替換jsonBody中的空值
        /// </summary>
        private void BuildJsonBody()
        {
            HashSet<string> keys = new HashSet<string>();
            foreach (var item in _bodyDictionary)
            {
                if (item.Value != null && item.Value.GetType().FullName == "MS.Internal.NamedObject")
                {
                    keys.Add(item.Key);
                }
            }
            foreach (var item in keys)
            {
                _bodyDictionary[item] = null;
            }
        }

        public RestApi OutRef(out IRestClient rc, out IRestResponse resp, out IRestRequest res)
        {
            rc = this._restClient;
            resp = this._response;
            res = this._request;
            return this;
        }

        public RestApi OutRef(out IRestResponse resp, out IRestRequest res)
        {
            resp = this._response;
            res = this._request;
            return this;
        }

        public RestApi OutRef(out IRestResponse resp)
        {
            resp = this._response;
            return this;
        }

        public RestApi OutRef(out IRestRequest res)
        {
            res = this._request;
            return this;
        }

        public long RespStatus()
        {
            return (long)this._response.StatusCode;
        }

        public int ToInt()
        {
            return (int)ExecFuncForReturnStruct<int>();
        }

        public long ToLong()
        {
            return (long)ExecFuncForReturnStruct<long>();
        }

        public float ToFloat()
        {
            return (float)ExecFuncForReturnStruct<float>();
        }

        public double ToFloat(Func<IRestResponse, double> func)
        {
            return (double)ExecFuncForReturnStruct<double>();
        }

        public Boolean ToBoolean()
        {
            return (Boolean)ExecFuncForReturnStruct<Boolean>();
        }

        public T To<T>() where T : class
        {
            return (T)ExecFuncForReturnClass<T>();
        }

        private object ExecFuncForReturnStruct<T>() where T : struct
        {
            return DynamicJson.DeserializeObject<T>(_response.Content);
        }

        private object ExecFuncForReturnClass<T>() where T : class
        {
            if (typeof(T) == typeof(string))
            {
                return _response.Content;
            }

            return DynamicJson.DeserializeObject<T>(_response.Content);
        }
    }
}