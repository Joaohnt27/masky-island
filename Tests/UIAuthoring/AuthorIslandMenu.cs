using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;
public static class AuthorIslandMenu {
 static TMP_FontAsset font;
 static void Set(Object o,string name,Object v){var s=new SerializedObject(o);s.FindProperty(name).objectReferenceValue=v;s.ApplyModifiedPropertiesWithoutUndo();}
 static void Place(RectTransform r,float x,float y,float w,float h){r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(w,h);}
 static RectTransform Rect(Transform p,string name,float x,float y,float w,float h){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(p,false);Place(r,x,y,w,h);return r;}
 static TMP_Text Text(Transform p,string name,string value,float x,float y,float w,float h,int size){var t=Rect(p,name,x,y,w,h).gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.fontSize=size;t.text=value;t.color=new Color(.11f,.23f,.23f);t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;return t;}
 static Button Button(Transform p,string name,string value,float x,float y,float w,float h){var r=Rect(p,name,x,y,w,h);var image=r.gameObject.AddComponent<Image>();image.color=new Color(.12f,.27f,.27f);var b=r.gameObject.AddComponent<Button>();b.targetGraphic=image;var text=Text(r,"Label",value,0,0,w-12,h-4,18);text.color=new Color(.97f,.91f,.75f);r.gameObject.AddComponent<ButtonSound>();return b;}
 static Image Picture(Transform p,string name,Sprite sprite,float x,float y,float w,float h){var r=Rect(p,name,x,y,w,h);var i=r.gameObject.AddComponent<Image>();i.sprite=sprite;i.preserveAspect=true;i.raycastTarget=false;return i;}

