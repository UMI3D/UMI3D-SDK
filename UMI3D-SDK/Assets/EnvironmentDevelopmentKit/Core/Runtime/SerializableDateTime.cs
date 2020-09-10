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
using UnityEngine;

namespace umi3d.edk
{

    [System.Serializable]
    public class SerializableDateTime : ISerializationCallbackReceiver
    {
        [HideInInspector] DateTime dateTime;
        [HideInInspector] [SerializeField] private string _dateTime;

        [HideInInspector] [SerializeField] private int day;
        [HideInInspector] [SerializeField] private int month;
        [HideInInspector] [SerializeField] private int year;

        [HideInInspector] [SerializeField] private int hours;
        [HideInInspector] [SerializeField] private int minutes;
        [HideInInspector] [SerializeField] private int seconds;

        [HideInInspector] [SerializeField] private bool setNow;

        public void OnAfterDeserialize()
        {
            if (setNow)
            {
                dateTime = DateTime.Now;
            }
            else
            {
                _dateTime = $"{day}/{month}/{year} {hours}:{minutes}:{seconds}";
                DateTime tmp;
                if (DateTime.TryParse(_dateTime, out tmp)) dateTime = tmp;
            }
        }

        public void OnBeforeSerialize()
        {
            day = dateTime.Day;
            month = dateTime.Month;
            year = dateTime.Year;
            hours = dateTime.Hour;
            minutes = dateTime.Minute;
            seconds = dateTime.Second;
            setNow = false;
            _dateTime = dateTime.ToString();
        }

        public override string ToString()
        {
            return dateTime.ToString();
        }

        public static implicit operator DateTime(SerializableDateTime date)
        {
            return (date.dateTime);
        }
        public static implicit operator SerializableDateTime(DateTime date)
        {
            return new SerializableDateTime() { dateTime = date };
        }

    }
}