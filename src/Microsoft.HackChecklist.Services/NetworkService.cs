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

using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.HackChecklist.Services.Contracts;
using Newtonsoft.Json.Linq;

namespace Microsoft.HackChecklist.Services
{
    public class NetworkService : INetworkService
    {
        public async Task<string> Get(string requestUrl)
        {
            var client = new HttpClient();

            var response = await client.GetAsync(requestUrl, HttpCompletionOption.ResponseContentRead);
            return await response.Content.ReadAsStringAsync();
        }
    }
}
