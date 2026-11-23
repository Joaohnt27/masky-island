using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object=UnityEngine.Object;
public static class AuthorFinale {
 static TMP_FontAsset font;
 static GameObject Named(string name) {foreach(var go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include))if(go.name==name)return go;throw new System.Exception(name);}
 static void Place(RectTransform r,float x,float y,float w,float h) {r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(w,h);}
 static RectTransform Rect(string name,Transform parent,float x,float y,float w,float h) {var go=new GameObject(name,typeof(RectTransform));go.layer=5;go.transform.SetParent(parent,false);var r=go.GetComponent<RectTransform>();Place(r,x,y,w,h);return r;}
 static TMP_Text Label(string name,Transform parent,string value,float y,float w,float h,float size) {var r=Rect(name,parent,0,y,w,h);var t=r.gameObject.AddComponent<TextMeshProUGUI>();Style(t,y,w,h,size);t.text=value;return t;}
 static void Style(TMP_Text t,float y,float w,float h,float size) {Place(t.rectTransform,0,y,w,h);t.font=font;t.fontSharedMaterial=font.material;t.fontStyle=FontStyles.Normal;t.margin=Vector4.zero;t.fontSize=size;t.enableAutoSizing=false;t.alignment=TextAlignmentOptions.Center;t.textWrappingMode=TextWrappingModes.Normal;t.color=new Color(.96f,.94f,.85f);t.raycastTarget=false;}
 static Button Button(string name,Transform parent,string text,float y,float width,UnityAction action) {var r=Rect(name,parent,0,y,width,52);var image=r.gameObject.AddComponent<Image>();image.color=new Color(.22f,.34f,.37f);var b=r.gameObject.AddComponent<Button>();b.targetGraphic=image;Label("Label",r,text,0,width-20,48,21);UnityEventTools.AddPersistentListener(b.onClick,action);return b;}
 static Material Mat(string name,Color color,bool transparent=false) {
  string path="Assets/Art/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Unlit"));AssetDatabase.CreateAsset(m,path);}m.SetColor("_BaseColor",color);
  if(transparent){m.SetFloat("_Surface",1);m.SetFloat("_SrcBlend",5);m.SetFloat("_DstBlend",10);m.SetFloat("_ZWrite",0);m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");m.renderQueue=3000;}return m;
 }
 static void Part(string name,Transform parent,Vector3 p,Vector3 scale,Material material,PrimitiveType type=PrimitiveType.Cube) {var go=GameObject.CreatePrimitive(type);go.name=name;Object.DestroyImmediate(go.GetComponent<Collider>());go.transform.SetParent(parent,false);go.transform.localPosition=p;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=material;}
 public static void Run() {
  font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
  foreach(string sceneName in new[]{"Island","Cave"}) {
   var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+sceneName+".unity");
   Style(Named("EndGameTitle").GetComponent<TMP_Text>(),205,820,65,40);Style(Named("EndGameMessage").GetComponent<TMP_Text>(),40,800,230,25);
   if(sceneName=="Island") Build();
   EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
  }AssetDatabase.SaveAssets();Debug.Log("FINALE AUTHORED");
 }
 static void Build() {
  var canvas=Named("GameplayMenusCanvas").transform;
  var root=new GameObject("LighthouseMechanism",typeof(LighthouseInteraction));root.transform.position=new Vector3(-6,-5.2f,-.35f);var interaction=root.GetComponent<LighthouseInteraction>();
  var stone=Mat("LighthouseStone",new Color(.77f,.76f,.65f));var red=Mat("LighthouseRoof",new Color(.48f,.20f,.14f));var dark=Mat("LighthouseFrame",new Color(.16f,.24f,.25f));var glow=Mat("LighthouseGlow",new Color(1,.85f,.36f));
  Part("Base",root.transform,new Vector3(0,-.05f,0),new Vector3(1,.25f,.7f),stone);
  Part("Tower",root.transform,new Vector3(0,.75f,0),new Vector3(.65f,1.4f,.55f),stone);
  Part("Stripe",root.transform,new Vector3(0,.85f,-.02f),new Vector3(.68f,.25f,.58f),red);
  Part("LanternBase",root.transform,new Vector3(0,1.5f,0),new Vector3(.9f,.16f,.7f),dark);
  Part("Lantern",root.transform,new Vector3(0,1.8f,0),new Vector3(.6f,.45f,.5f),dark);
  Part("Roof",root.transform,new Vector3(0,2.1f,0),new Vector3(.9f,.18f,.7f),red);
  var beacon=new GameObject("LighthouseBeacon");beacon.transform.SetParent(root.transform,false);beacon.transform.localPosition=new Vector3(0,1.8f,-.7f);
  Part("Light",beacon.transform,Vector3.zero,new Vector3(.6f,.45f,.25f),glow,PrimitiveType.Sphere);
  var beams=new GameObject("LightBeams",typeof(MeshFilter),typeof(MeshRenderer));beams.transform.SetParent(beacon.transform,false);
  string path="Assets/Art/Materials/LighthouseBeams.asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(mesh==null){mesh=new Mesh();mesh.vertices=new[]{Vector3.zero,new Vector3(6,.9f,0),new Vector3(6,-.2f,0),new Vector3(-6,.9f,0),new Vector3(-6,-.2f,0)};mesh.triangles=new[]{0,1,2,0,4,3};mesh.RecalculateNormals();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,path);}beams.GetComponent<MeshFilter>().sharedMesh=mesh;beams.GetComponent<MeshRenderer>().sharedMaterial=Mat("LighthouseBeam",new Color(1,.88f,.5f,.23f),true);beacon.SetActive(false);
  var panel=Rect("LighthousePanel",canvas,0,0,700,410);panel.SetAsFirstSibling();panel.gameObject.AddComponent<Image>().color=new Color(.06f,.12f,.15f,.98f);
  Label("LighthouseTitle",panel,"O FAROL DAS LEMBRANÇAS",140,660,55,30).color=new Color(.96f,.77f,.4f);
  var status=Label("LighthouseStatus",panel,"",40,630,115,24);
  var activate=Button("ActivateLighthouse",panel,"USAR MÁSCARA E ACENDER",-65,530,interaction.Activate);
  var close=Button("CloseLighthouse",panel,"VOLTAR À ILHA",-145,260,interaction.Close);
  var prompt=Label("LighthousePrompt",canvas,"[E] MECANISMO DO FAROL",0,600,40,20);var pr=prompt.rectTransform;pr.anchorMin=pr.anchorMax=pr.pivot=new Vector2(.5f,0);pr.anchoredPosition=new Vector2(0,22);prompt.gameObject.SetActive(false);
  var player=Named("Player").GetComponent<PlayerClickMovement>();var data=new SerializedObject(interaction);Set(data,"player",player.transform);Set(data,"playerMovement",player);Set(data,"interactionPrompt",prompt.gameObject);Set(data,"panel",panel.gameObject);Set(data,"statusText",status);Set(data,"activateButton",activate);Set(data,"closeButton",close);Set(data,"beacon",beacon);data.FindProperty("interactionDistance").floatValue=1.8f;data.ApplyModifiedPropertiesWithoutUndo();panel.gameObject.SetActive(false);
 }
 static void Set(SerializedObject data,string name,Object obj)=>data.FindProperty(name).objectReferenceValue=obj;
}
