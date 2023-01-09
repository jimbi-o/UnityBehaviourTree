using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BehaviourTree
{

    [StructLayout(LayoutKind.Explicit)]
    struct ValueUnion
    {
        [FieldOffset(0)]
        public int i;

        [FieldOffset(0)]
        public float f;

        [FieldOffset(0)]
        public bool b;
    }

    public class BlackBoard<Key>
    {
        private Dictionary<Key, ValueUnion> valueMap = new Dictionary<Key, ValueUnion>();
        private Dictionary<Key, Vector3>    vector3Map = new Dictionary<Key, Vector3>();
        private Dictionary<Key, GameObject> gameObjectMap = new Dictionary<Key, GameObject>();

        public void SetFloat(Key key, in float value)
        {
            var data = new ValueUnion();
            data.f = value;
            valueMap[key] = data;
        }

        public float GetFloat(in Key key)
        {
            if (!valueMap.ContainsKey(key))
            {
                return 0.0f;
            }
            return valueMap[key].f;
        }

        public void SetBool(Key key, in bool value)
        {
            var data = new ValueUnion();
            data.b = value;
            valueMap[key] = data;
        }

        public bool GetBool(in Key key)
        {
            if (!valueMap.ContainsKey(key))
            {
                return false;
            }
            return valueMap[key].b;
        }

        public void SetInt(Key key, in int value)
        {
            var data = new ValueUnion();
            data.i = value;
            valueMap[key] = data;
        }

        public int GetInt(in Key key)
        {
            if (!valueMap.ContainsKey(key))
            {
                return 0;
            }
            return valueMap[key].i;
        }

        public void SetVector3(Key key, in Vector3 value)
        {
            vector3Map[key] = value;
        }

        public Vector3 GetVector3(in Key key)
        {
            if (!vector3Map.ContainsKey(key))
            {
                return Vector3.zero;
            }
            return vector3Map[key];
        }

        public void SetGameObject(Key key, in GameObject value)
        {
            gameObjectMap[key] = value;
        }

        public GameObject GetGameObject(in Key key)
        {
            if (!gameObjectMap.ContainsKey(key))
            {
                return null;
            }
            return gameObjectMap[key];
        }
    }

    public class BTNode
    {
    }
}
