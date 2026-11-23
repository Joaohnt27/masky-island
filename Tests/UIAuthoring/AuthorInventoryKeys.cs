using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;
public static class AuthorInventoryKeys {
 static GameObject Named(string name) { foreach(var go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include))if(go.name==name)return go;throw new System.Exception(name); }
 static void Move(RectTransform r,float x,float y,float w,float h) { r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(w,h); }
 static TMP_Text Label(string name,Transform parent,string text,float y,float width,float height,float size) {
  var go=new GameObject(name,typeof(RectTransform));go.layer=5;go.transform.SetParent(parent,false);Move(go.GetComponent<RectTransform>(),0,y,width,height);
  var t=go.AddComponent<TextMeshProUGUI>();t.font=Named("InventoryHeading").GetComponent<TMP_Text>().font;t.text=text;t.fontSize=size;t.alignment=TextAlignmentOptions.Center;t.color=new Color(.96f,.94f,.85f);t.raycastTarget=false;return t;
 }
 public static void Run() { foreach(string sceneName in new[]{"Island","Cave"}) {
  var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+sceneName+".unity");
  var panel=Named("PlayerInventory").GetComponent<RectTransform>();Move(panel,0,0,560,285);panel.SetAsLastSibling();panel.GetComponent<Image>().raycastTarget=true;
  var heading=Named("InventoryHeading").GetComponent<TMP_Text>();Move(heading.rectTransform,0,105,520,35);heading.fontSize=23;
  for(int i=0;i<3;i++) { Move(Named("InventorySlot"+i).GetComponent<RectTransform>(),(i-1)*168,0,150,120);Move(Named("InventoryIcon"+i).GetComponent<RectTransform>(),0,12,64,64);var t=Named("InventoryItem"+i).GetComponent<TMP_Text>();Move(t.rectTransform,0,-42,146,25);t.fontSize=15; }
  var close=Label("CloseInventoryButton",panel,"FECHAR  [I / ESC]",-105,250,36,16);
  var closeObject=close.gameObject; Object.DestroyImmediate(close);
  var image=closeObject.AddComponent<Image>();image.color=new Color(.14f,.24f,.27f);image.raycastTarget=true;
  // Texto em filho mantém a ordem de desenho acima do fundo do botão.

  var button=image.gameObject.AddComponent<Button>();button.targetGraphic=image;
  Label("CloseInventoryLabel",button.transform,"FECHAR  [I / ESC]",0,240,32,16);
  var hint=Label("InventoryKeyHint",panel.parent,"[I]  INVENTÁRIO",0,190,32,17);
  var r=hint.rectTransform;r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(20,-20);
  var data=new SerializedObject(Named("CrystalInventoryController").GetComponent<CrystalInventoryUI>());
  data.FindProperty("keyHint").objectReferenceValue=hint.gameObject;data.FindProperty("closeButton").objectReferenceValue=button;data.ApplyModifiedPropertiesWithoutUndo();hint.gameObject.SetActive(false);panel.gameObject.SetActive(false);
  if(sceneName=="Island") {data=new SerializedObject(Named("MiningSpot").GetComponent<MiningSpot>());data.FindProperty("miningDuration").floatValue=4;data.FindProperty("crystalChance").floatValue=.35f;data.ApplyModifiedPropertiesWithoutUndo();}
  EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
 } Debug.Log("INVENTORY KEYS AUTHORED"); }
}
