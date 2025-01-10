using HarmonyLib;
using ProtoBuf;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
namespace CargoPath
{
    [HarmonyPatch(typeof(BaseBoat), "GenerateOceanPatrolPath", typeof(float), typeof(float))]
    public class BaseBoat_GenerateOceanPatrolPath
    {
        public class SerializedPathList { public List<VectorData> vectorData = new List<VectorData>(); }
        public static SerializedPathList serializedPathList;
        public static List<Vector3> CargoPath = new List<Vector3>();

        public static byte[] Get_SerializedData()
        {
            foreach (MapData MD in World.Serialization.world.maps)
            {
                try { if (System.Text.Encoding.ASCII.GetString(MD.data).Contains("SerializedPathList")) { return MD.data; } }
                catch { }
            }
            return null;
        }

        public static bool ReadMapData()
        {
            bool flag = false;
            if (serializedPathList != null) { return flag; }
            byte[] array = Get_SerializedData();
            if (array != null && array.Length != 0)
            {
                serializedPathList = DeSeriliseMapData<SerializedPathList>(array, out flag);
            }
            return flag;
        }

        public static T DeSeriliseMapData<T>(byte[] bytes, out bool flag)
        {
            T result;
            try
            {
                using (MemoryStream memoryStream = new MemoryStream(bytes))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                    T t = (T)((object)xmlSerializer.Deserialize(memoryStream));
                    flag = true;
                    result = t;
                }
            }
            catch
            {
                flag = false;
                result = default(T);
            }
            return result;
        }

        [HarmonyPrefix]
        static bool Prefix(ref List<Vector3> __result)
        {
            try
            {
                if (ReadMapData())
                {
                    if (serializedPathList != null)
                    {
                        List<Vector3> NewPath = new List<Vector3>();
                        foreach (VectorData vd in serializedPathList.vectorData)
                        {
                            NewPath.Add(new Vector3(vd.x, vd.y, vd.z));
                        }
                        CargoPath = NewPath;
                        __result = CargoPath;
                        return false;
                    }
                }
            }
            catch { }
            return true;
        }

        [HarmonyPostfix]
        static void Postfix(ref List<Vector3> __result)
        {
            try
            {
                if (CargoPath != null && CargoPath.Count != 0)
                {
                    UnityEngine.Debug.LogWarning("Cargoship Loaded Custom Path " + __result.Count + " Nodes.");
                    __result = CargoPath;
                    return;
                }
                CargoPath = __result;
            }
            catch { }
        }
    }
}