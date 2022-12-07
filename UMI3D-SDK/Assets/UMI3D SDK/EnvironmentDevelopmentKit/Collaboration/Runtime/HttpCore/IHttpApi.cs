/*
Copyright 2019 - 2021 Inetum

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using WebSocketSharp.Net;

/// <summary>
/// Behaviour of an API using HTTP GET/POST requests.
/// </summary>
public interface IHttpApi
{
    /// <summary>
    /// Define the authentification method used on routes.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="allowOldToken"></param>
    /// <returns></returns>
    bool isAuthenticated(HttpListenerRequest request, bool allowOldToken);
}
