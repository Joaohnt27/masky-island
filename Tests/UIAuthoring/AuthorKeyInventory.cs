using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;
public static class AuthorKeyInventory {
 static TMP_FontAsset font;
 static GameObject N(string name)=>Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include).First(g=>g.name==name);
 static void Set(Object target,string field,Object value){var s=new SerializedObject(target);s.FindProperty(field).objectReferenceValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
 static RectTransform Rect(Transform p,string n,float x,float y,float w,float h){var r=new GameObject(n,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(p,false);Place(r,x,y,w,h);return r;}
 static void Place(RectTransform r,float x,float y,float w,float h){r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(w,h);}
 static TMP_Text Text(Transform p,string n,string text,float x,float y,float w,float h,int size){var t=Rect(p,n,x,y,w,h).gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.text=text;t.fontSize=size;t.color=new Color(.12f,.23f,.23f);t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;return t;}
 public static void Run(){
 font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/FontesOriginais/DynaPuff-VariableFont_wdth,wght SDF.asset");const string path="Assets/Art/UI/KeyCrafting-v1.png";AssetDatabase.Refresh();
 var tex=new Texture2D(2,2);tex.LoadImage(File.ReadAllBytes(path));var pixels=tex.GetPixels32();float[] boundaries={0,.2f,.4f,.59f,.796f,1};string[] names={"KeyBlank","KeyMold","LighthouseKey","HatchClosed","HatchOpen"};var metadata=new SpriteMetaData[5];
 for(int i=0;i<5;i++){int x0=Mathf.RoundToInt(tex.width*boundaries[i]),x1=Mathf.RoundToInt(tex.width*boundaries[i+1]),minX=x1,minY=tex.height,maxX=x0,maxY=0;for(int y=0;y<tex.height;y++)for(int x=x0;x<x1;x++)if(pixels[y*tex.width+x].a>32){minX=Math.Min(minX,x);minY=Math.Min(minY,y);maxX=Math.Max(maxX,x);maxY=Math.Max(maxY,y);}metadata[i]=new SpriteMetaData{name=names[i],rect=new Rect(minX,minY,maxX-minX+1,maxY-minY+1),alignment=(int)SpriteAlignment.Center,pivot=Vector2.one*.5f};}
 Object.DestroyImmediate(tex);var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.maxTextureSize=4096;importer.textureCompression=TextureImporterCompression.Uncompressed;
#pragma warning disable 0618
 importer.spritesheet=metadata;
#pragma warning restore 0618
 importer.SaveAndReimport();var sprites=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();var atlas=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
 foreach(string name in new[]{"Beach","House","Island","Cave","Lighthouse"}){
 var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");var inv=Object.FindAnyObjectByType<CrystalInventoryUI>();var data=new SerializedObject(inv);var card=N("PlayerInventory");Place((RectTransform)card.transform,0,0,960,610);
 foreach(var t in card.GetComponentsInChildren<TMP_Text>(true))t.font=font;
 for(int i=0;i<3;i++){var old=card.transform.Find("InventorySlot"+i);if(old!=null)Object.DestroyImmediate(old.gameObject);}
 foreach(string n in new[]{"InventorySubtitle","InventoryHelp","InventoryFooter","ItemGrid","ItemDescription"}){var old=card.transform.Find(n);if(old!=null)Object.DestroyImmediate(old.gameObject);}
 Place((RectTransform)N("InventoryHeading").transform,-95,257,680,42);N("InventoryHeading").GetComponent<TMP_Text>().fontSize=24;
 Place((RectTransform)N("CloseInventoryButton").transform,350,257,190,40);
 var grid=Rect(card.transform,"ItemGrid",0,35,840,350);var icons=data.FindProperty("itemIcons");var labels=data.FindProperty("itemLabels");var buttons=data.FindProperty("itemButtons");icons.arraySize=labels.arraySize=buttons.arraySize=6;
 for(int i=0;i<6;i++){var slot=Rect(grid,"ItemSlot"+i,(i%3-1)*285,82-(i/3)*176,266,160);var face=slot.gameObject.AddComponent<Image>();face.color=new Color(.94f,.87f,.70f);var button=slot.gameObject.AddComponent<Button>();button.targetGraphic=face;slot.gameObject.AddComponent<ButtonSound>();var icon=Rect(slot,"ItemIcon",0,18,92,92).gameObject.AddComponent<RawImage>();icon.raycastTarget=false;var label=Text(slot,"ItemName","",0,-54,250,45,17);icons.GetArrayElementAtIndex(i).objectReferenceValue=icon;labels.GetArrayElementAtIndex(i).objectReferenceValue=label;buttons.GetArrayElementAtIndex(i).objectReferenceValue=button;}
 var detail=Text(card.transform,"ItemDescription","",0,-200,830,96,19);detail.color=new Color(.96f,.89f,.73f);Set(inv,"itemDescription",detail);
 var footer=Text(card.transform,"InventoryFooter","Selecione um item para examinar • I ou Esc para fechar",0,-278,850,30,15);footer.color=new Color(.88f,.79f,.60f);
 var visuals=data.FindProperty("itemVisuals");visuals.arraySize=7;for(int i=0;i<7;i++){var v=visuals.GetArrayElementAtIndex(i);v.FindPropertyRelative("item").enumValueIndex=i;Texture texture;UnityEngine.Rect uv=new UnityEngine.Rect(0,0,1,1);
 if(i<3)texture=AssetDatabase.LoadAssetAtPath<Texture>("Assets/Art/UI/Crystals/"+new[]{"Triangle","Circle","Square"}[i]+"-v1.png");else if(i==3)texture=AssetDatabase.LoadAssetAtPath<Texture>("Assets/Art/Tools/Pickaxe-v1.png");else{texture=atlas;var sprite=sprites.Single(p=>p.name==names[i-4]);uv=new UnityEngine.Rect(sprite.rect.x/atlas.width,sprite.rect.y/atlas.height,sprite.rect.width/atlas.width,sprite.rect.height/atlas.height);}v.FindPropertyRelative("texture").objectReferenceValue=texture;v.FindPropertyRelative("uv").rectValue=uv;}
 data.FindProperty("itemDescription").objectReferenceValue=detail;data.FindProperty("icons").arraySize=0;data.FindProperty("labels").arraySize=0;data.ApplyModifiedPropertiesWithoutUndo();
 var tasks=N("JournalTasks").GetComponent<TMP_Text>();tasks.enableAutoSizing=true;tasks.fontSizeMin=13;tasks.fontSizeMax=16;var notes=N("JournalNotes").GetComponent<TMP_Text>();notes.enableAutoSizing=true;notes.fontSizeMin=12;notes.fontSizeMax=15;
 if(name=="House"){var scroll=Object.FindAnyObjectByType<ScrollInteraction>();var ss=new SerializedObject(scroll);var old=(GameObject)ss.FindProperty("returnPanel").objectReferenceValue;if(old!=null){old.SetActive(false);foreach(var puzzle in old.GetComponentsInChildren<SymbolSequencePuzzle>(true))puzzle.enabled=false;}ss.ApplyModifiedPropertiesWithoutUndo();}
 if(name=="Cave"){
 var old=GameObject.Find("CaveKeyCache");if(old!=null)Object.DestroyImmediate(old);var root=new GameObject("CaveKeyCache");root.transform.position=new Vector3(.25f,-.15f,-1.6f);var cache=root.AddComponent<CaveKeyCache>();var hit=root.AddComponent<BoxCollider>();hit.size=new Vector3(2.3f,1.1f,.2f);
 var approach=new GameObject("CacheApproach").transform;approach.SetParent(root.transform);approach.position=new Vector3(.25f,-1.2f,-1);
 var visual=new GameObject("StoneHatch",typeof(SpriteRenderer));visual.transform.SetParent(root.transform,false);var sr=visual.GetComponent<SpriteRenderer>();sr.sprite=sprites.Single(p=>p.name=="HatchClosed");sr.sortingOrder=2;float scale=2.35f/sr.sprite.bounds.size.x;root.transform.localScale=Vector3.one;visual.transform.localScale=Vector3.one; // Sprite PPU provides world size below.
 var sizeRoot=new GameObject("HatchScale").transform;sizeRoot.SetParent(root.transform,false);sizeRoot.localScale=Vector3.one*scale;visual.transform.SetParent(sizeRoot,false);
 var loot=new GameObject("CacheMaterials");loot.transform.SetParent(root.transform,false);for(int i=0;i<2;i++){var item=new GameObject(names[i],typeof(SpriteRenderer));item.transform.SetParent(loot.transform,false);item.transform.localPosition=new Vector3(i==0?-.30f:.30f,0,-.15f);var r=item.GetComponent<SpriteRenderer>();r.sprite=sprites.Single(p=>p.name==names[i]);r.sortingOrder=3;item.transform.localScale=Vector3.one*(.55f/r.sprite.bounds.size.y);}
 Set(cache,"hatch",sr);Set(cache,"closedSprite",sr.sprite);Set(cache,"openSprite",sprites.Single(p=>p.name=="HatchOpen"));Set(cache,"loot",loot);Set(cache,"dialogue",Object.FindAnyObjectByType<DialogueController>());Set(cache,"player",Object.FindAnyObjectByType<PlayerClickMovement>().transform);Set(cache,"approachPoint",approach);var cs=new SerializedObject(cache);cs.FindProperty("interactionDistance").floatValue=.45f;cs.FindProperty("actionLabel").stringValue="Recolher materiais do esconderijo";cs.ApplyModifiedPropertiesWithoutUndo();
 }
 EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
 }
 AssetDatabase.SaveAssets();KeyInventoryValidation.Run();
 }
}