 static Color ink=new Color(.10f,.24f,.24f),cream=new Color(.98f,.91f,.74f);static Sprite paper;static Texture art;
 static GameObject Named(string name)=>Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include).First(x=>x.name==name);
 static void Full(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}
 static void Background(Transform parent,string name){var r=Rect(parent,name,0,0,1280,720);Full(r);var image=r.gameObject.AddComponent<RawImage>();image.texture=art;image.raycastTarget=false;var fit=r.gameObject.AddComponent<AspectRatioFitter>();fit.aspectMode=AspectRatioFitter.AspectMode.EnvelopeParent;fit.aspectRatio=(float)art.width/art.height;r.SetAsFirstSibling();}
 static void Paper(Image image){image.sprite=paper;image.type=Image.Type.Sliced;image.pixelsPerUnitMultiplier=3;image.color=Color.white;}
 static void Skin(Button b,string title,bool parchment=false){
 foreach(var g in b.GetComponentsInChildren<Graphic>(true))if(g.gameObject!=b.gameObject)g.enabled=false;
 var face=b.GetComponent<Image>();if(face==null)face=b.gameObject.AddComponent<Image>();face.enabled=true;face.raycastTarget=true;
 if(parchment){Paper(face);face.pixelsPerUnitMultiplier=9;}else{face.sprite=null;face.color=ink;}
 b.targetGraphic=face;b.transition=Selectable.Transition.ColorTint;var colors=b.colors;colors.normalColor=Color.white;colors.highlightedColor=new Color(1,.87f,.62f);colors.selectedColor=colors.highlightedColor;colors.pressedColor=new Color(.72f,.67f,.54f);colors.disabledColor=new Color(.72f,.72f,.64f,.8f);b.colors=colors;
 var r=(RectTransform)b.transform;var t=Text(r,"StyledLabel",title,0,0,r.sizeDelta.x-24,r.sizeDelta.y-8,20);t.color=parchment?ink:cream;t.raycastTarget=false;
 }
 public static void Run(){
 AssetDatabase.Refresh();font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/FontesOriginais/DynaPuff-VariableFont_wdth,wght SDF.asset");paper=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/DialoguePaper-v2.png");
 const string artPath="Assets/Art/Backgrounds/Menu/IslandBeforeStorm-v1.png";var importer=(TextureImporter)AssetImporter.GetAtPath(artPath);importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.mipmapEnabled=false;importer.SaveAndReimport();art=AssetDatabase.LoadAssetAtPath<Texture>(artPath);
 var scene=EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");var menu=Object.FindAnyObjectByType<MainMenuController>();var canvas=menu.mainMenuPanel.GetComponentInParent<Canvas>();var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1280,720);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;scaler.matchWidthOrHeight=.5f;
 Background(canvas.transform,"IslandMenuArt");
 menu.mainMenuPanel.transform.localPosition=Vector3.zero;menu.mainMenuPanel.transform.localScale=Vector3.one;
 foreach(var graphic in menu.mainMenuPanel.GetComponentsInChildren<Graphic>(true))graphic.enabled=false;
 string[] names={"PlayButton","SettingsButton","CreditsButton","ExitButton"};string[] labels={"JOGAR","CONFIGURAÇÕES","CRÉDITOS","SAIR"};
 for(int i=0;i<4;i++){var b=Named(names[i]).GetComponent<Button>();b.transform.SetParent(menu.mainMenuPanel.transform,false);Place((RectTransform)b.transform,-370,60-i*73,330,58);Skin(b,labels[i],true);}
 var title=Text(menu.mainMenuPanel.transform,"IslandMenuTitle","MASKY\nISLAND",-370,230,470,145,60);title.color=cream;title.lineSpacing=-15;
 var subtitle=Text(menu.mainMenuPanel.transform,"IslandMenuSubtitle","Uma luz para encontrar o caminho.",-370,128,450,35,17);subtitle.color=cream;
 var nick=(RectTransform)menu.nicknamePanel.transform;Full(nick);
 foreach(var g in nick.GetComponentsInChildren<Graphic>(true))if(g.GetComponentInParent<TMP_InputField>(true)==null)g.enabled=false;
 Named("NicknameBackground").GetComponent<Graphic>().enabled=false;
 var input=menu.nicknameInput;Place((RectTransform)input.transform,-370,-10,360,60);input.GetComponent<Image>().enabled=true;input.targetGraphic=input.GetComponent<Image>();input.transition=Selectable.Transition.ColorTint;var inputColors=input.colors;inputColors.normalColor=Color.white;inputColors.highlightedColor=Color.white;inputColors.selectedColor=Color.white;inputColors.pressedColor=Color.white;input.colors=inputColors;input.GetComponent<Image>().sprite=null;input.GetComponent<Image>().color=cream;input.textComponent.font=font;input.textComponent.fontSize=24;input.textComponent.color=ink;input.textComponent.enabled=true;
 var placeholder=input.placeholder as TMP_Text;if(placeholder!=null){placeholder.text="Seu nome";placeholder.font=font;placeholder.fontSize=24;placeholder.color=new Color(.28f,.4f,.38f);}
 var cont=Named("ContinueButton").GetComponent<Button>();cont.transform.SetParent(nick,false);Place((RectTransform)cont.transform,-370,-105,360,58);Skin(cont,"CONTINUAR",true);
 var cancel=Named("CancelButton").GetComponent<Button>();cancel.transform.SetParent(nick,false);Place((RectTransform)cancel.transform,-370,-178,220,44);Skin(cancel,"VOLTAR");
 var nt=Text(nick,"NameHeading","SUA HISTÓRIA\nCOMEÇA AQUI",-370,185,450,110,36);nt.color=cream;Text(nick,"NameHelp","Como devemos chamar você?",-370,70,440,45,20).color=cream;
 var settings=Object.FindAnyObjectByType<SettingsMenuUI>();var so=new SerializedObject(settings);so.FindProperty("font").objectReferenceValue=font;so.ApplyModifiedPropertiesWithoutUndo();var settingsPanel=(GameObject)so.FindProperty("settingsPanel").objectReferenceValue;Background(settingsPanel.transform,"SettingsIslandArt");var card=settingsPanel.transform.Find("SettingsCard");Paper(card.GetComponent<Image>());
 foreach(var raw in card.GetComponentsInChildren<RawImage>(true))raw.enabled=false;
 foreach(var t in card.GetComponentsInChildren<TMP_Text>(true)){t.font=font;t.fontSharedMaterial=font.material;t.color=ink;t.fontSize=Mathf.Min(t.fontSize,24);}
 foreach(var img in settingsPanel.GetComponentsInChildren<Image>(true))if(img.name=="Backdrop")img.color=new Color(.02f,.06f,.07f,.58f);
 foreach(var heading in card.GetComponentsInChildren<TMP_Text>(true))if(heading.text=="CONFIGURAÇÕES")heading.fontSize=32;
 foreach(var b in card.GetComponentsInChildren<Button>(true)){var label=b.GetComponentInChildren<TMP_Text>(true);Skin(b,label!=null?label.text:b.name);}
 foreach(var slider in card.GetComponentsInChildren<Slider>(true)){slider.interactable=true;slider.handleRect.gameObject.SetActive(true);var handle=slider.handleRect.GetComponent<Image>();handle.enabled=true;slider.targetGraphic=handle;var tint=slider.colors;tint.normalColor=Color.white;tint.highlightedColor=new Color(1,.84f,.5f);slider.colors=tint;slider.fillRect.GetComponent<Image>().enabled=true;slider.fillRect.GetComponent<Image>().color=new Color(.82f,.57f,.21f);slider.handleRect.GetComponent<Image>().color=ink;var bg=slider.transform.Find("Background");if(bg!=null)bg.GetComponent<Image>().color=ink;}
 foreach(var drop in card.GetComponentsInChildren<TMP_Dropdown>(true)){drop.GetComponent<Image>().enabled=true;drop.targetGraphic=drop.GetComponent<Image>();drop.transition=Selectable.Transition.ColorTint;var fieldColors=drop.colors;fieldColors.normalColor=Color.white;fieldColors.highlightedColor=new Color(1,.94f,.8f);fieldColors.selectedColor=fieldColors.highlightedColor;fieldColors.pressedColor=Color.white;drop.colors=fieldColors;drop.GetComponent<Image>().sprite=null;var arrow=drop.transform.Find("Arrow");if(arrow!=null){arrow.GetComponent<Image>().enabled=false;var mark=Rect(arrow,"DropdownChevron",0,0,13,10).gameObject.AddComponent<CrystalSymbolGraphic>();mark.Shape=GameManager.CrystalId.Triangle;mark.rectTransform.localRotation=Quaternion.Euler(0,0,180);mark.color=ink;mark.raycastTarget=false;}drop.GetComponent<Image>().color=new Color(.9f,.84f,.69f);foreach(var t in drop.GetComponentsInChildren<TMP_Text>(true)){t.font=font;t.fontSharedMaterial=font.material;t.color=ink;t.fontSize=21;}}
 var pages=so.FindProperty("pages");var controls=(GameObject)pages.GetArrayElementAtIndex(2).objectReferenceValue;
 foreach(var g in controls.GetComponentsInChildren<Graphic>(true))g.enabled=false;
 Text(controls.transform,"GameControls","W A S D  —  Mover\nClique  —  Interagir e seguir caminhos\nI  —  Mochila     M  —  Mapa     J  —  Diário\nEsc  —  Pausar ou fechar painéis",0,0,830,280,24);
 EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
 scene=EditorSceneManager.OpenScene("Assets/Scenes/Credits.unity");var credits=Object.FindAnyObjectByType<CreditsController>();var cs=new SerializedObject(credits);var content=(TMP_Text)cs.FindProperty("creditsText").objectReferenceValue;var viewport=(RectTransform)cs.FindProperty("viewport").objectReferenceValue;var creditsCanvas=content.GetComponentInParent<Canvas>();Background(creditsCanvas.transform,"CreditsIslandArt");Named("Background").GetComponent<Image>().color=new Color(.02f,.06f,.07f,.56f);
 var sheet=Rect(creditsCanvas.transform,"CreditsPaper",0,0,850,650);Paper(sheet.gameObject.AddComponent<Image>());sheet.SetSiblingIndex(2);
 var creditTitle=Named("Title").GetComponent<TMP_Text>();Place(creditTitle.rectTransform,0,270,730,60);creditTitle.font=font;creditTitle.fontSharedMaterial=font.material;creditTitle.fontSize=34;creditTitle.color=ink;
 Place(viewport,0,-5,710,440);content.rectTransform.sizeDelta=new Vector2(670,content.rectTransform.sizeDelta.y);content.font=font;content.fontSharedMaterial=font.material;content.fontSize=22;content.color=ink;content.textWrappingMode=TextWrappingModes.Normal;
 var back=Named("BackButton").GetComponent<Button>();Place((RectTransform)back.transform,0,-275,300,48);Skin(back,"VOLTAR AO MENU");
 EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();IslandMenuValidation.Run();
 }
}



