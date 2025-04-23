using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

public enum EEquipmentType
{
	Hair,
	BackHair,
	Head,
	Face,
	Hat,
	Accessory,
	Robe,
	UpperBody,
	LowerBody,
	BackStuff,
	HandStuff,
	MAX
}

#if UNITY_EDITOR
public class CharacterSpriteSlicer
{
    public struct SpriteSliceInfo
	{
		public string name;
		public Rect rect;
		public Vector2 pivot;
		public SpriteSliceInfo(string InName, Rect InRect, Vector2 InPivot)
		{
			name = InName;
			rect = InRect;
			pivot = InPivot;
		}
	}

	[MenuItem("Tools/Slice Character Spritesheets %&s")]
	public static void Slice()
	{
		var textures = Selection.GetFiltered<Texture2D>(SelectionMode.Assets);

		foreach (var texture in textures)
		{
			ProcessTexture(texture);
		}
	}


	static void ProcessTexture(Texture2D texture)
	{
		string path = AssetDatabase.GetAssetPath(texture);
		TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
		importer.isReadable = true;
		importer.textureType = TextureImporterType.Sprite;
		importer.spriteImportMode = SpriteImportMode.Multiple;
		importer.mipmapEnabled = false;
		importer.filterMode = FilterMode.Point;
		importer.spritePivot = Vector2.down;
		importer.textureCompression = TextureImporterCompression.Uncompressed;

		var textureSettings = new TextureImporterSettings();
		importer.ReadTextureSettings(textureSettings);
		textureSettings.spriteMeshType = SpriteMeshType.Tight;
		textureSettings.spriteExtrude = 1;

		importer.SetTextureSettings(textureSettings);

		

		Dictionary<string, SpriteSliceInfo> SliceInfos = new Dictionary<string, SpriteSliceInfo>();
		SliceInfos.Add("BackHair", new SpriteSliceInfo("BackHair", new Rect(0, 192, 64, 64), new Vector2(32, 36)));
		SliceInfos.Add("RightBackHair", new SpriteSliceInfo("RightBackHair", new Rect(64, 192, 32, 64), new Vector2(16, 32)));
		SliceInfos.Add("LeftBackHair", new SpriteSliceInfo("LeftBackHair", new Rect(96, 192, 32, 64), new Vector2(16, 32)));
		SliceInfos.Add("Hair", new SpriteSliceInfo("Hair", new Rect(0, 128, 64, 64), new Vector2(32, 18)));
		SliceInfos.Add("Head", new SpriteSliceInfo("Head", new Rect(0, 96, 64, 32), new Vector2(32, 5)));
		SliceInfos.Add("Eye", new SpriteSliceInfo("Eye", new Rect(0, 64, 64, 32), new Vector2(32, 12)));
		SliceInfos.Add("Accessory", new SpriteSliceInfo("Accessory", new Rect(0, 32, 64, 32), new Vector2(32, 7)));
		SliceInfos.Add("UpperBody", new SpriteSliceInfo("UpperBody", new Rect(0, 0, 32, 32), new Vector2(16, 9)));
		SliceInfos.Add("LowerBody", new SpriteSliceInfo("LowerBody", new Rect(32, 0, 32, 32), new Vector2(16, 16)));
		SliceInfos.Add("BackHat", new SpriteSliceInfo("BackHat", new Rect(64, 160, 64, 32), new Vector2(32, 16)));
		SliceInfos.Add("Hat", new SpriteSliceInfo("Hat", new Rect(64, 128, 64, 32), new Vector2(32, 16)));
		
		SliceInfos.Add("RightArm", new SpriteSliceInfo("RightArm", new Rect(64, 96, 32, 32), new Vector2(16, 22)));
		SliceInfos.Add("RightHand", new SpriteSliceInfo("RightHand", new Rect(64, 64, 32, 32), new Vector2(15, 19)));
		SliceInfos.Add("LeftArm", new SpriteSliceInfo("LeftArm", new Rect(96, 96, 32, 32), new Vector2(16, 22)));
		SliceInfos.Add("LeftHand", new SpriteSliceInfo("LeftHand", new Rect(96, 64, 32, 32), new Vector2(17, 19)));
		SliceInfos.Add("RightLeg", new SpriteSliceInfo("RightLeg", new Rect(64, 32, 32, 32), new Vector2(14, 18)));
		SliceInfos.Add("RightFoot", new SpriteSliceInfo("RightFoot", new Rect(64, 0, 32, 32), new Vector2(14, 16)));
		SliceInfos.Add("LeftLeg", new SpriteSliceInfo("LeftLeg", new Rect(96, 32, 32, 32), new Vector2(14, 18)));
		SliceInfos.Add("LeftFoot", new SpriteSliceInfo("LeftFoot", new Rect(96, 0, 32, 32), new Vector2(14, 16)));


		List<SpriteMetaData> newData = new List<SpriteMetaData>();

		for (int i = 0; i < importer.spritesheet.Length; i++)
        {
			string[] splitedName = importer.spritesheet[i].name.Split("_");
			string partName = splitedName[2];
			if (SliceInfos.ContainsKey(partName))
			{
				importer.spritesheet[i].name = texture.name + "_" + partName;
				importer.spritesheet[i].rect = SliceInfos[partName].rect;
				importer.spritesheet[i].pivot = SliceInfos[partName].pivot;
				SliceInfos.Remove(partName);

				newData.Add(importer.spritesheet[i]);
			}
		}

		foreach (var SliceInfo in SliceInfos)
        {
			SpriteMetaData smd = new SpriteMetaData();
			smd.alignment = (int)SpriteAlignment.Custom;
			smd.pivot = new Vector2(SliceInfo.Value.pivot.x / SliceInfo.Value.rect.width, SliceInfo.Value.pivot.y / SliceInfo.Value.rect.height);
			smd.name = texture.name +"_"+ SliceInfo.Key;
			smd.rect = SliceInfo.Value.rect;

			newData.Add(smd);
		}
		importer.spritesheet = newData.ToArray();
		AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
	}
	
	
	[MenuItem("Tools/MakeCSV %&s")]
	public static void MakeCSV()
	{
		var textures = Selection.GetFiltered<Texture2D>(SelectionMode.Assets);
		int count = 0;
		foreach (var texture in textures)
		{
			ProcessCSV(texture, count);
			count++;
		}
	}

