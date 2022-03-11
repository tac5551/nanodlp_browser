using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NanoDLP_Browser
{
    public static class FileIO
    {

        private static string fileName = @"config.xml";

        public static void SaveToXML(List<Dto> saveDtos)
        {
            //XmlSerializerオブジェクトを作成
            //オブジェクトの型を指定する
            System.Xml.Serialization.XmlSerializer serializer =
                new System.Xml.Serialization.XmlSerializer(typeof(List<Dto>));
            //書き込むファイルを開く（UTF-8 BOM無し）
            System.IO.StreamWriter sw = new System.IO.StreamWriter(
                fileName, false, new System.Text.UTF8Encoding(false));
            //シリアル化し、XMLファイルに保存する
            serializer.Serialize(sw, saveDtos);
            //ファイルを閉じる
            sw.Close();

        }

        public static List<Dto> LoadFromXML()
        {
            //XmlSerializerオブジェクトを作成
            System.Xml.Serialization.XmlSerializer serializer =
                new System.Xml.Serialization.XmlSerializer(typeof(List<Dto>));
            //読み込むファイルを開く
            System.IO.StreamReader sr = new System.IO.StreamReader(
                fileName, new System.Text.UTF8Encoding(false));
            //XMLファイルから読み込み、逆シリアル化する
            List<Dto> obj = (List<Dto>)serializer.Deserialize(sr);
            //ファイルを閉じる
            sr.Close();
            return obj;
        }

        public static void SaveFile(Dtos _dtos)
        {
            List<Dto> saveDtos = new List<Dto>();
            foreach (var each in _dtos)
            {
                if (each.ManualAdd == true)
                {
                    saveDtos.Add(each);
                }
            }

            SaveToXML(saveDtos);
        }

        public static void LoadFile(ref Dtos _dtos)
        {
            if (File.Exists(fileName))
            {
                List<Dto> saveDtos = LoadFromXML();

                foreach (var each in saveDtos)
                {
                    each.ManualAdd = true;
                    each.Enable = true;

                    _dtos.Add(each) ;
                }
            }
        }

    }
}
