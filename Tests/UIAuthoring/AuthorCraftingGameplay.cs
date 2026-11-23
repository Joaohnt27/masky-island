using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;
public static class AuthorCraftingGameplay {
 static TMP_FontAsset font;
 static void Set(Object o,string name,Object v){var s=new SerializedObject(o);s.FindProperty(name).objectReferenceValue=v;s.ApplyModifiedPropertiesWithoutUndo();}
 static void Place(RectTransform r,float x,float y,float w,float h){r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(w,h);}
 static RectTransform Rect(Transform p,string name,float x,float y,float w,float h){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(p,false);Place(r,x,y,w,h);return r;}
 static TMP_Text Text(Transform p,string name,string value,float x,float y,float w,float h,int size){var t=Rect(p,name,x,y,w,h).gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.fontSize=size;t.text=value;t.color=new Color(.11f,.23f,.23f);t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;return t;}
 static Button Button(Transform p,string name,string value,float x,float y,float w,float h){var r=Rect(p,name,x,y,w,h);var image=r.gameObject.AddComponent<Image>();image.color=new Color(.12f,.27f,.27f);var b=r.gameObject.AddComponent<Button>();b.targetGraphic=image;var text=Text(r,"Label",value,0,0,w-12,h-4,18);text.color=new Color(.97f,.91f,.75f);r.gameObject.AddComponent<ButtonSound>();return b;}
 static Image Picture(Transform p,string name,Sprite sprite,float x,float y,float w,float h){var r=Rect(p,name,x,y,w,h);var i=r.gameObject.AddComponent<Image>();i.sprite=sprite;i.preserveAspect=true;i.raycastTarget=false;return i;}
 public static void Run(){
 font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/FontesOriginais/DynaPuff-VariableFont_wdth,wght SDF.asset");AssetDatabase.Refresh();const string path="Assets/Art/UI/OpenBackpack-v1.png";var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Default;importer.mipmapEnabled=false;importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
 foreach(string name in new[]{"Beach","House","Island","Cave","Lighthouse"}){
 var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");var inv=Object.FindAnyObjectByType<CrystalInventoryUI>();var data=new SerializedObject(inv);var panel=(GameObject)data.FindProperty("panel").objectReferenceValue;var card=panel.transform.Find("PlayerInventory");Place((RectTransform)card,0,0,1020,680);card.GetComponent<Image>().color=Color.clear;var outline=card.GetComponent<Outline>();if(outline!=null)outline.enabled=false;
 var prior=card.Find("BagIllustration");if(prior!=null)Object.DestroyImmediate(prior.gameObject);var art=Rect(card,"BagIllustration",0,0,1020,680).gameObject.AddComponent<RawImage>();art.texture=AssetDatabase.LoadAssetAtPath<Texture>(path);art.raycastTarget=false;art.transform.SetAsFirstSibling();
 var heading=(TMP_Text)data.FindProperty("title").objectReferenceValue;Place(heading.rectTransform,-230,207,415,40);heading.fontSize=20;heading.enableAutoSizing=true;heading.fontSizeMin=16;heading.fontSizeMax=20;
 var close=(Button)data.FindProperty("closeButton").objectReferenceValue;Place((RectTransform)close.transform,280,207,200,36);Place(close.GetComponentInChildren<TMP_Text>(true).rectTransform,0,0,190,32);
 var grid=card.Find("ItemGrid");Place((RectTransform)grid,0,0,1020,680);var slots=data.FindProperty("itemButtons");var icons=data.FindProperty("itemIcons");var labels=data.FindProperty("itemLabels");float[] x={-317,-135,48};
 for(int i=0;i<slots.arraySize;i++){var b=(Button)slots.GetArrayElementAtIndex(i).objectReferenceValue;Place((RectTransform)b.transform,x[i%3],i<3?73:-98,155,140);var icon=(RawImage)icons.GetArrayElementAtIndex(i).objectReferenceValue;Place(icon.rectTransform,0,17,72,72);var label=(TMP_Text)labels.GetArrayElementAtIndex(i).objectReferenceValue;Place(label.rectTransform,0,-47,148,40);label.fontSize=15;label.enableAutoSizing=true;label.fontSizeMin=13;label.fontSizeMax=15;}
 var detail=(TMP_Text)data.FindProperty("itemDescription").objectReferenceValue;Place(detail.rectTransform,280,-57,212,203);detail.textWrappingMode=TextWrappingModes.Normal;detail.overflowMode=TextOverflowModes.Truncate;detail.fontSize=17;detail.enableAutoSizing=true;detail.fontSizeMin=15;detail.fontSizeMax=17;detail.color=new Color(.20f,.22f,.16f);
 var old=card.Find("ItemDetailIcon");if(old!=null)Object.DestroyImmediate(old.gameObject);var detailIcon=Rect(card,"ItemDetailIcon",280,85,100,100).gameObject.AddComponent<RawImage>();detailIcon.raycastTarget=false;data.FindProperty("detailIcon").objectReferenceValue=detailIcon;data.FindProperty("illustratedBag").boolValue=true;data.ApplyModifiedPropertiesWithoutUndo();
 var footer=card.Find("InventoryFooter").GetComponent<TMP_Text>();Place(footer.rectTransform,-35,-230,770,30);footer.fontSize=14;
 if(name=="House"){
 var canvas=Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include).First(c=>c.name=="GameplayMenusCanvas");var root=Rect(canvas.transform,"KeyCraftingPanel",0,0,0,0);root.anchorMin=Vector2.zero;root.anchorMax=Vector2.one;root.offsetMin=root.offsetMax=Vector2.zero;root.SetAsFirstSibling();root.gameObject.AddComponent<Image>().color=new Color(.02f,.04f,.04f,.8f);
 var board=Rect(root,"CraftingBoard",0,0,1100,620);var bg=board.gameObject.AddComponent<Image>();bg.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/DialoguePaper-v2.png");bg.type=Image.Type.Sliced;bg.pixelsPerUnitMultiplier=3;
 Text(board,"CraftingTitle","OFICINA DO FAROLEIRO",0,260,800,48,32);var instruction=Text(board,"CraftingInstruction","",0,201,920,44,24);
 var sprites=AssetDatabase.LoadAllAssetsAtPath("Assets/Art/UI/KeyCrafting-v1.png").OfType<Sprite>().ToArray();Sprite blank=sprites.Single(s=>s.name=="KeyBlank"),moldSprite=sprites.Single(s=>s.name=="KeyMold"),key=sprites.Single(s=>s.name=="LighthouseKey");
 var moldButton=Button(board,"CraftPlaceMold","COLOCAR MOLDE",-355,85,220,110);Place(moldButton.GetComponentInChildren<TMP_Text>().rectTransform,0,-35,210,34);Picture(moldButton.transform,"Icon",moldSprite,0,15,70,65);
 var metalButton=Button(board,"CraftPlaceMetal","ENCAIXAR METAL",-355,-65,220,110);Place(metalButton.GetComponentInChildren<TMP_Text>().rectTransform,0,-35,210,34);Picture(metalButton.transform,"Icon",blank,0,15,70,65);
 var work=Rect(board,"CraftingSurface",0,35,270,280);work.gameObject.AddComponent<Image>().color=new Color(.12f,.20f,.19f,.18f);
 var mold=Picture(work,"PlacedMold",moldSprite,0,0,190,250);var metal=Picture(work,"PlacedMetal",blank,0,0,85,160);var finished=Picture(work,"FinishedKey",key,0,0,120,220);
 Text(board,"CraftingHelp","APOIAR → ENCAIXAR → AJUSTAR\n\nClique em GOLPEAR ou pressione Espaço quando o indicador estiver na faixa dourada.\n\nErrou? Tente de novo. Os materiais estão seguros.",350,28,230,260,19);
 var progress=Text(board,"CraftingProgress","",0,-110,350,30,18);var timing=Rect(board,"CraftingTiming",0,-151,440,30);timing.gameObject.AddComponent<Image>().color=new Color(.13f,.24f,.24f);
 var target=Rect(timing,"StrikeTarget",0,0,100,28);target.gameObject.AddComponent<Image>().color=new Color(.94f,.67f,.23f);var cursor=Rect(timing,"StrikeCursor",0,0,6,44);cursor.gameObject.AddComponent<Image>().color=new Color(.98f,.94f,.80f);
 var action=Button(board,"CraftAction","GOLPEAR  [ESPAÇO]",0,-213,350,46);var closeCraft=Button(board,"CloseCrafting","SAIR  [ESC]",394,-273,190,36);var feedback=Text(board,"CraftingFeedback","",-104,-272,720,52,16);
 var host=new GameObject("KeyCraftingController");var crafting=host.AddComponent<KeyCraftingUI>();Set(crafting,"panel",root.gameObject);Set(crafting,"player",Object.FindAnyObjectByType<PlayerClickMovement>());Set(crafting,"instruction",instruction);Set(crafting,"feedback",feedback);Set(crafting,"progress",progress);Set(crafting,"actionLabel",action.GetComponentInChildren<TMP_Text>());Set(crafting,"moldButton",moldButton);Set(crafting,"metalButton",metalButton);Set(crafting,"actionButton",action);Set(crafting,"closeButton",closeCraft);Set(crafting,"mold",mold);Set(crafting,"metal",metal);Set(crafting,"finishedKey",finished);Set(crafting,"timing",timing.gameObject);Set(crafting,"cursor",cursor);Set(crafting,"target",target);Set(Object.FindAnyObjectByType<ScrollInteraction>(),"craftingUI",crafting);
 var story=new SerializedObject(Object.FindAnyObjectByType<StoryLocation>());var panels=story.FindProperty("focusPanels");panels.InsertArrayElementAtIndex(panels.arraySize);panels.GetArrayElementAtIndex(panels.arraySize-1).objectReferenceValue=root.gameObject;story.ApplyModifiedPropertiesWithoutUndo();root.gameObject.SetActive(false);
 }
 EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
 }
 AssetDatabase.SaveAssets();CraftingGameplayValidation.Run();
 }
}

