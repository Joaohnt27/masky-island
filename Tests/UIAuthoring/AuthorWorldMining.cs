using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class AuthorWorldMining
{
    static TMP_FontAsset font;
    static Color cream = new Color(.96f,.94f,.85f);
    static Color gold = new Color(.96f,.77f,.4f);
    static GameObject Named(string name) {
        foreach(var go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include)) if(go.name==name) return go;
        throw new System.Exception(name);
    }
    public static void Run() {
        font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        foreach(string sceneName in new[]{"Island","Cave"}) {
            var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+sceneName+".unity");
            var canvas=Named("GameplayMenusCanvas").transform;
            MakeInventory(canvas);
            if(sceneName=="Island") MakeMining(canvas);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        }
        AssetDatabase.SaveAssets();
        Debug.Log("WORLD MINING AUTHORED");
    }
    static void MakeInventory(Transform canvas) {
        var host=new GameObject("CrystalInventoryController",typeof(CrystalInventoryUI)); host.transform.SetParent(canvas,false);
        var panel=Box("PlayerInventory",canvas,20,-20,380,132,new Color(.06f,.12f,.15f,.96f));
        panel.anchorMin=panel.anchorMax=panel.pivot=new Vector2(0,1);
        var title=Label("InventoryHeading",panel,"INVENTÁRIO",0,47,350,24,16); title.color=gold;
        var icons=new CrystalSymbolGraphic[3]; var labels=new TMP_Text[3];
        Color[] colors={new Color(.98f,.69f,.32f),new Color(.36f,.83f,.85f),new Color(.69f,.58f,.98f)};
        for(int i=0;i<3;i++) {
            float x=(i-1)*118;
            var slot=Box("InventorySlot"+i,panel,x,-6,110,80,new Color(.10f,.19f,.22f));
            var icon=Rect("InventoryIcon"+i,slot,0,10,44,44).gameObject.AddComponent<CrystalSymbolGraphic>();
            icon.Shape=(GameManager.CrystalId)i; icon.color=colors[i]; icon.raycastTarget=false; icons[i]=icon;
            labels[i]=Label("InventoryItem"+i,slot,"VAZIO",0,-25,106,22,12);
        }
        var data=new SerializedObject(host.GetComponent<CrystalInventoryUI>());
        Set(data,"panel",panel.gameObject); Set(data,"title",title); Array(data,"icons",icons); Array(data,"labels",labels); data.ApplyModifiedPropertiesWithoutUndo();
        panel.gameObject.SetActive(false);
    }
    static void MakeMining(Transform canvas) {
        var spot=Named("MiningSpot").GetComponent<MiningSpot>();
        spot.transform.position=new Vector3(4.75f,-5.15f,-.1f);
        var approach=new GameObject("MiningApproach").transform; approach.SetParent(spot.transform,false); approach.localPosition=new Vector3(-.95f,0,0);
        var rockMat=Material("MiningRock",new Color(.26f,.34f,.36f));
        var lightMat=Material("MiningRockLight",new Color(.39f,.47f,.48f));
        var wood=Material("MiningHandle",new Color(.47f,.29f,.13f));
        var metal=Material("MiningMetal",new Color(.75f,.83f,.85f));
        var visual=new GameObject("MineableRock").transform; visual.SetParent(spot.transform,false);
        var main=Boulder("RockCore",visual,new Vector3(0,0,-.35f),new Vector3(.95f,.8f,.6f),rockMat); main.localRotation=Quaternion.Euler(12,8,18);
        Boulder("RockSide",visual,new Vector3(.4f,-.15f,-.38f),new Vector3(.65f,.55f,.5f),lightMat).localRotation=Quaternion.Euler(7,15,-20);
        Boulder("RockFront",visual,new Vector3(-.25f,-.22f,-.6f),new Vector3(.55f,.45f,.4f),lightMat).localRotation=Quaternion.Euler(0,12,20);
        var collider=spot.gameObject.AddComponent<BoxCollider>(); collider.center=new Vector3(.1f,0,-.4f); collider.size=new Vector3(1.5f,1.2f,.8f);
        var tool=new GameObject("MiningPickaxe").transform; tool.SetParent(spot.transform,false);
        Piece("Handle",tool,new Vector3(0,.23f,0),new Vector3(.07f,.58f,.06f),wood);
        Piece("PickHead",tool,new Vector3(0,.49f,0),new Vector3(.52f,.09f,.09f),metal);
        Piece("PickTip",tool,new Vector3(.22f,.43f,0),new Vector3(.07f,.18f,.07f),metal).localRotation=Quaternion.Euler(0,0,25);
        var data=new SerializedObject(spot); Set(data,"approachPoint",approach); data.FindProperty("interactionDistance").floatValue=1.65f; data.ApplyModifiedPropertiesWithoutUndo();
        var fx=spot.gameObject.AddComponent<MiningWorldFeedback>(); data=new SerializedObject(fx);
        Set(data,"spot",spot); Set(data,"player",Named("Player").transform); Set(data,"pickaxe",tool); Set(data,"rockVisual",visual); data.ApplyModifiedPropertiesWithoutUndo(); tool.gameObject.SetActive(false);
        var world=Box("WorldMiningPanel",canvas,20,20,570,125,new Color(.06f,.12f,.15f,.96f));
        world.anchorMin=world.anchorMax=world.pivot=new Vector2(0,0);
        var text=Label("WorldMiningMessage",world,"Clique na rocha para minerar.",0,23,530,52,19);
        var barRoot=Box("WorldMiningProgress",world,-65,-22,360,8,new Color(.2f,.3f,.32f));
        var fill=Box("Fill",barRoot,0,0,360,8,gold); var slider=barRoot.gameObject.AddComponent<Slider>(); fill.anchorMin=Vector2.zero; fill.anchorMax=Vector2.one; fill.sizeDelta=Vector2.zero; fill.anchoredPosition=Vector2.zero; slider.fillRect=fill; slider.minValue=0; slider.maxValue=1; slider.interactable=false;
        var cancelRect=Box("CancelMiningButton",world,202,-22,115,32,new Color(.16f,.25f,.28f)); var button=cancelRect.gameObject.AddComponent<Button>(); button.targetGraphic=cancelRect.GetComponent<Image>(); cancelRect.GetComponent<Image>().raycastTarget=true;
        Label("Label",cancelRect,"CANCELAR",0,0,110,28,14);
        data=new SerializedObject(Named("MiningUI").GetComponent<MiningUI>());
        Set(data,"worldMiningPanel",world.gameObject); Set(data,"worldMiningText",text); Set(data,"worldProgress",slider); Set(data,"worldCancel",button); data.ApplyModifiedPropertiesWithoutUndo();
        var hint=Named("MiningAreaHint").GetComponent<TMP_Text>(); hint.text="Clique na rocha da parte inferior direita para encontrar os cristais.";
        var r=hint.rectTransform; r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,0); r.anchoredPosition=new Vector2(0,20); r.sizeDelta=new Vector2(580,52); hint.fontSize=18;
        world.gameObject.SetActive(false);
    }
    static Material Material(string name,Color color) {
        string path="Assets/Art/Materials/"+name+".mat";
        var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(mat==null) { mat=new Material(Shader.Find("Universal Render Pipeline/Unlit")); AssetDatabase.CreateAsset(mat,path); }
        mat.SetColor("_BaseColor",color); return mat;
    }
    static Transform Boulder(string name,Transform parent,Vector3 position,Vector3 size,Material material) {
        const string path="Assets/Art/Materials/MiningBoulder.asset";
        var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if(mesh==null) {
            mesh=new Mesh(); mesh.name="MiningBoulder";
            var vertices=new System.Collections.Generic.List<Vector3>();
            var dark=new System.Collections.Generic.List<int>(); var light=new System.Collections.Generic.List<int>();
            for(int i=0;i<7;i++) {
                float a=i*Mathf.PI*2/7,b=(i+1)*Mathf.PI*2/7;
                int start=vertices.Count;
                vertices.Add(new Vector3(-.08f,.12f,-.5f));
                vertices.Add(new Vector3(Mathf.Cos(a)*.6f,Mathf.Sin(a)*.55f,0));
                vertices.Add(new Vector3(Mathf.Cos(b)*.6f,Mathf.Sin(b)*.55f,0));
                var face=i%3==0 ? light : dark;
                face.Add(start);face.Add(start+2);face.Add(start+1);
                start=vertices.Count;
                vertices.Add(new Vector3(0,0,.35f));
                vertices.Add(new Vector3(Mathf.Cos(a)*.6f,Mathf.Sin(a)*.55f,0));
                vertices.Add(new Vector3(Mathf.Cos(b)*.6f,Mathf.Sin(b)*.55f,0));
                dark.Add(start);dark.Add(start+1);dark.Add(start+2);
            }
            mesh.SetVertices(vertices);mesh.subMeshCount=2;mesh.SetTriangles(dark,0);mesh.SetTriangles(light,1);mesh.RecalculateNormals();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,path);
        }
        var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer)); go.transform.SetParent(parent,false);go.transform.localPosition=position;go.transform.localScale=size;
        go.GetComponent<MeshFilter>().sharedMesh=mesh;
        go.GetComponent<MeshRenderer>().sharedMaterials=new[]{material,Material("MiningRockFacet",new Color(.48f,.55f,.56f))};return go.transform;
    }
    static Transform Piece(string name,Transform parent,Vector3 position,Vector3 size,Material material) {
        var go=GameObject.CreatePrimitive(PrimitiveType.Cube); go.name=name; Object.DestroyImmediate(go.GetComponent<Collider>());
        go.transform.SetParent(parent,false); go.transform.localPosition=position; go.transform.localScale=size; go.GetComponent<Renderer>().sharedMaterial=material; return go.transform;
    }
    static RectTransform Rect(string name,Transform parent,float x,float y,float w,float h) {
        var go=new GameObject(name,typeof(RectTransform)); go.layer=5; go.transform.SetParent(parent,false); var r=go.GetComponent<RectTransform>();
        r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,.5f); r.anchoredPosition=new Vector2(x,y); r.sizeDelta=new Vector2(w,h); return r;
    }
    static RectTransform Box(string name,Transform parent,float x,float y,float w,float h,Color color) {
        var r=Rect(name,parent,x,y,w,h); var image=r.gameObject.AddComponent<Image>(); image.color=color; image.raycastTarget=false; return r;
    }
    static TMP_Text Label(string name,Transform parent,string value,float x,float y,float w,float h,float size) {
        var r=Rect(name,parent,x,y,w,h); var t=r.gameObject.AddComponent<TextMeshProUGUI>(); t.font=font; t.text=value; t.fontSize=size; t.alignment=TextAlignmentOptions.Center; t.color=cream; t.textWrappingMode=TextWrappingModes.Normal; t.raycastTarget=false; return t;
    }
    static void Set(SerializedObject data,string key,Object obj)=>data.FindProperty(key).objectReferenceValue=obj;
    static void Array(SerializedObject data,string key,Object[] items) { var p=data.FindProperty(key); p.arraySize=items.Length; for(int i=0;i<items.Length;i++)p.GetArrayElementAtIndex(i).objectReferenceValue=items[i]; }
}
