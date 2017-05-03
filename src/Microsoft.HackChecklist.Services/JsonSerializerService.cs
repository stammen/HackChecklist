//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using Microsoft.HackChecklist.Services.Contracts;

namespace Microsoft.HackChecklist.Services
{
    public class JsonSerializerService : IJsonSerializerService
    {
        public JsonSerializerService()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
            };
        }

        public string Serialize<T>(T data)
        {
            try
            {
                if (data == null) return string.Empty;

                return JsonConvert.SerializeObject(data);
            }
            catch
            {
                return string.Empty;
            }
        }

        public T Deserialize<T>(string strData)
        {
            try
            {
                return string.IsNullOrWhiteSpace(strData) ? default(T) 
                    : JsonConvert.DeserializeObject<T>(strData, new StringEnumConverter());
            }
            catch (Exception)
            {
                return default(T);
            }
        }        
    }
}
