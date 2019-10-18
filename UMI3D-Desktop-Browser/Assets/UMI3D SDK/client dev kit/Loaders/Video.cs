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
using UnityEngine;
using UnityEngine.Video;
using umi3d.common;


namespace umi3d.cdk
{
    public class Video : MonoBehaviour
    {

        public string dtoid = null;

        bool ready = false;

        public double progress = 0f;
        public double StartDate = 0;
        public bool PlayOnAwake = false;
        public bool Playing = false;
        public bool Paused = false;
        public bool Stoped = false;
        public bool Loop = false;

        long framecount = 0;


        public string _AudioSource = null;
        public AudioSource AudioSource {  set {  if (Player)Player.SetTargetAudioSource(0, value); } }
        string VideoUrl = null;

        public Resource VideoResource = new Resource();

        VideoPlayer Player;
        Renderer _renderer;

        public void Set(Video video)
        {
            progress = video.progress;
            StartDate = video.StartDate;
            PlayOnAwake = video.PlayOnAwake;
            Playing = video.Playing;
            Paused = video.Paused;
            Stoped = video.Stoped;
            Loop = video.Loop;
            VideoUrl = video.VideoUrl;
            VideoResource = video.VideoResource;
            StartCoroutine(_Set(video));
        }

        IEnumerator _Set(Video video)
        {
            GameObject audioObj;
            AudioSource audioSource;
            if (video._AudioSource != "")
            {
                while (!((audioObj = UMI3DBrowser.Scene.GetObject(video._AudioSource)) && (audioSource = audioObj.GetComponent<AudioSource>())))
                    yield return 0;
                audioSource.playOnAwake = false;
            } else
            {
                //Add AudioSource
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }

            while (!Player)
                yield return 0;

            //Set Audio Output to AudioSource
            Player.audioOutputMode = VideoAudioOutputMode.AudioSource;

            //Assign the Audio from Video to AudioSource to be played
            Player.EnableAudioTrack(0, true);
            Player.SetTargetAudioSource(0, audioSource);

            //Set video To Play then prepare Audio to prevent Buffering
            LoadClip(VideoResource);
        }


        public void setProgress(double startDate)
        {
            this.StartDate = startDate;
            setProgress();
        }

        void setProgress()
        {

            if (Player /*&& Playing*/ && ready)
            {
                //Debug.Log("set progress fc:" + framecount);
                var deltaTime = getTimeInMilliseconds() - StartDate;
                var _progress = (Player.frameCount > 0) ?  (deltaTime/1000 * Player.frameRate) / Player.frameCount : 0;
                var p= (_progress < 1)? _progress : 1;
                //Debug.Log("set progress "+_progress+ "->" + p + "   " + deltaTime);
                setFrame((long)(framecount * p));
            }

        }

        public void LoadVideo(Renderer renderer)
        {
            _renderer = renderer;
            Player = renderer.gameObject.AddComponent<VideoPlayer>();
            Player.playOnAwake = false;
            /*
            Stop();
            Player.audioOutputMode = UnityEngine.Video.VideoAudioOutputMode.AudioSource;
            Player.playbackSpeed = 1;
            LoadClip(VideoResource);
            //Player.frame = (long)(Player.frameCount * Progress);
            */
        }

        public void UpdateVideo()
        {
            if (Player)
                Player.isLooping = Loop;
        }

        public void Play()
        {
            if (Player)
            {
                if (ready)
                {
                    setProgress();
                    
                }
                else
                {
                    LoadClip(VideoResource);
                }
                Player.Play();
            }
        }

        public void Pause()
        {
            if (Player)
                Player.Pause();
        }
        public void Stop()
        {
            if (Player)
            {
                Player.Pause();
                setFrame(0);
            }
        }

        void setFrame(long frame)
        {
            if (Player && (long)Player.frameCount > frame)
            {
                Player.frame = frame;
                //Debug.Log(Player.frame + " -> " + frame);
            }
            //else Debug.Log("can't set frame");
        }

        public void LoadClip(Resource resource)
        {
            StartCoroutine(_LoadClip(resource));
        }

        IEnumerator _LoadClip(Resource resource)
        {
            var wait = new WaitForEndOfFrame();
            while (!Player)
            {
                yield return 0; // code will wait till file is completely read
            }
            //Debug.Log("load");
            if ((Player.url != resource.Url) /*|| !(Player.isPrepared || _LoadClipOnce)*/)
            {
                //Debug.Log("in load");
                Player.Pause();
                Player.url = resource.Url;
                //Debug.Log(Player.url);
                Player.Prepare();
                while (!Player.isPrepared)
                {
                    yield return wait; // code will wait till file is completely read
                }
                framecount = (long)Player.frameCount;
                if (Playing)
                {
                    Play();
                    //VideoPlayer.time = ((float)((getTimeInMilliseconds() - double.Parse(newdto.PlayStartTimeInMs)) % (audioclip.length * 1000))) / 1000f;
                }
                ready = true;
            }
            //Debug.Log("end load");
            yield return 0;
        }


        double getTimeInMilliseconds()
        {
            return (DateTime.Now - DateTime.MinValue).TotalMilliseconds;
        }

    }
}