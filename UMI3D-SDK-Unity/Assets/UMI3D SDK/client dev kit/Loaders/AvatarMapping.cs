using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using umi3d.common;


namespace umi3d.cdk
{
    public class AvatarMapping : MonoBehaviour
    {

        public string userId;

        public BonePairDictionary bonePairDictionary = new BonePairDictionary();

        public void SetMapping(string id, BonePairDictionary pairDictionary)
        {
            userId = id;
            bonePairDictionary = pairDictionary;
        }
    }
}