	static void ProcessCSV(Texture2D texture, int count)
	{
		string path = AssetDatabase.GetAssetPath(texture);
		TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
		
		// 11개의 타입 리스트
		Dictionary<EEquipmentType, List<string>> equipmentPartDict = new Dictionary<EEquipmentType, List<string>>();
		for (EEquipmentType i = EEquipmentType.Hair; i < EEquipmentType.MAX; i++)
		{
			equipmentPartDict.Add(i, new List<string>());
		}
		equipmentPartDict[EEquipmentType.BackHair].Add("BackHair");
		equipmentPartDict[EEquipmentType.Hair].Add("Hair");
		equipmentPartDict[EEquipmentType.Head].Add("Head");
		equipmentPartDict[EEquipmentType.Face].Add("Eye");
		equipmentPartDict[EEquipmentType.Hat].Add("Hat");
		equipmentPartDict[EEquipmentType.Hat].Add("BackHat");
		equipmentPartDict[EEquipmentType.Accessory].Add("Accessory");
		equipmentPartDict[EEquipmentType.Robe].Add("Robe");
		equipmentPartDict[EEquipmentType.Robe].Add("Robe_R");
		equipmentPartDict[EEquipmentType.Robe].Add("Robe_L");
		equipmentPartDict[EEquipmentType.UpperBody].Add("UpperBody");
		equipmentPartDict[EEquipmentType.UpperBody].Add("RightArm");
		equipmentPartDict[EEquipmentType.UpperBody].Add("RightHand");
		equipmentPartDict[EEquipmentType.UpperBody].Add("LeftArm");
		equipmentPartDict[EEquipmentType.UpperBody].Add("LeftHand");
		equipmentPartDict[EEquipmentType.LowerBody].Add("LowerBody");
		equipmentPartDict[EEquipmentType.LowerBody].Add("RightLeg");
		equipmentPartDict[EEquipmentType.LowerBody].Add("LeftLeg");
		equipmentPartDict[EEquipmentType.BackStuff].Add("BackStuff");
		equipmentPartDict[EEquipmentType.HandStuff].Add("HandStuff_L");
		equipmentPartDict[EEquipmentType.HandStuff].Add("HandStuff_R");
		
		// 파트가 나뉘어져있으면 csv 아웃풋으로 보낸다.
		Dictionary<EEquipmentType, List<string>> outputPartDict = new Dictionary<EEquipmentType, List<string>>();
		foreach (var equipmentPartInfo in equipmentPartDict)
		{
			for (int j = 0; j < equipmentPartInfo.Value.Count; j++)
			{
				string equipmentName = equipmentPartInfo.Value[j];
				string result = "null";
				for (int i = 0; i < importer.spritesheet.Length; i++)
				{
					string[] splitedName = importer.spritesheet[i].name.Split("_");
					string partName = splitedName[splitedName.Length-1];
					if (equipmentName == partName)
					{
						result = importer.spritesheet[i].name;
                        Debug.LogWarning("equipmentPartInfo.Key : " + importer.spritesheet[i].name);
						break;
                    }
                }

                if (outputPartDict.ContainsKey(equipmentPartInfo.Key) == false)
                {
                    outputPartDict.Add(equipmentPartInfo.Key, new List<string>());
                }
                outputPartDict[equipmentPartInfo.Key].Add(result);
            }
		}

		
		int index = texture.name.IndexOf('_'); // 첫 번째 '_' 위치 찾기
		// '_'가 있으면 해당 위치 이후부터 문자열을 추출
		string characterName = (index >= 0) ? texture.name.Substring(index + 1) : texture.name;
		string filePath = $"C:/Users/tiger/OneDrive/Output_{count}.csv";

		// 기본 이미지 경로 설정
		string basePath = "Assets/Sprites/Character/Character";

		// CSV 데이터 생성
		List<string> csvLines = new List<string>
		{
			"Name,Type,AssetPath" // CSV 헤더
		};

		Dictionary<EEquipmentType, string> equipmentNameDict = new Dictionary<EEquipmentType, string>();
		foreach (var outputPartInfo in outputPartDict)
		{
			string typeString = outputPartInfo.Key.ToString();
			string name = $"{typeString}_{characterName}_001";
			string assetPathString = "";
			for (int i = 0; i < outputPartInfo.Value.Count; i++)
			{
				if(outputPartInfo.Value[i] == "null")
				{
                    assetPathString += $"{outputPartInfo.Value[i]};";
                }
				else
				{
					assetPathString += $"{path}={outputPartInfo.Value[i]};";
				}
			}
			string result = Regex.Replace(assetPathString, ";$", ""); 
			csvLines.Add($"{name},{typeString},{result}");
			
			equipmentNameDict.Add(outputPartInfo.Key, name);
		}

		string characterDataSetLine = "";
		for (EEquipmentType i = EEquipmentType.Hair; i < EEquipmentType.MAX; i++)
		{
			if (equipmentNameDict.ContainsKey(i))
			{
				characterDataSetLine += $"{equipmentNameDict[i]},";
			}
			else
			{
				characterDataSetLine += $" ,";
			}
		}
		csvLines.Add(characterDataSetLine);
		
		// CSV 파일 저장
		try
		{
			File.WriteAllLines(filePath, csvLines);
		}
		catch (Exception ex)
		{
			Debug.LogError($"CSV 파일 저장 중 오류 발생: {ex.Message}");
		}
	}
}
#endif