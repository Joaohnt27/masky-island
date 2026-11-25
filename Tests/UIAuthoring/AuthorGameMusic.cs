using UnityEditor;
using UnityEngine;
public static class AuthorGameMusic {
 public static void Run(){
 var library=AssetDatabase.LoadAssetAtPath<GameMusicLibrary>("Assets/Resources/GameMusic.asset");
 if(library==null){library=ScriptableObject.CreateInstance<GameMusicLibrary>();AssetDatabase.CreateAsset(library,"Assets/Resources/GameMusic.asset");}
 foreach(string name in new[]{"menu_masky","exploracao_masky"}){
 var path="Assets/Audio/Music/"+name+".mp3";var importer=(AudioImporter)AssetImporter.GetAtPath(path);var settings=importer.defaultSampleSettings;settings.loadType=AudioClipLoadType.Streaming;importer.defaultSampleSettings=settings;importer.SaveAndReimport();
 var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(path);if(clip==null)throw new System.Exception("Missing music: "+path);
 if(name=="menu_masky")library.menu=clip;else library.exploration=clip;
 }
 EditorUtility.SetDirty(library);AssetDatabase.SaveAssets();MusicValidation.Run();
 }
}
