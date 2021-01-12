/*
Copyright 2019 Gfi Informatique

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

using System;
using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk.collaboration
{
	[RequireComponent(typeof(AudioSource))]
	public class MicrophoneListener : Singleton<MicrophoneListener>
	{
		/// <summary>
		/// Whether the microphone is running
		/// </summary>
		public static bool IsMute { get { return umi3d.cdk.collaboration.UMI3DCollaborationClientServer.Exists && umi3d.cdk.collaboration.UMI3DCollaborationClientServer.Instance?.ForgeClient != null ? umi3d.cdk.collaboration.UMI3DCollaborationClientServer.Instance.ForgeClient.muted : false; } set { if (umi3d.cdk.collaboration.UMI3DCollaborationClientServer.Exists && umi3d.cdk.collaboration.UMI3DCollaborationClientServer.Instance?.ForgeClient != null) umi3d.cdk.collaboration.UMI3DCollaborationClientServer.Instance.ForgeClient.muted = value; } }

		//[SerializeField]
		//bool _IsMute = false;


		///// <summary>
		///// Whether the microphone is running
		///// </summary>
		//public bool IsRecording { get; private set; }

		///// <summary>
		///// The frequency at which the mic is operating
		///// </summary>
		//public int Frequency { get; private set; }

		///// <summary>
		///// Last populated audio sample
		///// </summary>
		//public float[] Sample { get; private set; }

		///// <summary>
		///// Sample duration/length in milliseconds
		///// </summary>
		//public int SampleDurationMS { get; private set; }

		///// <summary>
		///// The length of the sample float array
		///// </summary>
		//public int SampleLength
		//{
		//	get { return Frequency * SampleDurationMS / 1000; }
		//}

		//AudioClip Clip;

		///// <summary>
		///// List of all the available Mic devices
		///// </summary>
		//public List<string> Devices { get; private set; }

		//int CurrentDeviceIndex;

		///// <summary>
		///// Gets the name of the Mic device currently in use
		///// </summary>
		//public string CurrentDeviceName
		//{
		//	get { return Devices[CurrentDeviceIndex]; }
		//}

		//AudioSource AudioSource;
		//int SampleCount = 0;

		//protected override void Awake()
		//{
		//	base.Awake();
		//	AudioSource = GetComponent<AudioSource>();

		//	Devices = new List<string>();
		//	foreach (var device in Microphone.devices)
		//		Devices.Add(device);
		//	CurrentDeviceIndex = 0;
		//}

		//void Update()
		//{
		//	if (AudioSource == null)
		//		AudioSource = gameObject.AddComponent<AudioSource>();

		//	AudioSource.mute = true;
		//	AudioSource.loop = true;
		//	AudioSource.maxDistance = AudioSource.minDistance = 0;
		//	AudioSource.spatialBlend = 0;

		//	if (IsRecording && !AudioSource.isPlaying)
		//		AudioSource.Play();
		//}

		///// <summary>
		///// Changes to a Mic device for Recording
		///// </summary>
		///// <param name="index">The index of the Mic device. Refer to <see cref="Devices"/></param>
		//public void ChangeDevice(int index)
		//{
		//	Microphone.End(CurrentDeviceName);
		//	CurrentDeviceIndex = index;
		//	Microphone.Start(CurrentDeviceName, true, 1, Frequency);
		//}

		/// <summary>
		/// Starts to stream the input of the current Mic device
		/// </summary>
		public void StartRecording(int frequency = 16000, int sampleLen = 10)
		{
			//StopRecording();
			//IsRecording = true;

			//Frequency = frequency;
			//SampleDurationMS = sampleLen;

			//Clip = Microphone.Start(CurrentDeviceName, true, 1, Frequency);
			//Debug.Log(Clip.channels);
			//Sample = new float[Frequency / 1000 * SampleDurationMS * Clip.channels];

			//AudioSource.clip = Clip;

			//StartCoroutine(ReadRawAudio());
		}

		/// <summary>
		/// Ends the Mic stream.
		/// </summary>
		public void StopRecording()
		{
			//if (!Microphone.IsRecording(CurrentDeviceName)) return;

			//IsRecording = false;

			//Microphone.End(CurrentDeviceName);
			//Destroy(Clip);
			//Clip = null;
			//AudioSource.Stop();

			//StopCoroutine(ReadRawAudio());
		}

		//IEnumerator ReadRawAudio()
		//{
		//	int loops = 0;
		//	int readAbsPos = 0;
		//	int prevPos = 0;
		//	float[] temp = new float[Sample.Length];

		//	while (Clip != null && Microphone.IsRecording(CurrentDeviceName))
		//	{
		//		bool isNewDataAvailable = true;

		//		while (isNewDataAvailable)
		//		{
		//			int currPos = Microphone.GetPosition(CurrentDeviceName);
		//			if (currPos < prevPos)
		//				loops++;
		//			prevPos = currPos;

		//			var currAbsPos = loops * Clip.samples + currPos;
		//			var nextReadAbsPos = readAbsPos + temp.Length;

		//			if (nextReadAbsPos < currAbsPos)
		//			{
		//				Clip.GetData(temp, readAbsPos % Clip.samples);

		//				Sample = temp;
		//				SampleCount++;
		//				if (!_IsMute && UMI3DCollaborationClientServer.Exists)
		//				{
		//					var dto = new AudioDto()
		//					{
		//						frequency = Frequency,
		//						pos = SampleCount,
		//						sample = Sample
		//					};
		//					Debug.Log("Send Audio");
		//					//UMI3DCollaborationClientServer.Instance.SendAudio(dto);
		//				}

		//				readAbsPos = nextReadAbsPos;
		//				isNewDataAvailable = true;
		//			}
		//			else
		//				isNewDataAvailable = false;
		//		}
		//		yield return null;
		//	}
		//}

		//protected override void OnDestroy()
		//{
		//	base.OnDestroy();
		//	Microphone.End(null);
		//}
	}
}
