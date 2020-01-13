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
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    public class VideoDtoLoader : AbstractDTOLoader<VideoDto, Video>
    {

        public override void LoadDTO(VideoDto dto, Action<Video> callback)
        {
            Video video = GetComponent<Video>() ?? gameObject.AddComponent<Video>();
            video._AudioSource = dto.AudioSource;
            UpdateFromDTO(video, null, dto);
            callback(video);
        }



        public override void UpdateFromDTO(Video Video, VideoDto olddto, VideoDto newdto)
        {
            if (newdto == null)
                return;

            UpdateProperties(Video, olddto, newdto);
            UpdateClip(Video, olddto, newdto);
            UpdateClipPlaying(Video, olddto, newdto);
            Video.UpdateVideo();
        }

        void UpdateProperties(Video Video, VideoDto olddto, VideoDto newdto)
        {
            Video.Playing = newdto.Playing;
            Video.Paused = newdto.Paused;
            Video.Loop = newdto.Loop;
            Video.PlayOnAwake = newdto.PlayOnAwake;
        }

        void UpdateClip(Video Video, VideoDto olddto, VideoDto newdto)
        {
            if (olddto == null || !olddto.VideoResource.Equals(newdto.VideoResource))
            {
                Resource resource = new Resource();
                resource.Set(newdto.VideoResource);
                Video.VideoResource = resource;
                LoadClip(resource, Video, newdto);
            }
        }

        void UpdateClipPlaying(Video Video, VideoDto olddto, VideoDto newdto)
        {
            if (olddto == null)
            {
                if (newdto.Playing)
                {
                    Video.Play();
                }
                else if (newdto.Paused)
                {
                    Video.Pause();
                }
                else
                {
                    Video.Stop();
                }
            }
            else
            {
                if (!olddto.Playing && newdto.Playing)
                {
                    Video.Play();

                }
                else if (olddto.Playing && !newdto.Playing)
                {
                    if (newdto.Paused)
                    {
                        Video.Pause();
                    }
                    else
                    {
                        Video.Stop();
                    }
                }
            }

            double sd = double.Parse(newdto.StartTimeInMS);
            if (sd != Video.StartDate)
            {
                Video.setProgress(sd);
                //Video.StartDate = double.Parse( newdto.StartTimeInMS );
            }


        }

        void LoadClip(Resource resource, Video Video, VideoDto newdto)
        {
            Video.LoadClip(resource);
        }

        double getTimeInMilliseconds()
        {
            return (DateTime.Now - DateTime.MinValue).TotalMilliseconds;
        }
    }
}