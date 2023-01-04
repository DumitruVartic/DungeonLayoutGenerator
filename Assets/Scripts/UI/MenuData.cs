using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Map.Generator;

namespace UI
{
    public class MenuData : MonoBehaviour
    {
        [SerializeField] string seed = "";
        [SerializeField] TMP_InputField SizeX;
        [SerializeField] TMP_InputField SizeY;
        [SerializeField] TMP_InputField SizeZ;
        [SerializeField] TMP_InputField RoomMaxSizeX;
        [SerializeField] TMP_InputField RoomMaxSizeY;
        [SerializeField] TMP_InputField RoomMaxSizeZ;
        [SerializeField] TMP_InputField RoomCount;
        [SerializeField] TMP_InputField RandomSeed;
        [SerializeField] TMP_InputField Probability;
        [SerializeField] TMP_Dropdown SelectedAlghoritm;
        Map.Generator generator;

        private void GetInstance()
        {
            generator = GameObject.Find("Generator").GetComponent<Map.Generator>();
        }

        public string Seed { get => GetSeed(); set => SetSeed(value); }

        string GetSeed()
        {
            GetMenuData();
            SeedEncryption();
            return seed;
        }

        void SetSeed(string seedData)
        {
            seed = seedData;
            SeedDecryption();
            SetMenuData();
        }

        Vector3Int GetSize()
        {
            GetMenuData();
            return size;
        }

        public Vector3Int Size { get => GetSize(); }
        Vector3Int size;
        Vector3Int roomMaxSize;
        int roomCount;
        int randomSeed;
        double probabilityToSelect;
        Alghoritm alghoritm;

        private void GetMenuData()
        {
            GetInstance();
            alghoritm = (Alghoritm)SelectedAlghoritm.value;
            size.x = Convert.ToInt32(SizeX.text);
            size.y = Convert.ToInt32(SizeY.text);
            size.z = Convert.ToInt32(SizeZ.text);
            roomMaxSize.x = Convert.ToInt32(RoomMaxSizeX.text);
            roomMaxSize.y = Convert.ToInt32(RoomMaxSizeY.text);
            roomMaxSize.z = Convert.ToInt32(RoomMaxSizeZ.text);
            roomCount = Convert.ToInt32(RoomCount.text);
            randomSeed = Convert.ToInt32(RandomSeed.text);
            if (generator.addExtraEdges)
                probabilityToSelect = Convert.ToDouble(Probability.text);
            else
                probabilityToSelect = 0;
        }

        private void SetMenuData()
        {
            SelectedAlghoritm.value = Convert.ToInt32(alghoritm);
            SizeX.text = Convert.ToString(size.x);
            SizeY.text = Convert.ToString(size.y);
            SizeZ.text = Convert.ToString(size.z);
            RoomMaxSizeX.text = Convert.ToString(roomMaxSize.x);
            RoomMaxSizeY.text = Convert.ToString(roomMaxSize.y);
            RoomMaxSizeZ.text = Convert.ToString(roomMaxSize.z);
            RoomCount.text = Convert.ToString(roomCount);
            RandomSeed.text = Convert.ToString(randomSeed);
            Probability.text = Convert.ToString(probabilityToSelect);
        }

        private void SeedEncryption()
        {
            seed =
            "SX" + size.x + "SY" + size.y + "SZ" + size.z +
                "RX" + roomMaxSize.x + "RY" + roomMaxSize.y + "RZ" + roomMaxSize.z +
                "R" + roomCount + "A" + Convert.ToInt32(alghoritm) + "P" + probabilityToSelect + "RS" + randomSeed;
        }

        private void SeedDecryption()
        {
            string valid = "SXSYSZRXRYRZRPARS";
            bool found = false;
            string last = string.Empty;
            foreach (string s in SplitAlpha(seed))
            {
                if (found)
                {
                    found = false;
                    switch (last)
                    {
                        case "SX": size.x = Convert.ToInt32(s); break;
                        case "SY": size.y = Convert.ToInt32(s); break;
                        case "SZ": size.z = Convert.ToInt32(s); break;
                        case "RX": roomMaxSize.x = Convert.ToInt32(s); break;
                        case "RY": roomMaxSize.y = Convert.ToInt32(s); break;
                        case "RZ": roomMaxSize.z = Convert.ToInt32(s); break;
                        case "R": roomCount = Convert.ToInt32(s); break;
                        case "A": alghoritm = (Alghoritm)Convert.ToInt32(s); break;
                        case "P": probabilityToSelect = Convert.ToDouble(s); break;
                        case "RS": randomSeed = Convert.ToInt32(s); break;
                        default: Debug.Log("Incorect seed"); break;
                    }
                }

                if (valid.Contains(s.ToUpper()))
                {
                    found = true;
                    last = s.ToUpper();
                }
            }
        }

        private static IEnumerable<string> SplitAlpha(string seed)
        {
            var words = new List<string> { string.Empty };
            for (var i = 0; i < seed.Length; i++)
            {
                words[words.Count - 1] += seed[i];
                if (i + 1 < seed.Length && char.IsLetter(seed[i]) != char.IsLetter(seed[i + 1]))
                {
                    words.Add(string.Empty);
                }
            }
            return words;
        }
    }
}