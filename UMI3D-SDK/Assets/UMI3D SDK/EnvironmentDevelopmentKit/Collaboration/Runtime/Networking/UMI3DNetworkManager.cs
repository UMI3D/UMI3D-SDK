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
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using BeardedManStudios.SimpleJSON;

/// <summary>
/// This class provides a NetworkManager which is able to customize the port given to a master sever.
/// </summary>
public class UMI3DNetworkManager : NetworkManager
{
	/// <summary>
	/// Converts all data required to register this server to a master server into a json.
	/// </summary>
	/// <param name="server"></param>
	/// <param name="connectionPort"></param>
	/// <param name="id"></param>
	/// <param name="serverName"></param>
	/// <param name="type"></param>
	/// <param name="mode"></param>
	/// <param name="comment"></param>
	/// <param name="useElo"></param>
	/// <param name="eloRequired"></param>
	/// <returns></returns>
	public virtual JSONNode MasterServerRegisterData(NetWorker server, string connectionPort, string id, string serverName, string type, string mode, string comment = "", bool useElo = false, int eloRequired = 0)
	{
		JSONNode sendData = base.MasterServerRegisterData(server, id, serverName, type, mode, comment, useElo, eloRequired);

		sendData["register"]["port"] = connectionPort;

		return sendData;
	}
}
